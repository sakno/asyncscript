using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace DynamicScript.Runtime.Environment.ExpressionTrees
{
    using Compiler.Ast;
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;
    using Enumerable = System.Linq.Enumerable;

    [ComVisible(false)]
    [Serializable]
    sealed class ScriptSehExpression: ScriptExpression<ScriptCodeTryElseFinallyExpression, IScriptObject>
    {
        private ScriptSehExpression(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        public ScriptSehExpression(ScriptCodeTryElseFinallyExpression expression)
            : base(expression, ScriptSehExpressionFactory.Instance)
        {
        }

        public override IScriptObject Compile(InterpreterState state)
        {
            return DynamicScriptInterpreter.Run(Expression, state);
        }

        public static ScriptCodeTryElseFinallyExpression CreateExpression(IScriptObject dangerousCode)
        {
            var result = new ScriptCodeTryElseFinallyExpression();
            result.DangerousCode.Expression = dangerousCode is IScriptCodeElement<ScriptCodeExpression> ?
                ((IScriptCodeElement<ScriptCodeExpression>)dangerousCode).CodeObject :
                ScriptConstantExpression.CreateExpression(dangerousCode);
            return result.Completed ? result : null;
        }

        protected override ScriptCodeTryElseFinallyExpression CreateExpression(IList<IScriptObject> args, InterpreterState state)
        {
            return args.Count == 1 ? CreateExpression(args[0]) : null;
        }
    }
}
