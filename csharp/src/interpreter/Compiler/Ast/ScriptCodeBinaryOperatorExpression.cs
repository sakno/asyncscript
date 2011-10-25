using System;
using System.CodeDom;
using System.Linq.Expressions;

namespace DynamicScript.Compiler.Ast
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;
    using StringBuilder = System.Text.StringBuilder;
    using OperatorToken = Operator;

    /// <summary>
    /// Represents binary operator expression.
    /// This class cannot be inherited.
    /// </summary>
    [ComVisible(false)]
    [Serializable]
    public sealed class ScriptCodeBinaryOperatorExpression: ScriptCodeExpression, IEquatable<ScriptCodeBinaryOperatorExpression>
    {
        /// <summary>
        /// Initializes a new binary operator expression.
        /// </summary>
        public ScriptCodeBinaryOperatorExpression()
        {
        }

        /// <summary>
        /// Initializes a new binary operator expression.
        /// </summary>
        /// <param name="leftOperator"></param>
        /// <param name="operator"></param>
        /// <param name="rightOperator"></param>
        public ScriptCodeBinaryOperatorExpression(ScriptCodeExpression leftOperator, ScriptCodeBinaryOperatorType @operator, ScriptCodeExpression rightOperator)
        {
            Left = leftOperator;
            Operator = @operator;
            Right = rightOperator;
        }

        /// <summary>
        /// Gets or sets left operand of the binary expression.
        /// </summary>
        public ScriptCodeExpression Left
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets binary operator.
        /// </summary>
        public ScriptCodeBinaryOperatorType Operator
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets right operand of the binary expression.
        /// </summary>
        public ScriptCodeExpression Right
        {
            get;
            set;
        }

        /// <summary>
        /// Returns a string that represents the current expression.
        /// </summary>
        /// <returns>A string that represents the current expression.</returns>
        public override string ToString()
        {
            return string.Concat(Punctuation.LeftBrace, 
                Left, 
                Punctuation.WhiteSpace, ToString(Operator), Punctuation.WhiteSpace, 
                Right, Punctuation.RightBracket);
        }

        internal static string ToString(ScriptCodeBinaryOperatorType @operator)
        {
            switch (@operator)
            {
                case ScriptCodeBinaryOperatorType.Initializer:
                    return OperatorToken.Initializer;
                case ScriptCodeBinaryOperatorType.Add:
                    return OperatorToken.Plus;
                case ScriptCodeBinaryOperatorType.Subtract:
                    return OperatorToken.Minus;
                case ScriptCodeBinaryOperatorType.MemberAccess:
                    return OperatorToken.MemberAccess;
                case ScriptCodeBinaryOperatorType.AdditiveAssign:
                    return OperatorToken.PlusAssignment;
                case ScriptCodeBinaryOperatorType.Expansion:
                    return OperatorToken.UnionAssignment;
                case ScriptCodeBinaryOperatorType.Reduction:
                    return OperatorToken.IntersectionAssignment;
                case ScriptCodeBinaryOperatorType.Assign:
                    return OperatorToken.Assignment;
                case ScriptCodeBinaryOperatorType.ValueEquality:
                    return OperatorToken.ValueEquality;
                case ScriptCodeBinaryOperatorType.ValueInequality:
                    return OperatorToken.ValueInequality;
                case ScriptCodeBinaryOperatorType.ReferenceEquality:
                    return OperatorToken.ReferenceEquality;
                case ScriptCodeBinaryOperatorType.ReferenceInequality:
                    return OperatorToken.ReferenceInequality;
                case ScriptCodeBinaryOperatorType.Multiply:
                    return OperatorToken.Asterisk;
                case ScriptCodeBinaryOperatorType.GreaterThan:
                    return OperatorToken.GreaterThan;
                case ScriptCodeBinaryOperatorType.GreaterThanOrEqual:
                    return OperatorToken.GreaterThanOrEqual;
                case ScriptCodeBinaryOperatorType.LessThan:
                    return OperatorToken.LessThan;
                case ScriptCodeBinaryOperatorType.LessThanOrEqual:
                    return OperatorToken.LessThanOrEqual;
                case ScriptCodeBinaryOperatorType.Exclusion:
                    return OperatorToken.Exclusion;
                case ScriptCodeBinaryOperatorType.MetadataDiscovery:
                    return OperatorToken.MetadataDiscovery;
                case ScriptCodeBinaryOperatorType.Coalesce:
                    return OperatorToken.Coalesce;
                case ScriptCodeBinaryOperatorType.InstanceOf:
                    return Keyword.Is;
                case ScriptCodeBinaryOperatorType.PartOf:
                    return Keyword.In;
                case ScriptCodeBinaryOperatorType.TypeCast:
                    return Keyword.To;
                case ScriptCodeBinaryOperatorType.Union:
                    return OperatorToken.Union;
                case ScriptCodeBinaryOperatorType.OrElse:
                    return OperatorToken.OrElse;
                case ScriptCodeBinaryOperatorType.AndAlso:
                    return OperatorToken.AndAlso;
                case ScriptCodeBinaryOperatorType.Divide:
                    return OperatorToken.Slash;
                case ScriptCodeBinaryOperatorType.DivideAssign:
                    return OperatorToken.SlashAssignment;
                case ScriptCodeBinaryOperatorType.MultiplicativeAssign:
                    return OperatorToken.AsteriskAssignment;
                case ScriptCodeBinaryOperatorType.SubtractiveAssign:
                    return OperatorToken.MinusAssignment;
                default:
                    return string.Empty;
            }
        }

        internal override bool Completed
        {
            get { return Left != null && Right != null; }
        }

        private static ScriptCodeIntegerExpression Add(ScriptCodeIntegerExpression left, ScriptCodeIntegerExpression right)
        {
            return new ScriptCodeIntegerExpression(unchecked(left.Value + right.Value));
        }

        private static ScriptCodeRealExpression Add(ScriptCodeIntegerExpression left, ScriptCodeRealExpression right)
        {
            return new ScriptCodeRealExpression(unchecked(left.Value + right.Value));
        }

        private static ScriptCodeRealExpression Add(ScriptCodeRealExpression left, ScriptCodeRealExpression right)
        {
            return new ScriptCodeRealExpression(unchecked(left.Value + right.Value));
        }

        /// <summary>
        /// Reduces binary expression.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public override ScriptCodeExpression Reduce(InterpretationContext context)
        {
            switch (Operator)
            {
                case ScriptCodeBinaryOperatorType.Add:
                    if (Left is ScriptCodeIntegerExpression && Right is ScriptCodeIntegerExpression)    //int+int
                        return Add((ScriptCodeIntegerExpression)Left, (ScriptCodeIntegerExpression)Right);
                    else if (Left is ScriptCodeIntegerExpression && Right is ScriptCodeRealExpression)  //int+real
                        return Add((ScriptCodeIntegerExpression)Left, (ScriptCodeRealExpression)Right);
                    else if (Left is ScriptCodeRealExpression && Right is ScriptCodeRealExpression)     //real+real
                        return Add((ScriptCodeRealExpression)Left, (ScriptCodeRealExpression)Right);
                    else if (Left is ScriptCodeRealExpression && Right is ScriptCodeIntegerExpression)   //real+int
                        return Add((ScriptCodeIntegerExpression)Right, (ScriptCodeRealExpression)Left);
                    break;
            }
            return base.Reduce(context);
        }

        /// <summary>
        /// Determines whether this expression is reducible.
        /// </summary>
        public override bool CanReduce
        {
            get
            {
                switch (Operator)
                {
                    case ScriptCodeBinaryOperatorType.Add: return Left is ScriptCodeIntegerExpression && Right is ScriptCodeIntegerExpression;
                    default: return false;
                }
                
            }
        }

        /// <summary>
        /// Converts expression to the statement.
        /// </summary>
        /// <param name="expr">The expression to be converted.</param>
        /// <returns>The statement that encapsulates the expression.</returns>
        public static explicit operator ScriptCodeExpressionStatement(ScriptCodeBinaryOperatorExpression expr)
        {
            return new ScriptCodeExpressionStatement<ScriptCodeBinaryOperatorExpression>(expr);
        }

        /// <summary>
        /// Determines whether this expression represents the same tree as other expression.
        /// </summary>
        /// <param name="other">Other expression tree to compare.</param>
        /// <returns><see langword="true"/> if this expression represents the same tree as other expression; otherwise, <see langword="false"/>.</returns>
        public override bool Equals(ScriptCodeExpression other)
        {
            return Equals(other as ScriptCodeBinaryOperatorExpression);
        }

        /// <summary>
        /// Determines whether this expression represents the same tree as other expression.
        /// </summary>
        /// <param name="other">Other expression tree to compare.</param>
        /// <returns><see langword="true"/> if this expression represents the same tree as other expression; otherwise, <see langword="false"/>.</returns>
        public bool Equals(ScriptCodeBinaryOperatorExpression other)
        {
            return other != null && Completed && other.Completed && Operator == other.Operator && Left.Equals(other.Left) && Right.Equals(other.Right);
        }

        /// <summary>
        /// Returns a LINQ expression that produces this object.
        /// </summary>
        /// <returns></returns>
        protected override Expression Restore()
        {
            var ctor = LinqHelpers.BodyOf<ScriptCodeExpression, ScriptCodeBinaryOperatorType, ScriptCodeExpression, ScriptCodeBinaryOperatorExpression, NewExpression>((left, oper, right) => new ScriptCodeBinaryOperatorExpression(left, oper, right));
            return ctor.Update(new[] { LinqHelpers.Restore(Left), LinqHelpers.Constant(Operator), LinqHelpers.Restore(Right) });
        }

        internal override void Verify()
        {
        }

        internal override ScriptCodeExpression Visit(ISyntaxTreeNode parent, Converter<ISyntaxTreeNode, ISyntaxTreeNode> visitor)
        {
            if (Left != null) Left = Left.Visit(this, visitor);
            if (Right != null) Right = Right.Visit(this, visitor);
            return visitor.Invoke(this) as ScriptCodeExpression ?? this;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected override ScriptCodeExpression Clone()
        {
            return new ScriptCodeBinaryOperatorExpression(Extensions.Clone(Left), Operator, Extensions.Clone(Right));
        }
    }
}
