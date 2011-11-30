using System;

namespace DynamicScript.Runtime.Environment
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;
    using Compiler.Ast;

    [ComVisible(false)]
    public interface IScriptCustomContract: IScriptContract, IScriptMetaContract
    {
        IScriptAction GetOverloadedOperator(ScriptCodeUnaryOperatorType @operator);

        IScriptAction OverloadedInvoke { get; }

        IScriptAction GetOverloadedOperator(ScriptCodeBinaryOperatorType @operator);
    }
}
