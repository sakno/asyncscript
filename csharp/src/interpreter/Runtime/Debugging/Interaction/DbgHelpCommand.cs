using System;

namespace DynamicScript.Runtime.Debugging.Interaction
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;
    using QScriptIO = Hosting.DynamicScriptIO;

    [ComVisible(false)]
    sealed class DbgHelpCommand: IDebuggerCommand
    {
        public static string Help
        {
            get { return DebuggerStrings.HelpCommand; }
        }

        public bool Execute(IScriptDebuggerSession session, BreakPointReachedEventArgs bp)
        {
            QScriptIO.Output.WriteLine(Help);
            QScriptIO.Output.WriteLine(DbgContinueCommand.Help);
            QScriptIO.Output.WriteLine(DbgLeaveCommand.Help);
            QScriptIO.Output.WriteLine(DbgExecCommand.Help);
            return true;
        }
    }
}
