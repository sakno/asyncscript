using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace DynamicScript.Runtime.Environment.ExpressionTrees
{
    using Compiler.Ast;
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;

    [Serializable]
    [ComVisible(false)]
    sealed class ScriptForEachExpression : ScriptExpression<ScriptCodeForEachLoopExpression, IScriptObject>
    {
        private ScriptForEachExpression(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        public ScriptForEachExpression(ScriptCodeForEachLoopExpression expression)
            : base(expression, ScriptForEachExpressionFactory.Instance)
        {
            expression.SuppressResult = false;
        }

        public override IScriptObject Compile(InterpreterState state)
        {
            return DynamicScriptInterpreter.Run(Expression, state);
        }

        public static ScriptCodeForEachLoopExpression CreateExpression(ScriptCodeForEachLoopExpression.LoopVariable declaration, ScriptCodeExpression iterator, ScriptCodeForEachLoopExpression.YieldGrouping grouping, IEnumerable<IScriptObject> body)
        {
            var result = new ScriptCodeForEachLoopExpression
            {
                Variable = declaration,
                Iterator = iterator,
                SuppressResult = false,
                Grouping = grouping,
            };
            ScriptStatementFactory.CreateStatements(body, result.Body);
            return result.Completed ? result : null;
        }

        public static ScriptCodeForEachLoopExpression CreateExpression(ScriptCodeForEachLoopExpression.LoopVariable declaration, ScriptCodeExpression iterator, IScriptObject grouping, IEnumerable<IScriptObject> body)
        {
            if (grouping is IBinaryOperatorInvoker)
                return CreateExpression(declaration, iterator, new ScriptCodeForEachLoopExpression.OperatorGrouping(((IBinaryOperatorInvoker)grouping).Operator), body);
            else if (grouping is ScriptString)
            {
                var @operator = Parser.ParseBinaryOperator((ScriptString)grouping);
                return CreateExpression(declaration, iterator, @operator.HasValue ? new ScriptCodeForEachLoopExpression.OperatorGrouping(@operator.Value) : null, body);
            }
            else if (grouping is IScriptExpression<ScriptCodeExpression>)
                return CreateExpression(declaration, iterator, new ScriptCodeForEachLoopExpression.CustomGrouping(((IScriptExpression<ScriptCodeExpression>)grouping).CodeObject), body);
            else return CreateExpression(declaration, iterator, default(ScriptCodeForEachLoopExpression.YieldGrouping), body);
        }

        public static ScriptCodeForEachLoopExpression CreateExpression(IScriptCodeElement<ScriptCodeLoopWithVariableExpression.LoopVariable> declaration, ScriptCodeExpression iterator, IScriptObject grouping, IEnumerable<IScriptObject> body)
        {
            return declaration != null && iterator!=null ? CreateExpression(declaration.CodeObject, iterator, grouping, body) : null;
        }

        public static ScriptCodeForEachLoopExpression CreateExpression(IScriptObject declaration, IScriptObject iterator, IScriptObject grouping, IScriptObject body)
        {
            return CreateExpression(declaration as IScriptCodeElement<ScriptCodeLoopWithVariableExpression.LoopVariable>,
                iterator is IScriptExpression<ScriptCodeExpression> ? ((IScriptExpression<ScriptCodeExpression>)iterator).CodeObject : ScriptConstantExpression.CreateExpression(iterator),
                grouping,
                body as IEnumerable<IScriptObject>);
        }

        protected override ScriptCodeForEachLoopExpression CreateExpression(IList<IScriptObject> args, InterpreterState state)
        {
            return args.Count == 4 ? CreateExpression(args[0], args[1], args[2], args[3]) : null;
        }
    }
}
