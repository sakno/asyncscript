using System;

namespace DynamicScript.Runtime.Environment
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;

    [ComVisible(false)]
    interface IScriptComplementation: IScriptContract
    {
        IScriptContract NegatedContract { get; }
    }
}
