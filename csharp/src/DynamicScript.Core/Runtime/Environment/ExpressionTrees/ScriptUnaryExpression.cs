using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace DynamicScript.Runtime.Environment.ExpressionTrees
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;
    using Compiler.Ast;

    [ComVisible(false)]
    sealed class ScriptUnaryExpression : ScriptExpression<ScriptCodeUnaryOperatorExpression, IScriptObject>
    {
        private ScriptUnaryExpression(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        public ScriptUnaryExpression(ScriptCodeUnaryOperatorExpression expression)
            : base(expression, ScriptUnaryExpressionFactory.Instance)
        {
        }

        public override IScriptObject Compile(InterpreterState state)
        {
            var operand = Convert(Expression.Operand) ?? Void;
            if (operand is IScriptExpression<ScriptCodeExpression>)
                operand = ((IScriptExpression<ScriptCodeExpression>)operand).Compile(state);
            return operand.UnaryOperation(Expression.Operator, state);
        }

        public static ScriptCodeUnaryOperatorExpression CreateExpression(IScriptObject operand, ScriptCodeUnaryOperatorType @operator)
        {
            return new ScriptCodeUnaryOperatorExpression
            {
                Operator = @operator,
                Operand = operand is IScriptExpression<ScriptCodeExpression> ? ((IScriptExpression<ScriptCodeExpression>)operand).CodeObject : ScriptConstantExpression.CreateExpression(operand)
            };
        }

        public static ScriptCodeUnaryOperatorExpression CreateExpression(IScriptObject operand, ScriptString @operator)
        {
            if (operand == null) operand = Void;
            var op = Parser.ParseOperator(@operator ?? ScriptString.Empty, false);
            return op is ScriptCodeUnaryOperatorType ? CreateExpression(operand, (ScriptCodeUnaryOperatorType)op) : null;
        }

        protected override ScriptCodeUnaryOperatorExpression CreateExpression(IList<IScriptObject> args, InterpreterState state)
        {
            return args.Count == 2 ? CreateExpression(args[0], args[1] as ScriptString) : null;
        }
    }
}
