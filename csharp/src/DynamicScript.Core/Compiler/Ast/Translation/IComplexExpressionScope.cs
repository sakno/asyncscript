using System;

namespace DynamicScript.Compiler.Ast.Translation
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;

    /// <summary>
    /// Represents a lexical scope produced by the complex expression, such as action implementation.
    /// </summary>
    /// <typeparam name="TExpression"></typeparam>
    [ComVisible(false)]
    public interface IComplexExpressionScope<out TExpression> : ILexicalScope
        where TExpression : ScriptCodeExpression
    {
        /// <summary>
        /// Gets expression associated with the scope.
        /// </summary>
        TExpression Expression { get; }
    }
}
