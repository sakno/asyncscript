using System;

namespace DynamicScript.Runtime.Environment
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;

    [ComVisible(false)]
    [ScriptObject.SlotStore]
    interface IStringContractSlots
    {
        IRuntimeSlot IsInterned { get; }
        IRuntimeSlot Empty { get; }
        IRuntimeSlot Concat { get; }
        IRuntimeSlot Language { get; }
        IRuntimeSlot Cmp { get; }
        IRuntimeSlot Equ { get; }
        IRuntimeSlot Length { get; }
        IRuntimeSlot Substr { get; }
        IRuntimeSlot IndexOf { get; }
        IRuntimeSlot Insert { get; }
    }
}
