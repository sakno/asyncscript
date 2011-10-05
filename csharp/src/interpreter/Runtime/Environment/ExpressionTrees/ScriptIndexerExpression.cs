using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace DynamicScript.Runtime.Environment.ExpressionTrees
{
    using Compiler.Ast;
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;

    [ComVisible(false)]
    [Serializable]
    sealed class ScriptIndexerExpression: ScriptExpression<ScriptCodeIndexerExpression, IScriptObject>
    {
        private ScriptIndexerExpression(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        public ScriptIndexerExpression(ScriptCodeIndexerExpression expression)
            : base(expression, ScriptIndexerExpressionFactory.Instance)
        {
        }

        public override IScriptObject Compile(InterpreterState state)
        {
            return DynamicScriptInterpreter.Run(Expression, state);
        }

        public static ScriptCodeIndexerExpression CreateExpression(IEnumerable<IScriptObject> args)
        {
            var result = new ScriptCodeIndexerExpression();
            ScriptExpressionFactory.CreateExpressions(args, result.ArgList);
            return result;
        }

        protected override ScriptCodeIndexerExpression CreateExpression(IList<IScriptObject> args, InterpreterState state)
        {
            switch (args.Count)
            {
                case 0: return CreateExpression(new IScriptObject[0]);
                case 1: return CreateExpression(args[0] as IEnumerable<IScriptObject> ?? args);
                default: return CreateExpression(args);
            }
        }
    }
}
