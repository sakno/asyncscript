using System;
using System.Threading;
using System.Collections.Generic;

namespace DynamicScript.Runtime.Debugging
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;

    /// <summary>
    /// Represents debugger session.
    /// </summary>
    [ComVisible(false)]
    
    public interface IScriptDebuggerSession
    {
        /// <summary>
        /// Gets main thread.
        /// </summary>
        Thread MainThread { get; }

        /// <summary>
        /// Occurs when break point located in the source code is reached.
        /// </summary>
        event EventHandler<BreakPointReachedEventArgs> BreakPointReached;

        /// <summary>
        /// Begins time measurement for the current thread.
        /// </summary>
        void BeginTimeMeasurement();

        /// <summary>
        /// Ends time measurement for the current thread.
        /// </summary>
        /// <returns>The amount of time elapsed since <see cref="BeginTimeMeasurement"/> method call.</returns>
        TimeSpan EndTimeMeasurement();

        /// <summary>
        /// Gets loaded script modules.
        /// </summary>
        IEnumerable<Uri> Modules
        {
            get;
        }

        /// <summary>
        /// Occurs when script module is loaded.
        /// </summary>
        event EventHandler<ScriptModuleLoadedEventArgs> ModuleLoaded;
    }
}
