using System;

namespace DynamicScript.Runtime.Environment.ExpressionTrees
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;

    [ComVisible(false)]
    [ScriptObject.SlotStore]
    interface IActionExpressionFactorySlots: ICodeElementFactorySlots
    {
        IRuntimeSlot Sig { get; }

        IRuntimeSlot Body { get; }
    }
}
