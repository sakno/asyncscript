using System;

namespace DynamicScript.Compiler.Ast.Translation
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;

    /// <summary>
    /// Represents analyzer error mode.
    /// </summary>
    
    [ComVisible(false)]
    [Serializable]
    public enum ErrorMode
    {
        /// <summary>
        /// Terminates analysis of code document.
        /// </summary>
        Panic = 0,

        /// <summary>
        /// Analyzer raises error event and continue working with code document.
        /// </summary>
        Tolerant,
    }
}
