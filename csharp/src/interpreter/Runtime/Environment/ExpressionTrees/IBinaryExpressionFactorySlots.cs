using System;

namespace DynamicScript.Runtime.Environment.ExpressionTrees
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;

    [ComVisible(false)]
    [ScriptObject.SlotStore]
    interface IBinaryExpressionFactorySlots: ICodeElementFactorySlots
    {
        IRuntimeSlot Invoker { get; }

        IRuntimeSlot Left { get; }

        IRuntimeSlot Right { get; }

        IRuntimeSlot Operator { get; }
    }
}
