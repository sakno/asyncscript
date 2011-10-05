using System;

namespace DynamicScript.Compiler.Ast.Translation
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;

    /// <summary>
    /// Represents lexical scope produced by the action implementation.
    /// </summary>
    [ComVisible(false)]
    public interface IActionLexicalScope : IComplexExpressionScope<ScriptCodeActionImplementationExpression>
    {
    }
}
