using System;
using System.Dynamic;
using System.Linq.Expressions;
using System.Runtime.Serialization;

namespace DynamicScript.Runtime.Environment
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;
    using SystemConverter = System.Convert;
    using InterpretationContext = Compiler.Ast.InterpretationContext;

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
        public static readonly ScriptReal NaN = double.NaN;

        /// <summary>
        /// Represents largest possible real.
        /// </summary>
        public static readonly ScriptReal MaxValue = double.MaxValue;

        /// <summary>
        /// Represents smallest possible real.
        /// </summary>
        public static readonly ScriptReal MinValue = double.MinValue;

        /// <summary>
        /// Represents the smallest positive real.
        /// </summary>
        public static readonly ScriptReal Epsilon = double.Epsilon;

        /// <summary>
        /// Represents positive infinity.
        /// </summary>
        public static readonly ScriptReal PositiveInfinity = double.PositiveInfinity;

        /// <summary>
        /// Represents negative infinity.
        /// </summary>
        public static readonly ScriptReal NegativeInfinity = double.NegativeInfinity;

        /// <summary>
        /// Provides conversion from <see cref="System.Double"/> object to DynamicScript-compliant representation.
        /// </summary>
        /// <param name="value">Real value to be converted.</param>
        /// <returns>DynamicScript-compliant representation of <see cref="System.Double"/> object.</returns>
        public static implicit operator ScriptReal(double value)
        {
            if (value == double.NaN)
                return NaN;
            else if (double.IsPositiveInfinity(value))
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

        /// <summary>
        /// Computes the sum, or union of the current object with the specified.
        /// </summary>
        /// <param name="right">The second operand of the addition operation.</param>
        /// <param name="state">Internal interpretation state.</param>
        /// <returns>The result of the binary operation interpretation.</returns>
        protected override IScriptObject Add(IScriptObject right, InterpreterState state)
        {
            if (right.OneOf<ScriptBoolean, ScriptInteger, ScriptReal>())
                return Add(Convert(right));
            else if (IsVoid(right))
                return Add(Zero);
            else if (state.Context == InterpretationContext.Unchecked)
                return ScriptObject.Void;
            else throw new UnsupportedOperationException(state);
        }

        private ScriptReal Add(double right)
        {
            return Value + right;
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
        /// Divides the current object using the specified.
        /// </summary>
        /// <param name="right">The right operand of the division operator.</param>
        /// <param name="state">Internal interpreter state.</param>
        /// <returns>The division result.</returns>
        protected override IScriptObject Divide(IScriptObject right, InterpreterState state)
        {
            if (right.OneOf<ScriptBoolean, ScriptInteger, ScriptReal>())
                return Divide(Convert(right), state);
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

        /// <summary>
        /// Determines whether the the current object is equal to another.
        /// </summary>
        /// <param name="right">Other object to be compared.</param>
        /// <param name="state">Internal interpreter state.</param>
        /// <returns>The comparison result.</returns>
        protected override IScriptObject Equals(IScriptObject right, InterpreterState state)
        {
            if (right.OneOf<ScriptBoolean, ScriptInteger, ScriptReal>())
                return Equals(Convert(right), state);
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

        /// <summary>
        /// Determines whether the current object is not equal to another.
        /// </summary>
        /// <param name="right">The second operand.</param>
        /// <param name="state">Internal interpreter state.</param>
        /// <returns>The comparison result.</returns>
        /// <remarks>This method is implemented by default.</remarks>
        protected override IScriptObject NotEquals(IScriptObject right, InterpreterState state)
        {
            if (right.OneOf<ScriptBoolean, ScriptInteger, ScriptReal>())
                return NotEquals(Convert(right), state);
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
                return GreaterThan(Zero, state);
            else if (state.Context == InterpretationContext.Unchecked)
                return ScriptObject.Void;
            else throw new UnsupportedOperationException(state);
        }

        private ScriptBoolean GreaterThan(double right, InterpreterState state)
        {
            return Value > right;
        }

        /// <summary>
        /// Determines whether the current object is greater than or equal to the specified object.
        /// </summary>
        /// <param name="right">The second object to compare.</param>
        /// <param name="state">Internal interpreter state.</param>
        /// <returns><see langword="true"/>the current object is greater than or equal to the specified object; otherwise, <see langword="false"/>.</returns>
        protected override IScriptObject GreaterThanOrEqual(IScriptObject right, InterpreterState state)
        {
            if (right.OneOf<ScriptBoolean, ScriptInteger, ScriptReal>())
                return GreaterThanOrEqual(Convert(right), state);
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

        /// <summary>
        /// Determines whether the current object is less than the specified object.
        /// </summary>
        /// <param name="right">The second object to compare.</param>
        /// <param name="state">Internal interpreter state.</param>
        /// <returns><see langword="true"/>the current object is less than the specified object; otherwise, <see langword="false"/>.</returns>
        protected override IScriptObject LessThan(IScriptObject right, InterpreterState state)
        {
            if (right.OneOf<ScriptBoolean, ScriptInteger, ScriptReal>())
                return LessThan(Convert(right), state);
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

        /// <summary>
        /// Determines whether the current object is less than or equal to the specified object.
        /// </summary>
        /// <param name="right">The second object to compare.</param>
        /// <param name="state">Internal interpreter state.</param>
        /// <returns><see langword="true"/>the current object is less than or equal to the specified object; otherwise, <see langword="false"/>.</returns>
        protected override IScriptObject LessThanOrEqual(IScriptObject right, InterpreterState state)
        {
            if (right.OneOf<ScriptBoolean, ScriptInteger, ScriptReal>())
                return LessThanOrEqual(Convert(right), state);
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

        /// <summary>
        /// Computes multiplication between the current object and the specified object.
        /// </summary>
        /// <param name="right">The second operand.</param>
        /// <param name="state">Internal interpreter state.</param>
        /// <returns>The multiplication of the two objects.</returns>
        protected override IScriptObject Multiply(IScriptObject right, InterpreterState state)
        {
            if (right.OneOf<ScriptBoolean, ScriptInteger, ScriptReal>())
                return Multiply(Convert(right));
            else if (IsVoid(right))
                return Multiply(Zero);
            else if (state.Context == InterpretationContext.Unchecked)
                return ScriptObject.Void;
            else throw new UnsupportedOperationException(state);
        }

        private ScriptReal Multiply(double right)
        {
            return Value * right;
        }

        /// <summary>
        /// Computes subtraction between the current object and the specified object.
        /// </summary>
        /// <param name="right">The subtrahend.</param>
        /// <param name="state">Internal interpreter result.</param>
        /// <returns>The subtraction result.</returns>
        protected override IScriptObject Subtract(IScriptObject right, InterpreterState state)
        {
            if (right.OneOf<ScriptBoolean, ScriptInteger, ScriptReal>())
                return Subtract(Convert(right));
            else if (IsVoid(right))
                return Subtract(Zero);
            else if (state.Context == InterpretationContext.Unchecked)
                return ScriptObject.Void;
            else throw new UnsupportedOperationException(state);
        }

        private ScriptReal Subtract(double right)
        {
            return Value - right;
        }

        /// <summary>
        /// Computes the remainder after dividing the current object by the second.
        /// </summary>
        /// <param name="right">The right operand.</param>
        /// <param name="state">Internal interpreter state.</param>
        /// <returns>The remainder.</returns>
        protected override IScriptObject Modulo(IScriptObject right, InterpreterState state)
        {
            if (right.OneOf<ScriptBoolean, ScriptInteger, ScriptReal>())
                return Modulo(Convert(right), state);
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
    }
}
