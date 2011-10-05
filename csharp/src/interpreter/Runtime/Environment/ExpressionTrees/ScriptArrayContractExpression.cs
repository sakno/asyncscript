using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace DynamicScript.Runtime.Environment.ExpressionTrees
{
    using Compiler.Ast;
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;
    using SystemConverter = System.Convert;

    /// <summary>
    /// Represents runtime representation of the array contract
    /// </summary>
    [ComVisible(false)]
    sealed class ScriptArrayContractExpression : ScriptExpression<ScriptCodeArrayContractExpression, ScriptArrayContract>
    {
        private ScriptArrayContractExpression(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        public ScriptArrayContractExpression(ScriptCodeArrayContractExpression expression)
            : base(expression, ScriptArrayContractExpressionFactory.Instance)
        {
        }

        public override ScriptArrayContract Compile(InterpreterState state)
        {
            var elementContract = ScriptExpressionFactory.Compile(Expression.ElementContract, state) as IScriptContract;
            return IsVoid(elementContract) ? null : new ScriptArrayContract(elementContract, Expression.Rank);
        }

        public static ScriptCodeArrayContractExpression CreateExpression(IScriptObject elementContract, ScriptInteger rank)
        {
            var ec = elementContract is IScriptExpression<ScriptCodeExpression> ? ((IScriptExpression<ScriptCodeExpression>)elementContract).CodeObject : ScriptConstantExpression.CreateExpression(elementContract);
            return ec != null ?
                new ScriptCodeArrayContractExpression { ElementContract = ec, Rank = rank != null && rank.IsInt32 ? SystemConverter.ToInt32(rank) : 0 } 
                : null;
        }

        protected override ScriptCodeArrayContractExpression CreateExpression(IList<IScriptObject> args, InterpreterState state)
        {
            return args.Count == 2 ? CreateExpression(args[0], args[1] as ScriptInteger) : null;
        }
    }
}
