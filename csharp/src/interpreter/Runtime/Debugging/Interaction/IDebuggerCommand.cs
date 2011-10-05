using System;

namespace DynamicScript.Runtime.Debugging.Interaction
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;

    /// <summary>
    /// Represents interactive debugger command.
    /// </summary>
    [ComVisible(false)]
    interface IDebuggerCommand
    {
        /// <summary>
        /// Executes command.
        /// </summary>
        /// <param name="session">Debugger session.</param>
        /// <param name="bp">Break point.</param>
        /// <returns><see langword="true"/> to continue interactive mode; otherwise, <see langword="false"/>.</returns>
        bool Execute(IScriptDebuggerSession session, BreakPointReachedEventArgs bp);
    }
}
