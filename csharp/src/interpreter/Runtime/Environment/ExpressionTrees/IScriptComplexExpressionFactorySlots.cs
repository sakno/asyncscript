using System;

namespace DynamicScript.Runtime.Environment.ExpressionTrees
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;

    [ComVisible(false)]
    interface IScriptComplexExpressionFactorySlots: ICodeElementFactorySlots
    {
        IRuntimeSlot Statements { get; }
    }
}
