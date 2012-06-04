using System;
using System.Dynamic;
using System.Linq.Expressions;
using System.Runtime.Serialization;
using System.ComponentModel;

namespace DynamicScript.Runtime.Environment
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;
    using SystemConverter = System.Convert;
    using Compiler.Ast;

    /// <summary>
    /// Represents boolean object.
    /// This class cannot be inherited.
    /// </summary>
    [CLSCompliant(false)]
    [ComVisible(false)]
    [Serializable]
    public sealed class ScriptBoolean : ScriptConvertibleObject<ScriptBooleanContract, bool>
    {
        #region Nested Types
        [ComVisible(false)]
        internal sealed class BooleanConverter : RuntimeConverter<bool>
        {
            public override bool Convert(bool input, out IScriptObject result)
            {
                result = (ScriptBoolean)input;
                return true;
            }
        }
        #endregion
        private ScriptBoolean(SerializationInfo info, StreamingContext context)
            : this(Deserialize(info))
        {
        }

        /// <summary>
        /// Initializes a new DynamicScript-compliant boolean value.
        /// </summary>
        /// <param name="value">An instance of <see cref="System.Boolean"/> object that represent content of the integer object.</param>
        private ScriptBoolean(bool value)
            : base(ScriptBooleanContract.Instance, value)
        {
            
        }

        /// <summary>
        /// Represents false.
        /// </summary>
        public static readonly ScriptBoolean False = new ScriptBoolean(false);

        /// <summary>
        /// Represents true.
        /// </summary>
        public static readonly ScriptBoolean True = new ScriptBoolean(true);

        internal static MemberExpression New(bool value)
        {
            return value ?
                LinqHelpers.BodyOf<Func<ScriptBoolean>, MemberExpression>(() => True) :
                LinqHelpers.BodyOf<Func<ScriptBoolean>, MemberExpression>(() => False);
        }

        /// <summary>
        /// Provides conversion from <see cref="System.Boolean"/> object to DynamicScript-compliant representation.
        /// </summary>
        /// <param name="value">Boolean value to be converted.</param>
        /// <returns>DynamicScript-compliant representation of <see cref="System.Boolean"/> object.</returns>
        public static implicit operator ScriptBoolean(bool value)
        {
            return value ? True : False;
        }

        /// <summary>
        /// Applies bitwise complement or logicat negation.
        /// </summary>
        /// <param name="state">Internal interpreter state.</param>
        /// <returns>The operation result.</returns>
        protected override IScriptObject Not(InterpreterState state)
        {
            return Not();
        }

        private ScriptBoolean Not()
        {
            return !Value;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="right"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        public IScriptObject Or(IConvertible right, InterpreterState state)
        {
            switch (right != null ? right.GetTypeCode() : TypeCode.Object)
            {
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                    return (ScriptInteger)(SystemConverter.ToInt64(right) | SystemConverter.ToInt64(Value));
                case TypeCode.Boolean:
                    return (ScriptBoolean)(Value | SystemConverter.ToBoolean(right));
                default:
                    if (state.Context == InterpretationContext.Unchecked)
                        return Void;
                    else throw new UnsupportedOperationException(state);
            }
        }

        /// <summary>
        /// Computes logical or, bitwise or, or union.
        /// </summary>
        /// <param name="right">The right operand.</param>
        /// <param name="state">Internal interpreter state.</param>
        /// <returns>The result of the binary operation.</returns>
        protected override IScriptObject Or(IScriptObject right, InterpreterState state)
        {
            if (right is IConvertible)
                return Or((IConvertible)right, state);
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
        public IScriptObject And(IConvertible right, InterpreterState state)
        {
            switch (right != null ? right.GetTypeCode() : TypeCode.Object)
            {
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                    return (ScriptInteger)(SystemConverter.ToInt64(right) & SystemConverter.ToInt64(Value));
                case TypeCode.Boolean:
                    return (ScriptBoolean)(Value & SystemConverter.ToBoolean(right));
                default:
                    if (state.Context == InterpretationContext.Unchecked)
                        return Void;
                    else throw new UnsupportedOperationException(state);
            }
        }

        /// <summary>
        /// Computies logical and, bitwise and, or intersection.
        /// </summary>
        /// <param name="right">The right operand.</param>
        /// <param name="state">Internal interpreter state.</param>
        /// <returns>The result of the binary operation.</returns>
        protected override IScriptObject And(IScriptObject right, InterpreterState state)
        {
            if (right is IConvertible)
                return And((IConvertible)right, state);
            else if (IsVoid(right))
                return False;
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
        public IScriptObject ExclusiveOr(IConvertible right, InterpreterState state)
        {
            switch (right != null ? right.GetTypeCode() : TypeCode.Object)
            {
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                    return (ScriptInteger)(SystemConverter.ToInt64(right) ^ SystemConverter.ToInt64(Value));
                case TypeCode.Boolean:
                    return (ScriptBoolean)(Value ^ SystemConverter.ToBoolean(right));
                default:
                    if (state.Context == InterpretationContext.Unchecked)
                        return Void;
                    else throw new UnsupportedOperationException(state);
            }
        }

        /// <summary>
        /// Computes exclusive or, or difference between two objects.
        /// </summary>
        /// <param name="right">The second operand.</param>
        /// <param name="state">Internal interpreter state.</param>
        /// <returns>The computation result.</returns>
        protected override IScriptObject ExclusiveOr(IScriptObject right, InterpreterState state)
        {
            if (right is IConvertible)
                return ExclusiveOr((IConvertible)right, state);
            else if (IsVoid(right))
                return True;
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
        public ScriptBoolean Equals(IConvertible right, InterpreterState state)
        {
            switch (right != null ? right.GetTypeCode() : TypeCode.Object)
            {
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.Boolean:
                    return (ScriptBoolean)(Value == SystemConverter.ToBoolean(right));
                default:
                    return False;
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
                return Value ? False : True;
            else return False;
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
                case TypeCode.Boolean:
                    return Value && !SystemConverter.ToBoolean(right);
                default:
                    return False;
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
            if (right is IConvertible)
                return GreaterThan((IConvertible)right, state);
            else if (IsVoid(right))
                return this;
            else return False;
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
                return True;
            else return False;
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
                case TypeCode.Boolean:
                    return Value || !SystemConverter.ToBoolean(right);
                default:
                    return False;
            }
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
                case TypeCode.Boolean:
                    return !Value && SystemConverter.ToBoolean(right);
                default:
                    return False;
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
            return right is IConvertible ? LessThan((IConvertible)right, state) : False;
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
                case TypeCode.Boolean:
                    return !Value || SystemConverter.ToBoolean(right);
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
                return Value ? False : True;
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
                return NotEquals((IConvertible)right, state);
            else if (IsVoid(right))
                return this;
            else return True;
        }

        /// <summary>
        /// Returns a value indicating that the current value is equivalent to 'true'.
        /// </summary>
        /// <param name="value">The value to be converted.</param>
        /// <returns>The conversion result.</returns>
        public static bool operator true(ScriptBoolean value)
        {
            return value != null ? value.Value == true : false;
        }

        /// <summary>
        /// Returns a value indicating that the current value is equivalent to 'false'.
        /// </summary>
        /// <param name="value">The value to be converted.</param>
        /// <returns>The conversion result.</returns>
        public static bool operator false(ScriptBoolean value)
        {
            return value != null ? value.Value == false : true;
        }

        /// <summary>
        /// Inverse boolean value.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static ScriptBoolean operator !(ScriptBoolean value)
        {
            return value.Not();
        }

        internal override ScriptObject Intern(InterpreterState state)
        {
            return Value ? True : False;
        }

        private static Expression ExclusiveOr(Expression left, Expression right, ParameterExpression state)
        {
            var call = LinqHelpers.BodyOf<ScriptBoolean, IConvertible, InterpreterState, IScriptObject, MethodCallExpression>((i, r, s) => i.ExclusiveOr(r, s));
            return call.Update(left, new Expression[] { Expression.TypeAs(right, typeof(IConvertible)), state });
        }

        private static Expression Or(Expression left, Expression right, ParameterExpression state)
        {
            var call = LinqHelpers.BodyOf<ScriptBoolean, IConvertible, InterpreterState, IScriptObject, MethodCallExpression>((i, r, s) => i.Or(r, s));
            return call.Update(left, new Expression[] { Expression.TypeAs(right, typeof(IConvertible)), state });
        }

        private static Expression And(Expression left, Expression right, ParameterExpression state)
        {
            var call = LinqHelpers.BodyOf<ScriptBoolean, IConvertible, InterpreterState, IScriptObject, MethodCallExpression>((i, r, s) => i.And(r, s));
            return call.Update(left, new Expression[] { Expression.TypeAs(right, typeof(IConvertible)), state });
        }

        private static Expression NotEquals(Expression left, Expression right, ParameterExpression state)
        {
            var call = LinqHelpers.BodyOf<ScriptBoolean, IConvertible, InterpreterState, IScriptObject, MethodCallExpression>((i, r, s) => i.NotEquals(r, s));
            return call.Update(left, new Expression[] { Expression.TypeAs(right, typeof(IConvertible)), state });
        }

        private static Expression Equals(Expression left, Expression right, ParameterExpression state)
        {
            var call = LinqHelpers.BodyOf<ScriptBoolean, IConvertible, InterpreterState, IScriptObject, MethodCallExpression>((i, r, s) => i.Equals(r, s));
            return call.Update(left, new Expression[] { Expression.TypeAs(right, typeof(IConvertible)), state });
        }

        private static Expression LessThanOrEqual(Expression left, Expression right, ParameterExpression state)
        {
            var call = LinqHelpers.BodyOf<ScriptBoolean, IConvertible, InterpreterState, IScriptObject, MethodCallExpression>((i, r, s) => i.LessThanOrEqual(r, s));
            return call.Update(left, new Expression[] { Expression.TypeAs(right, typeof(IConvertible)), state });
        }

        private static Expression GreaterThanOrEqual(Expression left, Expression right, ParameterExpression state)
        {
            var call = LinqHelpers.BodyOf<ScriptBoolean, IConvertible, InterpreterState, IScriptObject, MethodCallExpression>((i, r, s) => i.GreaterThanOrEqual(r, s));
            return call.Update(left, new Expression[] { Expression.TypeAs(right, typeof(IConvertible)), state });
        }

        private static Expression LessThan(Expression left, Expression right, ParameterExpression state)
        {
            var call = LinqHelpers.BodyOf<ScriptBoolean, IConvertible, InterpreterState, IScriptObject, MethodCallExpression>((i, r, s) => i.LessThan(r, s));
            return call.Update(left, new Expression[] { Expression.TypeAs(right, typeof(IConvertible)), state });
        }

        private static Expression GreaterThan(Expression left, Expression right, ParameterExpression state)
        {
            var call = LinqHelpers.BodyOf<ScriptBoolean, IConvertible, InterpreterState, IScriptObject, MethodCallExpression>((i, r, s) => i.GreaterThan(r, s));
            return call.Update(left, new Expression[] { Expression.TypeAs(right, typeof(IConvertible)), state });
        }

        internal static Expression Inline(Expression lvalue, ScriptCodeBinaryOperatorType @operator, Expression rvalue, ScriptTypeCode rtype, ParameterExpression state)
        {
            lvalue = Expression.Convert(lvalue, typeof(ScriptBoolean));
            switch ((int)@operator << 8 | (byte)rtype)//emulates two-dimensional switch
            {
                //inlining of operator ^
                case (int)ScriptCodeBinaryOperatorType.Exclusion << 8 | (byte)ScriptTypeCode.Boolean:    //boolean ^ boolean
                    lvalue = UnderlyingValue(lvalue);
                    rvalue = UnderlyingValue(Expression.Convert(rvalue, typeof(ScriptBoolean)));
                    return Expression.Convert(Expression.ExclusiveOr(lvalue, rvalue), typeof(ScriptBoolean));
                case (int)ScriptCodeBinaryOperatorType.Exclusion << 8 | (byte)ScriptTypeCode.Integer: //boolean ^ integer
                    lvalue = Expression.Convert(UnderlyingValue(lvalue), ScriptInteger.UnderlyingType);
                    rvalue = ScriptInteger.UnderlyingValue(Expression.Convert(rvalue, typeof(ScriptInteger)));
                    return Expression.Convert(Expression.ExclusiveOr(lvalue, rvalue), typeof(ScriptInteger));
                case (int)ScriptCodeBinaryOperatorType.Exclusion << 8 | (byte)ScriptTypeCode.Void:    //boolean ^ void
                    lvalue = UnderlyingValue(lvalue);
                    return Expression.Convert(Expression.ExclusiveOr(lvalue, LinqHelpers.Constant(false)), typeof(ScriptBoolean));
                case (int)ScriptCodeBinaryOperatorType.Exclusion << 8 | (byte)ScriptTypeCode.Unknown: //boolean ^ <other>
                    return ExclusiveOr(lvalue, rvalue, state);
                //inlining of operator |
                case (int)ScriptCodeBinaryOperatorType.Union << 8 | (byte)ScriptTypeCode.Boolean:    //boolean | boolean
                    lvalue = UnderlyingValue(lvalue);
                    rvalue = UnderlyingValue(Expression.Convert(rvalue, typeof(ScriptBoolean)));
                    return Expression.Convert(Expression.Or(lvalue, rvalue), typeof(ScriptBoolean));
                case (int)ScriptCodeBinaryOperatorType.Union << 8 | (byte)ScriptTypeCode.Integer: //boolean | integer
                    lvalue = Expression.Convert(UnderlyingValue(lvalue), ScriptInteger.UnderlyingType);
                    rvalue = ScriptInteger.UnderlyingValue(Expression.Convert(rvalue, typeof(ScriptInteger)));
                    return Expression.Convert(Expression.Or(lvalue, rvalue), typeof(ScriptInteger));
                case (int)ScriptCodeBinaryOperatorType.Union << 8 | (byte)ScriptTypeCode.Void:    //boolean | void
                    return lvalue;
                case (int)ScriptCodeBinaryOperatorType.Union << 8 | (byte)ScriptTypeCode.Unknown: //boolean | <other>
                    return Or(lvalue, rvalue, state);
                //inlining of operator &
                case (int)ScriptCodeBinaryOperatorType.Intersection << 8 | (byte)ScriptTypeCode.Boolean:    //boolean & boolean
                    lvalue = UnderlyingValue(lvalue);
                    rvalue = UnderlyingValue(Expression.Convert(rvalue, typeof(ScriptBoolean)));
                    return Expression.Convert(Expression.And(lvalue, rvalue), typeof(ScriptBoolean));
                case (int)ScriptCodeBinaryOperatorType.Intersection << 8 | (byte)ScriptTypeCode.Integer: //boolean & integer
                    lvalue = Expression.Convert(UnderlyingValue(lvalue), ScriptInteger.UnderlyingType);
                    rvalue = ScriptInteger.UnderlyingValue(Expression.Convert(rvalue, typeof(ScriptInteger)));
                    return Expression.Convert(Expression.And(lvalue, rvalue), typeof(ScriptInteger));
                case (int)ScriptCodeBinaryOperatorType.Intersection << 8 | (byte)ScriptTypeCode.Void:    //boolean & void
                    return New(false);
                case (int)ScriptCodeBinaryOperatorType.Intersection << 8 | (byte)ScriptTypeCode.Unknown: //boolean & <other>
                    return And(lvalue, rvalue, state);
                //inlining of operator <>
                case (int)ScriptCodeBinaryOperatorType.ValueInequality << 8 | (byte)ScriptTypeCode.Boolean:    //boolean <> boolean
                    lvalue = UnderlyingValue(lvalue);
                    rvalue = UnderlyingValue(Expression.Convert(rvalue, typeof(ScriptBoolean)));
                    return Expression.Convert(Expression.NotEqual(lvalue, rvalue), typeof(ScriptBoolean));
                case (int)ScriptCodeBinaryOperatorType.ValueInequality << 8 | (byte)ScriptTypeCode.Integer: //boolean <> integer
                    lvalue = UnderlyingValue(lvalue);
                    rvalue = ScriptInteger.UnderlyingValue(Expression.Convert(rvalue, typeof(ScriptInteger)));
                    rvalue = Expression.Convert(rvalue, UnderlyingType);
                    return Expression.Convert(Expression.NotEqual(lvalue, rvalue), typeof(ScriptBoolean));
                case (int)ScriptCodeBinaryOperatorType.ValueInequality << 8 | (byte)ScriptTypeCode.Void:    //boolean <> void
                    return Expression.Convert(lvalue, typeof(ScriptBoolean));
                case (int)ScriptCodeBinaryOperatorType.ValueInequality << 8 | (byte)ScriptTypeCode.Unknown: //boolean <> <other>
                    return NotEquals(lvalue, rvalue, state);
                //inlining of operator ==
                case (int)ScriptCodeBinaryOperatorType.ValueEquality << 8 | (byte)ScriptTypeCode.Boolean:    //boolean == boolean
                    lvalue = UnderlyingValue(lvalue);
                    rvalue = UnderlyingValue(Expression.Convert(rvalue, UnderlyingType));
                    return Expression.Convert(Expression.Equal(lvalue, rvalue), typeof(ScriptBoolean));
                case (int)ScriptCodeBinaryOperatorType.ValueEquality << 8 | (byte)ScriptTypeCode.Integer: //boolean == integer
                    lvalue = UnderlyingValue(lvalue);
                    rvalue = ScriptInteger.UnderlyingValue(Expression.Convert(rvalue, typeof(ScriptInteger)));
                    rvalue = Expression.Convert(rvalue, UnderlyingType);
                    return Expression.Convert(Expression.Equal(lvalue, rvalue), typeof(ScriptBoolean));
                case (int)ScriptCodeBinaryOperatorType.ValueEquality << 8 | (byte)ScriptTypeCode.Void:    //boolean == void
                    return Expression.Convert(Expression.Not(lvalue), typeof(ScriptBoolean));
                case (int)ScriptCodeBinaryOperatorType.ValueEquality << 8 | (byte)ScriptTypeCode.Unknown: //boolean == <other>
                    return Equals(lvalue, rvalue, state);
                //inlining of operator <=
                case (int)ScriptCodeBinaryOperatorType.LessThanOrEqual << 8 | (byte)ScriptTypeCode.Boolean:    //boolean <= boolean
                    lvalue = UnderlyingValue(lvalue);
                    rvalue = UnderlyingValue(Expression.Convert(rvalue, typeof(ScriptBoolean)));
                    return Expression.Convert(Expression.OrElse(Expression.Not(lvalue), rvalue), typeof(ScriptBoolean));
                case (int)ScriptCodeBinaryOperatorType.LessThanOrEqual << 8 | (byte)ScriptTypeCode.Integer: //boolean <= integer
                    lvalue = UnderlyingValue(lvalue);
                    rvalue = ScriptInteger.UnderlyingValue(Expression.Convert(rvalue, typeof(ScriptInteger)));
                    rvalue = Expression.Convert(rvalue, UnderlyingType);
                    return Expression.Convert(Expression.OrElse(Expression.Not(lvalue), rvalue), typeof(ScriptBoolean));
                case (int)ScriptCodeBinaryOperatorType.LessThanOrEqual << 8 | (byte)ScriptTypeCode.Void:    //boolean <= void
                    return Expression.Convert(Expression.IsFalse(lvalue), typeof(ScriptBoolean));
                case (int)ScriptCodeBinaryOperatorType.LessThanOrEqual << 8 | (byte)ScriptTypeCode.Unknown: //boolean <= <other>
                    return LessThanOrEqual(lvalue, rvalue, state);
                //inlining of operator >=
                case (int)ScriptCodeBinaryOperatorType.GreaterThanOrEqual << 8 | (byte)ScriptTypeCode.Boolean:    //boolean >= boolean
                    lvalue = UnderlyingValue(lvalue);
                    rvalue = UnderlyingValue(Expression.Convert(rvalue, typeof(ScriptBoolean)));
                    return Expression.Convert(Expression.OrElse(lvalue, Expression.Not(rvalue)), typeof(ScriptBoolean));
                case (int)ScriptCodeBinaryOperatorType.GreaterThanOrEqual << 8 | (byte)ScriptTypeCode.Integer: //boolean >= integer
                    lvalue = UnderlyingValue(lvalue);
                    rvalue = ScriptInteger.UnderlyingValue(Expression.Convert(rvalue, typeof(ScriptInteger)));
                    rvalue = Expression.Convert(rvalue, UnderlyingType);
                    return Expression.Convert(Expression.OrElse(lvalue, Expression.Not(rvalue)), typeof(ScriptBoolean));
                case (int)ScriptCodeBinaryOperatorType.GreaterThanOrEqual << 8 | (byte)ScriptTypeCode.Void:    //boolean >= void
                    return New(true);
                case (int)ScriptCodeBinaryOperatorType.GreaterThanOrEqual << 8 | (byte)ScriptTypeCode.Unknown: //boolean >= <other>
                    return GreaterThanOrEqual(lvalue, rvalue, state);
                //inlining of operator <
                case (int)ScriptCodeBinaryOperatorType.LessThan << 8 | (byte)ScriptTypeCode.Boolean:    //boolean < boolean
                    lvalue = UnderlyingValue(lvalue);
                    rvalue = UnderlyingValue(Expression.Convert(rvalue, typeof(ScriptInteger)));
                    return Expression.Convert(Expression.AndAlso(Expression.Not(lvalue), rvalue), typeof(ScriptBoolean));
                case (int)ScriptCodeBinaryOperatorType.LessThan << 8 | (byte)ScriptTypeCode.Integer: //boolean < integer
                    lvalue = UnderlyingValue(lvalue);
                    rvalue = ScriptInteger.UnderlyingValue(Expression.Convert(rvalue, typeof(ScriptInteger)));
                    rvalue = Expression.Convert(rvalue, UnderlyingType);
                    return Expression.Convert(Expression.AndAlso(Expression.Not(lvalue), rvalue), typeof(ScriptBoolean));
                case (int)ScriptCodeBinaryOperatorType.LessThan << 8 | (byte)ScriptTypeCode.Void:    //boolean < void
                    lvalue = UnderlyingValue(lvalue);
                    return New(false);
                case (int)ScriptCodeBinaryOperatorType.LessThan << 8 | (byte)ScriptTypeCode.Unknown: //integer < <other>
                    return LessThan(lvalue, rvalue, state);
                //inlining of operator >
                case (int)ScriptCodeBinaryOperatorType.GreaterThan << 8 | (byte)ScriptTypeCode.Boolean:    //boolean > boolean
                    lvalue = UnderlyingValue(lvalue);
                    rvalue = UnderlyingValue(Expression.Convert(rvalue, typeof(ScriptBoolean)));
                    return Expression.Convert(Expression.AndAlso(lvalue, Expression.Not(rvalue)), typeof(ScriptBoolean));
                case (int)ScriptCodeBinaryOperatorType.GreaterThan << 8 | (byte)ScriptTypeCode.Integer: //integer > boolean
                    lvalue = UnderlyingValue(lvalue);
                    rvalue = ScriptInteger.UnderlyingValue(Expression.Convert(rvalue, typeof(ScriptInteger)));
                    rvalue = Expression.Convert(rvalue, UnderlyingType);
                    return Expression.Convert(Expression.AndAlso(lvalue, Expression.Not(rvalue)), typeof(ScriptBoolean));
                case (int)ScriptCodeBinaryOperatorType.GreaterThan << 8 | (byte)ScriptTypeCode.Void:    //boolean > void
                    return lvalue;
                case (int)ScriptCodeBinaryOperatorType.GreaterThan << 8 | (byte)ScriptTypeCode.Unknown: //integer > <other>
                    return GreaterThan(lvalue, rvalue, state);
                //inlining of operator +
                case (int)ScriptCodeBinaryOperatorType.Add << 8 | (byte)ScriptTypeCode.String: //boolean + string
                    return ScriptString.Concat(((UnaryExpression)lvalue).Operand, rvalue);
                case (int)ScriptCodeBinaryOperatorType.Add << 8 | (byte)ScriptTypeCode.Integer: //boolean + integer
                    lvalue = Expression.Condition(UnderlyingValue(lvalue), ScriptInteger.New(1), ScriptInteger.New(0));
                    lvalue = ScriptInteger.UnderlyingValue(lvalue);
                    rvalue = ScriptInteger.UnderlyingValue(Expression.Convert(rvalue, typeof(ScriptInteger)));
                    return Expression.Convert(Expression.Condition(InterpreterState.IsUncheckedContext(state), Expression.Add(lvalue, rvalue), Expression.AddChecked(lvalue, rvalue)), typeof(ScriptInteger));
                //inlining of operator -
                case (int)ScriptCodeBinaryOperatorType.Subtract << 8 | (byte)ScriptTypeCode.Integer: //boolean - integer
                    lvalue = Expression.Condition(UnderlyingValue(lvalue), ScriptInteger.New(1), ScriptInteger.New(0));
                    lvalue = ScriptInteger.UnderlyingValue(lvalue);
                    rvalue = ScriptInteger.UnderlyingValue(Expression.Convert(rvalue, typeof(ScriptInteger)));
                    return Expression.Convert(Expression.Condition(InterpreterState.IsUncheckedContext(state), Expression.Subtract(lvalue, rvalue), Expression.SubtractChecked(lvalue, rvalue)), typeof(ScriptInteger));
                //inlining of operator *
                case (int)ScriptCodeBinaryOperatorType.Multiply << 8 | (byte)ScriptTypeCode.Integer: //boolean * integer
                    lvalue = Expression.Condition(UnderlyingValue(lvalue), ScriptInteger.New(1), ScriptInteger.New(0));
                    lvalue = ScriptInteger.UnderlyingValue(lvalue);
                    rvalue = ScriptInteger.UnderlyingValue(Expression.Convert(rvalue, typeof(ScriptInteger)));
                    return Expression.Convert(Expression.Condition(InterpreterState.IsUncheckedContext(state), Expression.Multiply(lvalue, rvalue), Expression.MultiplyChecked(lvalue, rvalue)), typeof(ScriptInteger));
                //inlining of operator /
                case (int)ScriptCodeBinaryOperatorType.Divide << 8 | (byte)ScriptTypeCode.Integer: //boolean / integer
                    lvalue = Expression.Condition(UnderlyingValue(lvalue), ScriptInteger.New(1), ScriptInteger.New(0));
                    lvalue = ScriptInteger.UnderlyingValue(lvalue);
                    rvalue = ScriptInteger.UnderlyingValue(Expression.Convert(rvalue, typeof(ScriptInteger)));
                    return Expression.Convert(Expression.Divide(lvalue, rvalue), typeof(ScriptInteger));
                //inlining of operator %
                case (int)ScriptCodeBinaryOperatorType.Modulo << 8 | (byte)ScriptTypeCode.Integer: //boolean % integer
                    lvalue = Expression.Condition(UnderlyingValue(lvalue), ScriptInteger.New(1), ScriptInteger.New(0));
                    lvalue = ScriptInteger.UnderlyingValue(lvalue);
                    rvalue = ScriptInteger.UnderlyingValue(Expression.Convert(rvalue, typeof(ScriptInteger)));
                    return Expression.Convert(Expression.Modulo(lvalue, rvalue), typeof(ScriptInteger));
                default:
                    switch (@operator)
                    {
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
