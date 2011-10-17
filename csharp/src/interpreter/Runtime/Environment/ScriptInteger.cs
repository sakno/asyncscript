using System;
using System.Dynamic;
using System.Linq.Expressions;
using System.Runtime.Serialization;
using System.Collections.Generic;

namespace DynamicScript.Runtime.Environment
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;
    using SystemConverter = System.Convert;
    using InterpretationContext = Compiler.Ast.InterpretationContext;
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

        [ComVisible(false)]
        private sealed class BitAccessor : Indexer, IEquatable<BitAccessor>
        {
            private readonly long m_value;
            private readonly int m_position;

            public BitAccessor(long value, int position)
                : base(ScriptBooleanContract.Instance)
            {
                m_value = value;
                m_position = position;
            }

            /// <summary>
            /// Gets value.
            /// </summary>
            public long Value
            {
                get { return m_value; }
            }

            /// <summary>
            /// Gets bit position.
            /// </summary>
            public int Position
            {
                get { return m_position; }
            }

            protected override bool IsReadOnly
            {
                get { return true; }
            }

            public ScriptBoolean GetValue()
            {
                return (m_value & 1L << m_position) > 0;
            }

            public override IScriptObject GetValue(InterpreterState state)
            {
                return GetValue();
            }

            public override void SetValue(IScriptObject value, InterpreterState state)
            {
                throw new ConstantCannotBeChangedException(state);
            }

            public bool Equals(BitAccessor accessor)
            {
                return accessor != null && Position == accessor.Position && Value == accessor.Value;
            }

            public override bool Equals(IRuntimeSlot other)
            {
                return Equals(other as BitAccessor);
            }

            public override bool Equals(object other)
            {
                return Equals(other as BitAccessor);
            }

            public override int GetHashCode()
            {
                return Position << 1 ^ Value.GetHashCode();
            }
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
     
        /// <summary>
        /// Determines whether the the current object is equal to another.
        /// </summary>
        /// <param name="right">Other object to be compared.</param>
        /// <param name="state">Internal interpreter state.</param>
        /// <returns>The comparison result.</returns>
        protected override IScriptObject Equals(IScriptObject right, InterpreterState state)
        {
            if (right.OneOf<ScriptBoolean, ScriptInteger>())
                return Equals(Convert(right), state);
            else if (right is ScriptReal)
                return Equals((ScriptReal)right, state);
            else if (IsVoid(right))
                return Equals(Zero, state);
            else if (state.Context == InterpretationContext.Unchecked)
                return ScriptObject.Void;
            else throw new UnsupportedOperationException(state);
        }

        private ScriptBoolean Equals(double right, InterpreterState state)
        {
            return Value == right;
        }

        private ScriptBoolean Equals(long right, InterpreterState state)
        {
            return Value == right;
        }

        /// <summary>
        /// Computes the sum, or union of the current object with the specified.
        /// </summary>
        /// <param name="right">The second operand of the addition operation.</param>
        /// <param name="state">Internal interpretation state.</param>
        /// <returns>The result of the binary operation interpretation.</returns>
        protected override IScriptObject Add(IScriptObject right, InterpreterState state)
        {
            if (right.OneOf<ScriptBoolean, ScriptInteger>())
                return Add(Convert(right), state);
            else if (right is ScriptReal)
                return Add((ScriptReal)right, state);
            else if (IsVoid(right))
                return Add(Zero, state);
            else if (state.Context == InterpretationContext.Unchecked)
                return ScriptObject.Void;
            else throw new UnsupportedOperationException(state);
        }

        internal static ScriptInteger Add(ScriptInteger left, ScriptInteger right, InterpreterState state)
        {
            return left.Add(right, state);
        }

        private ScriptReal Add(double right, InterpreterState state)
        {
            return state.Context == InterpretationContext.Unchecked ? unchecked(Value + right) : checked(Value + right); 
        }

        private ScriptInteger Add(long right, InterpreterState state)
        {
            return state.Context == InterpretationContext.Unchecked ? unchecked(Value + right) : checked(Value + right);
        }

        /// <summary>
        /// Subtracts the current integer from the specified.
        /// </summary>
        /// <param name="right"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        protected override IScriptObject Subtract(IScriptObject right, InterpreterState state)
        {
            if (right.OneOf<ScriptBoolean, ScriptInteger>())
                return Subtract(Convert(right), state);
            else if (right is ScriptReal)
                return Subtract((ScriptReal)right, state);
            else if (IsVoid(right))
                return Subtract(Zero, state);
            else if (state.Context == InterpretationContext.Unchecked)
                return ScriptObject.Void;
            else throw new UnsupportedOperationException(state);
        }

        private ScriptReal Subtract(double right, InterpreterState state)
        {
            return state.Context == InterpretationContext.Unchecked ? unchecked(Value - right) : checked(Value - right);
        }

        private ScriptInteger Subtract(long right, InterpreterState state)
        {
            return state.Context == InterpretationContext.Unchecked ? unchecked(Value - right) : checked(Value - right);
        }

        /// <summary>
        /// Performs prefixed increment on the object.
        /// </summary>
        /// <param name="state">Internal interpreter state.</param>
        /// <returns>The incremented object.</returns>
        protected override IScriptObject PreIncrementAssign(InterpreterState state)
        {
            return new ScriptInteger(state.Context == InterpretationContext.Unchecked ? unchecked(Value + 1) : checked(Value + 1));
        }

        /// <summary>
        /// Performs postfixed increment on the object.
        /// </summary>
        /// <param name="state">Internal interpreter state.</param>
        /// <returns>The incremented object.</returns>
        protected override IScriptObject PostIncrementAssign(InterpreterState state)
        {
            return PreIncrementAssign(state);
        }

        /// <summary>
        /// Determines whether the current object is less than the specified object.
        /// </summary>
        /// <param name="right">The second object to compare.</param>
        /// <param name="state">Internal interpreter state.</param>
        /// <returns><see langword="true"/>the current object is less than the specified object; otherwise, <see langword="false"/>.</returns>
        protected override IScriptObject LessThan(IScriptObject right, InterpreterState state)
        {
            if (right.OneOf<ScriptBoolean, ScriptInteger>())
                return LessThan(Convert(right), state);
            else if (right is ScriptReal)
                return LessThan((ScriptReal)right, state);
            else if (IsVoid(right))
                return LessThan(Zero, state);
            else if (state.Context == InterpretationContext.Unchecked)
                return ScriptObject.Void;
            else throw new UnsupportedOperationException(state);
        }

        private ScriptBoolean LessThan(double right, InterpreterState state)
        {
            return Value < right;
        }

        private ScriptBoolean LessThan(long right, InterpreterState state)
        {
            return Value < right;
        }

        /// <summary>
        /// Determines whether the current object is greater than the specified object.
        /// </summary>
        /// <param name="right">The second object to compare.</param>
        /// <param name="state">Internal interpreter state.</param>
        /// <returns><see langword="true"/>the current object is greater than the specified object; otherwise, <see langword="false"/>.</returns>
        protected override IScriptObject GreaterThan(IScriptObject right, InterpreterState state)
        {
            if (right.OneOf<ScriptBoolean, ScriptInteger>())
                return GreaterThan(Convert(right), state);
            else if (right is ScriptReal)
                return GreaterThan((ScriptReal)right, state);
            else if (IsVoid(right))
                return GreaterThan(Zero, state);
            else if (state.Context == InterpretationContext.Unchecked)
                return ScriptObject.Void;
            else throw new UnsupportedOperationException(state);
        }

        private ScriptBoolean GreaterThan(double right, InterpreterState state)
        {
            return Value > right;
        }

        private ScriptBoolean GreaterThan(long right, InterpreterState state)
        {
            return Value > right;
        }

        /// <summary>
        /// Computes the remainder after dividing the current object by the second.
        /// </summary>
        /// <param name="right">The right operand.</param>
        /// <param name="state">Internal interpreter state.</param>
        /// <returns>The remainder.</returns>
        protected override IScriptObject Modulo(IScriptObject right, InterpreterState state)
        {
            if (right.OneOf<ScriptBoolean, ScriptInteger>())
                return Modulo(Convert(right), state);
            else if (right is ScriptReal)
                return Modulo((ScriptReal)right, state);
            else if (IsVoid(right))
                return Modulo(Zero, state);
            else if (state.Context == InterpretationContext.Unchecked)
                return ScriptObject.Void;
            else throw new UnsupportedOperationException(state);   
        }

        private ScriptReal Modulo(double right, InterpreterState state)
        {
            return Value % right;
        }

        private ScriptInteger Modulo(long right, InterpreterState state)
        {
            return Value % right;
        }

        /// <summary>
        /// Computes multiplication between the current object and the specified object.
        /// </summary>
        /// <param name="right">The second operand.</param>
        /// <param name="state">Internal interpreter state.</param>
        /// <returns>The multiplication of the two objects.</returns>
        protected override IScriptObject Multiply(IScriptObject right, InterpreterState state)
        {
            if (right.OneOf<ScriptBoolean, ScriptInteger>())
                return Multiply(Convert(right), state);
            else if (right is ScriptReal)
                return Multiply((ScriptReal)right, state);
            else if (IsVoid(right))
                return Multiply(Zero, state);
            else if (state.Context == InterpretationContext.Unchecked)
                return ScriptObject.Void;
            else throw new UnsupportedOperationException(state);   
        }

        private ScriptReal Multiply(double right, InterpreterState state)
        {
            return state.Context == InterpretationContext.Unchecked ? unchecked(Value * right) : checked(Value * right); 
        }

        private ScriptInteger Multiply(long right, InterpreterState state)
        {
            return state.Context == InterpretationContext.Unchecked ? unchecked(Value * right) : checked(Value * right);
        }

        /// <summary>
        /// Computies logical and, bitwise and, or intersection.
        /// </summary>
        /// <param name="right">The right operand.</param>
        /// <param name="state">Internal interpreter state.</param>
        /// <returns>The result of the binary operation.</returns>
        protected override IScriptObject And(IScriptObject right, InterpreterState state)
        {
            if (right.OneOf<ScriptBoolean, ScriptInteger>())
                return And(Convert(right), state);
            else if (IsVoid(right))
                return And(Zero, state);
            else if (state.Context == InterpretationContext.Unchecked)
                return ScriptObject.Void;
            else throw new UnsupportedOperationException(state);   
        }

        private ScriptInteger And(long right, InterpreterState state)
        {
            return Value & right;
        }

        /// <summary>
        /// Computes logical or, bitwise or, or union.
        /// </summary>
        /// <param name="right">The right operand.</param>
        /// <param name="state">Internal interpreter state.</param>
        /// <returns>The result of the binary operation.</returns>
        protected override IScriptObject Or(IScriptObject right, InterpreterState state)
        {
            if (right.OneOf<ScriptBoolean, ScriptInteger>())
                return Or(Convert(right), state);
            else if (IsVoid(right))
                return Or(Zero, state);
            else if (state.Context == InterpretationContext.Unchecked)
                return ScriptObject.Void;
            else throw new UnsupportedOperationException(state); 
        }

        private ScriptInteger Or(long right, InterpreterState state)
        {
            return Value | right;
        }

        /// <summary>
        /// Computes exclusive or, or difference between two objects.
        /// </summary>
        /// <param name="right">The second operand.</param>
        /// <param name="state">Internal interpreter state.</param>
        /// <returns>The computation result.</returns>
        protected override IScriptObject ExclusiveOr(IScriptObject right, InterpreterState state)
        {
            if (right.OneOf<ScriptBoolean, ScriptInteger>())
                return ExclusiveOr(Convert(right), state);
            else if (IsVoid(right))
                return ExclusiveOr(Zero, state);
            else if (state.Context == InterpretationContext.Unchecked)
                return ScriptObject.Void;
            else throw new UnsupportedOperationException(state); 
        }

        private ScriptInteger ExclusiveOr(long right, InterpreterState state)
        {
            return Value ^ right;
        }

        /// <summary>
        /// Divides the current object using the specified.
        /// </summary>
        /// <param name="right">The right operand of the division operator.</param>
        /// <param name="state">Internal interpreter state.</param>
        /// <returns>The division result.</returns>
        protected override IScriptObject Divide(IScriptObject right, InterpreterState state)
        {
            if (right.OneOf<ScriptBoolean, ScriptInteger>())
                return Divide(Convert(right), state);
            else if (right is ScriptReal)
                return Divide((ScriptReal)right, state);
            else if (IsVoid(right))
                return Divide(Zero, state);
            else if (state.Context == InterpretationContext.Unchecked)
                return ScriptObject.Void;
            else throw new UnsupportedOperationException(state); 
        }

        private ScriptReal Divide(double right, InterpreterState state)
        {
            return Value / right;
        }

        private ScriptInteger Divide(long right, InterpreterState state)
        {
            return Value / right;
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
        /// Applies bitwise complement or logicat negation.
        /// </summary>
        /// <param name="state">Internal interpreter state.</param>
        /// <returns>The operation result.</returns>
        protected override IScriptObject Not(InterpreterState state)
        {
            return new ScriptInteger(~Value);
        }

        /// <summary>
        /// Determines whether the current object is not equal to another.
        /// </summary>
        /// <param name="right">The second operand.</param>
        /// <param name="state">Internal interpreter state.</param>
        /// <returns>The comparison result.</returns>
        protected override IScriptObject NotEquals(IScriptObject right, InterpreterState state)
        {
            if (right.OneOf<ScriptBoolean, ScriptInteger>())
                return NotEquals(Convert(right), state);
            else if (right is ScriptReal)
                return NotEquals((ScriptReal)right, state);
            else if (IsVoid(right))
                return NotEquals(Zero, state);
            else if (state.Context == InterpretationContext.Unchecked)
                return ScriptObject.Void;
            else throw new UnsupportedOperationException(state); 
        }

        private ScriptBoolean NotEquals(double right, InterpreterState state)
        {
            return !Equals(right, state);
        }

        private ScriptBoolean NotEquals(long right, InterpreterState state)
        {
            return !Equals(right, state);
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
        /// Determines whether the current object is greater than or equal to the specified object.
        /// </summary>
        /// <param name="right">The second object to compare.</param>
        /// <param name="state">Internal interpreter state.</param>
        /// <returns><see langword="true"/>the current object is greater than or equal to the specified object; otherwise, <see langword="false"/>.</returns>
        protected override IScriptObject GreaterThanOrEqual(IScriptObject right, InterpreterState state)
        {
            if (right.OneOf<ScriptBoolean, ScriptInteger>())
                return GreaterThanOrEqual(Convert(right), state);
            else if (right is ScriptReal)
                return GreaterThanOrEqual((ScriptReal)right, state);
            else if (IsVoid(right))
                return GreaterThanOrEqual(Zero, state);
            else if (state.Context == InterpretationContext.Unchecked)
                return ScriptObject.Void;
            else throw new UnsupportedOperationException(state); 
        }

        private ScriptBoolean GreaterThanOrEqual(double right, InterpreterState state)
        {
            return Value >= right;
        }

        private ScriptBoolean GreaterThanOrEqual(long right, InterpreterState state)
        {
            return Value >= right;
        }

        /// <summary>
        /// Determines whether the current object is less than or equal to the specified object.
        /// </summary>
        /// <param name="right">The second object to compare.</param>
        /// <param name="state">Internal interpreter state.</param>
        /// <returns><see langword="true"/>the current object is less than or equal to the specified object; otherwise, <see langword="false"/>.</returns>
        protected override IScriptObject LessThanOrEqual(IScriptObject right, InterpreterState state)
        {
            if (right.OneOf<ScriptBoolean, ScriptInteger>())
                return LessThanOrEqual(Convert(right), state);
            else if (right is ScriptReal)
                return LessThanOrEqual((ScriptReal)right, state);
            else if (IsVoid(right))
                return LessThanOrEqual(Zero, state);
            else if (state.Context == InterpretationContext.Unchecked)
                return ScriptObject.Void;
            else throw new UnsupportedOperationException(state); 
        }

        private ScriptBoolean LessThanOrEqual(double right, InterpreterState state)
        {
            return Value <= right; 
        }

        private ScriptBoolean LessThanOrEqual(long right, InterpreterState state)
        {
            return Value <= right;
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
        /// Gets bit accessor.
        /// </summary>
        /// <param name="bitpos"></param>
        /// <returns></returns>
        public RuntimeSlotBase this[int bitpos]
        {
            get { return new BitAccessor(Value, bitpos); }
        }

        private RuntimeSlotBase this[IScriptObject bitpos, InterpreterState state]
        {
            get
            {
                switch (ScriptIntegerContract.Convert(ref bitpos))
                {
                    case true:
                        return this[SystemConverter.ToInt32(bitpos)];
                    default:
                        throw new ContractBindingException(bitpos, ScriptIntegerContract.Instance, state);
                }
            }
        }

        /// <summary>
        /// Gets bitwise accessor.
        /// </summary>
        /// <param name="args"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        public override RuntimeSlotBase this[IScriptObject[] args, InterpreterState state]
        {
            get
            {
                switch (args.LongLength)
                {
                    case 1L:
                        return this[args[0], state];
                    default:
                        throw new ActionArgumentsMistmatchException(state);
                }
            }
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
    }
}
