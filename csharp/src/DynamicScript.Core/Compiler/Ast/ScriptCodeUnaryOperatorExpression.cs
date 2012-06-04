using System;
using System.CodeDom;
using System.Linq.Expressions;

namespace DynamicScript.Compiler.Ast
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;
    using OperatorToken = Operator;

    /// <summary>
    /// Represents unary expression.
    /// This class cannot be inherited.
    /// </summary>
    [Serializable]
    [ComVisible(false)]
    public sealed class ScriptCodeUnaryOperatorExpression : ScriptCodeExpression, IEquatable<ScriptCodeUnaryOperatorExpression>
    {
        /// <summary>
        /// Initializes a new empty unary expression.
        /// </summary>
        public ScriptCodeUnaryOperatorExpression()
        {
        }

        /// <summary>
        /// Initializes a new unary expression.
        /// </summary>
        /// <param name="operator"></param>
        /// <param name="operand"></param>
        public ScriptCodeUnaryOperatorExpression(ScriptCodeUnaryOperatorType @operator, ScriptCodeExpression operand)
            :this()
        {
            if (operand == null) throw new ArgumentNullException("operand");
            Operand = operand;
            Operator = @operator;
        }

        /// <summary>
        /// Gets or sets operator type.
        /// </summary>
        public ScriptCodeUnaryOperatorType Operator
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets operand.
        /// </summary>
        public ScriptCodeExpression Operand
        {
            get;
            set;
        }

        /// <summary>
        /// Reduces unary expression.
        /// </summary>
        /// <param name="context">Interpretation context.</param>
        /// <returns></returns>
        public override ScriptCodeExpression Reduce(InterpretationContext context)
        {
            switch (Operator)
            {
                case ScriptCodeUnaryOperatorType.Intern:
                    if (Operand is ScriptCodePrimitiveExpression)
                        ((ScriptCodePrimitiveExpression)Operand).IsInterned = true;
                    return Operand;
                case ScriptCodeUnaryOperatorType.Plus:
                    if (Operand is ScriptCodeIntegerExpression || Operand is ScriptCodeRealExpression)
                        return Operand;
                    break;
                case ScriptCodeUnaryOperatorType.Minus:
                    if (Operand is ScriptCodeIntegerExpression)
                        return ((ScriptCodeIntegerExpression)Operand).Minus();
                    else if (Operand is ScriptCodeRealExpression)
                        return ((ScriptCodeRealExpression)Operand).Minus();
                    break;
                case ScriptCodeUnaryOperatorType.Negate:
                    if (Operand is ScriptCodeBooleanExpression)
                        return ((ScriptCodeBooleanExpression)Operand).Negate();
                    else if (Operand is ScriptCodeIntegerExpression)
                        return ((ScriptCodeIntegerExpression)Operand).Negate();
                    break;
                case ScriptCodeUnaryOperatorType.SquarePrefix:
                    if (Operand is ScriptCodeIntegerExpression)
                        return ((ScriptCodeIntegerExpression)Operand).Square(context);
                    else if (Operand is ScriptCodeRealExpression)
                        return ((ScriptCodeRealExpression)Operand).Square();
                    break;
                case ScriptCodeUnaryOperatorType.TypeOf:
                    if (Operand is IStaticContractBinding<ScriptCodeExpression>)
                        return ((IStaticContractBinding<ScriptCodeExpression>)Operand).Contract;
                    break;
                case ScriptCodeUnaryOperatorType.IncrementPostfix:
                case ScriptCodeUnaryOperatorType.SquarePostfix:
                case ScriptCodeUnaryOperatorType.DecrementPostfix:
                    if (Operand is ScriptCodeIntegerExpression || Operand is ScriptCodeRealExpression)
                        return Operand;
                    break;
                case ScriptCodeUnaryOperatorType.DecrementPrefix:
                    if (Operand is ScriptCodeIntegerExpression)
                        return ((ScriptCodeIntegerExpression)Operand).Decrement(context);
                    else if (Operand is ScriptCodeRealExpression)
                        return ((ScriptCodeRealExpression)Operand).Decrement();
                    break;
                case ScriptCodeUnaryOperatorType.IncrementPrefix:
                    if (Operand is ScriptCodeIntegerExpression)
                        return ((ScriptCodeIntegerExpression)Operand).Increment(context);
                    else if (Operand is ScriptCodeRealExpression)
                        return ((ScriptCodeRealExpression)Operand).Increment();
                    break;
            }
            return base.Reduce(context);
        }

        /// <summary>
        /// Gets a value indicating that this unary expression can be reduced.
        /// </summary>
        public override bool CanReduce
        {
            get
            {
                switch (Operator)
                {
                    case ScriptCodeUnaryOperatorType.Intern: return Operand is ScriptCodeIntegerExpression || Operand is ScriptCodeRealExpression || Operand is ScriptCodeStringExpression;
                    case ScriptCodeUnaryOperatorType.Minus: return Operand is ScriptCodeIntegerExpression || Operand is ScriptCodeRealExpression;
                    case ScriptCodeUnaryOperatorType.Negate: return Operand is ScriptCodeBooleanExpression || Operand is ScriptCodeIntegerExpression;
                    case ScriptCodeUnaryOperatorType.Plus: return Operand is ScriptCodeIntegerExpression || Operand is ScriptCodeRealExpression;
                    case ScriptCodeUnaryOperatorType.SquarePrefix:
                    case ScriptCodeUnaryOperatorType.SquarePostfix: return Operand is ScriptCodeIntegerExpression || Operand is ScriptCodeRealExpression;
                    case ScriptCodeUnaryOperatorType.TypeOf: return Operand is IStaticContractBinding<ScriptCodeExpression>;
                    case ScriptCodeUnaryOperatorType.DecrementPostfix:
                    case ScriptCodeUnaryOperatorType.DecrementPrefix:
                    case ScriptCodeUnaryOperatorType.IncrementPostfix:
                    case ScriptCodeUnaryOperatorType.IncrementPrefix:
                        return Operand is ScriptCodeIntegerExpression || Operand is ScriptCodeRealExpression;
                    default: return base.CanReduce;
                }
            }
        }

        internal static string ToString(ScriptCodeUnaryOperatorType @operator)
        {
            switch (@operator)
            {
                case ScriptCodeUnaryOperatorType.IncrementPrefix:
                    return OperatorToken.DoublePlus;
                case ScriptCodeUnaryOperatorType.DecrementPrefix:
                    return OperatorToken.DoubleMinus;
                case ScriptCodeUnaryOperatorType.Minus:
                    return OperatorToken.Minus;
                case ScriptCodeUnaryOperatorType.Plus:
                    return OperatorToken.Plus;
                case ScriptCodeUnaryOperatorType.SquarePrefix:
                    return OperatorToken.DoubleAsterisk;
                case ScriptCodeUnaryOperatorType.DecrementPostfix:
                    return OperatorToken.DoublePlus;
                case ScriptCodeUnaryOperatorType.IncrementPostfix:
                    return OperatorToken.DoublePlus;
                case ScriptCodeUnaryOperatorType.SquarePostfix:
                    return OperatorToken.DoubleAsterisk;
                case ScriptCodeUnaryOperatorType.TypeOf:
                    return OperatorToken.TypeOf;
                case ScriptCodeUnaryOperatorType.Intern:
                    return  OperatorToken.TypeOf;
                default:
                    return String.Empty;
            }
        }

        /// <summary>
        /// Returns a string that represents the current expression.
        /// </summary>
        /// <returns>A string that represents the current expression.</returns>
        public override string ToString()
        {
            switch (Operator)
            {
                case ScriptCodeUnaryOperatorType.IncrementPrefix:
                case ScriptCodeUnaryOperatorType.DecrementPrefix:
                case ScriptCodeUnaryOperatorType.Minus:
                case ScriptCodeUnaryOperatorType.Plus:
                case ScriptCodeUnaryOperatorType.SquarePrefix:
                    return string.Concat(ToString(Operator), Operand);
                case ScriptCodeUnaryOperatorType.DecrementPostfix:
                case ScriptCodeUnaryOperatorType.IncrementPostfix:
                case ScriptCodeUnaryOperatorType.SquarePostfix:
                case ScriptCodeUnaryOperatorType.TypeOf:
                case ScriptCodeUnaryOperatorType.Intern:
                    return string.Concat(Operand, ToString(Operator));
                default:
                    return String.Empty;
            }
        }

        internal override bool Completed
        {
            get { return Operand != null; }
        }

        /// <summary>
        /// Converts expression to the statement.
        /// </summary>
        /// <param name="expr">The expression to be converted.</param>
        /// <returns>The statement that encapsulates the expression.</returns>
        public static explicit operator ScriptCodeExpressionStatement(ScriptCodeUnaryOperatorExpression expr)
        {
            return new ScriptCodeExpressionStatement<ScriptCodeUnaryOperatorExpression>(expr);
        }

        /// <summary>
        /// Determines whether this expression represents the same tree as other expression.
        /// </summary>
        /// <param name="other">Other expression tree to compare.</param>
        /// <returns><see langword="true"/> if this expression represents the same tree as other expression; otherwise, <see langword="false"/>.</returns>
        public override bool Equals(ScriptCodeExpression other)
        {
            return Equals(other as ScriptCodeExpression);
        }

        /// <summary>
        /// Determines whether this expression represents the same tree as other expression.
        /// </summary>
        /// <param name="other">Other expression tree to compare.</param>
        /// <returns><see langword="true"/> if this expression represents the same tree as other expression; otherwise, <see langword="false"/>.</returns>
        public bool Equals(ScriptCodeUnaryOperatorExpression other)
        {
            return other != null && Completed && other.Completed && Operator == other.Operator && Operand.Equals(other.Operand);
        }

        /// <summary>
        /// Returns a LINQ expression that produces this object.
        /// </summary>
        /// <returns></returns>
        protected override Expression Restore()
        {
            var ctor = LinqHelpers.BodyOf<ScriptCodeUnaryOperatorType, ScriptCodeExpression, ScriptCodeUnaryOperatorExpression, NewExpression>((@operator, operand) => new ScriptCodeUnaryOperatorExpression(@operator, operand));
            return ctor.Update(new[] { LinqHelpers.Constant(Operator), LinqHelpers.Restore(Operand) });
        }

        internal override void Verify()
        {
            throw new NotImplementedException();
        }

        internal override ScriptCodeExpression Visit(ISyntaxTreeNode parent, Converter<ISyntaxTreeNode, ISyntaxTreeNode> visitor)
        {
            if (Operand != null) Operand = Operand.Visit(this, visitor) as ScriptCodeExpression ?? Operand;
            return visitor.Invoke(this) as ScriptCodeExpression;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected override ScriptCodeExpression Clone()
        {
            return new ScriptCodeUnaryOperatorExpression(Operator, Extensions.Clone(Operand));
        }
    }
}
