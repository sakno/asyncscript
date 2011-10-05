using System;
using System.Collections.Generic;

namespace DynamicScript.Runtime
{
    using ScriptCodeExpression = Compiler.Ast.ScriptCodeExpression;
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;

    /// <summary>
    /// Represents an expression tree at runtime.
    /// </summary>
    /// <typeparam name="TExpression">Type of the expression.</typeparam>
    [ComVisible(false)]
    public interface IScriptExpression<out TExpression> : IScriptCodeElement<TExpression>
        where TExpression : ScriptCodeExpression
    {
        /// <summary>
        /// Compiles code element.
        /// </summary>
        /// <param name="state">Internal interpreter state.</param>
        /// <returns>Compilation result.</returns>
        IScriptObject Compile(InterpreterState state);
    }
}
