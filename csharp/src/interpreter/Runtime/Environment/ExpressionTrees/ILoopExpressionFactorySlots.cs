using System;

namespace DynamicScript.Runtime.Environment.ExpressionTrees
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;

    [ScriptObject.SlotStore]
    [ComVisible(false)]
    interface ILoopExpressionFactorySlots: IComplexExpressionFactorySlots
    {
        IRuntimeSlot Grouping { get; }
    }
}
