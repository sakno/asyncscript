using System;

namespace DynamicScript.Runtime.Environment.Threading
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;

    [ComVisible(false)]
    sealed class ScriptWorkItemQueue: IScriptWorkItemQueue
    {
        public static readonly IScriptObject Timeout = new ScriptCompositeObject(null);

        #region Nested Types
        
        [ComVisible(false)]
        private sealed class ScriptWorkItemAction : ScriptFunc
        {
            private readonly ScriptWorkItem m_workItem;

            public ScriptWorkItemAction(IScriptObject target, ScriptWorkItem workItem)
                : base(ScriptSuperContract.Instance, target)
            {
                m_workItem = workItem;
            }

            protected override IScriptObject Invoke(InterpreterState state)
            {
                return m_workItem.Invoke(This, state);
            }
        }

        [ComVisible(false)]
        private sealed class ScriptWorkItemState : IWorkItemState<TimeSpan, IScriptObject>
        {
            private readonly IScriptObject m_await;
            private readonly InterpreterState m_state;
            private IScriptObject m_result;

            public ScriptWorkItemState(IScriptObject await, InterpreterState state)
            {
                m_await = await ?? ScriptObject.Void;
                m_state = state;
            }

            private void WaitOne(TimeSpan timeout, IScriptObject fault)
            {
                var result = m_await.Invoke(new IScriptObject[] { new ScriptReal(timeout.TotalMilliseconds), fault }, m_state);
                m_result = ReferenceEquals(fault, result) ? null : result;
            }

            public IScriptObject Result
            {
                get 
                {
                    if (m_result == null) WaitOne(TimeSpan.Zero, Timeout);
                    return m_result;
                }
            }

            public bool WaitOne(TimeSpan tm)
            {
                if (m_result == null) WaitOne(tm, Timeout);
                return m_result != null;
            }

            public bool IsCompleted
            {
                get { return m_result != null; }
            }
        }
        #endregion

        private readonly IScriptObject m_implementation;

        public ScriptWorkItemQueue(IScriptObject implementation)
        {
            m_implementation = implementation;
        }

        public IWorkItemState<TimeSpan, IScriptObject> Enqueue(IScriptObject target, ScriptWorkItem workItem, InterpreterState state)
        {
            return new ScriptWorkItemState(m_implementation[ScriptNativeQueue.EnqueueActionName, state].GetValue(state).Invoke(new[] { new ScriptWorkItemAction(target, workItem) }, state), state);
        }
    }
}
