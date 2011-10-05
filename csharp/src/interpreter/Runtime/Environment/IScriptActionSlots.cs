using System;

namespace DynamicScript.Runtime.Environment
{
    [ScriptObject.SlotStore]
    interface IScriptActionSlots
    {
        IRuntimeSlot Owner { get; }
        IRuntimeSlot Ret { get; }
    }
}
