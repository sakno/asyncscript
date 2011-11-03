using System;

namespace DynamicScript.Runtime
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;

    /// <summary>
    /// Represents work item queue.
    /// </summary>
    /// <remarks>This is a core interface of the event-driven programming in DynamicScript.</remarks>
    [ComVisible(false)]
    public interface IWorkItemQueue
    {
        /// <summary>
        /// Enqueues a new work item.
        /// </summary>
        /// <typeparam name="TWaitCriteria"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="arguments"></param>
        /// <param name="task"></param>
        /// <returns></returns>
        WorkItemAwait<TWaitCriteria, TResult> Enqueue<TWaitCriteria, TResult>(object[] arguments, Delegate task);
    }
}
