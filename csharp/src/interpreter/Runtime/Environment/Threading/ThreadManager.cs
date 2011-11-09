using System;
using System.Threading;
using System.Threading.Tasks;
using System.Linq.Expressions;
using System.ComponentModel;

namespace DynamicScript.Runtime.Environment.Threading
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;
    using DynamicMetaObject = System.Dynamic.DynamicMetaObject;

    /// <summary>
    /// Represents DynamicScript synchronization manager.
    /// </summary>
    [ComVisible(false)]
    public static class ThreadManager
    {
        private static IScriptWorkItemQueue m_queue;

        /// <summary>
        /// Gets or sets the default queue.
        /// </summary>
        public static IScriptWorkItemQueue Queue
        {
            get
            {
                if (m_queue == null) m_queue = new DefaultQueue();
                return m_queue;
            }
            set { m_queue = value; }
        }

        /// <summary>
        /// Creates a new work item queue from its script representation.
        /// </summary>
        /// <param name="queue"></param>
        /// <returns></returns>
        public static IScriptWorkItemQueue CreateQueue(IScriptObject queue)
        {
            if (ScriptObject.IsVoid(queue))
                return Queue;
            else if (queue is IScriptWorkItemQueue)
                return (IScriptWorkItemQueue)queue;
            else return new ScriptWorkItemQueue(queue);
        }
    }
}
