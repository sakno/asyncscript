using System;

namespace DynamicScript.Runtime.Environment.ExpressionTrees
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;

    [ComVisible(false)]
    [ScriptObject.SlotStore]
    interface IUnaryExpressionFactorySlots: ICodeElementFactorySlots
    {
        IRuntimeSlot Invoker { get; }

        IRuntimeSlot Operand { get; }

        IRuntimeSlot Operator { get; }
    }
}
