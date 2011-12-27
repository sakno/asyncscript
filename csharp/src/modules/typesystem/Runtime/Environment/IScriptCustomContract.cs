using System;

namespace DynamicScript.Runtime.Environment
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;
    using Compiler.Ast;

    [ComVisible(false)]
    public interface IScriptCustomContract: IScriptContract, IScriptMetaContract
    {
        IScriptFunction GetOverloadedOperator(ScriptCodeUnaryOperatorType @operator);

        IScriptFunction OverloadedInvoke { get; }

        IScriptFunction GetOverloadedOperator(ScriptCodeBinaryOperatorType @operator);
    }
}
