using System;
using System.Collections.Generic;

namespace DynamicScript.Compiler.Ast.Translation
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;

    /// <summary>
    /// Represents a lexical scope produced by the quoted expression.
    /// </summary>
    [ComVisible(false)]
    public interface IQuoteLexicalScope : IComplexExpressionScope<ScriptCodeQuoteExpression>
    {
    }
}
