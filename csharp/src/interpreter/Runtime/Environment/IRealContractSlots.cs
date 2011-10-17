using System;

namespace DynamicScript.Runtime.Environment
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;

    [ComVisible(false)]
    [ScriptCompositeObject.SlotStore]
    interface IRealContractSlots
    {
        IRuntimeSlot Abs { get; }

        IRuntimeSlot Sum { get; }

        IRuntimeSlot Rem { get; }

        IRuntimeSlot NaN { get; }

        IRuntimeSlot Min { get; }

        IRuntimeSlot Max { get; }

        IRuntimeSlot Epsilon { get; }

        IRuntimeSlot IsInterned { get; }

        IRuntimeSlot PINF { get; }  //positive infinity

        IRuntimeSlot NINF { get; }  //negative infinity
    }
}
