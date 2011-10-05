using System;

namespace DynamicScript.Compiler.Ast
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;

    [ComVisible(false)]
    interface IScriptExpressionStatement: ISyntaxTreeNode
    {
        ScriptCodeExpression Expression { get; }
    }
}
