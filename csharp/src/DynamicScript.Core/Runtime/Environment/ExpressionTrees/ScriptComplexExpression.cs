using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace DynamicScript.Runtime.Environment.ExpressionTrees
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;
    using Compiler.Ast;

    [ComVisible(false)]
    [Serializable]
    sealed class ScriptComplexExpression: ScriptExpression<ScriptCodeComplexExpression, IScriptObject>
    {
        private ScriptComplexExpression(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        public ScriptComplexExpression(ScriptCodeComplexExpression expression)
            : base(expression, ScriptComplexExpressionFactory.Instance)
        {
        }

        public static ScriptCodeComplexExpression CreateExpression(IEnumerable<IScriptObject> statements)
        {
            var result = new ScriptCodeComplexExpression();
            ScriptStatementFactory.CreateStatements(statements, result.Body);
            return result;
        }



        public override IScriptObject Compile(InterpreterState state)
        {
            return DynamicScriptInterpreter.Run(Expression.Body, state);
        }

        protected override ScriptCodeComplexExpression CreateExpression(IList<IScriptObject> args, InterpreterState state)
        {
            switch (args.Count)
            {
                case 0: return null;
                case 1: return CreateExpression(args[0] as IEnumerable<IScriptObject> ?? args);
                default: return CreateExpression(args);
            }
        }
    }
}
