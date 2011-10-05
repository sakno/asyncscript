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

        public static ScriptCodeTryElseFinallyExpression CreateExpression(IEnumerable<IScriptObject> dangerousCode)
        {
            var result = new ScriptCodeTryElseFinallyExpression();
            ScriptStatementFactory.CreateStatements(dangerousCode, result.DangerousCode);
            return result;
        }

        protected override ScriptCodeTryElseFinallyExpression CreateExpression(IList<IScriptObject> args, InterpreterState state)
        {
            switch (args.Count)
            {
                case 0: return CreateExpression(Enumerable.Empty<IScriptObject>());
                case 1: return CreateExpression(args[0] as IEnumerable<IScriptObject> ?? args);
                default: return CreateExpression(args);
            }
        }
    }
}
