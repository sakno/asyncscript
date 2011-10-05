using System;

namespace DynamicScript.Runtime.Environment
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;
    using ScriptCodeUnaryOperatorType = Compiler.Ast.ScriptCodeUnaryOperatorType;

    [ComVisible(false)]
    interface IUnaryOperatorInvoker : IScriptAction
    {
        ScriptCodeUnaryOperatorType Operator { get; }
    }
}
