using System;

namespace DynamicScript.Runtime
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;

    /// <summary>
    /// Represents script wrapper for native .NET object.
    /// </summary>
    [ComVisible(false)]
    [CLSCompliant(false)]
    public interface INativeObject: IScriptObject
    {
        /// <summary>
        /// Gets wrapped object.
        /// </summary>
        object Instance { get; }
    }
}
