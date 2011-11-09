using System;
using System.Linq.Expressions;
using System.Threading;
using System.Collections.Generic;
using System.Reflection;

namespace DynamicScript.Runtime.Environment.Threading
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;
    using TransparentActionAttribute = Debugging.TransparentActionAttribute;

    /// <summary>
    /// Represents asynchronous action.
    /// This class cannot be inherited.
    /// </summary>
    [ComVisible(false)]
    sealed class ScriptLazyAction : ScriptActionBase, IScriptLazyAction, IScriptLazyActionSlots
    {
        #region Nested Types
        /// <summary>
        /// Represents work item that executes the passed script object synchronously.
        /// This class cannot be inherited.
        /// </summary>
        [ComVisible(false)]
        private sealed class SynchronousItem
        {
            /// <summary>
            /// Represents synchronous action invocation arguments.
            /// </summary>
            public readonly IList<IScriptObject> Arguments;

            /// <summary>
            /// Initializes a new synchronous work item with the specified set of arguments.
            /// </summary>
            /// <param name="arguments">Invocation arguments.</param>
            public SynchronousItem(IList<IScriptObject> arguments)
            {
                Arguments = arguments ?? new IScriptObject[0];
            }

            public static IScriptObject Invoke(IScriptObject synchronous, IList<IScriptObject> arguments, IScriptObject acceptor, InterpreterState state)
            {
                var result = default(IScriptObject);
                var error = default(IScriptObject);
                try
                {
                    result = synchronous.Invoke(arguments, state);
                }
                catch (Exception e)
                {
                    result = Void;
                    error = e is ScriptFault ? ((ScriptFault)e).Fault : new ScriptWrappedException(e);
                }
                finally
                {
                    if (!IsVoid(acceptor))
                        acceptor.Invoke(new[] { error ?? Void, result }, state);
                }
                return result;
            }

            public IScriptObject Invoke(IScriptObject synchronous, InterpreterState state)
            {
                var acceptor = default(IScriptObject);
                var args = default(IList<IScriptObject>);
                switch (Arguments.Count)
                {
                    case 0: acceptor = null; args = Arguments; break;   //no callback
                    case 1: acceptor = Arguments[0]; args = new IScriptObject[0]; break;   //only callback
                    default:
                        args = new IScriptObject[Arguments.Count - 1];
                        acceptor = Arguments[args.Count];               //acceptor is the last argument
                        Arguments.CopyTo(0, args, 0, args.Count);
                        break;
                }
                return Invoke(synchronous, args, acceptor, state);
            }

            public static implicit operator ScriptWorkItem(SynchronousItem item)
            {
                return item != null ? new ScriptWorkItem(item.Invoke) : null;
            }
        }

        [ComVisible(false)]
        private sealed class QueueSlot : RuntimeSlotBase, IEquatable<QueueSlot>
        {
            public const string Name = "queue";
            private readonly IScriptLazyAction m_action;

            public QueueSlot(IScriptLazyAction action)
            {
                m_action = action;
            }

            public override IScriptObject GetValue(InterpreterState state)
            {
                return m_action.Queue is IScriptObject ? (IScriptObject)m_action.Queue : new ScriptNativeQueue(m_action.Queue);
            }

            public override void SetValue(IScriptObject value, InterpreterState state)
            {
                m_action.Queue = ThreadManager.CreateQueue(value);
            }

            public override IScriptContract ContractBinding
            {
                get { return ScriptNativeQueue.ContractBinding; }
            }

            public override RuntimeSlotAttributes Attributes
            {
                get { return RuntimeSlotAttributes.None; }
            }

            protected override ICollection<string> Slots
            {
                get { return new string[0]; }
            }

            public override bool DeleteValue()
            {
                m_action.Queue = null;
                return true;
            }

            public bool Equals(QueueSlot slot)
            {
                return slot != null && ReferenceEquals(m_action, slot.m_action);
            }

            public override bool Equals(IRuntimeSlot other)
            {
                return Equals(other as QueueSlot);
            }

            public override int GetHashCode()
            {
                return m_action.GetHashCode();
            }
        }
        #endregion

        private readonly ScriptActionBase m_synchronous;
        private IScriptWorkItemQueue m_queue;
        private IRuntimeSlot m_qslot;

        private static IEnumerable<ScriptActionContract.Parameter> CreateAsyncParameters(IEnumerable<ScriptActionContract.Parameter> parameters, IScriptContract resultType)
        {
            foreach (var p in parameters) yield return p;
            yield return new ScriptActionContract.Parameter("acceptor", new ScriptAcceptorContract(IsVoid(resultType) ? null : resultType));
        }

        private static ScriptActionContract CreateAsyncContract(ScriptActionContract synchronousSignature)
        {
            return new ScriptActionContract(CreateAsyncParameters(synchronousSignature.Parameters, synchronousSignature.ReturnValueContract), Void);
        }

        /// <summary>
        /// Initializes a new asynchronous action using the specified synchronous action.
        /// </summary>
        /// <param name="synchronous">Synchronous implementation.</param>
        public ScriptLazyAction(ScriptActionBase synchronous)
            : base(CreateAsyncContract(synchronous.ContractBinding), synchronous.This)
        {
            m_synchronous = synchronous;
        }

        internal static NewExpression New(Expression actionContract, Expression @this, LambdaExpression implementation, string sourceCode)
        {
            var ctor = LinqHelpers.BodyOf<ScriptActionBase, ScriptLazyAction, NewExpression>(a => new ScriptLazyAction(a));
            return ctor.Update(new[] { ScriptRuntimeAction.New(actionContract, @this, implementation, sourceCode) });
        }

        /// <summary>
        /// Gets or sets work item queue that is used to enqueue encapsulated synchronous action.
        /// </summary>
        public IScriptWorkItemQueue Queue
        {
            get { return m_queue ?? ThreadManager.Queue; }
            set { m_queue = value; }
        }

        /// <summary>
        /// Enqueues synchronous action.
        /// </summary>
        /// <param name="arguments">Invocation arguments.</param>
        /// <param name="state">Internal interpreter state.</param>
        /// <returns>The lambda object that can be used to synchronize with enqueued lambda.</returns>
        protected override IScriptObject InvokeCore(IList<IScriptObject> arguments, InterpreterState state)
        {
            return ScriptNativeQueue.CreateAwaitLambda(Queue.Enqueue(m_synchronous, new SynchronousItem(arguments), state));
        }

        IRuntimeSlot IScriptLazyActionSlots.Queue
        {
            get { return Cache(ref m_qslot, () => new QueueSlot(this)); }
        }
    }
}
