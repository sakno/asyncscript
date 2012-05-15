using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace DynamicScript.Compiler.Ast.Translation.LinqExpressions
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;

    /// <summary>
    /// Represents compile-time optimizer for LINQ-ET translator.
    /// </summary>
    [ComVisible(false)]
    public interface ICodeOptimizer
    {
        /// <summary>
        /// Inlines function call.
        /// </summary>
        /// <param name="functionName">The name of the function.</param>
        /// <param name="arguments">Function call arguments.</param>
        /// <param name="state">Internal interpreter state.</param>
        /// <returns>An expression that represents function call. If it is <see langword="null"/> then function cannot be inlined.</returns>
        Expression InlineFunctionCall(string functionName, IList<Expression> arguments, ParameterExpression state);
    }
}
