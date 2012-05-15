using System;

namespace DynamicScript.Compiler.Ast
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;

    /// <summary>
    /// Represents variable declaration style.
    /// </summary>
    [ComVisible(false)]
    [Serializable]
    [Flags]
    internal enum SlotDeclarationStyle:byte
    {
        /// <summary>
        /// Variable declaration is invalid.
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// Declaration contains variable type only.
        /// </summary>
        ContractBindingOnly = 0x01,

        /// <summary>
        /// Declaration contains variable initialization expression only.
        /// </summary>
        InitExpressionOnly = 0x02,

        /// <summary>
        /// Declaration contains variable initialization expression and type both.
        /// </summary>
        TypeAndInitExpression = ContractBindingOnly | InitExpressionOnly
    }
}
