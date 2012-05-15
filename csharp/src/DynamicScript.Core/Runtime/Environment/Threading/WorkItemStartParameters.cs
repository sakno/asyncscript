using System;

namespace DynamicScript.Runtime.Environment.Threading
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;

    [ComVisible(false)]
    sealed class WorkItemStartParameters
    {
        private readonly IScriptObject m_target;
        public readonly ScriptWorkItem WorkItem;
        private readonly InterpreterState m_state;

        public WorkItemStartParameters(IScriptObject t, ScriptWorkItem item, InterpreterState s)
        {
            m_target = t;
            WorkItem = item;
            m_state = s;
        }

        public IScriptObject UnwrapTargetAndStart()
        {
            return WorkItem.Invoke(m_target is IScriptProxyObject ? ((IScriptProxyObject)m_target).Unwrap(m_state) : m_target, m_state);
        }

        public IScriptObject Start()
        {
            return WorkItem.Invoke(m_target, m_state);
        }
    }
}
