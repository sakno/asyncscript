using System;

namespace DynamicScript.Runtime.Debugging.Interaction
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;

    [ComVisible(false)]
    sealed class DbgContinueCommand: IDebuggerCommand
    {
        public bool Execute(IScriptDebuggerSession session, BreakPointReachedEventArgs bp)
        {
            return false;
        }

        public static string Help
        {
            get { return DebuggerStrings.ContinueCommand; }
        }
    }
}
