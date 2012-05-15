using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.CodeDom;

namespace DynamicScript.Runtime.Environment.ExpressionTrees
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;
    using Compiler.Ast;

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
            return ScriptFunctionInvoker.Compile(Expression);
        }

        public static ScriptCodeForkExpression CreateExpression(IScriptCodeElement<ScriptCodeExpression> body)
        {
            var result = new ScriptCodeForkExpression();
            result.Body.Expression = body != null ? body.CodeObject : ScriptCodeVoidExpression.Instance;
            return result;
        }

        protected override ScriptCodeForkExpression CreateExpression(IList<IScriptObject> args, InterpreterState state)
        {
            return args.Count == 1 ? CreateExpression(args as IScriptCodeElement<ScriptCodeExpression>) : null;
        }
    }
}
