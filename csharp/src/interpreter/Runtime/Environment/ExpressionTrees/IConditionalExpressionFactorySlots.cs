using System;

namespace DynamicScript.Runtime.Environment.ExpressionTrees
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;

    [ComVisible(false)]
    [ScriptObject.SlotStore]
    interface IConditionalExpressionFactorySlots: ICodeElementFactorySlots
    {
        IRuntimeSlot IfTrue { get; }

        IRuntimeSlot IfFalse { get; }

        IRuntimeSlot Cond { get; }
    }
}
