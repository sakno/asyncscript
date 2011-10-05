using System;

namespace DynamicScript.Runtime
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;

    /// <summary>
    /// Represents script cache entry.
    /// </summary>
    [ComVisible(false)]
    
    public interface IScriptCookie
    {
        /// <summary>
        /// Gets compiled script.
        /// </summary>
        /// <remarks>This property never returns <see langword="null"/>.</remarks>
        ScriptInvoker CompiledScript { get; }

        /// <summary>
        /// Gets or sets result of the compiled script execution.
        /// </summary>
        IScriptObject ScriptResult { get; set; }
    }
}
