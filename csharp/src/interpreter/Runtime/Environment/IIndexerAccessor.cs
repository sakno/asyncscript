using System;

namespace DynamicScript.Runtime.Environment
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;

    [ComVisible(false)]
    interface IIndexerAccessor: IScriptFunction
    {
        IScriptContract ItemContract { get; }
        IScriptContract[] Indicies { get; }
    }
}
