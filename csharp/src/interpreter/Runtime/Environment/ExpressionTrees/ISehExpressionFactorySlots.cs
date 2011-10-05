using System;

namespace DynamicScript.Runtime.Environment.ExpressionTrees
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;

    [ComVisible(false)]
    [ScriptObject.SlotStore]
    interface ISehExpressionFactorySlots: ICodeElementFactorySlots
    {
        IRuntimeSlot GetFinallyBody { get; }

        IRuntimeSlot SetFinallyBody { get; }

        IRuntimeSlot GetTrapBody { get; }

        IRuntimeSlot SetTrapBody { get; }

        IRuntimeSlot GetTrapVar { get; }

        IRuntimeSlot Traps { get; } //trap count
    }
}
