using System;
using System.Collections.Concurrent;

namespace DynamicScript.Runtime.Environment.Threading
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;

    /// <summary>
    /// Represents work item queue that provides lazy computing without creating a new threads.
    /// This class cannot be inherited.
    /// </summary>
    [ComVisible(false)]
    public sealed class LazyQueue :  IScriptWorkItemQueue
    {
        #region Nested Types
        [ComVisible(false)]
        private sealed class QueueItem : IWorkItemState<TimeSpan, IScriptObject>
        {
            private IScriptObject m_result;
            private Exception m_error;
            private readonly IScriptObject m_target;
            public readonly ScriptWorkItem WorkItem;
            private readonly InterpreterState m_state;

            public QueueItem(IScriptObject target, ScriptWorkItem workItem, InterpreterState state)
            {
                m_target = target;
                WorkItem = workItem;
                m_state = state;
            }

            IScriptObject IWorkItemState<TimeSpan, IScriptObject>.Result
            {
                get 
                {
                    if (m_error != null) throw m_error;
                    return m_result;
                }
            }

            bool IWorkItemState<TimeSpan, IScriptObject>.WaitOne(TimeSpan criteria)
            {
                try
                {
                    m_result = WorkItem.Invoke(m_target is IScriptProxyObject ? ((IScriptProxyObject)m_target).Unwrap(m_state) : m_target, m_state);
                }
                catch (Exception e)
                {
                    m_error = e;
                }
                return true;
            }

            bool IWorkItemState<TimeSpan, IScriptObject>.IsCompleted
            {
                get { return m_result != null; }
            }
        }
        #endregion

        /// <summary>
        /// Creates a new lazy work item.
        /// </summary>
        /// <param name="target"></param>
        /// <param name="workItem"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        public static IWorkItemState<TimeSpan, IScriptObject> Enqueue(IScriptObject target, ScriptWorkItem workItem, InterpreterState state)
        {
            return new QueueItem(target, workItem, state);
        }

        IWorkItemState<TimeSpan, IScriptObject> IScriptWorkItemQueue.Enqueue(IScriptObject target, ScriptWorkItem workItem, InterpreterState state)
        {
            return Enqueue(target, workItem, state);
        }
    }
}
