using System;
using System.Threading;
using System.Collections.Concurrent;

namespace DynamicScript.Runtime.Environment.Threading
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;
    using SystemEnvironment = System.Environment;

    /// <summary>
    /// Represents default auto-scalable queue.
    /// This class cannot be inherited.
    /// </summary>
    [ComVisible(false)]
    public sealed class DefaultQueue : IScriptWorkItemQueue
    {
        #region Nested Types
        /// <summary>
        /// Represents enqueued work item.
        /// This class cannot be inherited.
        /// </summary>
        [ComVisible(false)]
        private sealed class WorkItemState : IWorkItemState<TimeSpan, IScriptObject>
        {
            private readonly IScriptObject Target;
            private readonly ScriptWorkItem WorkItem;
            private readonly InterpreterState State;
            private IScriptObject m_result;
            private Exception m_error;

            public WorkItemState(IScriptObject t, ScriptWorkItem wi, InterpreterState state)
            {
                Target = t;
                WorkItem = wi;
                State = state;
            }

            public IScriptObject Execute()
            {
                try
                {
                    m_result = WorkItem.Invoke(Target is IScriptProxyObject ? ((IScriptProxyObject)Target).Unwrap(State) : Target, State);
                }
                catch (Exception e)
                {
                    m_error = e;
                }
                return m_result;
            }

            public IScriptObject Result
            {
                get { if (m_error != null)throw m_error; return m_result; }
            }

            private bool IsCompleted()
            {
                return m_result != null || m_error != null;
            }

            bool IWorkItemState<TimeSpan, IScriptObject>.IsCompleted
            {
                get { return IsCompleted(); }
            }

            bool IWorkItemState<TimeSpan, IScriptObject>.WaitOne(TimeSpan timeout)
            {
                return SpinWait.SpinUntil(IsCompleted, timeout);
            }
        }
        #endregion   

        private readonly ConcurrentQueue<WorkItemState> m_queue = new ConcurrentQueue<WorkItemState>();
        private int m_active = 0;

        private void ProcessQueue(object state)
        {
            var workItem = default(WorkItemState);
            while (m_queue.TryDequeue(out workItem))
                workItem.Execute();
            Interlocked.Decrement(ref m_active);
        }

        /// <summary>
        /// Gets a value indicating whether the queue has active executed threads.
        /// </summary>
        public bool IsActive
        {
            get { return m_active > 0; }
        }

        private void ProcessQueue()
        {
            for (var i = m_active; i < SystemEnvironment.ProcessorCount; i++)
            {
                ThreadPool.QueueUserWorkItem(ProcessQueue, m_queue);
                Interlocked.Increment(ref m_active);
            }
        }

        /// <summary>
        /// Enqueues a new work item into the default queue.
        /// </summary>
        /// <param name="target"></param>
        /// <param name="workItem"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        public IWorkItemState<TimeSpan, IScriptObject> Enqueue(IScriptObject target, ScriptWorkItem workItem, InterpreterState state)
        {
            var workItemState = new WorkItemState(target, workItem, state);
            m_queue.Enqueue(workItemState);
            ProcessQueue();
            return workItemState;
        }
    }
}
