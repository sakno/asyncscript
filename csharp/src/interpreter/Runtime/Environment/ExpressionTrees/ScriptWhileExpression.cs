using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace DynamicScript.Runtime.Environment.ExpressionTrees
{
    using Compiler.Ast;
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;

    [ComVisible(false)]
    [Serializable]
    sealed class ScriptWhileExpression: ScriptExpression<ScriptCodeWhileLoopExpression, IScriptObject>
    {
        private ScriptWhileExpression(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        public ScriptWhileExpression(ScriptCodeWhileLoopExpression expression)
            : base(expression, ScriptWhileExpressionFactory.Instance)
        {
        }

        public override IScriptObject Compile(InterpreterState state)
        {
            return DynamicScriptInterpreter.Run(Expression, state);
        }

        public static ScriptCodeWhileLoopExpression CreateExpression(bool postEval, ScriptCodeExpression condition, ScriptCodeWhileLoopExpression.YieldGrouping grouping, IScriptObject body)
        {
            var result = new ScriptCodeWhileLoopExpression
            {
                Condition = condition,
                Style = postEval ? ScriptCodeWhileLoopExpression.LoopStyle.EvaluateConditionAfterBody : ScriptCodeWhileLoopExpression.LoopStyle.EvaluateConditionBeforeBody,
                Grouping = grouping,
                SuppressResult = false,
                Body = body is IScriptCodeElement<ScriptCodeExpression> ? ((IScriptCodeElement<ScriptCodeExpression>)body).CodeObject :
                ScriptConstantExpression.CreateExpression(body)
            };
            return result.Completed ? result : null;
        }

        public static ScriptCodeWhileLoopExpression CreateExpression(ScriptBoolean postEval, ScriptCodeExpression condition, IScriptObject grouping, IScriptObject body)
        {
            if (grouping is ScriptString)
            {
                var @operator = Parser.ParseBinaryOperator((ScriptString)grouping);
                return CreateExpression(postEval, condition, @operator.HasValue ? new ScriptCodeWhileLoopExpression.OperatorGrouping(@operator.Value) : null, body);
            }
            else if (grouping is IBinaryOperatorInvoker)
                return CreateExpression(postEval, condition, new ScriptCodeWhileLoopExpression.OperatorGrouping(((IBinaryOperatorInvoker)grouping).Operator), body);
            else if (grouping is IScriptCodeElement<ScriptCodeExpression>)
                return CreateExpression(postEval, condition, new ScriptCodeWhileLoopExpression.CustomGrouping(((IScriptCodeElement<ScriptCodeExpression>)grouping).CodeObject), body);
            else return CreateExpression(postEval, condition, default(ScriptCodeWhileLoopExpression.YieldGrouping), body);
        }

        public static ScriptCodeWhileLoopExpression CreateExpression(ScriptBoolean postEval, IScriptCodeElement<ScriptCodeExpression> condition, IScriptObject grouping, IScriptObject body)
        {
            return condition != null ? CreateExpression(postEval, condition.CodeObject, grouping, body) : null;
        }

        protected override ScriptCodeWhileLoopExpression CreateExpression(IList<IScriptObject> args, InterpreterState state)
        {
            return args.Count == 4 ? CreateExpression(args[0] as ScriptBoolean, args[1] as IScriptCodeElement<ScriptCodeExpression>, args[2], args[3]) : null;
        }
    }
}
