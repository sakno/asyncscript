using System;

namespace DynamicScript.Runtime.Environment.Threading
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;

    /// <summary>
    /// Represents object that controls state of asynchronous task.
    /// </summary>
    [ComVisible(false)]
    interface IScriptAsyncResult : IAsyncResult
    {
        /// <summary>
        /// Cancels background work.
        /// </summary>
        void Cancel();

        /// <summary>
        /// Gets or sets progress notifier.
        /// </summary>
        IScriptAction Notifier { get; set; }

        /// <summary>
        /// Gets result of asynchronous task execution.
        /// </summary>
        IScriptObject Result { get; }

        /// <summary>
        /// Gets duration of asynchronous task execution.
        /// </summary>
        TimeSpan Duration { get; }

        /// <summary>
        /// Gets error occured during task execution.
        /// </summary>
        ScriptFault Error { get; }
    }
}
