using System;

namespace DynamicScript.Runtime.Environment
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;

    /// <summary>
    /// This interface is reserved for future use.
    /// </summary>
    [ComVisible(false)]
    interface IAggregator : IScriptObject
    {
        IScriptObject Aggregate(IScriptObject obj, InterpreterState state);
    }
}
