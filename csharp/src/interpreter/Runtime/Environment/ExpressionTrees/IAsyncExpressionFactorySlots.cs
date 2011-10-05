using System;

namespace DynamicScript.Runtime.Environment.ExpressionTrees
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;

    [ComVisible(false)]
    [ScriptObject.SlotStore]
    interface IAsyncExpressionFactorySlots: ICodeElementFactorySlots
    {
        IRuntimeSlot Contract { get; }
    }
}
