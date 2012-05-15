using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace DynamicScript.Runtime.Environment.ExpressionTrees
{
    using Compiler.Ast;
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;

    [ComVisible(false)]
    [Serializable]
    sealed class ScriptInvocationExpression : ScriptExpression<ScriptCodeInvocationExpression, IScriptObject>
    {
        private ScriptInvocationExpression(SerializationInfo info, StreamingContext context)
            : base(info, context)
        { }

        public ScriptInvocationExpression(ScriptCodeInvocationExpression expression)
            : base(expression, ScriptInvocationExpressionFactory.Instance)
        { }

        public override IScriptObject Compile(InterpreterState state)
        {
            return DynamicScriptInterpreter.Run(Expression, state);
        }

        public static ScriptCodeInvocationExpression CreateExpression(IEnumerable<IScriptObject> args)
        {
            var result = new ScriptCodeInvocationExpression();
            ScriptExpressionFactory.CreateExpressions(args, result.ArgList);
            return result;
        }

        protected override ScriptCodeInvocationExpression CreateExpression(IList<IScriptObject> args, InterpreterState state)
        {
            switch (args.Count)
            {
                case 0: return CreateExpression(EmptyArray);
                case 1: return CreateExpression(args[0] as IEnumerable<IScriptObject> ?? args);
                default: return CreateExpression(args);
            }
        }
    }
}
