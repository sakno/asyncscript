using System;

namespace DynamicScript.Runtime.Environment.ExpressionTrees
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;

    [ComVisible(false)]
    [ScriptObject.SlotStore]
    interface IFaultStatementFactorySlots
    {
        IRuntimeSlot Exec { get; }

        IRuntimeSlot Error { get; }
    }
}
