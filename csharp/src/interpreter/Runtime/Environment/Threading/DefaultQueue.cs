using System;
using System.Threading;
using System.Linq.Expressions;

namespace DynamicScript.Runtime.Environment.Threading
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;

    /// <summary>
    /// Represents default event queue.
    /// This class cannot be inherited.
    /// </summary>
    [ComVisible(false)]
    public sealed class DefaultQueue : IScriptWorkItemQueue
    {
        #region Nested Types

        /// <summary>
        /// Represents running state of the script work item.
        /// This class cannot be inherited.
        /// </summary>
        [ComVisible(false)]
        private sealed class QueueItem: EventWaitHandle
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

            public bool WaitOne(TimeSpan timeout, out IScriptObject result)
            {
                switch (m_result != null || WaitOne(timeout))
                {
                    case true:
                        if (m_error != null) throw m_error;
                        result = m_result ?? ScriptObject.Void; 
                        return true;
                    default: result = null; return false;
                }
            }

            public bool WaitOne(WaitHandle handle, out IScriptObject result)
            {
                switch (m_result != null || WaitAny(new[] { this, handle }) == 0)
                {
                    case true:
                        if (m_error != null) throw m_error;
                        result = m_result ?? ScriptObject.Void;
                        return true;
                    default: result = null; return false;
                }
            }

            public static implicit operator WorkItemAwait<TimeSpan, IScriptObject>(QueueItem handle)
            {
                return handle != null ? new WorkItemAwait<TimeSpan, IScriptObject>(handle.WaitOne) : null;
            }

            protected override void Dispose(bool explicitDisposing)
            {
                m_error = null;
                m_result = null;
                base.Dispose(explicitDisposing);
            }
        }
        #endregion

        private DefaultQueue()
        {
        }

        /// <summary>
        /// Represents singleton instance of the default queue.
        /// </summary>
        public static readonly DefaultQueue Instance = new DefaultQueue();

        internal static MemberExpression InstanceField
        {
            get { return LinqHelpers.BodyOf<Func<DefaultQueue>, MemberExpression>(() => Instance); }
        }

        WorkItemAwait<TimeSpan, IScriptObject> IScriptWorkItemQueue.Enqueue(IScriptObject target, ScriptWorkItem workItem, InterpreterState state)
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
        public static WorkItemAwait<TimeSpan, IScriptObject> Enqueue(IScriptObject target, ScriptWorkItem workItem, InterpreterState state)
        {
            return QueueItem.Enqueue(target, workItem, state);
        }
    }
}
