using System;

namespace DynamicScript.Runtime.Environment.Threading
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;

    [ComVisible(false)]
    interface IScriptLazyFunction: IScriptFunction
    {
        IScriptWorkItemQueue Queue { get; set; }
    }
}
