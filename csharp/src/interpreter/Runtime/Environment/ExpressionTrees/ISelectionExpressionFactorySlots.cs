using System;

namespace DynamicScript.Runtime.Environment.ExpressionTrees
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;

    [ComVisible(false)]
    [ScriptObject.SlotStore]
    interface ISelectionExpressionFactorySlots: ICodeElementFactorySlots
    {
        IRuntimeSlot GetDef { get; }

        IRuntimeSlot SetDef { get; }

        IRuntimeSlot GetCaseValues { get; }

        IRuntimeSlot SetCaseValues { get; }

        IRuntimeSlot GetCaseBody { get; }

        IRuntimeSlot SetCaseBody { get; }

        IRuntimeSlot Cases { get; } //count of cases
    }
}
