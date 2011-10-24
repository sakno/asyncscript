using System;
using System.Linq.Expressions;
using System.Threading;
using System.Collections.Generic;

namespace DynamicScript.Runtime.Environment.Threading
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;
    using TransparentActionAttribute = Debugging.TransparentActionAttribute;

    /// <summary>
    /// Represents asynchronous action.
    /// This class cannot be inherited.
    /// </summary>
    [ComVisible(false)]
    sealed class ScriptAsyncAction: ScriptActionBase
    {
        #region Nested Types
        [ComVisible(false)]
        [SlotStore]
        private interface IWrappedScriptActionSlots
        {
            IRuntimeSlot Cancelled { get; }
            IRuntimeSlot Notify { get; }
        }

        [ComVisible(false)]
        private sealed class CancelledSlot : RuntimeSlotBase<IScriptAsyncActionExecutionContext, ScriptBoolean>
        {
            public CancelledSlot(IScriptAsyncActionExecutionContext executionContext)
                : base(executionContext, ScriptBooleanContract.Instance, RuntimeSlotAttributes.Immutable)
            {
            }

            public override ScriptBoolean Value
            {
                get { return Target.Cancelled; }
                set { }
            }
        }

        [ComVisible(false)]
        private sealed class NotifyAction : ScriptAction<ScriptReal, IScriptObject>
        {
            private const string FirstParamName = "progress";
            private const string SecondParamName = "state";

            private readonly IScriptAsyncActionExecutionContext m_context;

            public NotifyAction(IScriptAsyncActionExecutionContext executionContext)
                : base(FirstParamName, ScriptRealContract.Instance, SecondParamName, ScriptSuperContract.Instance)
            {
                m_context = executionContext;
            }

            protected override void Invoke(ScriptReal progress, IScriptObject asyncState, InterpreterState state)
            {
                m_context.Notify(progress, asyncState, state);
            }
        }

        [ComVisible(false)]
        [TransparentAction]
        private sealed class WrappedScriptAction : ScriptActionBase, IWrappedScriptActionSlots
        {
            private IRuntimeSlot m_cancelled;
            private IRuntimeSlot m_notify;
            private readonly IScriptAction m_action;
            private readonly IScriptAsyncActionExecutionContext m_context;

            public WrappedScriptAction(ScriptActionBase action, IScriptAsyncActionExecutionContext executionContext)
                : base(action)
            {
                m_action = action;
                m_context = executionContext;
            }

            IRuntimeSlot IWrappedScriptActionSlots.Cancelled
            {
                get { return Cache(ref m_cancelled, () => new CancelledSlot(m_context)); }
            }

            IRuntimeSlot IWrappedScriptActionSlots.Notify
            {
                get { return CacheConst(ref m_notify, () => new NotifyAction(m_context)); }
            }

            protected override IScriptObject InvokeCore(IList<IScriptObject> args, InterpreterState state)
            {
                return m_action.Invoke(args, state);
            }
        }

        [ComVisible(false)]
        private sealed class AsyncInvocationContext
        {
            private readonly IList<IScriptObject> m_arguments;
            private readonly InterpreterState m_state;
            private readonly ScriptActionBase m_action;
            private readonly AsyncCallback m_callback;

            public AsyncInvocationContext(ScriptActionBase action, IList<IScriptObject> args, InterpreterState state, AsyncCallback callback=null)
            {
                if (action == null) throw new ArgumentNullException("action");
                m_action = action;
                m_arguments = args ?? new IScriptObject[0];
                m_state = state ?? InterpreterState.Current;
                m_callback = callback;
            }

            public bool EndExecution(IScriptAsyncResult ar)
            {
                switch (m_callback != null)
                {
                    case true:
                        m_callback.Invoke(ar);
                        return true;
                    default: return false;
                }
            }

            public ScriptFault WrapException(Exception e)
            {
                return new ScriptFault(e.Message, m_state);
            }

            public IScriptObject Invoke(IScriptAsyncActionExecutionContext executionContext)
            {
                return new WrappedScriptAction(m_action, executionContext).Invoke(m_arguments, m_state);
            }
        }

        /// <summary>
        /// Represents execution context.
        /// This class cannot be inherited.
        /// </summary>
        [ComVisible(false)]
        private sealed class AsynchronousFlowControl : IScriptAsyncActionExecutionContext
        {
            private bool m_cancelled = false;
            private IScriptAction m_notifier;

            bool IScriptAsyncActionExecutionContext.Cancelled
            {
                get { return m_cancelled; }
            }

            /// <summary>
            /// Notifies action bounded to this context that the execution is cancelled.
            /// </summary>
            public void Cancel()
            {
                m_cancelled = true;
            }

            /// <summary>
            /// Gets or sets notifier.
            /// </summary>
            public IScriptAction Notifier
            {
                get { return m_notifier; }
                set { m_notifier = value; }
            }

            private static bool Notify(IScriptAction notifier, IScriptObject[] args, InterpreterState state)
            {
                switch (notifier != null && notifier.CanInvoke(args))
                {
                    case true:
                        notifier.Invoke(args, state);
                        return true;
                    default: return false;
                }
            }

            void IScriptAsyncActionExecutionContext.Notify(double progress, IScriptObject asyncState, InterpreterState state)
            {
                Notify(m_notifier, new[] { new ScriptReal(progress), asyncState ?? ScriptObject.Void }, state);
            }
        }

        [ComVisible(false)]
        private sealed class ScriptAsyncResultSlim : IScriptAsyncResult
        {
            private readonly EventWaitHandle m_handle;
            private readonly AsynchronousFlowControl m_context;
            private IScriptObject m_result;
            private ScriptFault m_error;
            private DateTime? m_stopTime;
            private DateTime m_startTime;

            private ScriptAsyncResultSlim()
            {
                m_startTime = DateTime.Now;
                m_handle = new ManualResetEvent(false);
                m_result = null;
                m_context = new AsynchronousFlowControl();
            }

            public static ScriptAsyncResultSlim QueueAction(ScriptActionBase action, IList<IScriptObject> args, InterpreterState state, AsyncCallback callback = null)
            {
                var control = new ScriptAsyncResultSlim();
                ThreadPool.QueueUserWorkItem(control.QueueAction, new AsyncInvocationContext(action, args, state, callback));
                return control;
            }

            private void QueueAction(AsyncInvocationContext context)
            {
                try
                {
                    m_result = context.Invoke(m_context) ?? ScriptObject.Void;
                }
                catch (ScriptFault e)
                {
                    m_error = e;
                }
                catch (Exception e)
                {
                    m_error = context.WrapException(e);
                }
                finally
                {
                    m_handle.Set();
                    m_stopTime = DateTime.Now;
                    context.EndExecution(this);
                }
            }

            private void QueueAction(object context)
            {
                QueueAction((AsyncInvocationContext)context);
            }

            /// <summary>
            /// Gets error occured during action execution.
            /// </summary>
            public ScriptFault Error
            {
                get { return m_error; }
            }

            /// <summary>
            /// Cancels asynchronous execution.
            /// </summary>
            public void Cancel()
            {
                m_context.Cancel();
            }

            /// <summary>
            /// Gets or sets notifier.
            /// </summary>
            public IScriptAction Notifier
            {
                get { return m_context.Notifier; }
                set { m_context.Notifier = value; }
            }

            /// <summary>
            /// Gets result of asynchronous action execution.
            /// </summary>
            public IScriptObject Result
            {
                get { return m_result ?? Void; }
            }

            /// <summary>
            /// Gets duration of action execution.
            /// </summary>
            public TimeSpan Duration
            {
                get { return (m_stopTime.HasValue ? m_stopTime.Value : DateTime.Now) - m_startTime; }
            }

            object IAsyncResult.AsyncState
            {
                get { return null; }
            }

            public WaitHandle AsyncWaitHandle
            {
                get { return m_handle; }
            }

            bool IAsyncResult.CompletedSynchronously
            {
                get { return false; }
            }

            /// <summary>
            /// Gets a value indicating that the asynchronous action execution is completed.
            /// </summary>
            public bool IsCompleted
            {
                get { return m_result != null; }
            }
        }
        #endregion

        private readonly ScriptActionBase m_action;

        private ScriptAsyncAction(ScriptActionBase syncAction)
            : base(new ScriptActionContract(syncAction.Parameters, new ScriptAsyncResultContract(syncAction.ReturnValueContract)), syncAction.This)
        {
            m_action = syncAction;
        }

        /// <summary>
        /// Transforms synchronous action to its asynchronous representation.
        /// </summary>
        /// <param name="syncAction"></param>
        /// <returns></returns>
        public static ScriptAsyncAction FromSynchronous(ScriptActionBase syncAction)
        {
            return syncAction is ScriptAsyncAction ? ((ScriptAsyncAction)syncAction) : new ScriptAsyncAction(syncAction);
        }

        internal static MethodCallExpression Bind(Expression actionContract, Expression @this, LambdaExpression implementation)
        {
            var fromsync = LinqHelpers.BodyOf<ScriptRuntimeAction, ScriptAsyncAction, MethodCallExpression>(a => FromSynchronous(a));
            return fromsync.Update(null, new[] { ScriptRuntimeAction.New(actionContract, @this, implementation) });
        }

        protected override IScriptObject InvokeCore(IList<IScriptObject> arguments, InterpreterState state)
        {
            return new ScriptAsyncResult(ScriptAsyncResultSlim.QueueAction(m_action, arguments, state), m_action.ReturnValueContract);
        }
    }
}
