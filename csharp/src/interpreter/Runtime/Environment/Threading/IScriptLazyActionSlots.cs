﻿using System;

namespace DynamicScript.Runtime.Environment.Threading
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;

    [ComVisible(false)]
    [ScriptObject.SlotStore]
    interface IScriptLazyActionSlots: IScriptLazyAction
    {
        new IRuntimeSlot Queue { get; }
    }
}
