using System;

namespace DynamicScript.Runtime.Environment.ExpressionTrees
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;

    [ComVisible(false)]
    [ScriptObject.SlotStore]
    interface IAwaitExpressionFactorySlots: ICodeElementFactorySlots
    {
        IRuntimeSlot AsyncResult { get; }

        IRuntimeSlot Synchronizer { get; }
    }
}
