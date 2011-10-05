using System;
using System.Dynamic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using System.ComponentModel;

namespace DynamicScript.Runtime.Environment.Threading
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;
    using WaitHandle = System.Threading.WaitHandle;

    /// <summary>
    /// Represents asynchronous object that holds asynchronous task result.
    /// This class cannot be inherited.
    /// </summary>
    [ComVisible(false)]
    public sealed class ScriptAsyncObject: ScriptProxyObject, ISynchronizable
    {
        #region Nested Types
        [ComVisible(false)]
        private sealed class ScriptTaskItem
        {
            private readonly InterpreterState RuntimeState;
            private readonly Func<IScriptObject, InterpreterState, IScriptObject> Implementation;
            private readonly IScriptObject This;

            public ScriptTaskItem(Func<IScriptObject, InterpreterState, IScriptObject> task, IScriptObject @this, InterpreterState state)
            {
                if (task == null) throw new ArgumentNullException("task");
                if (state == null) throw new ArgumentNullException("state");
                RuntimeState = state;
                Implementation = task;
                This = IsVoid(@this) ? state.Global : @this;
            }

            private IScriptObject Invoke()
            {
                return Implementation.Invoke(This, RuntimeState);
            }

            public static implicit operator Func<IScriptObject>(ScriptTaskItem item)
            {
                return item != null ? new Func<IScriptObject>(item.Invoke) : null;
            }
        }
        #endregion

        private Task<IScriptObject> m_task;

        /// <summary>
        /// Creates a new asynchronous object.
        /// </summary>
        /// <param name="task">The delegate that produces the object and will be executed synchronously.</param>
        /// <param name="this">Scope object.</param>
        /// <param name="state">Internal interpreter state.</param>
        public ScriptAsyncObject(Func<IScriptObject, InterpreterState, IScriptObject> task, IScriptObject @this, InterpreterState state)
        {
            m_task = new Task<IScriptObject>(new ScriptTaskItem(task, @this, state));
            m_task.Start();
        }

        #region Runtime Helpers

        internal static NewExpression Bind(Expression<Func<IScriptObject, InterpreterState, IScriptObject>> task, Expression @this, ParameterExpression stateVar)
        {
            var ctor = LinqHelpers.BodyOf<Func<IScriptObject, InterpreterState, IScriptObject>, IScriptObject, InterpreterState, ScriptAsyncObject, NewExpression>((t, o, s) => new ScriptAsyncObject(t, o, s));
            return ctor.Update(new Expression[] { task, @this, stateVar });
        }
        #endregion

        /// <summary>
        /// Gets task associated with the current asynchronous object.
        /// </summary>
        internal Task<IScriptObject> Task
        {
            get { return m_task; }
        }

        /// <summary>
        /// This method is not supported.
        /// </summary>
        /// <param name="operation"></param>
        /// <returns></returns>
        public override bool Apply(Func<IScriptObject, IScriptObject> operation)
        {
            return false;
        }

        /// <summary>
        /// Finalizes task and returns result.
        /// </summary>
        /// <returns>The result obtained from the asynchronous task.</returns>
        protected override IScriptObject UnwrapCore()
        {
            m_task.Wait();
            switch (m_task.Exception != null)
            {
                case true: throw m_task.Exception.InnerException;
                default: return m_task.Result;
            }
        }

        /// <summary>
        /// Gets contract binding for the asynchronous object.
        /// </summary>
        /// <returns>The contract binding for the asynchronous object.</returns>
        [MethodImpl(MethodImplOptions.Synchronized)]
        public override IScriptContract GetContractBinding()
        {
            return Task.IsCompleted ? Task.Result.GetContractBinding() : ScriptSuperContract.Instance;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        bool ISynchronizable.Await(WaitHandle handle, TimeSpan timeout)
        {
            switch (handle != null)
            {
                case true:
                    IAsyncResult ar = m_task;
                    return Task.IsCompleted || WaitHandle.WaitAny(new[] { ar.AsyncWaitHandle, handle }, timeout) == 0;
                default:
                    return Task.Wait(timeout);
            }
        }
    }
}
