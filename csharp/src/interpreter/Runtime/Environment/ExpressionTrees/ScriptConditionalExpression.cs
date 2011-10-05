using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace DynamicScript.Runtime.Environment.ExpressionTrees
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;
    using Compiler.Ast;

    [ComVisible(false)]
    [Serializable]
    sealed class ScriptConditionalExpression : ScriptExpression<ScriptCodeConditionalExpression, IScriptObject>
    {
        private ScriptConditionalExpression(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        public ScriptConditionalExpression(ScriptCodeConditionalExpression expression)
            : base(expression, ScriptConditionalExpressionFactory.Instance)
        {
        }

        public override IScriptObject Compile(InterpreterState state)
        {
            return DynamicScriptInterpreter.Run(Expression, state);
        }

        public static ScriptCodeConditionalExpression CreateExpression(IScriptObject condition, IEnumerable<IScriptObject> thenBranch, IEnumerable<IScriptObject> elseBranch = null)
        {
            var result = new ScriptCodeConditionalExpression
            {
                Condition = condition is IScriptExpression<ScriptCodeExpression> ?
                ((IScriptExpression<ScriptCodeExpression>)condition).CodeObject :
                ScriptConstantExpression.CreateExpression(condition)
            };
            ScriptStatementFactory.CreateStatements(thenBranch, result.ThenBranch);
            ScriptStatementFactory.CreateStatements(elseBranch, result.ElseBranch);
            return result.Completed ? result : null;
        }

        protected override ScriptCodeConditionalExpression CreateExpression(IList<IScriptObject> args, InterpreterState state)
        {
            //0 - condition, 1 - then-body, 2 - else-body
            switch (args.Count)
            {
                case 2:
                    return CreateExpression(args[0], args[1] as IEnumerable<IScriptObject>);
                case 3:
                    return CreateExpression(args[0], args[1] as IEnumerable<IScriptObject>, args[2] as IEnumerable<IScriptObject>);
                default:
                    return null;
            }
        }
    }
}
