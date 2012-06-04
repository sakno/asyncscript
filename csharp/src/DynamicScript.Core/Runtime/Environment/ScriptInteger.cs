using System;
using System.Dynamic;
using System.Linq.Expressions;
using System.Runtime.Serialization;
using System.Collections.Generic;

namespace DynamicScript.Runtime.Environment
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;
    using SystemConverter = System.Convert;
    using Compiler.Ast;
    using CultureInfo = System.Globalization.CultureInfo;

    /// <summary>
    /// Represents DynamicScript-compliant representation of the integer.
    /// This class cannot be inherited.
    /// </summary>
    [ComVisible(false)]
    [CLSCompliant(false)]
    [Serializable]
    public sealed class ScriptInteger : ScriptConvertibleObject<ScriptIntegerContract, long>
    {
        #region Nested Types
        [ComVisible(false)]
        internal abstract class ConverterBase<T> : RuntimeConverter<T>
            where T : struct, IConvertible
        {

            public sealed override bool Convert(T input, out IScriptObject result)
            {
                result = new ScriptInteger(SystemConverter.ToInt64(input));
                return true;
            }
        }

        /// <summary>
        /// Represents converter from <see cref="System.Int64"/> to DynamicScript-compliant representation.
        /// This class cannot be inherited.
        /// </summary>
        [ComVisible(false)]
        internal sealed class Int64Converter : ConverterBase<long>
        {
        }

        /// <summary>
        /// Represents converter from <see cref="System.Int32"/> to DynamicScript-compliant representation.
        /// This class cannot be inherited.
        /// </summary>
        [ComVisible(false)]
        internal sealed class Int32Converter : ConverterBase<int>
        {
        }

        /// <summary>
        /// Represents converter from <see cref="System.Int16"/> to DynamicScript-compliant representation.
        /// This class cannot be inherited.
        /// </summary>
        [ComVisible(false)]
        internal sealed class Int16Converter : ConverterBase<short>
        {
        }

        /// <summary>
        /// Represents converter from <see cref="System.Byte"/> to DynamicScript-compliant representation.
        /// This class cannot be inherited.
        /// </summary>
        [ComVisible(false)]
        internal sealed class UInt8Converter : ConverterBase<byte>
        {
        }

        /// <summary>
        /// Represents converter from <see cref="System.SByte"/> to DynamicScript-compliant representation.
        /// This class cannot be inherited.
        /// </summary>
        [ComVisible(false)]
        internal sealed class Int8Converter : ConverterBase<sbyte>
        {
        }

        /// <summary>
        /// Represents converter from <see cref="System.UInt16"/> to DynamicScript-compliant representation.
        /// This class cannot be inherited.
        /// </summary>
        [ComVisible(false)]
        internal sealed class UInt16Converter : ConverterBase<ushort>
        {
        }

        /// <summary>
        /// Represents converter from <see cref="System.UInt32"/> to DynamicScript-compliant representation.
        /// This class cannot be inherited.
        /// </summary>
        [ComVisible(false)]
        internal sealed class UInt32Converter : ConverterBase<uint>
        {
        }
        #endregion

        private ScriptInteger(SerializationInfo info, StreamingContext context)
            : this(Deserialize(info))
        {
        }

        /// <summary>
        /// Initializes a new integer value.
        /// </summary>
        /// <param name="value">An instance of <see cref="System.Int64"/> object that represent content of the integer object.</param>
        public ScriptInteger(long value)
            : base(ScriptIntegerContract.Instance, value)
        {
        }

        internal ScriptInteger(char value)
            : this((long)value)
        {
        }

        /// <summary>
        /// Represents zero value.
        /// </summary>
        public static readonly ScriptInteger Zero = new ScriptInteger(0L);

        /// <summary>
        /// Represents the smallest possible value.
        /// </summary>
        public static readonly ScriptInteger MinValue = new ScriptInteger(long.MinValue);

        /// <summary>
        /// Represents the largest possible value.
        /// </summary>
        public static readonly ScriptInteger MaxValue = new ScriptInteger(long.MaxValue);

        /// <summary>
        /// Represents 1 value.
        /// </summary>b
        public static readonly ScriptInteger One = new ScriptInteger(1L);

        /// <summary>
        /// Provides implicit conversion from <see cref="System.Int64"/> to its DynamicScript-compliant representation.
        /// </summary>
        /// <param name="value">The value to be converted.</param>
        /// <returns>Conversion result.</returns>
        public static implicit operator ScriptInteger(long value)
        {
            switch (value)
            {
                case 0L: return Zero;
                case 1L: return One;
                case long.MaxValue: return MaxValue;
                case long.MinValue: return MinValue;
                default: return new ScriptInteger(value);
            }
        }

        internal static Expression New(long value)
        {
            switch (value)
            {
                case 0L: return LinqHelpers.BodyOf<Func<ScriptInteger>, MemberExpression>(() => Zero);
                case 1L: return LinqHelpers.BodyOf<Func<ScriptInteger>, MemberExpression>(() => One);
                case long.MaxValue: return LinqHelpers.BodyOf<Func<ScriptInteger>, MemberExpression>(() => MaxValue);
                case long.MinValue: return LinqHelpers.BodyOf<Func<ScriptInteger>, MemberExpression>(() => MinValue);
                default: return LinqHelpers.Convert<ScriptInteger, long>(value);
            }
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
                case TypeCode.Byte:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.Boolean:
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
                return (ScriptBoolean)(Value == 0L);
            else return ScriptBoolean.False;
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
                case TypeCode.Boolean:
                case TypeCode.Int16:
                case TypeCode.Byte:
                case TypeCode.Int32:
                case TypeCode.Int64:
                    return (ScriptInteger)(state.Context == InterpretationContext.Unchecked ? unchecked(Value + SystemConverter.ToInt64(right)) : checked(Value + SystemConverter.ToInt64(right)));
                case TypeCode.Single:
                case TypeCode.Double:
                    return (ScriptReal)(Value + SystemConverter.ToDouble(right));
                case TypeCode.String:
                    return (ScriptString)string.Concat(right);
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
        /// 
        /// </summary>
        /// <param name="right"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        public IScriptObject Subtract(IConvertible right, InterpreterState state)
        {
            switch (right != null ? right.GetTypeCode() : TypeCode.Object)
            {
                case TypeCode.Boolean:
                case TypeCode.Int16:
                case TypeCode.Byte:
                case TypeCode.Int32:
                case TypeCode.Int64:
                    return (ScriptInteger)(state.Context == InterpretationContext.Unchecked ? unchecked(Value - SystemConverter.ToInt64(right)) : checked(Value - SystemConverter.ToInt64(right)));
                case TypeCode.Single:
                case TypeCode.Double:
                    return (ScriptReal)(Value - SystemConverter.ToDouble(right));
                default:
                    if (state.Context == InterpretationContext.Unchecked)
                        return Void;
                    else throw new UnsupportedOperationException(state);
            }
        }

        /// <summary>
        /// Subtracts the current integer from the specified.
        /// </summary>
        /// <param name="right"></param>
        /// <param name="state"></param>
        /// <returns></returns>
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
        /// <param name="value"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        public static ScriptInteger Increment(ScriptInteger value, InterpreterState state)
        {
            return (state.Context == InterpretationContext.Unchecked ? unchecked(value + 1) : checked(value + 1));
        }

        /// <summary>
        /// Performs prefixed increment on the object.
        /// </summary>
        /// <param name="state">Internal interpreter state.</param>
        /// <returns>The incremented object.</returns>
        protected override IScriptObject PreIncrementAssign(InterpreterState state)
        {
            return Increment(this, state);
        }

        /// <summary>
        /// Performs postfixed increment on the object.
        /// </summary>
        /// <param name="state">Internal interpreter state.</param>
        /// <returns>The incremented object.</returns>
        protected override IScriptObject PostIncrementAssign(InterpreterState state)
        {
            return this;
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
                case TypeCode.Byte:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.Boolean:
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
                return (ScriptBoolean)(Value < 0L);
            else return ScriptBoolean.False;
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
                case TypeCode.Byte:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.Boolean:
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
            if (right is IConvertible)
                return GreaterThan((IConvertible)right, state);
            else if (IsVoid(right))
                return (ScriptBoolean)(Value > 0L);
            else return ScriptBoolean.False;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="right"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        public IScriptObject Modulo(IConvertible right, InterpreterState state)
        {
            switch (right != null ? right.GetTypeCode() : TypeCode.Object)
            {
                case TypeCode.Byte:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.Boolean:
                    return (ScriptInteger)(Value % SystemConverter.ToInt64(right));
                case TypeCode.Single:
                case TypeCode.Double:
                    return (ScriptReal)(Value % SystemConverter.ToDouble(right));
                default:
                    if (state.Context == InterpretationContext.Unchecked)
                        return Void;
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
                throw new DivideByZeroException();
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
        public IScriptObject Multiply(IConvertible right, InterpreterState state)
        {
            switch (right != null ? right.GetTypeCode() : TypeCode.Object)
            {
                case TypeCode.Byte:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.Boolean:
                    return (ScriptInteger)(state.Context == InterpretationContext.Unchecked ? unchecked(Value * SystemConverter.ToInt64(right)) : checked(Value * SystemConverter.ToInt64(right)));
                default:
                    if (state.Context == InterpretationContext.Unchecked)
                        return Void;
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
                return Zero;
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
        public ScriptInteger And(IConvertible right, InterpreterState state)
        {
            switch (right != null ? right.GetTypeCode() : TypeCode.Object)
            {
                case TypeCode.Byte:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.Boolean:
                    return Value & SystemConverter.ToInt64(right);
                default:
                    if (state.Context == InterpretationContext.Unchecked)
                        return Zero;
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
                return Zero;
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
        public ScriptInteger Or(IConvertible right, InterpreterState state)
        {
            switch (right != null ? right.GetTypeCode() : TypeCode.Object)
            {
                case TypeCode.Byte:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.Boolean:
                    return Value | SystemConverter.ToInt64(right);
                default:
                    if (state.Context == InterpretationContext.Unchecked)
                        return Zero;
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
        public ScriptInteger ExclusiveOr(IConvertible right, InterpreterState state)
        {
            switch (right != null ? right.GetTypeCode() : TypeCode.Object)
            {
                case TypeCode.Byte:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.Boolean:
                    return Value ^ SystemConverter.ToInt64(right);
                default:
                    if (state.Context == InterpretationContext.Unchecked)
                        return Zero;
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
                return (ScriptInteger)(Value ^ 0L);
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
        public IScriptObject Divide(IConvertible right, InterpreterState state)
        {
            switch (right != null ? right.GetTypeCode() : TypeCode.Object)
            {
                case TypeCode.Boolean:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.Byte:
                    return (ScriptInteger)(Value / SystemConverter.ToInt64(right));
                case TypeCode.Single:
                case TypeCode.Double:
                    return (ScriptReal)(Value / SystemConverter.ToDouble(right));
                default:
                    if (state.Context == InterpretationContext.Unchecked)
                        return Void;
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
                throw new DivideByZeroException();
            else if (state.Context == InterpretationContext.Unchecked)
                return ScriptObject.Void;
            else throw new UnsupportedOperationException(state); 
        }

        /// <summary>
        /// Applies bitwise complement or logicat negation.
        /// </summary>
        /// <param name="state">Internal interpreter state.</param>
        /// <returns>The operation result.</returns>
        protected override IScriptObject Not(InterpreterState state)
        {
            return new ScriptInteger(~Value);
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
        protected override IScriptObject NotEquals(IScriptObject right, InterpreterState state)
        {
            if (right is IConvertible)
                return NotEquals((IConvertible)right, state);
            else if (IsVoid(right))
                return (ScriptBoolean)(Value != 0L);
            else return ScriptBoolean.True;
        }

        /// <summary>
        /// Applies negation to the object.
        /// </summary>
        /// <param name="state">Internal interpreter state.</param>
        /// <returns>Negation result.</returns>
        protected override IScriptObject UnaryMinus(InterpreterState state)
        {
            return new ScriptInteger(-Value);
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
        /// Applies postfixed ** operator to the current object.
        /// </summary>
        /// <param name="state">Internal interpreter state.</param>
        /// <returns>The operation result</returns>
        protected override IScriptObject PostSquareAssign(InterpreterState state)
        {
            return new ScriptInteger(state.Context == InterpretationContext.Unchecked ? unchecked(Value * Value) : checked(Value * Value));
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
                case TypeCode.Boolean:
                case TypeCode.Int16:
                case TypeCode.Byte:
                case TypeCode.Int32:
                case TypeCode.Int64:
                    return Value >= SystemConverter.ToInt64(right);
                case TypeCode.Single:
                case TypeCode.Double:
                    return Value >= SystemConverter.ToDouble(right);
                default: return false;
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
                return (ScriptBoolean)(Value >= 0L);
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
                case TypeCode.Boolean:
                case TypeCode.Int16:
                case TypeCode.Byte:
                case TypeCode.Int32:
                case TypeCode.Int64:
                    return Value <= SystemConverter.ToInt64(right);
                case TypeCode.Single:
                case TypeCode.Double:
                    return Value <= SystemConverter.ToDouble(right);
                default: return false;
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
                return (ScriptBoolean)(Value <= 0L);
            else return ScriptBoolean.False;
        }

        /// <summary>
        /// Performs postfixed decrement on the object.
        /// </summary>
        /// <param name="state">Internal interpreter state.</param>
        /// <returns>The decremented object.</returns>
        protected override IScriptObject PostDecrementAssign(InterpreterState state)
        {
            return new ScriptInteger(state.Context == InterpretationContext.Unchecked ? unchecked(Value - 1) : checked(Value - 1));
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
        /// Gets bit on the specified position in the 64-bit integer.
        /// </summary>
        /// <param name="bitpos"></param>
        /// <returns></returns>
        public bool this[int bitpos]
        {
            get { return (Value & 1L << bitpos) > 0; }
        }

        /// <summary>
        /// Gets bit at the specified position.
        /// </summary>
        /// <param name="indicies"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        public override IScriptObject this[IList<IScriptObject> indicies, InterpreterState state]
        {
            get
            {
                if (indicies.Count == 1)
                {
                    var bitpos = indicies[0] as ScriptInteger;
                    if (bitpos == null || !bitpos.IsInt32) throw new UnsupportedOperationException(state);
                    else return (ScriptBoolean)this[(int)bitpos];
                }
                else return base[indicies, state];
            }
            set { base[indicies, state] = value; }
        }

        /// <summary>
        /// Determines whether this value can be converted into <see cref="System.Byte"/>
        /// without overflow.
        /// </summary>
        public bool IsByte
        {
            get { return Value.Between(byte.MinValue, byte.MaxValue);}
        }

        /// <summary>
        /// Determines whether this value can be converted into <see cref="System.Int32"/>
        /// without overflow.
        /// </summary>
        public bool IsInt32
        {
            get { return Value.Between(int.MinValue, int.MaxValue); }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="formatProvider"></param>
        /// <returns></returns>
        public static ScriptInteger TryParse(string value, CultureInfo formatProvider)
        {
            var result = default(long);
            return long.TryParse(value, System.Globalization.NumberStyles.Any, formatProvider, out result) ? new ScriptInteger(result) : null;
        }

        private static Expression ExclusiveOr(Expression left, Expression right, ParameterExpression state)
        {
            var call = LinqHelpers.BodyOf<ScriptInteger, IConvertible, InterpreterState, IScriptObject, MethodCallExpression>((i, r, s) => i.ExclusiveOr(r, s));
            return call.Update(left, new Expression[] { Expression.TypeAs(right, typeof(IConvertible)), state });
        }

        private static Expression Or(Expression left, Expression right, ParameterExpression state)
        {
            var call = LinqHelpers.BodyOf<ScriptInteger, IConvertible, InterpreterState, IScriptObject, MethodCallExpression>((i, r, s) => i.Or(r, s));
            return call.Update(left, new Expression[] { Expression.TypeAs(right, typeof(IConvertible)), state });
        }

        private static Expression And(Expression left, Expression right, ParameterExpression state)
        {
            var call = LinqHelpers.BodyOf<ScriptInteger, IConvertible, InterpreterState, IScriptObject, MethodCallExpression>((i, r, s) => i.And(r, s));
            return call.Update(left, new Expression[] { Expression.TypeAs(right, typeof(IConvertible)), state });
        }

        private static Expression NotEquals(Expression left, Expression right, ParameterExpression state)
        {
            var call = LinqHelpers.BodyOf<ScriptInteger, IConvertible, InterpreterState, IScriptObject, MethodCallExpression>((i, r, s) => i.NotEquals(r, s));
            return call.Update(left, new Expression[] { Expression.TypeAs(right, typeof(IConvertible)), state });
        }

        private static Expression Equals(Expression left, Expression right, ParameterExpression state)
        {
            var call = LinqHelpers.BodyOf<ScriptInteger, IConvertible, InterpreterState, IScriptObject, MethodCallExpression>((i, r, s) => i.Equals(r, s));
            return call.Update(left, new Expression[] { Expression.TypeAs(right, typeof(IConvertible)), state });
        }

        private static Expression LessThanOrEqual(Expression left, Expression right, ParameterExpression state)
        {
            var call = LinqHelpers.BodyOf<ScriptInteger, IConvertible, InterpreterState, IScriptObject, MethodCallExpression>((i, r, s) => i.LessThanOrEqual(r, s));
            return call.Update(left, new Expression[] { Expression.TypeAs(right, typeof(IConvertible)), state });
        }

        private static Expression GreaterThanOrEqual(Expression left, Expression right, ParameterExpression state)
        {
            var call = LinqHelpers.BodyOf<ScriptInteger, IConvertible, InterpreterState, IScriptObject, MethodCallExpression>((i, r, s) => i.GreaterThanOrEqual(r, s));
            return call.Update(left, new Expression[] { Expression.TypeAs(right, typeof(IConvertible)), state });
        }

        private static Expression LessThan(Expression left, Expression right, ParameterExpression state)
        {
            var call = LinqHelpers.BodyOf<ScriptInteger, IConvertible, InterpreterState, IScriptObject, MethodCallExpression>((i, r, s) => i.LessThan(r, s));
            return call.Update(left, new Expression[] { Expression.TypeAs(right, typeof(IConvertible)), state });
        }

        private static Expression GreaterThan(Expression left, Expression right, ParameterExpression state)
        {
            var call = LinqHelpers.BodyOf<ScriptInteger, IConvertible, InterpreterState, IScriptObject, MethodCallExpression>((i, r, s) => i.GreaterThan(r, s));
            return call.Update(left, new Expression[] { Expression.TypeAs(right, typeof(IConvertible)), state });
        }

        private static Expression Modulo(Expression left, Expression right, ParameterExpression state)
        {
            var call = LinqHelpers.BodyOf<ScriptInteger, IConvertible, InterpreterState, IScriptObject, MethodCallExpression>((i, r, s) => i.Modulo(r, s));
            return call.Update(left, new Expression[] { Expression.TypeAs(right, typeof(IConvertible)), state });
        }

        private static Expression Divide(Expression left, Expression right, ParameterExpression state)
        {
            var call = LinqHelpers.BodyOf<ScriptInteger, IConvertible, InterpreterState, IScriptObject, MethodCallExpression>((i, r, s) => i.Divide(r, s));
            return call.Update(left, new Expression[] { Expression.TypeAs(right, typeof(IConvertible)), state });
        }

        private static Expression Multiply(Expression left, Expression right, ParameterExpression state)
        {
            var call = LinqHelpers.BodyOf<ScriptInteger, IConvertible, InterpreterState, IScriptObject, MethodCallExpression>((i, r, s) => i.Multiply(r, s));
            return call.Update(left, new Expression[] { Expression.TypeAs(right, typeof(IConvertible)), state });
        }

        private static Expression Subtract(Expression left, Expression right, ParameterExpression state)
        {
            var call = LinqHelpers.BodyOf<ScriptInteger, IConvertible, InterpreterState, IScriptObject, MethodCallExpression>((i, r, s) => i.Subtract(r, s));
            return call.Update(left, new Expression[] { Expression.TypeAs(right, typeof(IConvertible)), state });
        }

        private static Expression Add(Expression left, Expression right, ParameterExpression state)
        {
            var call = LinqHelpers.BodyOf<ScriptInteger, IConvertible, InterpreterState, IScriptObject, MethodCallExpression>((i, r, s) => i.Add(r, s));
            return call.Update(left, new Expression[] { Expression.TypeAs(right, typeof(IConvertible)), state });
        }

        /// <summary>
        /// Produces a binary expression as inlined statement.
        /// </summary>
        /// <param name="lvalue"></param>
        /// <param name="operator"></param>
        /// <param name="rvalue"></param>
        /// <param name="rtype"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        internal static Expression Inline(Expression lvalue, ScriptCodeBinaryOperatorType @operator, Expression rvalue, ScriptTypeCode rtype, ParameterExpression state)
        {
            lvalue = Expression.Convert(lvalue, typeof(ScriptInteger));
            switch ((int)@operator << 8 | (byte)rtype)//emulates two-dimensional switch
            {
                //inlining of operator ^
                case (int)ScriptCodeBinaryOperatorType.Exclusion << 8 | (byte)ScriptTypeCode.Integer:    //integer ^ integer
                    lvalue = UnderlyingValue(lvalue);
                    rvalue = UnderlyingValue(Expression.Convert(rvalue, typeof(ScriptInteger)));
                    return Expression.Convert(Expression.Or(lvalue, rvalue), typeof(ScriptInteger));
                case (int)ScriptCodeBinaryOperatorType.Exclusion << 8 | (byte)ScriptTypeCode.Boolean: //integer ^ boolean
                    lvalue = UnderlyingValue(lvalue);
                    rvalue = ScriptBoolean.UnderlyingValue(Expression.Convert(rvalue, typeof(ScriptBoolean)));
                    rvalue = Expression.Convert(rvalue, UnderlyingType);
                    return Expression.Convert(Expression.ExclusiveOr(lvalue, rvalue), typeof(ScriptInteger));
                case (int)ScriptCodeBinaryOperatorType.Exclusion << 8 | (byte)ScriptTypeCode.Void:    //integer ^ void
                    lvalue = UnderlyingValue(lvalue);
                    return Expression.Convert(Expression.ExclusiveOr(lvalue, LinqHelpers.Constant(0L)), typeof(ScriptInteger));
                case (int)ScriptCodeBinaryOperatorType.Exclusion << 8 | (byte)ScriptTypeCode.Unknown: //integer ^ <other>
                    return ExclusiveOr(lvalue, rvalue, state);
                //inlining of operator |
                case (int)ScriptCodeBinaryOperatorType.Union << 8 | (byte)ScriptTypeCode.Integer:    //integer | integer
                    lvalue = UnderlyingValue(lvalue);
                    rvalue = UnderlyingValue(Expression.Convert(rvalue, typeof(ScriptInteger)));
                    return Expression.Convert(Expression.Or(lvalue, rvalue), typeof(ScriptInteger));
                case (int)ScriptCodeBinaryOperatorType.Union << 8 | (byte)ScriptTypeCode.Boolean: //integer | boolean
                    lvalue = UnderlyingValue(lvalue);
                    rvalue = ScriptBoolean.UnderlyingValue(Expression.Convert(rvalue, typeof(ScriptBoolean)));
                    rvalue = Expression.Convert(rvalue, UnderlyingType);
                    return Expression.Convert(Expression.Or(lvalue, rvalue), typeof(ScriptInteger));
                case (int)ScriptCodeBinaryOperatorType.Union << 8 | (byte)ScriptTypeCode.Void:    //integer | void
                    return lvalue;
                case (int)ScriptCodeBinaryOperatorType.Union << 8 | (byte)ScriptTypeCode.Unknown: //integer | <other>
                    return Or(lvalue, rvalue, state);
                //inlining of operator &
                case (int)ScriptCodeBinaryOperatorType.Intersection << 8 | (byte)ScriptTypeCode.Integer:    //integer & integer
                    lvalue = UnderlyingValue(lvalue);
                    rvalue = UnderlyingValue(Expression.Convert(rvalue, typeof(ScriptInteger)));
                    return Expression.Convert(Expression.And(lvalue, rvalue), typeof(ScriptInteger));
                case (int)ScriptCodeBinaryOperatorType.Intersection << 8 | (byte)ScriptTypeCode.Boolean: //integer & boolean
                    lvalue = UnderlyingValue(lvalue);
                    rvalue = ScriptBoolean.UnderlyingValue(Expression.Convert(rvalue, typeof(ScriptBoolean)));
                    rvalue = Expression.Convert(rvalue, UnderlyingType);
                    return Expression.Convert(Expression.And(lvalue, rvalue), typeof(ScriptInteger));
                case (int)ScriptCodeBinaryOperatorType.Intersection << 8 | (byte)ScriptTypeCode.Void:    //integer & void
                    return New(0L);
                case (int)ScriptCodeBinaryOperatorType.Intersection << 8 | (byte)ScriptTypeCode.Unknown: //integer & <other>
                    return And(lvalue, rvalue, state);
                //inlining of operator <>
                case (int)ScriptCodeBinaryOperatorType.ValueInequality << 8 | (byte)ScriptTypeCode.Integer:    //integer <> integer
                    lvalue = UnderlyingValue(lvalue);
                    rvalue = UnderlyingValue(Expression.Convert(rvalue, typeof(ScriptInteger)));
                    return Expression.Convert(Expression.NotEqual(lvalue, rvalue), typeof(ScriptBoolean));
                case (int)ScriptCodeBinaryOperatorType.ValueInequality << 8 | (byte)ScriptTypeCode.Real:    //integer <> real
                    lvalue = Expression.Convert(UnderlyingValue(lvalue), typeof(double));
                    rvalue = ScriptReal.UnderlyingValue(Expression.Convert(rvalue, typeof(ScriptReal)));
                    return Expression.Convert(Expression.NotEqual(lvalue, rvalue), typeof(ScriptBoolean));
                case (int)ScriptCodeBinaryOperatorType.ValueInequality << 8 | (byte)ScriptTypeCode.Boolean: //integer <> boolean
                    lvalue = UnderlyingValue(lvalue);
                    rvalue = ScriptBoolean.UnderlyingValue(Expression.Convert(rvalue, typeof(ScriptBoolean)));
                    rvalue = Expression.Convert(rvalue, UnderlyingType);
                    return Expression.Convert(Expression.NotEqual(lvalue, rvalue), typeof(ScriptBoolean));
                case (int)ScriptCodeBinaryOperatorType.ValueInequality << 8 | (byte)ScriptTypeCode.Void:    //integer <> void
                    lvalue = UnderlyingValue(lvalue);
                    return Expression.Convert(Expression.NotEqual(lvalue, LinqHelpers.Constant(0L)), typeof(ScriptBoolean));
                case (int)ScriptCodeBinaryOperatorType.ValueInequality << 8 | (byte)ScriptTypeCode.Unknown: //integer <> <other>
                    return NotEquals(lvalue, rvalue, state);
                //inlining of operator ==
                case (int)ScriptCodeBinaryOperatorType.ValueEquality << 8 | (byte)ScriptTypeCode.Integer:    //integer == integer
                    lvalue = UnderlyingValue(lvalue);
                    rvalue = UnderlyingValue(Expression.Convert(rvalue, typeof(ScriptInteger)));
                    return Expression.Convert(Expression.Equal(lvalue, rvalue), typeof(ScriptBoolean));
                case (int)ScriptCodeBinaryOperatorType.ValueEquality << 8 | (byte)ScriptTypeCode.Real:    //integer == real
                    lvalue = Expression.Convert(UnderlyingValue(lvalue), typeof(double));
                    rvalue = ScriptReal.UnderlyingValue(Expression.Convert(rvalue, typeof(ScriptReal)));
                    return Expression.Convert(Expression.Equal(lvalue, rvalue), typeof(ScriptBoolean));
                case (int)ScriptCodeBinaryOperatorType.ValueEquality << 8 | (byte)ScriptTypeCode.Boolean: //integer == boolean
                    lvalue = UnderlyingValue(lvalue);
                    rvalue = ScriptBoolean.UnderlyingValue(Expression.Convert(rvalue, typeof(ScriptBoolean)));
                    rvalue = Expression.Convert(rvalue, UnderlyingType);
                    return Expression.Convert(Expression.Equal(lvalue, rvalue), typeof(ScriptBoolean));
                case (int)ScriptCodeBinaryOperatorType.ValueEquality << 8 | (byte)ScriptTypeCode.Void:    //integer == void
                    lvalue = UnderlyingValue(lvalue);
                    return Expression.Convert(Expression.Equal(lvalue, LinqHelpers.Constant(0L)), typeof(ScriptBoolean));
                case (int)ScriptCodeBinaryOperatorType.ValueEquality << 8 | (byte)ScriptTypeCode.Unknown: //integer == <other>
                    return Equals(lvalue, rvalue, state);
                //inlining of operator <=
                case (int)ScriptCodeBinaryOperatorType.LessThanOrEqual << 8 | (byte)ScriptTypeCode.Integer:    //integer <= integer
                    lvalue = UnderlyingValue(lvalue);
                    rvalue = UnderlyingValue(Expression.Convert(rvalue, typeof(ScriptInteger)));
                    return Expression.Convert(Expression.LessThanOrEqual(lvalue, rvalue), typeof(ScriptBoolean));
                case (int)ScriptCodeBinaryOperatorType.LessThanOrEqual << 8 | (byte)ScriptTypeCode.Real:    //integer <= real
                    lvalue = Expression.Convert(UnderlyingValue(lvalue), typeof(double));
                    rvalue = ScriptReal.UnderlyingValue(Expression.Convert(rvalue, typeof(ScriptReal)));
                    return Expression.Convert(Expression.LessThanOrEqual(lvalue, rvalue), typeof(ScriptBoolean));
                case (int)ScriptCodeBinaryOperatorType.LessThanOrEqual << 8 | (byte)ScriptTypeCode.Boolean: //integer <= boolean
                    lvalue = UnderlyingValue(lvalue);
                    rvalue = ScriptBoolean.UnderlyingValue(Expression.Convert(rvalue, typeof(ScriptBoolean)));
                    rvalue = Expression.Convert(rvalue, UnderlyingType);
                    return Expression.Convert(Expression.LessThanOrEqual(lvalue, rvalue), typeof(ScriptBoolean));
                case (int)ScriptCodeBinaryOperatorType.LessThanOrEqual << 8 | (byte)ScriptTypeCode.Void:    //integer <= void
                    lvalue = UnderlyingValue(lvalue);
                    return Expression.Convert(Expression.LessThanOrEqual(lvalue, LinqHelpers.Constant(0L)), typeof(ScriptBoolean));
                case (int)ScriptCodeBinaryOperatorType.LessThanOrEqual << 8 | (byte)ScriptTypeCode.Unknown: //integer <= <other>
                    return LessThanOrEqual(lvalue, rvalue, state);
                //inlining of operator >=
                case (int)ScriptCodeBinaryOperatorType.GreaterThanOrEqual << 8 | (byte)ScriptTypeCode.Integer:    //integer >= integer
                    lvalue = UnderlyingValue(lvalue);
                    rvalue = UnderlyingValue(Expression.Convert(rvalue, typeof(ScriptInteger)));
                    return Expression.Convert(Expression.GreaterThanOrEqual(lvalue, rvalue), typeof(ScriptBoolean));
                case (int)ScriptCodeBinaryOperatorType.GreaterThanOrEqual << 8 | (byte)ScriptTypeCode.Real:    //integer >= real
                    lvalue = Expression.Convert(UnderlyingValue(lvalue), typeof(double));
                    rvalue = ScriptReal.UnderlyingValue(Expression.Convert(rvalue, typeof(ScriptReal)));
                    return Expression.Convert(Expression.GreaterThanOrEqual(lvalue, rvalue), typeof(ScriptBoolean));
                case (int)ScriptCodeBinaryOperatorType.GreaterThanOrEqual << 8 | (byte)ScriptTypeCode.Boolean: //integer >= boolean
                    lvalue = UnderlyingValue(lvalue);
                    rvalue = ScriptBoolean.UnderlyingValue(Expression.Convert(rvalue, typeof(ScriptBoolean)));
                    rvalue = Expression.Convert(rvalue, UnderlyingType);
                    return Expression.Convert(Expression.GreaterThanOrEqual(lvalue, rvalue), typeof(ScriptBoolean));
                case (int)ScriptCodeBinaryOperatorType.GreaterThanOrEqual << 8 | (byte)ScriptTypeCode.Void:    //integer >= void
                    lvalue = UnderlyingValue(lvalue);
                    return Expression.Convert(Expression.GreaterThanOrEqual(lvalue, LinqHelpers.Constant(0L)), typeof(ScriptBoolean));
                case (int)ScriptCodeBinaryOperatorType.GreaterThanOrEqual << 8 | (byte)ScriptTypeCode.Unknown: //integer >= <other>
                    return GreaterThanOrEqual(lvalue, rvalue, state);
                //inlining of operator <
                case (int)ScriptCodeBinaryOperatorType.LessThan << 8 | (byte)ScriptTypeCode.Integer:    //integer < integer
                    lvalue = UnderlyingValue(lvalue);
                    rvalue = UnderlyingValue(Expression.Convert(rvalue, typeof(ScriptInteger)));
                    return Expression.Convert(Expression.LessThan(lvalue, rvalue), typeof(ScriptBoolean));
                case (int)ScriptCodeBinaryOperatorType.LessThan << 8 | (byte)ScriptTypeCode.Real:    //integer < real
                    lvalue = Expression.Convert(UnderlyingValue(lvalue), typeof(double));
                    rvalue = ScriptReal.UnderlyingValue(Expression.Convert(rvalue, typeof(ScriptReal)));
                    return Expression.Convert(Expression.LessThan(lvalue, rvalue), typeof(ScriptBoolean));
                case (int)ScriptCodeBinaryOperatorType.LessThan << 8 | (byte)ScriptTypeCode.Boolean: //integer < boolean
                    lvalue = UnderlyingValue(lvalue);
                    rvalue = ScriptBoolean.UnderlyingValue(Expression.Convert(rvalue, typeof(ScriptBoolean)));
                    rvalue = Expression.Convert(rvalue, UnderlyingType);
                    return Expression.Convert(Expression.LessThan(lvalue, rvalue), typeof(ScriptBoolean));
                case (int)ScriptCodeBinaryOperatorType.LessThan << 8 | (byte)ScriptTypeCode.Void:    //integer < void
                    lvalue = UnderlyingValue(lvalue);
                    return Expression.Convert(Expression.LessThan(lvalue, LinqHelpers.Constant(0L)), typeof(ScriptBoolean));
                case (int)ScriptCodeBinaryOperatorType.LessThan << 8 | (byte)ScriptTypeCode.Unknown: //integer < <other>
                    return LessThan(lvalue, rvalue, state);
                //inlining of operator >
                case (int)ScriptCodeBinaryOperatorType.GreaterThan << 8 | (byte)ScriptTypeCode.Integer:    //integer > integer
                    lvalue = UnderlyingValue(lvalue);
                    rvalue = UnderlyingValue(Expression.Convert(rvalue, typeof(ScriptInteger)));
                    return Expression.Convert(Expression.GreaterThan(lvalue, rvalue), typeof(ScriptBoolean));
                case (int)ScriptCodeBinaryOperatorType.GreaterThan << 8 | (byte)ScriptTypeCode.Real:    //integer > real
                    lvalue = Expression.Convert(UnderlyingValue(lvalue), typeof(double));
                    rvalue = ScriptReal.UnderlyingValue(Expression.Convert(rvalue, typeof(ScriptReal)));
                    return Expression.Convert(Expression.GreaterThan(lvalue, rvalue), typeof(ScriptBoolean));
                case (int)ScriptCodeBinaryOperatorType.GreaterThan << 8 | (byte)ScriptTypeCode.Boolean: //integer > boolean
                    lvalue = UnderlyingValue(lvalue);
                    rvalue = ScriptBoolean.UnderlyingValue(Expression.Convert(rvalue, typeof(ScriptBoolean)));
                    rvalue = Expression.Convert(rvalue, UnderlyingType);
                    return Expression.Convert(Expression.GreaterThan(lvalue, rvalue), typeof(ScriptBoolean));
                case (int)ScriptCodeBinaryOperatorType.GreaterThan << 8 | (byte)ScriptTypeCode.Void:    //integer > void
                    lvalue = UnderlyingValue(lvalue);
                    return Expression.Convert(Expression.GreaterThan(lvalue, LinqHelpers.Constant(0L)), typeof(ScriptBoolean));
                case (int)ScriptCodeBinaryOperatorType.GreaterThan << 8 | (byte)ScriptTypeCode.Unknown: //integer > <other>
                    return GreaterThan(lvalue, rvalue, state);
                //inlining of operator %
                case (int)ScriptCodeBinaryOperatorType.Modulo << 8 | (byte)ScriptTypeCode.Integer:    //integer % integer
                    lvalue = UnderlyingValue(lvalue);
                    rvalue = UnderlyingValue(Expression.Convert(rvalue, typeof(ScriptInteger)));
                    return Expression.Convert(Expression.Modulo(lvalue, rvalue), typeof(ScriptInteger));
                case (int)ScriptCodeBinaryOperatorType.Modulo << 8 | (byte)ScriptTypeCode.Real:    //integer % real
                    lvalue = Expression.Convert(UnderlyingValue(lvalue), typeof(double));
                    rvalue = ScriptReal.UnderlyingValue(Expression.Convert(rvalue, typeof(ScriptReal)));
                    return Expression.Convert(Expression.Modulo(lvalue, rvalue), typeof(ScriptReal));
                case (int)ScriptCodeBinaryOperatorType.Modulo << 8 | (byte)ScriptTypeCode.Boolean: //integer % boolean
                    lvalue = UnderlyingValue(lvalue);
                    rvalue = ScriptBoolean.UnderlyingValue(Expression.Convert(rvalue, typeof(ScriptBoolean)));
                    rvalue = Expression.Convert(rvalue, UnderlyingType);
                    return Expression.Convert(Expression.Modulo(lvalue, rvalue), typeof(ScriptInteger));
                case (int)ScriptCodeBinaryOperatorType.Modulo << 8 | (byte)ScriptTypeCode.Void:    //integer % void
                    return BinaryOperation(lvalue, ScriptCodeBinaryOperatorType.Modulo, rvalue, state);
                case (int)ScriptCodeBinaryOperatorType.Modulo << 8 | (byte)ScriptTypeCode.Unknown: //integer % <other>
                    return Modulo(lvalue, rvalue, state);
                //inlining of operator /
                case (int)ScriptCodeBinaryOperatorType.Divide << 8 | (byte)ScriptTypeCode.Integer:    //integer / integer
                    lvalue = UnderlyingValue(lvalue);
                    rvalue = UnderlyingValue(Expression.Convert(rvalue, typeof(ScriptInteger)));
                    return Expression.Convert(Expression.Divide(lvalue, rvalue), typeof(ScriptInteger));
                case (int)ScriptCodeBinaryOperatorType.Divide << 8 | (byte)ScriptTypeCode.Real:    //integer / real
                    lvalue = Expression.Convert(UnderlyingValue(lvalue), typeof(double));
                    rvalue = ScriptReal.UnderlyingValue(Expression.Convert(rvalue, typeof(ScriptReal)));
                    return Expression.Convert(Expression.Divide(lvalue, rvalue), typeof(ScriptReal));
                case (int)ScriptCodeBinaryOperatorType.Divide << 8 | (byte)ScriptTypeCode.Boolean: //integer / boolean
                    lvalue = UnderlyingValue(lvalue);
                    rvalue = ScriptBoolean.UnderlyingValue(Expression.Convert(rvalue, typeof(ScriptBoolean)));
                    rvalue = Expression.Convert(rvalue, UnderlyingType);
                    return Expression.Convert(Expression.Divide(lvalue, rvalue), typeof(ScriptInteger));
                case (int)ScriptCodeBinaryOperatorType.Divide << 8 | (byte)ScriptTypeCode.Void:    //integer / void
                    return BinaryOperation(lvalue, ScriptCodeBinaryOperatorType.Divide, rvalue, state);
                case (int)ScriptCodeBinaryOperatorType.Divide << 8 | (byte)ScriptTypeCode.Unknown: //integer / <other>
                    return Divide(lvalue, rvalue, state);
                //inlining of operator *
                case (int)ScriptCodeBinaryOperatorType.Multiply << 8 | (byte)ScriptTypeCode.Integer:    //integer * integer
                    lvalue = UnderlyingValue(lvalue);
                    rvalue = UnderlyingValue(Expression.Convert(rvalue, typeof(ScriptInteger)));
                    return Expression.Convert(Expression.Condition(InterpreterState.IsUncheckedContext(state), Expression.Multiply(lvalue, rvalue), Expression.MultiplyChecked(lvalue, rvalue)), typeof(ScriptInteger));
                case (int)ScriptCodeBinaryOperatorType.Multiply << 8 | (byte)ScriptTypeCode.Real:    //integer * real
                    lvalue = Expression.Convert(UnderlyingValue(lvalue), typeof(double));
                    rvalue = ScriptReal.UnderlyingValue(Expression.Convert(rvalue, typeof(ScriptReal)));
                    return Expression.Convert(Expression.Multiply(lvalue, rvalue), typeof(ScriptReal));
                case (int)ScriptCodeBinaryOperatorType.Multiply << 8 | (byte)ScriptTypeCode.Boolean: //integer * boolean
                    lvalue = UnderlyingValue(lvalue);
                    rvalue = ScriptBoolean.UnderlyingValue(Expression.Convert(rvalue, typeof(ScriptBoolean)));
                    rvalue = Expression.Convert(rvalue, UnderlyingType);
                    return Expression.Convert(Expression.Condition(InterpreterState.IsUncheckedContext(state), Expression.Subtract(lvalue, rvalue), Expression.SubtractChecked(lvalue, rvalue)), typeof(ScriptInteger));
                case (int)ScriptCodeBinaryOperatorType.Multiply << 8 | (byte)ScriptTypeCode.Void:    //integer * void
                    return New(0L);
                case (int)ScriptCodeBinaryOperatorType.Multiply << 8 | (byte)ScriptTypeCode.Unknown: //integer * <other>
                    return Multiply(lvalue, rvalue, state);
                //inlining of operator -
                case (int)ScriptCodeBinaryOperatorType.Subtract << 8 | (byte)ScriptTypeCode.Integer:    //integer - integer
                    lvalue = UnderlyingValue(lvalue);
                    rvalue = UnderlyingValue(Expression.Convert(rvalue, typeof(ScriptInteger)));
                    return Expression.Convert(Expression.Condition(InterpreterState.IsUncheckedContext(state), Expression.Subtract(lvalue, rvalue), Expression.SubtractChecked(lvalue, rvalue)), typeof(ScriptInteger));
                case (int)ScriptCodeBinaryOperatorType.Subtract << 8 | (byte)ScriptTypeCode.Real:    //integer - real
                    lvalue = Expression.Convert(UnderlyingValue(lvalue), typeof(double));
                    rvalue = ScriptReal.UnderlyingValue(Expression.Convert(rvalue, typeof(ScriptReal)));
                    return Expression.Convert(Expression.Subtract(lvalue, rvalue), typeof(ScriptReal));
                case (int)ScriptCodeBinaryOperatorType.Subtract << 8 | (byte)ScriptTypeCode.Boolean: //integer - boolean
                    lvalue = UnderlyingValue(lvalue);
                    rvalue = ScriptBoolean.UnderlyingValue(Expression.Convert(rvalue, typeof(ScriptBoolean)));
                    rvalue = Expression.Convert(rvalue, UnderlyingType);
                    return Expression.Convert(Expression.Condition(InterpreterState.IsUncheckedContext(state), Expression.Subtract(lvalue, rvalue), Expression.SubtractChecked(lvalue, rvalue)), typeof(ScriptInteger));
                case (int)ScriptCodeBinaryOperatorType.Subtract << 8 | (byte)ScriptTypeCode.Void:    //integer - void
                    return lvalue;
                case (int)ScriptCodeBinaryOperatorType.Subtract << 8 | (byte)ScriptTypeCode.Unknown: //integer - <other>
                    return Subtract(lvalue, rvalue, state);
                //inlining of operator +
                case (int)ScriptCodeBinaryOperatorType.Add << 8 | (byte)ScriptTypeCode.Integer: //integer + integer
                    lvalue = UnderlyingValue(lvalue);
                    rvalue = UnderlyingValue(Expression.Convert(rvalue, typeof(ScriptInteger)));
                    return Expression.Convert(Expression.Condition(InterpreterState.IsUncheckedContext(state), Expression.Add(lvalue, rvalue), Expression.AddChecked(lvalue, rvalue)), typeof(ScriptInteger));
                case (int)ScriptCodeBinaryOperatorType.Add << 8 | (byte)ScriptTypeCode.Real:    //integer + real
                    lvalue = Expression.Convert(UnderlyingValue(lvalue), typeof(double));
                    rvalue = ScriptReal.UnderlyingValue(Expression.Convert(rvalue, typeof(ScriptReal)));
                    return Expression.Convert(Expression.Add(lvalue, rvalue), typeof(ScriptReal));
                case (int)ScriptCodeBinaryOperatorType.Add << 8 | (byte)ScriptTypeCode.Boolean: //integer + boolean
                    lvalue = UnderlyingValue(lvalue);
                    rvalue = ScriptBoolean.UnderlyingValue(Expression.Convert(rvalue, typeof(ScriptBoolean)));
                    rvalue = Expression.Convert(rvalue, UnderlyingType);
                    return Expression.Convert(Expression.Condition(InterpreterState.IsUncheckedContext(state), Expression.Add(lvalue, rvalue), Expression.AddChecked(lvalue, rvalue)), typeof(ScriptInteger));
                case (int)ScriptCodeBinaryOperatorType.Add << 8 | (byte)ScriptTypeCode.String: //integer + string
                    return ScriptString.Concat(((UnaryExpression)lvalue).Operand, rvalue);
                case (int)ScriptCodeBinaryOperatorType.Add << 8 | (byte)ScriptTypeCode.Void:    //integer + void
                    return lvalue;
                case (int)ScriptCodeBinaryOperatorType.Add << 8 | (byte)ScriptTypeCode.Unknown: //integer + <other>
                    return Add(lvalue, rvalue, state);
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
