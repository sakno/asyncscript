using System;

namespace DynamicScript.Runtime.Environment.ExpressionTrees
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;

    [ComVisible(false)]
    interface IActionContractExpressionFactorySlots: ICodeElementFactorySlots
    {
        IRuntimeSlot Retval { get; }

        IRuntimeSlot Params { get; }
    }
}
