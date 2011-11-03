using System;

namespace DynamicScript.Runtime.Environment.Threading
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;

    /// <summary>
    /// Represents task queue.
    /// </summary>
    [ComVisible(false)]
    public interface IScriptWorkItemQueue
    {
        /// <summary>
        /// Enqueues a new work item.
        /// </summary>
        /// <param name="target">An object that represents scope object.</param>
        /// <param name="workItem">A work item to enqueue.</param>
        /// <param name="state">Internal interpreter state.</param>
        /// <returns>A delegate that can be used to synchronize the caller thread with work item state.</returns>
        WorkItemAwait<TimeSpan, IScriptObject> Enqueue(IScriptObject target, ScriptWorkItem workItem, InterpreterState state);
    }
}
