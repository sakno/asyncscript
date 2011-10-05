using System;

namespace DynamicScript.Runtime.Environment
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;

    [ComVisible(false)]
    [ScriptObject.SlotStore]
    interface IArraySlots: IScriptArray, IIterableSlots
    {
        IRuntimeSlot Rank { get; }
        IRuntimeSlot GetItem { get; }
        IRuntimeSlot SetItem { get; }
        IRuntimeSlot Length { get; }
        IRuntimeSlot UpperBound { get; }
    }
}
