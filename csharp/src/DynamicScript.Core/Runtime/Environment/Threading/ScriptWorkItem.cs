using System;

namespace DynamicScript.Runtime.Environment.Threading
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;

    /// <summary>
    /// Represents script task that can be enqueued by the event emitter.
    /// </summary>
    /// <param name="target">An object that represents the global scope.</param>
    /// <param name="state">Internal interpreter state.</param>
    /// <returns>Invocation result.</returns>
    [ComVisible(false)]
    public delegate IScriptObject ScriptWorkItem(IScriptObject target, InterpreterState state);
}
