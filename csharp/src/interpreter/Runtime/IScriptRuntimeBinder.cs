using System;

namespace DynamicScript.Runtime
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;

    /// <summary>
    /// Represents an interface for runtime binders.
    /// </summary>
    [ComVisible(false)]
    interface IScriptRuntimeBinder
    {
        /// <summary>
        /// Gets interpreter internal state.
        /// </summary>
        InterpreterState State { get; }
    }
}
