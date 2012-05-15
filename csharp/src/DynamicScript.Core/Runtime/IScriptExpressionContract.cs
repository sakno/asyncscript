using System;
using System.Collections.Generic;

namespace DynamicScript.Runtime
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;
    using ScriptCodeExpression = Compiler.Ast.ScriptCodeExpression;

    /// <summary>
    /// Represents an expression tree type.
    /// </summary>
    /// <typeparam name="TExpression">Type of the expression tree.</typeparam>
    [ComVisible(false)]
    public interface IScriptExpressionContract<out TExpression> : IScriptCodeElementFactory<TExpression, IScriptExpression<TExpression>>
        where TExpression : ScriptCodeExpression
    {
    }
}
