using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace DynamicScript.Runtime.Environment.ExpressionTrees
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;
    using Compiler.Ast;

    [ComVisible(false)]
    [Serializable]
    sealed class ScriptExpressionStatement: ScriptStatement<ScriptCodeExpressionStatement>
    {
        private ScriptExpressionStatement(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        public ScriptExpressionStatement(ScriptCodeExpressionStatement statement)
            : base(statement, ScriptExpressionStatementFactory.Instance)
        {
        }

        public ScriptExpressionStatement(ScriptCodeExpression expression)
            : this(new ScriptCodeExpressionStatement(expression))
        {
        }

        public static ScriptCodeExpressionStatement CreateStatement(IScriptObject value)
        {
            var expression = value is IScriptExpression<ScriptCodeExpression> ?
                ((IScriptExpression<ScriptCodeExpression>)value).CodeObject :
                ScriptConstantExpression.CreateExpression(value);
            return expression != null ? new ScriptCodeExpressionStatement(expression) : null;
        }

        protected override ScriptCodeExpressionStatement CreateStatement(IList<IScriptObject> args, InterpreterState state)
        {
            return args.Count == 1 ? CreateStatement(args[0]) : null;
        }
    }
}
