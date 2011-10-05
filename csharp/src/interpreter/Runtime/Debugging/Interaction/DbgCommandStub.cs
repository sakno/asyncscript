using System;

namespace DynamicScript.Runtime.Debugging.Interaction
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;

    [ComVisible(false)]
    sealed class DbgCommandStub: IDebuggerCommand
    {
        private DbgCommandStub()
        {
        }

        public static readonly DbgCommandStub Instance = new DbgCommandStub();

        public bool Execute(IScriptDebuggerSession session, BreakPointReachedEventArgs bp)
        {
            return true;
        }
    }
}
