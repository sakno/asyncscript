using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace DynamicScript.Runtime.Environment.ExpressionTrees
{
    using Compiler.Ast;
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;

    [ComVisible(false)]
    sealed class ScriptForExpression: ScriptExpression<ScriptCodeForLoopExpression, IScriptObject>
    {
        private ScriptForExpression(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        public ScriptForExpression(ScriptCodeForLoopExpression expression)
            : base(expression, ScriptForExpressionFactory.Instance)
        {
        }

        public override IScriptObject Compile(InterpreterState state)
        {
            return DynamicScriptInterpreter.Run(Expression, state);
        }

        public static ScriptCodeForLoopExpression CreateExpression(ScriptCodeForLoopExpression.LoopVariable declaration, ScriptCodeExpression condition, ScriptCodeForLoopExpression.YieldGrouping grouping, IEnumerable<IScriptObject> body)
        {
            var result = new ScriptCodeForLoopExpression { Condition = condition, SuppressResult = false, Variable = declaration, Grouping = grouping };
            ScriptStatementFactory.CreateStatements(body, result.Body);
            return result.Completed ? result : null;
        }

        public static ScriptCodeForLoopExpression CreateExpression(ScriptCodeForLoopExpression.LoopVariable declaration, ScriptCodeExpression condition, IScriptObject grouping, IEnumerable<IScriptObject> body)
        {
            if (grouping is ScriptString)
            {
                var @operator = Parser.ParseBinaryOperator((ScriptString)grouping);
                return CreateExpression(declaration, condition, @operator.HasValue ? new ScriptCodeForLoopExpression.OperatorGrouping(@operator.Value) : null, body);
            }
            else if (grouping is IBinaryOperatorInvoker)
                return CreateExpression(declaration, condition, new ScriptCodeForLoopExpression.OperatorGrouping(((IBinaryOperatorInvoker)grouping).Operator), body);
            else if (grouping is IScriptExpression<ScriptCodeExpression>)
                return CreateExpression(declaration, condition, new ScriptCodeForLoopExpression.CustomGrouping(((IScriptExpression<ScriptCodeExpression>)grouping).CodeObject), body);
            else return CreateExpression(declaration, condition, default(ScriptCodeForLoopExpression.YieldGrouping), body);
        }

        public static ScriptCodeForLoopExpression CreateExpression(IScriptCodeElement<ScriptCodeLoopWithVariableExpression.LoopVariable> declaration, IScriptCodeElement<ScriptCodeExpression> condition, IScriptObject grouping, IEnumerable<IScriptObject> body)
        {
            return declaration != null && condition != null ? CreateExpression(declaration.CodeObject, ((IScriptExpression<ScriptCodeExpression>)condition).CodeObject, grouping, body) : null;
        }

        protected override ScriptCodeForLoopExpression CreateExpression(IList<IScriptObject> args, InterpreterState state)
        {
            return args.Count == 4 ? CreateExpression(args[0] as IScriptCodeElement<ScriptCodeLoopWithVariableExpression.LoopVariable>, args[1] as IScriptCodeElement<ScriptCodeExpression>, args[2], args[3] as IEnumerable<IScriptObject>) : null;
        }
    }
}
