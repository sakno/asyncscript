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

        public static ScriptCodeConditionalExpression CreateExpression(IScriptObject condition, IScriptObject thenBranch, IScriptObject elseBranch = null)
        {
            return new ScriptCodeConditionalExpression
             {
                 Condition = condition is IScriptCodeElement<ScriptCodeExpression> ?
                 ((IScriptCodeElement<ScriptCodeExpression>)condition).CodeObject :
                 ScriptConstantExpression.CreateExpression(condition),
                 ThenBranch = thenBranch is IScriptCodeElement<ScriptCodeExpression> ?
                 ((IScriptCodeElement<ScriptCodeExpression>)thenBranch).CodeObject :
                 ScriptConstantExpression.CreateExpression(thenBranch),
                 ElseBranch = elseBranch is IScriptCodeElement<ScriptCodeExpression> ?
                 ((IScriptCodeElement<ScriptCodeExpression>)elseBranch).CodeObject :
                 ScriptConstantExpression.CreateExpression(elseBranch)
             };
        }

        protected override ScriptCodeConditionalExpression CreateExpression(IList<IScriptObject> args, InterpreterState state)
        {
            //0 - condition, 1 - then-body, 2 - else-body
            switch (args.Count)
            {
                case 2:
                    return CreateExpression(args[0], args[1]);
                case 3:
                    return CreateExpression(args[0], args[1], args[2]);
                default:
                    return null;
            }
        }
    }
}
