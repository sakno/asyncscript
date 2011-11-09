using System;

namespace DynamicScript.Runtime.Environment.Threading
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;

    [ComVisible(false)]
    [ScriptObject.SlotStore]
    interface IScriptLazyAction: IScriptAction
    {
        IScriptWorkItemQueue Queue { get; set; }
    }
}
