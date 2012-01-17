using System;
using System.Dynamic;
using System.Linq.Expressions;
using System.Runtime.Serialization;

namespace DynamicScript.Runtime.Environment
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;
    using SystemConverter = System.Convert;
    using Compiler.Ast;

    /// <summary>
    /// Represents real object.
    /// This class cannot be inherited.
    /// </summary>
    [CLSCompliant(false)]
    [ComVisible(false)]
    [Serializable]
    public sealed class ScriptReal : ScriptConvertibleObject<ScriptRealContract, double>
    {
        #region Nested Types
        [ComVisible(false)]
        internal sealed class DoubleConverter : RuntimeConverter<double>
        {
            public override bool Convert(double input, out IScriptObject result)
            {
                result = new ScriptReal(input);
                return true;
            }
        }

        [ComVisible(false)]
        internal sealed class SingleConverter : RuntimeConverter<float>
        {
            public override bool Convert(float input, out IScriptObject result)
            {
                result = new ScriptReal(input);
                return true;
            }
        }
        #endregion
        private ScriptReal(SerializationInfo info, StreamingContext context)
            : this(Deserialize(info))
        {
        }

        /// <summary>
        /// Initializes a new DynamicScript-compliant real value.
        /// </summary>
        /// <param name="value">An instance of <see cref="System.Double"/> object that represent content of the real object.</param>
        public ScriptReal(double value)
            : base(ScriptRealContract.Instance, value)
        {

        }

        /// <summary>
        /// Represents zero value.
        /// </summary>
        public static readonly ScriptReal Zero = 0.0;

        /// <summary>
        /// Represents not a number.
        /// </summary>
        public static readonly ScriptReal NaN = new ScriptReal(double.NaN);

        /// <summary>
        /// Represents largest possible real.
        /// </summary>
        public static readonly ScriptReal MaxValue = new ScriptReal(double.MaxValue);

        /// <summary>
        /// Represents smallest possible real.
        /// </summary>
        public static readonly ScriptReal MinValue = new ScriptReal(double.MinValue);

        /// <summary>
        /// Represents the smallest positive real.
        /// </summary>
        public static readonly ScriptReal Epsilon = new ScriptReal(double.Epsilon);

        /// <summary>
        /// Represents positive infinity.
        /// </summary>
        public static readonly ScriptReal PositiveInfinity = new ScriptReal(double.PositiveInfinity);

        /// <summary>
        /// Represents negative infinity.
        /// </summary>
        public static readonly ScriptReal NegativeInfinity = new ScriptReal(double.NegativeInfinity);

        /// <summary>
        /// Provides conversion from <see cref="System.Double"/> object to DynamicScript-compliant representation.
        /// </summary>
        /// <param name="value">Real value to be converted.</param>
        /// <returns>DynamicScript-compliant representation of <see cref="System.Double"/> object.</returns>
        public static implicit operator ScriptReal(double value)
        {
            if (double.IsPositiveInfinity(value))
                return PositiveInfinity;
            else if (double.IsNegativeInfinity(value))
                return NegativeInfinity;
            else if (double.IsNaN(value))
                return NaN;
            else if (value == default(double))
                return Zero;
            else if (value == double.MaxValue)
                return MaxValue;
            else if (value == double.MinValue)
                return MinValue;
            else return new ScriptReal(value);
        }

        internal static Expression New(double value)
        {
            if (double.IsPositiveInfinity(value))
                return LinqHelpers.BodyOf<Func<ScriptReal>, MemberExpression>(() => PositiveInfinity);
            else if (double.IsNegativeInfinity(value))
                return LinqHelpers.BodyOf<Func<ScriptReal>, MemberExpression>(() => NegativeInfinity);
            else if (double.IsNaN(value))
                return LinqHelpers.BodyOf<Func<ScriptReal>, MemberExpression>(() => NaN);
            else if (value == default(double))
                return LinqHelpers.BodyOf<Func<ScriptReal>, MemberExpression>(() => Zero);
            else if (value == double.MaxValue)
                return LinqHelpers.BodyOf<Func<ScriptReal>, MemberExpression>(() => MaxValue);
            else if (value == double.MinValue)
                return LinqHelpers.BodyOf<Func<ScriptReal>, MemberExpression>(() => MinValue);
            else return LinqHelpers.Convert<ScriptReal, double>(value);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="right"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        public IScriptObject Add(IConvertible right, InterpreterState state)
        {
            switch (right != null ? right.GetTypeCode() : TypeCode.Object)
            {
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                    return (ScriptReal)(Value + SystemConverter.ToInt64(right));
                case TypeCode.Single:
                case TypeCode.Double:
                    return (ScriptReal)(Value + SystemConverter.ToDouble(right));
                case TypeCode.String:
                    return (ScriptString)string.Concat(Value, right);
                default:
                    if (state.Context == InterpretationContext.Unchecked)
                        return Void;
                    else throw new UnsupportedOperationException(state);
            }
        }

        /// <summary>
        /// Computes the sum, or union of the current object with the specified.
        /// </summary>
        /// <param name="right">The second operand of the addition operation.</param>
        /// <param name="state">Internal interpretation state.</param>
        /// <returns>The result of the binary operation interpretation.</returns>
        protected override IScriptObject Add(IScriptObject right, InterpreterState state)
        {
            if (right is IConvertible)
                return Add((IConvertible)right, state);
            else if (IsVoid(right))
                return this;
            else if (state.Context == InterpretationContext.Unchecked)
                return Void;
            else throw new UnsupportedOperationException(state);
        }

        /// <summary>
        /// Returns coalesce result.
        /// </summary>
        /// <param name="right">The right operand of coalescing operation.</param>
        /// <param name="state">Internal interpreter state.</param>
        /// <returns>The operation result.</returns>
        protected override IScriptObject Coalesce(IScriptObject right, InterpreterState state)
        {
            return this;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="right"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        public ScriptReal Divide(IConvertible right, InterpreterState state)
        {
            switch (right != null ? right.GetTypeCode() : TypeCode.Object)
            {
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                    return Value / SystemConverter.ToInt64(right);
                case TypeCode.Single:
                case TypeCode.Double:
                    return Value / SystemConverter.ToDouble(right);
                default:
                    if (state.Context == InterpretationContext.Unchecked)
                        return Zero;
                    else throw new UnsupportedOperationException(state);
            }
        }

        /// <summary>
        /// Divides the current object using the specified.
        /// </summary>
        /// <param name="right">The right operand of the division operator.</param>
        /// <param name="state">Internal interpreter state.</param>
        /// <returns>The division result.</returns>
        protected override IScriptObject Divide(IScriptObject right, InterpreterState state)
        {
            if (right is IConvertible)
                return Divide((IConvertible)right, state);
            else if (IsVoid(right))
                return (ScriptReal)(Value / 0.0);
            else if (state.Context == InterpretationContext.Unchecked)
                return ScriptObject.Void;
            else throw new UnsupportedOperationException(state);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="right"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        public ScriptBoolean Equals(IConvertible right, InterpreterState state)
        {
            switch (right != null ? right.GetTypeCode() : TypeCode.Object)
            {
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                    return Value == SystemConverter.ToInt64(right);
                case TypeCode.Single:
                case TypeCode.Double:
                    return Value == SystemConverter.ToDouble(right);
                default:
                    return false;
            }
        }

        /// <summary>
        /// Determines whether the the current object is equal to another.
        /// </summary>
        /// <param name="right">Other object to be compared.</param>
        /// <param name="state">Internal interpreter state.</param>
        /// <returns>The comparison result.</returns>
        protected override IScriptObject Equals(IScriptObject right, InterpreterState state)
        {
            if (right is IConvertible)
                return Equals((IConvertible)right, state);
            else if (IsVoid(right))
                return (ScriptBoolean)(Value == 0.0);
            else return ScriptBoolean.False;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="right"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        public ScriptBoolean NotEquals(IConvertible right, InterpreterState state)
        {
            return !Equals(right, state);
        }

        /// <summary>
        /// Determines whether the current object is not equal to another.
        /// </summary>
        /// <param name="right">The second operand.</param>
        /// <param name="state">Internal interpreter state.</param>
        /// <returns>The comparison result.</returns>
        /// <remarks>This method is implemented by default.</remarks>
        protected override IScriptObject NotEquals(IScriptObject right, InterpreterState state)
        {
            if (right is IConvertible)
                return NotEquals(Convert(right), state);
            else if (IsVoid(right))
                return (ScriptBoolean)(Value != 0.0);
            else return ScriptBoolean.True;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="right"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        public ScriptBoolean GreaterThan(IConvertible right, InterpreterState state)
        {
            switch (right != null ? right.GetTypeCode() : TypeCode.Object)
            {
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                    return Value > SystemConverter.ToInt64(right);
                case TypeCode.Single:
                case TypeCode.Double:
                    return Value > SystemConverter.ToDouble(right);
                default:
                    return false;
            }
        }

        /// <summary>
        /// Determines whether the current object is greater than the specified object.
        /// </summary>
        /// <param name="right">The second object to compare.</param>
        /// <param name="state">Internal interpreter state.</param>
        /// <returns><see langword="true"/>the current object is greater than the specified object; otherwise, <see langword="false"/>.</returns>
        protected override IScriptObject GreaterThan(IScriptObject right, InterpreterState state)
        {
            if (right.OneOf<ScriptBoolean, ScriptInteger, ScriptReal>())
                return GreaterThan(Convert(right), state);
            else if (IsVoid(right))
                return (ScriptBoolean)(Value > 0.0);
            else return ScriptBoolean.False;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="right"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        public ScriptBoolean GreaterThanOrEqual(IConvertible right, InterpreterState state)
        {
            switch (right != null ? right.GetTypeCode() : TypeCode.Object)
            {
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                    return Value >= SystemConverter.ToInt64(right);
                case TypeCode.Single:
                case TypeCode.Double:
                    return Value >= SystemConverter.ToDouble(right);
                default:
                    return false;
            }
        }

        /// <summary>
        /// Determines whether the current object is greater than or equal to the specified object.
        /// </summary>
        /// <param name="right">The second object to compare.</param>
        /// <param name="state">Internal interpreter state.</param>
        /// <returns><see langword="true"/>the current object is greater than or equal to the specified object; otherwise, <see langword="false"/>.</returns>
        protected override IScriptObject GreaterThanOrEqual(IScriptObject right, InterpreterState state)
        {
            if (right is IConvertible)
                return GreaterThanOrEqual((IConvertible)right, state);
            else if (IsVoid(right))
                return (ScriptBoolean)(Value >= 0.0);
            else return ScriptBoolean.False;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="right"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        public ScriptBoolean LessThan(IConvertible right, InterpreterState state)
        {
            switch (right != null ? right.GetTypeCode() : TypeCode.Object)
            {
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                    return Value < SystemConverter.ToInt64(right);
                case TypeCode.Single:
                case TypeCode.Double:
                    return Value < SystemConverter.ToDouble(right);
                default:
                    return false;
            }
        }

        /// <summary>
        /// Determines whether the current object is less than the specified object.
        /// </summary>
        /// <param name="right">The second object to compare.</param>
        /// <param name="state">Internal interpreter state.</param>
        /// <returns><see langword="true"/>the current object is less than the specified object; otherwise, <see langword="false"/>.</returns>
        protected override IScriptObject LessThan(IScriptObject right, InterpreterState state)
        {
            if (right is IConvertible)
                return LessThan((IConvertible)right, state);
            else if (IsVoid(right))
                return (ScriptBoolean)(Value < 0.0);
            else return ScriptBoolean.False;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="right"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        public ScriptBoolean LessThanOrEqual(IConvertible right, InterpreterState state)
        {
            switch (right != null ? right.GetTypeCode() : TypeCode.Object)
            {
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                    return Value <= SystemConverter.ToInt64(right);
                case TypeCode.Single:
                case TypeCode.Double:
                    return Value <= SystemConverter.ToDouble(right);
                default:
                    return false;
            }
        }

        /// <summary>
        /// Determines whether the current object is less than or equal to the specified object.
        /// </summary>
        /// <param name="right">The second object to compare.</param>
        /// <param name="state">Internal interpreter state.</param>
        /// <returns><see langword="true"/>the current object is less than or equal to the specified object; otherwise, <see langword="false"/>.</returns>
        protected override IScriptObject LessThanOrEqual(IScriptObject right, InterpreterState state)
        {
            if (right is IConvertible)
                return LessThanOrEqual((IConvertible)right, state);
            else if (IsVoid(right))
                return (ScriptBoolean)(Value <= 0.0);
            else return ScriptBoolean.False;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="right"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        public ScriptReal Multiply(IConvertible right, InterpreterState state)
        {
            switch (right != null ? right.GetTypeCode() : TypeCode.Object)
            {
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                    return Value * SystemConverter.ToInt64(right);
                case TypeCode.Single:
                case TypeCode.Double:
                    return Value * SystemConverter.ToDouble(right);
                default:
                    if (state.Context == InterpretationContext.Unchecked)
                        return Zero;
                    else throw new UnsupportedOperationException(state);
            }
        }

        /// <summary>
        /// Computes multiplication between the current object and the specified object.
        /// </summary>
        /// <param name="right">The second operand.</param>
        /// <param name="state">Internal interpreter state.</param>
        /// <returns>The multiplication of the two objects.</returns>
        protected override IScriptObject Multiply(IScriptObject right, InterpreterState state)
        {
            if (right is IConvertible)
                return Multiply((IConvertible)right, state);
            else if (IsVoid(right))
                return (ScriptReal)(Value * 0.0);
            else if (state.Context == InterpretationContext.Unchecked)
                return Void;
            else throw new UnsupportedOperationException(state);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="right"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        public ScriptReal Subtract(IConvertible right, InterpreterState state)
        {
            switch (right != null ? right.GetTypeCode() : TypeCode.Object)
            {
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                    return Value - SystemConverter.ToInt64(right);
                case TypeCode.Single:
                case TypeCode.Double:
                    return Value - SystemConverter.ToDouble(right);
                default:
                    if (state.Context == InterpretationContext.Unchecked)
                        return Zero;
                    else throw new UnsupportedOperationException(state);
            }
        }

        /// <summary>
        /// Computes subtraction between the current object and the specified object.
        /// </summary>
        /// <param name="right">The subtrahend.</param>
        /// <param name="state">Internal interpreter result.</param>
        /// <returns>The subtraction result.</returns>
        protected override IScriptObject Subtract(IScriptObject right, InterpreterState state)
        {
            if (right is IConvertible)
                return Subtract((IConvertible)right, state);
            else if (IsVoid(right))
                return this;
            else if (state.Context == InterpretationContext.Unchecked)
                return Void;
            else throw new UnsupportedOperationException(state);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="right"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        public ScriptReal Modulo(IConvertible right, InterpreterState state)
        {
            switch (right != null ? right.GetTypeCode() : TypeCode.Object)
            {
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                    return Value % SystemConverter.ToInt64(right);
                case TypeCode.Single:
                case TypeCode.Double:
                    return Value % SystemConverter.ToDouble(right);
                default:
                    if (state.Context == InterpretationContext.Unchecked)
                        return Zero;
                    else throw new UnsupportedOperationException(state);
            }
        }

        /// <summary>
        /// Computes the remainder after dividing the current object by the second.
        /// </summary>
        /// <param name="right">The right operand.</param>
        /// <param name="state">Internal interpreter state.</param>
        /// <returns>The remainder.</returns>
        protected override IScriptObject Modulo(IScriptObject right, InterpreterState state)
        {
            if (right is IConvertible)
                return Modulo((IConvertible)right, state);
            else if (IsVoid(right))
                return (ScriptReal)(Value % 0.0);
            else if (state.Context == InterpretationContext.Unchecked)
                return Void;
            else throw new UnsupportedOperationException(state);
        }

        /// <summary>
        /// Applies negation to the object.
        /// </summary>
        /// <param name="state">Internal interpreter state.</param>
        /// <returns>Negation result.</returns>
        protected override IScriptObject UnaryMinus(InterpreterState state)
        {
            return new ScriptReal(-Value);
        }

        /// <summary>
        /// Applies unary plus to the object.
        /// </summary>
        /// <param name="state">Internal interpreter state.</param>
        /// <returns>The operation result.</returns>
        protected override IScriptObject UnaryPlus(InterpreterState state)
        {
            return this;
        }

        /// <summary>
        /// Performs postfixed decrement on the object.
        /// </summary>
        /// <param name="state">Internal interpreter state.</param>
        /// <returns>The decremented object.</returns>
        protected override IScriptObject PostDecrementAssign(InterpreterState state)
        {
            return new ScriptReal(Value - 1);
        }

        /// <summary>
        /// Performs prefixed decrement on the object.
        /// </summary>
        /// <param name="state">Internal interpreter state.</param>
        /// <returns>The decremented object.</returns>
        protected override IScriptObject PreDecrementAssign(InterpreterState state)
        {
            return PostDecrementAssign(state);
        }

        /// <summary>
        /// Performs postfixed increment on the object.
        /// </summary>
        /// <param name="state">Internal interpreter state.</param>
        /// <returns>The incremented object.</returns>
        protected override IScriptObject PostIncrementAssign(InterpreterState state)
        {
            return new ScriptReal(Value + 1);
        }

        /// <summary>
        /// Performs prefixed increment on the object.
        /// </summary>
        /// <param name="state">Internal interpreter state.</param>
        /// <returns>The incremented object.</returns>
        protected override IScriptObject PreIncrementAssign(InterpreterState state)
        {
            return PostIncrementAssign(state);
        }

        /// <summary>
        /// Applies postfixed ** operator to the current object.
        /// </summary>
        /// <param name="state">Internal interpreter state.</param>
        /// <returns>The operation result</returns>
        protected override IScriptObject PostSquareAssign(InterpreterState state)
        {
            return (ScriptReal)(Value * Value);
        }

        /// <summary>
        /// Applies prefixed ** operator to the current object.
        /// </summary>
        /// <param name="state">Internal interpreter state.</param>
        /// <returns>The operation result</returns>
        protected override IScriptObject PreSquareAssign(InterpreterState state)
        {
            return PostSquareAssign(state);
        }

        private static Expression NotEquals(Expression left, Expression right, ParameterExpression state)
        {
            var call = LinqHelpers.BodyOf<ScriptReal, IConvertible, InterpreterState, IScriptObject, MethodCallExpression>((i, r, s) => i.NotEquals(r, s));
            return call.Update(left, new Expression[] { Expression.TypeAs(right, typeof(IConvertible)), state });
        }

        private static Expression Equals(Expression left, Expression right, ParameterExpression state)
        {
            var call = LinqHelpers.BodyOf<ScriptReal, IConvertible, InterpreterState, IScriptObject, MethodCallExpression>((i, r, s) => i.Equals(r, s));
            return call.Update(left, new Expression[] { Expression.TypeAs(right, typeof(IConvertible)), state });
        }

        private static Expression LessThan(Expression left, Expression right, ParameterExpression state)
        {
            var call = LinqHelpers.BodyOf<ScriptReal, IConvertible, InterpreterState, IScriptObject, MethodCallExpression>((i, r, s) => i.LessThan(r, s));
            return call.Update(left, new Expression[] { Expression.TypeAs(right, typeof(IConvertible)), state });
        }

        private static Expression LessThanOrEqual(Expression left, Expression right, ParameterExpression state)
        {
            var call = LinqHelpers.BodyOf<ScriptReal, IConvertible, InterpreterState, IScriptObject, MethodCallExpression>((i, r, s) => i.LessThanOrEqual(r, s));
            return call.Update(left, new Expression[] { Expression.TypeAs(right, typeof(IConvertible)), state });
        }

        private static Expression GreaterThanOrEqual(Expression left, Expression right, ParameterExpression state)
        {
            var call = LinqHelpers.BodyOf<ScriptReal, IConvertible, InterpreterState, IScriptObject, MethodCallExpression>((i, r, s) => i.GreaterThanOrEqual(r, s));
            return call.Update(left, new Expression[] { Expression.TypeAs(right, typeof(IConvertible)), state });
        }

        private static Expression GreaterThan(Expression left, Expression right, ParameterExpression state)
        {
            var call = LinqHelpers.BodyOf<ScriptReal, IConvertible, InterpreterState, IScriptObject, MethodCallExpression>((i, r, s) => i.GreaterThan(r, s));
            return call.Update(left, new Expression[] { Expression.TypeAs(right, typeof(IConvertible)), state });
        }

        private static Expression Modulo(Expression left, Expression right, ParameterExpression state)
        {
            var call = LinqHelpers.BodyOf<ScriptReal, IConvertible, InterpreterState, IScriptObject, MethodCallExpression>((i, r, s) => i.Modulo(r, s));
            return call.Update(left, new Expression[] { Expression.TypeAs(right, typeof(IConvertible)), state });
        }

        private static Expression Divide(Expression left, Expression right, ParameterExpression state)
        {
            var call = LinqHelpers.BodyOf<ScriptReal, IConvertible, InterpreterState, IScriptObject, MethodCallExpression>((i, r, s) => i.Divide(r, s));
            return call.Update(left, new Expression[] { Expression.TypeAs(right, typeof(IConvertible)), state });
        }

        private static Expression Multiply(Expression left, Expression right, ParameterExpression state)
        {
            var call = LinqHelpers.BodyOf<ScriptReal, IConvertible, InterpreterState, IScriptObject, MethodCallExpression>((i, r, s) => i.Multiply(r, s));
            return call.Update(left, new Expression[] { Expression.TypeAs(right, typeof(IConvertible)), state });
        }

        private static Expression Subtract(Expression left, Expression right, ParameterExpression state)
        {
            var call = LinqHelpers.BodyOf<ScriptReal, IConvertible, InterpreterState, IScriptObject, MethodCallExpression>((i, r, s) => i.Subtract(r, s));
            return call.Update(left, new Expression[] { Expression.TypeAs(right, typeof(IConvertible)), state });
        }

        private static Expression Add(Expression left, Expression right, ParameterExpression state)
        {
            var call = LinqHelpers.BodyOf<ScriptReal, IConvertible, InterpreterState, IScriptObject, MethodCallExpression>((i, r, s) => i.Add(r, s));
            return call.Update(left, new Expression[] { Expression.TypeAs(right, typeof(IConvertible)), state });
        }

        internal static Expression Inline(Expression lvalue, ScriptCodeBinaryOperatorType @operator, Expression rvalue, ScriptTypeCode rtype, ParameterExpression state)
        {
            lvalue = Expression.Convert(lvalue, typeof(ScriptReal));
            switch ((int)@operator << 8 | (byte)rtype)//emulates two-dimensional switch
            {
                //inlining of operator <>
                case (int)ScriptCodeBinaryOperatorType.ValueInequality << 8 | (byte)ScriptTypeCode.Real:    //real <> real
                    lvalue = UnderlyingValue(lvalue);
                    rvalue = UnderlyingValue(Expression.Convert(rvalue, typeof(ScriptReal)));
                    return Expression.Convert(Expression.NotEqual(lvalue, rvalue), typeof(ScriptBoolean));
                case (int)ScriptCodeBinaryOperatorType.ValueInequality << 8 | (byte)ScriptTypeCode.Integer:    //real <> integer
                    lvalue = UnderlyingValue(lvalue);
                    rvalue = Expression.Convert(ScriptInteger.UnderlyingValue(Expression.Convert(rvalue, typeof(ScriptReal))), typeof(double));
                    return Expression.Convert(Expression.NotEqual(lvalue, rvalue), typeof(ScriptBoolean));
                case (int)ScriptCodeBinaryOperatorType.ValueInequality << 8 | (byte)ScriptTypeCode.Void:    //real <> void
                    lvalue = UnderlyingValue(lvalue);
                    return Expression.Convert(Expression.NotEqual(lvalue, LinqHelpers.Constant(0.0)), typeof(ScriptBoolean));
                case (int)ScriptCodeBinaryOperatorType.ValueInequality << 8 | (byte)ScriptTypeCode.Unknown: //real <> <other>
                    return NotEquals(lvalue, rvalue, state);
                //inlining of operator ==
                case (int)ScriptCodeBinaryOperatorType.ValueEquality << 8 | (byte)ScriptTypeCode.Real:    //real == real
                    lvalue = UnderlyingValue(lvalue);
                    rvalue = UnderlyingValue(Expression.Convert(rvalue, typeof(ScriptReal)));
                    return Expression.Convert(Expression.Equal(lvalue, rvalue), typeof(ScriptBoolean));
                case (int)ScriptCodeBinaryOperatorType.ValueEquality << 8 | (byte)ScriptTypeCode.Integer:    //real == integer
                    lvalue = UnderlyingValue(lvalue);
                    rvalue = Expression.Convert(ScriptInteger.UnderlyingValue(Expression.Convert(rvalue, typeof(ScriptInteger))), typeof(double));
                    return Expression.Convert(Expression.Equal(lvalue, rvalue), typeof(ScriptBoolean));
                case (int)ScriptCodeBinaryOperatorType.ValueEquality << 8 | (byte)ScriptTypeCode.Void:    //real == void
                    lvalue = UnderlyingValue(lvalue);
                    return Expression.Convert(Expression.Equal(lvalue, LinqHelpers.Constant(0.0)), typeof(ScriptBoolean));
                case (int)ScriptCodeBinaryOperatorType.ValueEquality << 8 | (byte)ScriptTypeCode.Unknown: //real == <other>
                    return Equals(lvalue, rvalue, state);
                //inlining of operator <=
                case (int)ScriptCodeBinaryOperatorType.LessThanOrEqual << 8 | (byte)ScriptTypeCode.Real:    //real <= real
                    lvalue = UnderlyingValue(lvalue);
                    rvalue = UnderlyingValue(Expression.Convert(rvalue, typeof(ScriptReal)));
                    return Expression.Convert(Expression.LessThanOrEqual(lvalue, rvalue), typeof(ScriptBoolean));
                case (int)ScriptCodeBinaryOperatorType.LessThanOrEqual << 8 | (byte)ScriptTypeCode.Integer:    //real <= integer
                    lvalue = UnderlyingValue(lvalue);
                    rvalue = Expression.Convert(ScriptInteger.UnderlyingValue(Expression.Convert(rvalue, typeof(ScriptInteger))), typeof(double));
                    return Expression.Convert(Expression.LessThanOrEqual(lvalue, rvalue), typeof(ScriptBoolean));
                case (int)ScriptCodeBinaryOperatorType.LessThanOrEqual << 8 | (byte)ScriptTypeCode.Void:    //real <= void
                    lvalue = UnderlyingValue(lvalue);
                    return Expression.Convert(Expression.LessThanOrEqual(lvalue, LinqHelpers.Constant(0.0)), typeof(ScriptBoolean));
                case (int)ScriptCodeBinaryOperatorType.LessThanOrEqual << 8 | (byte)ScriptTypeCode.Unknown: //real <= <other>
                    return LessThanOrEqual(lvalue, rvalue, state);
                //inlining of operator >=
                case (int)ScriptCodeBinaryOperatorType.GreaterThanOrEqual << 8 | (byte)ScriptTypeCode.Real:    //real >= real
                    lvalue = UnderlyingValue(lvalue);
                    rvalue = UnderlyingValue(Expression.Convert(rvalue, typeof(ScriptReal)));
                    return Expression.Convert(Expression.GreaterThanOrEqual(lvalue, rvalue), typeof(ScriptBoolean));
                case (int)ScriptCodeBinaryOperatorType.GreaterThanOrEqual << 8 | (byte)ScriptTypeCode.Integer:    //real >= integer
                    lvalue = UnderlyingValue(lvalue);
                    rvalue = Expression.Convert(ScriptInteger.UnderlyingValue(Expression.Convert(rvalue, typeof(ScriptInteger))), typeof(double));
                    return Expression.Convert(Expression.GreaterThanOrEqual(lvalue, rvalue), typeof(ScriptBoolean));
                case (int)ScriptCodeBinaryOperatorType.GreaterThanOrEqual << 8 | (byte)ScriptTypeCode.Void:    //real >= void
                    lvalue = UnderlyingValue(lvalue);
                    return Expression.Convert(Expression.GreaterThanOrEqual(lvalue, LinqHelpers.Constant(0.0)), typeof(ScriptBoolean));
                case (int)ScriptCodeBinaryOperatorType.GreaterThanOrEqual << 8 | (byte)ScriptTypeCode.Unknown: //real >= <other>
                    return GreaterThanOrEqual(lvalue, rvalue, state);
                //inlining of operator <
                case (int)ScriptCodeBinaryOperatorType.LessThan << 8 | (byte)ScriptTypeCode.Real:    //real < real
                    lvalue = UnderlyingValue(lvalue);
                    rvalue = UnderlyingValue(Expression.Convert(rvalue, typeof(ScriptReal)));
                    return Expression.Convert(Expression.LessThan(lvalue, rvalue), typeof(ScriptBoolean));
                case (int)ScriptCodeBinaryOperatorType.LessThan << 8 | (byte)ScriptTypeCode.Integer:    //real < integer
                    lvalue = UnderlyingValue(lvalue);
                    rvalue = Expression.Convert(ScriptInteger.UnderlyingValue(Expression.Convert(rvalue, typeof(ScriptInteger))), typeof(double));
                    return Expression.Convert(Expression.LessThan(lvalue, rvalue), typeof(ScriptBoolean));
                case (int)ScriptCodeBinaryOperatorType.LessThan << 8 | (byte)ScriptTypeCode.Void:    //real < void
                    lvalue = UnderlyingValue(lvalue);
                    return Expression.Convert(Expression.LessThan(lvalue, LinqHelpers.Constant(0.0)), typeof(ScriptBoolean));
                case (int)ScriptCodeBinaryOperatorType.LessThan << 8 | (byte)ScriptTypeCode.Unknown: //real < <other>
                    return LessThan(lvalue, rvalue, state);
                //inlining of operator >
                case (int)ScriptCodeBinaryOperatorType.GreaterThan << 8 | (byte)ScriptTypeCode.Real:    //real > real
                    lvalue = UnderlyingValue(lvalue);
                    rvalue = UnderlyingValue(Expression.Convert(rvalue, typeof(ScriptReal)));
                    return Expression.Convert(Expression.GreaterThan(lvalue, rvalue), typeof(ScriptBoolean));
                case (int)ScriptCodeBinaryOperatorType.GreaterThan << 8 | (byte)ScriptTypeCode.Integer:    //real > integer
                    lvalue = UnderlyingValue(lvalue);
                    rvalue = Expression.Convert(ScriptInteger.UnderlyingValue(Expression.Convert(rvalue, typeof(ScriptInteger))), typeof(double));
                    return Expression.Convert(Expression.GreaterThan(lvalue, rvalue), typeof(ScriptBoolean));
                case (int)ScriptCodeBinaryOperatorType.GreaterThan << 8 | (byte)ScriptTypeCode.Void:    //real > void
                    lvalue = UnderlyingValue(lvalue);
                    return Expression.Convert(Expression.GreaterThan(lvalue, LinqHelpers.Constant(0.0)), typeof(ScriptBoolean));
                case (int)ScriptCodeBinaryOperatorType.GreaterThan << 8 | (byte)ScriptTypeCode.Unknown: //real > <other>
                    return GreaterThan(lvalue, rvalue, state);
                //inlining of operator %
                case (int)ScriptCodeBinaryOperatorType.Modulo << 8 | (byte)ScriptTypeCode.Real:    //real % real
                    lvalue = UnderlyingValue(lvalue);
                    rvalue = UnderlyingValue(Expression.Convert(rvalue, typeof(ScriptReal)));
                    return Expression.Convert(Expression.Modulo(lvalue, rvalue), typeof(ScriptReal));
                case (int)ScriptCodeBinaryOperatorType.Modulo << 8 | (byte)ScriptTypeCode.Integer:    //real % integer
                    lvalue = UnderlyingValue(lvalue);
                    rvalue = Expression.Convert(ScriptInteger.UnderlyingValue(Expression.Convert(rvalue, typeof(ScriptInteger))), typeof(double));
                    return Expression.Convert(Expression.Modulo(lvalue, rvalue), typeof(ScriptReal));
                case (int)ScriptCodeBinaryOperatorType.Modulo << 8 | (byte)ScriptTypeCode.Void:    //real % void
                    return BinaryOperation(lvalue, ScriptCodeBinaryOperatorType.Modulo, rvalue, state);
                case (int)ScriptCodeBinaryOperatorType.Modulo << 8 | (byte)ScriptTypeCode.Unknown: //real % <other>
                    return Modulo(lvalue, rvalue, state);
                //inlining of operator /
                case (int)ScriptCodeBinaryOperatorType.Divide << 8 | (byte)ScriptTypeCode.Real:    //real / real
                    lvalue = UnderlyingValue(lvalue);
                    rvalue = UnderlyingValue(Expression.Convert(rvalue, typeof(ScriptReal)));
                    return Expression.Convert(Expression.Divide(lvalue, rvalue), typeof(ScriptInteger));
                case (int)ScriptCodeBinaryOperatorType.Divide << 8 | (byte)ScriptTypeCode.Integer:    //real / integer
                    lvalue = UnderlyingValue(lvalue);
                    rvalue = Expression.Convert(ScriptInteger.UnderlyingValue(Expression.Convert(rvalue, typeof(ScriptInteger))), typeof(double));
                    return Expression.Convert(Expression.Divide(lvalue, rvalue), typeof(ScriptReal));
                case (int)ScriptCodeBinaryOperatorType.Divide << 8 | (byte)ScriptTypeCode.Void:    //real / void
                    return BinaryOperation(lvalue, ScriptCodeBinaryOperatorType.Divide, rvalue, state);
                case (int)ScriptCodeBinaryOperatorType.Divide << 8 | (byte)ScriptTypeCode.Unknown: //real / <other>
                    return Divide(lvalue, rvalue, state);
                //inlining of operator *
                case (int)ScriptCodeBinaryOperatorType.Multiply << 8 | (byte)ScriptTypeCode.Real:    //real * real
                    lvalue = UnderlyingValue(lvalue);
                    rvalue = UnderlyingValue(Expression.Convert(rvalue, typeof(ScriptReal)));
                    return Expression.Convert(Expression.Multiply(lvalue, rvalue), typeof(ScriptReal));
                case (int)ScriptCodeBinaryOperatorType.Multiply << 8 | (byte)ScriptTypeCode.Integer:    //real * integer
                    lvalue = UnderlyingValue(lvalue);
                    rvalue = Expression.Convert(ScriptInteger.UnderlyingValue(Expression.Convert(rvalue, typeof(ScriptInteger))), typeof(double));
                    return Expression.Convert(Expression.Multiply(lvalue, rvalue), typeof(ScriptReal));
                case (int)ScriptCodeBinaryOperatorType.Multiply << 8 | (byte)ScriptTypeCode.Void:    //real * void
                    lvalue = UnderlyingValue(lvalue);
                    return Expression.Convert(Expression.Multiply(lvalue, LinqHelpers.Constant(0.0)), typeof(ScriptReal));
                case (int)ScriptCodeBinaryOperatorType.Multiply << 8 | (byte)ScriptTypeCode.Unknown: //real * <other>
                    return Multiply(lvalue, rvalue, state);
                //inlining of operator -
                case (int)ScriptCodeBinaryOperatorType.Subtract << 8 | (byte)ScriptTypeCode.Real:    //real - real
                    lvalue = UnderlyingValue(lvalue);
                    rvalue = UnderlyingValue(Expression.Convert(rvalue, typeof(ScriptReal)));
                    return Expression.Convert(Expression.Subtract(lvalue, rvalue), typeof(ScriptReal));
                case (int)ScriptCodeBinaryOperatorType.Subtract << 8 | (byte)ScriptTypeCode.Integer:    //real - integer
                    lvalue = UnderlyingValue(lvalue);
                    rvalue = Expression.Convert(ScriptInteger.UnderlyingValue(Expression.Convert(rvalue, typeof(ScriptInteger))), typeof(double));
                    return Expression.Convert(Expression.Subtract(lvalue, rvalue), typeof(ScriptReal));
                case (int)ScriptCodeBinaryOperatorType.Subtract << 8 | (byte)ScriptTypeCode.Void:    //real - void
                    return lvalue;
                case (int)ScriptCodeBinaryOperatorType.Subtract << 8 | (byte)ScriptTypeCode.Unknown: //real - <other>
                    return Subtract(lvalue, rvalue, state);
                //inlining of operator +
                case (int)ScriptCodeBinaryOperatorType.Add << 8 | (byte)ScriptTypeCode.Real: //real + real
                    lvalue = UnderlyingValue(lvalue);
                    rvalue = UnderlyingValue(Expression.Convert(rvalue, typeof(ScriptReal)));
                    return Expression.Convert(Expression.Add(lvalue, rvalue), typeof(ScriptInteger));
                case (int)ScriptCodeBinaryOperatorType.Add << 8 | (byte)ScriptTypeCode.Integer:    //real + integer
                    lvalue = UnderlyingValue(lvalue);
                    rvalue = Expression.Convert(ScriptInteger.UnderlyingValue(Expression.Convert(rvalue, typeof(ScriptInteger))), typeof(double));
                    return Expression.Convert(Expression.Add(lvalue, rvalue), typeof(ScriptReal));
                case (int)ScriptCodeBinaryOperatorType.Add << 8 | (byte)ScriptTypeCode.String: //real + string
                    return ScriptString.Concat(((UnaryExpression)lvalue).Operand, rvalue);
                case (int)ScriptCodeBinaryOperatorType.Add << 8 | (byte)ScriptTypeCode.Void:    //real + void
                    return lvalue;
                case (int)ScriptCodeBinaryOperatorType.Add << 8 | (byte)ScriptTypeCode.Unknown: //real + <other>
                    return Add(lvalue, rvalue, state);
                default:
                    switch (@operator)
                    {
                        case ScriptCodeBinaryOperatorType.Coalesce:
                            return Expression.Condition(Expression.NotEqual(UnderlyingValue(lvalue), LinqHelpers.Constant(0.0)), lvalue, rvalue);
                        case ScriptCodeBinaryOperatorType.GreaterThan:
                        case ScriptCodeBinaryOperatorType.GreaterThanOrEqual:
                        case ScriptCodeBinaryOperatorType.LessThan:
                        case ScriptCodeBinaryOperatorType.LessThanOrEqual:
                        case ScriptCodeBinaryOperatorType.ValueEquality:
                        case ScriptCodeBinaryOperatorType.ReferenceEquality:
                            return ScriptBoolean.New(false);
                        case ScriptCodeBinaryOperatorType.ValueInequality:
                        case ScriptCodeBinaryOperatorType.ReferenceInequality:
                            return ScriptBoolean.New(true);
                        case ScriptCodeBinaryOperatorType.TypeCast:
                            return BinaryOperation(lvalue, ScriptCodeBinaryOperatorType.TypeCast, rvalue, state);
                        default:
                            return Expression.Condition(InterpreterState.IsUncheckedContext(state), MakeVoid(), UnsupportedOperationException.Throw(state));
                    }
            }
        }
    }
}
