using System;

namespace DynamicScript.Compiler.Ast
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;

    /// <summary>
    /// Represents literal expression.
    /// </summary>
    [ComVisible(false)]
    interface ILiteralExpression<out TContract>: ISyntaxTreeNode, IStaticContractBinding<TContract>
        where TContract: ScriptCodeExpression
    {
        /// <summary>
        /// Gets literal value.
        /// </summary>
        IConvertible Value { get; }
    }
}
