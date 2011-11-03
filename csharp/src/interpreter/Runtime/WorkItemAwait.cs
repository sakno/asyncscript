using System;

namespace DynamicScript.Runtime
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;

    /// <summary>
    /// Represents a delegate that can be used to synchronize with queued task.
    /// </summary>
    /// <param name="waitCriteria">Wait criteria.</param>
    /// <param name="result"></param>
    /// <returns></returns>
    [ComVisible(false)]
    public delegate bool WorkItemAwait<TWaitCriteria, TResult>(TWaitCriteria waitCriteria, out TResult result);
}
