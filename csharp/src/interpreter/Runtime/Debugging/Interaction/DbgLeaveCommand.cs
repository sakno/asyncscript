using System;

namespace DynamicScript.Runtime.Debugging.Interaction
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;

    [ComVisible(false)]
    sealed class DbgLeaveCommand: IDebuggerCommand
    {
        public bool Execute(IScriptDebuggerSession session, BreakPointReachedEventArgs bp)
        {
            bp.Cancel = true;
            return false;
        }

        public static string Help
        {
            get { return DebuggerStrings.LeaveCommand; }
        }
    }
}
