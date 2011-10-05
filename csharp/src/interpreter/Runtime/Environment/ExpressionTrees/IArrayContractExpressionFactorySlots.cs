using System;

namespace DynamicScript.Runtime.Environment.ExpressionTrees
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;

    [ComVisible(false)]
    [ScriptObject.SlotStore]
    interface IArrayContractExpressionFactorySlots: ICodeElementFactorySlots
    {
        IRuntimeSlot Elem { get; }

        IRuntimeSlot Rank { get; }
    }
}
