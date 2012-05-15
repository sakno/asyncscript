using System;
using System.Threading;

namespace DynamicScript.Runtime.Environment.Threading
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;

    /// <summary>
    /// Represents virtual queue that produces a thread for each work item.
    /// This class cannot be inherited.
    /// </summary>
    [ComVisible(false)]
    public sealed class ParallelQueue: IScriptWorkItemQueue
    {
        #region Nested Types

        /// <summary>
        /// Represents running state of the script work item.
        /// This class cannot be inherited.
        /// </summary>
        [ComVisible(false)]
        private sealed class QueueItem: EventWaitHandle, IWorkItemState<TimeSpan, IScriptObject>, IWorkItemState<WaitHandle, IScriptObject>
        {
            private IScriptObject m_result;
            private Exception m_error;

            private QueueItem()
                : base(false, EventResetMode.ManualReset)
            {
            }

            private void Enqueue(WorkItemStartParameters parameters)
            {
                try
                {
                    m_result = parameters.UnwrapTargetAndStart();
                }
                catch (Exception e)
                {
                    m_error = e;
                }
                finally
                {
                    Set();
                }
            }

            private void Enqueue(object parameters)
            {
                Enqueue((WorkItemStartParameters)parameters);
            }

            public static QueueItem Enqueue(IScriptObject target, ScriptWorkItem workItem, InterpreterState state)
            {
                var item = new QueueItem();
                ThreadPool.QueueUserWorkItem(item.Enqueue, new WorkItemStartParameters(target, workItem, state));
                return item;
            }

            protected override void Dispose(bool explicitDisposing)
            {
                m_error = null;
                m_result = null;
                base.Dispose(explicitDisposing);
            }

            public IScriptObject Result
            {
                get 
                {
                    if (m_error != null) throw m_error;
                    return m_result; 
                }
            }

            

            public bool IsCompleted
            {
                get { return m_result != null; }
            }

            public bool WaitOne(WaitHandle handle)
            {
                return WaitAny(new[] { this, handle }) == 0;
            }
        }
        #endregion

        private ParallelQueue()
        {
        }

        /// <summary>
        /// Represents singleton instance of the default queue.
        /// </summary>
        public static readonly ParallelQueue Instance = new ParallelQueue();

        IWorkItemState<TimeSpan, IScriptObject> IScriptWorkItemQueue.Enqueue(IScriptObject target, ScriptWorkItem workItem, InterpreterState state)
        {
            return Enqueue(target, workItem, state);
        }

        /// <summary>
        /// Creates a new computation thread for the specified user work item.
        /// </summary>
        /// <param name="target">An object passed to the user work item in the parallel thread.</param>
        /// <param name="workItem">A user work item to execute in the separated thread.</param>
        /// <param name="state">Internal interpreter state.</param>
        /// <returns></returns>
        public static IWorkItemState<TimeSpan, IScriptObject> Enqueue(IScriptObject target, ScriptWorkItem workItem, InterpreterState state)
        {
            return QueueItem.Enqueue(target, workItem, state);
        }
    }
}
