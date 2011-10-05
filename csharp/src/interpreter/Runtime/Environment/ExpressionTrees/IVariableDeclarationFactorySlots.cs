using System;

namespace DynamicScript.Runtime.Environment.ExpressionTrees
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;

    [ComVisible(false)]
    [ScriptObject.SlotStore]
    interface IVariableDeclarationFactorySlots: ICodeElementFactorySlots
    {
        IRuntimeSlot Name { get; }

        IRuntimeSlot IsConst { get; }

        IRuntimeSlot InitExpr { get; }

        IRuntimeSlot Contract { get; }
    }
}
