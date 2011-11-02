using System;
using System.Dynamic;
using System.Linq.Expressions;
using System.Runtime.Serialization;
using System.ComponentModel;

namespace DynamicScript.Runtime.Environment
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;
    using SystemConverter = System.Convert;
    using InterpretationContext = Compiler.Ast.InterpretationContext;

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

        internal static MemberExpression TrueField
        {
            get { return LinqHelpers.BodyOf<Func<ScriptBoolean>, MemberExpression>(() => True); }
        }

        internal static MemberExpression FalseField
        {
            get { return LinqHelpers.BodyOf<Func<ScriptBoolean>, MemberExpression>(() => False); }
        } 

        private static ScriptBoolean Create(bool value)
        {
            return value ? True : False;
        }

        /// <summary>
        /// Provides conversion from <see cref="System.Boolean"/> object to DynamicScript-compliant representation.
        /// </summary>
        /// <param name="value">Boolean value to be converted.</param>
        /// <returns>DynamicScript-compliant representation of <see cref="System.Boolean"/> object.</returns>
        public static implicit operator ScriptBoolean(bool value)
        {
            return Create(value);
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
        /// Computes logical or, bitwise or, or union.
        /// </summary>
        /// <param name="right">The right operand.</param>
        /// <param name="state">Internal interpreter state.</param>
        /// <returns>The result of the binary operation.</returns>
        protected override IScriptObject Or(IScriptObject right, InterpreterState state)
        {
            if (right is ScriptBoolean)
                return Or((ScriptBoolean)right, state);
            else if (right is ScriptInteger)
                return Or((ScriptInteger)right, state);
            else if (IsVoid(right))
                return Or(False, state);
            else if (state.Context == InterpretationContext.Unchecked)
                return ContractBinding.FromVoid(state);
            else throw new UnsupportedOperationException(state);
        }

        private ScriptInteger Or(long right, InterpreterState state)
        {
            return SystemConverter.ToInt64(Value) | right;
        }

        private ScriptBoolean Or(bool right, InterpreterState state)
        {
            return Value | right;
        }

        /// <summary>
        /// Computies logical and, bitwise and, or intersection.
        /// </summary>
        /// <param name="right">The right operand.</param>
        /// <param name="state">Internal interpreter state.</param>
        /// <returns>The result of the binary operation.</returns>
        protected override IScriptObject And(IScriptObject right, InterpreterState state)
        {
            if (right is ScriptBoolean)
                return And((ScriptBoolean)right, state);
            else if (right is ScriptInteger)
                return And((ScriptInteger)right, state);
            else if (IsVoid(right))
                return And(False, state);
            else if (state.Context == InterpretationContext.Unchecked)
                return ContractBinding.FromVoid(state);
            else throw new UnsupportedOperationException(state);
        }

        private ScriptInteger And(long right, InterpreterState state)
        {
            return SystemConverter.ToInt64(Value) & right;
        }

        private ScriptBoolean And(bool right, InterpreterState state)
        {
            return Value & right;
        }

        /// <summary>
        /// Computes exclusive or, or difference between two objects.
        /// </summary>
        /// <param name="right">The second operand.</param>
        /// <param name="state">Internal interpreter state.</param>
        /// <returns>The computation result.</returns>
        protected override IScriptObject ExclusiveOr(IScriptObject right, InterpreterState state)
        {
            if (right is ScriptBoolean)
                return ExclusiveOr((ScriptBoolean)right, state);
            else if (right is ScriptInteger)
                return ExclusiveOr((ScriptInteger)right, state);
            else if (IsVoid(right))
                return ExclusiveOr(False, state);
            else if (state.Context == InterpretationContext.Unchecked)
                return ContractBinding.FromVoid(state);
            else throw new UnsupportedOperationException(state);
        }

        private ScriptInteger ExclusiveOr(long right, InterpreterState state)
        {
            return SystemConverter.ToInt64(Value) ^ right;
        }

        private ScriptBoolean ExclusiveOr(bool right, InterpreterState state)
        {
            return Value ^ right;
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
            else if (IsVoid(right))
                return Equals(False, state);
            else if (state.Context == InterpretationContext.Unchecked)
                return ContractBinding.FromVoid(state);
            else throw new UnsupportedOperationException(state);
        }

        private ScriptBoolean Equals(bool right, InterpreterState state)
        {
            return Value == right;
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
            else if (IsVoid(right))
                return GreaterThan(False, state);
            else if (state.Context == InterpretationContext.Unchecked)
                return ContractBinding.FromVoid(state);
            else throw new UnsupportedOperationException(state);
        }

        private ScriptBoolean GreaterThan(bool right, InterpreterState state)
        {
            return Value.CompareTo(right) > 0;
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
            else if (IsVoid(right))
                return GreaterThanOrEqual(False, state);
            else if (state.Context == InterpretationContext.Unchecked)
                return ContractBinding.FromVoid(state);
            else throw new UnsupportedOperationException(state);
        }

        private ScriptBoolean GreaterThanOrEqual(bool right, InterpreterState state)
        {
            return Value.CompareTo(right) >= 0;
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
            else if (IsVoid(right))
                return LessThan(False, state);
            else if (state.Context == InterpretationContext.Unchecked)
                return ContractBinding.FromVoid(state);
            else throw new UnsupportedOperationException(state);
        }

        private ScriptBoolean LessThan(bool right, InterpreterState state)
        {
            return Value.CompareTo(right) < 0;
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
        /// Determines whether the current object is less than or equal to the specified object.
        /// </summary>
        /// <param name="right">The second object to compare.</param>
        /// <param name="state">Internal interpreter state.</param>
        /// <returns><see langword="true"/>the current object is less than or equal to the specified object; otherwise, <see langword="false"/>.</returns>
        protected override IScriptObject LessThanOrEqual(IScriptObject right, InterpreterState state)
        {
            if (right.OneOf<ScriptBoolean, ScriptInteger>())
                return LessThan(Convert(right), state);
            else if (IsVoid(right))
                return LessThan(False, state);
            else if (state.Context == InterpretationContext.Unchecked)
                return ContractBinding.FromVoid(state);
            else throw new UnsupportedOperationException(state);
        }

        private ScriptBoolean LessThanOrEqual(bool right, InterpreterState state)
        {
            return Value.CompareTo(right) <= 0;
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
            if (right.OneOf<ScriptBoolean, ScriptInteger>())
                return NotEquals(Convert(right), state);
            else if (IsVoid(right))
                return NotEquals(False, state);
            else if (state.Context == InterpretationContext.Unchecked)
                return ContractBinding.FromVoid(state);
            else throw new UnsupportedOperationException(state);
        }

        private ScriptBoolean NotEquals(bool right, InterpreterState state)
        {
            return Value != right;
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
            return this ? True : False;
        }
    }
}
