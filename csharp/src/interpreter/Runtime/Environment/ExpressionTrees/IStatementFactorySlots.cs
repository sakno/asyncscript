using System;

namespace DynamicScript.Runtime.Environment.ExpressionTrees
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;

    [ComVisible(false)]
    [ScriptObject.SlotStore]
    interface IStatementFactorySlots
    {
        IRuntimeSlot FaultDef { get; }

        IRuntimeSlot ContinueDef { get; }

        IRuntimeSlot LeaveDef { get; }

        IRuntimeSlot ReturnDef { get; }

        IRuntimeSlot Empty { get; }

        IRuntimeSlot Expression { get; }

        IRuntimeSlot Variable { get; }

        IRuntimeSlot Init { get; }

        IRuntimeSlot Clone { get; }

        IRuntimeSlot LoopVar { get; }

        IRuntimeSlot Visit { get; }
    }
}
