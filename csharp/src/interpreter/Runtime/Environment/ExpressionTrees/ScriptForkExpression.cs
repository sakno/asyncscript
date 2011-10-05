using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.CodeDom;

namespace DynamicScript.Runtime.Environment.ExpressionTrees
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;
    using Compiler.Ast;
    using ScriptAsyncObject = Threading.ScriptAsyncObject;

    [ComVisible(false)]
    [Serializable]
    sealed class ScriptForkExpression : ScriptExpression<ScriptCodeForkExpression, ScriptFunc>
    {
        private ScriptForkExpression(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {

        }

        public ScriptForkExpression(ScriptCodeForkExpression expression)
            : base(expression, ScriptForkExpressionFactory.Instance)
        {
        }

        public override ScriptFunc Compile(InterpreterState state)
        {
            return ScriptActionInvoker.Compile(Expression.Body);
        }

        public static ScriptCodeForkExpression CreateExpression(IEnumerable<IScriptObject> statements)
        {
            var result = new ScriptCodeForkExpression();
            ScriptStatementFactory.CreateStatements(statements, result.Body);
            return result;
        }

        protected override ScriptCodeForkExpression CreateExpression(IList<IScriptObject> args, InterpreterState state)
        {
            return args.Count == 1 ? CreateExpression(args as IEnumerable<IScriptObject>) : null;
        }
    }
}
