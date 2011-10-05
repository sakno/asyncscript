using System;

namespace DynamicScript.Runtime.Environment
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;

    [ComVisible(false)]
    [ScriptObject.SlotStore]
    interface IIntegerContractSlots
    {
        IRuntimeSlot Size { get; }
        IRuntimeSlot Max { get; }
        IRuntimeSlot Min { get; }
        IRuntimeSlot Even { get; }
        IRuntimeSlot Odd { get; }
        IRuntimeSlot Abs { get; }
        IRuntimeSlot Sum { get; }
        IRuntimeSlot Rem { get; }
        IRuntimeSlot IsInterned { get; }
    }
}
