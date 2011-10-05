using System;

namespace DynamicScript.Runtime
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;
    using WaitHandle = System.Threading.WaitHandle;

    /// <summary>
    /// Represents an interface for DynamicScript object that represents asynchronous result.
    /// </summary>
    [ComVisible(false)]
    public interface ISynchronizable: IScriptObject
    {
        /// <summary>
        /// Blocks the calling thread until the current object will not be synchronized.
        /// </summary>
        /// <param name="handle">Synchronization handle. Can be <see langword="null"/>.</param>
        /// <param name="timeout">Synchronization timeout.</param>
        bool Await(WaitHandle handle, TimeSpan timeout);
    }
}
