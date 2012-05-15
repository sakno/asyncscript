using System;

namespace DynamicScript.Runtime
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;

    /// <summary>
    /// Represents asynchronous state of the user work item.
    /// </summary>
    /// <typeparam name="TWaitCriteria"></typeparam>
    /// <typeparam name="TResult"></typeparam>
    [ComVisible(false)]
    public interface IWorkItemState<in TWaitCriteria, out TResult>
    {
        /// <summary>
        /// Gets result without blocking of the caller thread.
        /// </summary>
        TResult Result { get; }

        /// <summary>
        /// Blocks the caller thread until the result will not be obtained.
        /// </summary>
        /// <param name="criteria"></param>
        /// <returns></returns>
        bool WaitOne(TWaitCriteria criteria);

        /// <summary>
        /// Gets a value indicating whether the work item is completed.
        /// </summary>
        bool IsCompleted { get; }
    }
}
