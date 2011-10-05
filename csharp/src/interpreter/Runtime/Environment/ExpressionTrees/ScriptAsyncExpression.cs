using System;
using System.Runtime.Serialization;
using System.Collections.Generic;

namespace DynamicScript.Runtime.Environment.ExpressionTrees
{
    using Compiler.Ast;
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;
    using ScriptAsyncResultContract = Threading.ScriptAsyncResultContract;

    /// <summary>
    /// Represents runtime representation of the asynchronous data type definition.
    /// This class cannot be inherited.
    /// </summary>
    [ComVisible(false)]
    [Serializable]
    sealed class ScriptAsyncExpression : ScriptExpression<ScriptCodeAsyncExpression, IScriptContract>
    {
        private ScriptAsyncExpression(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        public ScriptAsyncExpression(ScriptCodeAsyncExpression asyncExpr)
            : base(asyncExpr, ScriptAsyncExpressionFactory.Instance)
        {
        }

        /// <summary>
        /// Compiles asynchronous contract definition.
        /// </summary>
        /// <param name="state"></param>
        /// <returns></returns>
        public override IScriptContract Compile(InterpreterState state)
        {
            var contract = (IScriptExpression<ScriptCodeExpression>)Convert(Expression.Contract);
            return new ScriptAsyncResultContract(contract.Compile(state) as IScriptContract);
        }

        public static ScriptCodeAsyncExpression CreateExpression(IScriptObject contractDef)
        {
            return new ScriptCodeAsyncExpression 
            {
                Contract = contractDef is IScriptExpression<ScriptCodeExpression> ? ((IScriptExpression<ScriptCodeExpression>)contractDef).CodeObject : ScriptConstantExpression.CreateExpression(contractDef) 
            };
        }

        protected override ScriptCodeAsyncExpression CreateExpression(IList<IScriptObject> args, InterpreterState state)
        {
            return args.Count == 1 ? CreateExpression(args[0]) : null;
        }
    }
}
