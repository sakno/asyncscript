using System;

namespace DynamicScript.Compiler.Ast
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;

    /// <summary>
    /// Represents DynamicScript code interpretation context.
    /// </summary>
    [Serializable]
    
    [ComVisible(false)]
    public enum InterpretationContext : byte
    {
        /// <summary>
        /// Represents checked context.
        /// </summary>
        Checked = 0,

        /// <summary>
        /// Represents default(checked) context.
        /// </summary>
        Default = Checked,

        /// <summary>
        /// Represents unchecked context.
        /// </summary>
        Unchecked = 1,
    }
}
