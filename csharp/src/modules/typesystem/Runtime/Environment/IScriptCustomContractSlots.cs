using System;

namespace DynamicScript.Runtime.Environment
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;

    [ComVisible(false)]
    [ScriptObject.SlotStore]
    interface IScriptCustomContractSlots
    {
        IRuntimeSlot Constructor { get; }

        IRuntimeSlot Aggregates { get; }


    }
}
