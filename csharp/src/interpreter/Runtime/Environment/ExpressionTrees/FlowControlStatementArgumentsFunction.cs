using System;
using System.CodeDom;

namespace DynamicScript.Runtime.Environment.ExpressionTrees
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;
    using Compiler.Ast;

    [ComVisible(false)]
    sealed class FlowControlStatementArgumentsFunction<TFlowControl> : ScriptFunc<IScriptStatement<TFlowControl>>
        where TFlowControl : ScriptCodeStatement, ICodeFlowControlInstruction
    {
        public const string Name = "args";
        private const string FirstParamName = "cs";

        public FlowControlStatementArgumentsFunction(IScriptStatementContract<TFlowControl> inputArgContract)
            : base(FirstParamName, inputArgContract, new ScriptArrayContract(ScriptExpressionFactory.Instance))
        {
        }

        protected override IScriptObject Invoke(IScriptStatement<TFlowControl> flowControl, InterpreterState state)
        {
            switch (flowControl != null)
            {
                case true:
                    var result = new ScriptList(flowControl.CodeObject.ArgList.Count);
                    foreach (ScriptCodeExpression a in flowControl.CodeObject.ArgList)
                        result.Add(Convert(a) ?? Void);
                    return result;
                default: return ScriptArray.Empty(ScriptSuperContract.Instance);
            }
        }
    }
}
