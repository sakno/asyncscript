using System;

namespace DynamicScript.Compiler.Ast
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;

    /// <summary>
    /// Represents literal expression.
    /// </summary>
    [ComVisible(false)]
    interface ILiteralExpression: ISyntaxTreeNode
    {
        /// <summary>
        /// Gets literal value.
        /// </summary>
        IConvertible Value { get; }
    }
}
