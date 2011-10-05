using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace DynamicScript.Runtime.Environment.ExpressionTrees
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;
    using LinqExpressionTranslator = Compiler.Ast.Translation.LinqExpressions.LinqExpressionTranslator;
    using Compiler.Ast;

    /// <summary>
    /// Represents runtime representation of the binary expression.
    /// This class cannot be inherited.
    /// </summary>
    [ComVisible(false)]
    [Serializable]
    sealed class ScriptBinaryExpression : ScriptExpression<ScriptCodeBinaryOperatorExpression, IScriptObject>
    {
        private ScriptBinaryExpression(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        public ScriptBinaryExpression(ScriptCodeBinaryOperatorExpression expression)
            : base(expression, ScriptBinaryExpressionFactory.Instance)
        {
        }

        public override IScriptObject Compile(InterpreterState state)
        {
            var left = Convert(Expression.Left) ?? Void;
            if (left is IScriptExpression<ScriptCodeExpression>)
                left = ((IScriptExpression<ScriptCodeExpression>)left).Compile(state);
            var right = Convert(Expression.Right) ?? Void;
            if (right is IScriptExpression<ScriptCodeExpression>)
                right = ((IScriptExpression<ScriptCodeExpression>)right).Compile(state);
            return left.BinaryOperation(Expression.Operator, right, state);
        }

        public static ScriptCodeBinaryOperatorExpression CreateExpression(IScriptObject left, ScriptCodeBinaryOperatorType @operator, IScriptObject right)
        {
            return new ScriptCodeBinaryOperatorExpression
            {
                Operator = @operator,
                Left = left is IScriptExpression<ScriptCodeExpression> ? ((IScriptExpression<ScriptCodeExpression>)left).CodeObject : ScriptConstantExpression.CreateExpression(left),
                Right = right is IScriptExpression<ScriptCodeExpression> ? ((IScriptExpression<ScriptCodeExpression>)right).CodeObject : ScriptConstantExpression.CreateExpression(right)
            };
        }

        public static ScriptCodeBinaryOperatorExpression CreateExpression(IScriptObject left, ScriptString @operator, IScriptObject right)
        {
            if (left == null) left = Void;
            if (right == null) right = Void;
            var op = Parser.ParseOperator(@operator ?? ScriptString.Empty, true);
            return op is ScriptCodeBinaryOperatorType ? CreateExpression(left, (ScriptCodeBinaryOperatorType)op, right) : null;
        }

        protected override ScriptCodeBinaryOperatorExpression CreateExpression(IList<IScriptObject> args, InterpreterState state)
        {
            return args.Count == 3 ? CreateExpression(args[0], args[1] as ScriptString, args[2]) : null;
        }
    }
}
