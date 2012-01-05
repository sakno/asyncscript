using System;
using System.Linq.Expressions;
using System.Runtime.Serialization;
using System.Collections.Generic;

namespace DynamicScript.Runtime.Environment
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;
    using SystemConverter = System.Convert;
    using InterpretationContext = Compiler.Ast.InterpretationContext;
    using Parallel = System.Threading.Tasks.Parallel;
    using StringBuilder = System.Text.StringBuilder;
    using SystemMath = System.Math;
    using Enumerable = System.Linq.Enumerable;

    /// <summary>
    /// Represents string object.
    /// This class cannot be inherited.
    /// </summary>
    [CLSCompliant(false)]
    [ComVisible(false)]
    [Serializable]
    public sealed class ScriptString : ScriptConvertibleObject<ScriptStringContract, string>, IScriptContainer
    {
        #region Nested Types
        [ComVisible(false)]
        internal sealed class StringConverter : RuntimeConverter<string>
        {
            public override bool Convert(string input, out IScriptObject result)
            {
                result = new ScriptString(input);
                return true;
            }
        }
        #endregion

        private static readonly AggregatedSlotCollection<ScriptString> StaticSlots = new AggregatedSlotCollection<ScriptString>
        {
            {"length", (owner, state) => new ScriptInteger(owner.Length), ScriptIntegerContract.Instance}
        };

        private ScriptString(SerializationInfo info, StreamingContext context)
            : this(Deserialize(info))
        {
        }

        /// <summary>
        /// Initializes a new DynamicScript-compliant string.
        /// </summary>
        /// <param name="value">An instance of <see cref="System.String"/> object that represent content of the string object. Cannot be <see langword="null"/>.</param>
        /// <exception cref="System.ArgumentNullException"><paramref name="value"/> is <see langword="null"/>.</exception>
        public ScriptString(string value)
            : base(ScriptStringContract.Instance, value ?? string.Empty)
        {
        }

        internal ScriptString(StringBuilder builder)
            : this(builder != null ? builder.ToString() : string.Empty)
        {
        }

        /// <summary>
        /// Initializes a new script string.
        /// </summary>
        /// <param name="c"></param>
        /// <param name="count"></param>
        public ScriptString(char c, int count = 1)
            : this(new string(c, count))
        {
        }

        /// <summary>
        /// Gets a value indicating that this string is empty.
        /// </summary>
        public bool IsEmpty
        {
            get { return Value.Length == 0; }
        }

        /// <summary>
        /// Represents an empty string.
        /// </summary>
        public static readonly ScriptString Empty = new ScriptString(String.Empty);

        /// <summary>
        /// Represents white space.
        /// </summary>
        public static readonly ScriptString WhiteSpace = new ScriptString(" ");

        internal static Expression New(string value)
        {
            if (value == null || value.Length == 0)
                return LinqHelpers.BodyOf<Func<ScriptString>, MemberExpression>(() => Empty);
            else if (string.Equals(value, " "))
                return LinqHelpers.BodyOf<Func<ScriptString>, MemberExpression>(() => WhiteSpace);
            else return LinqHelpers.Convert<ScriptString, string>(value);
        }

        /// <summary>
        /// Provides conversion from <see cref="System.String"/> object to DynamicScript-compliant representation.
        /// </summary>
        /// <param name="value">String value to be converted.</param>
        /// <returns>DynamicScript-compliant representation of <see cref="System.String"/> object.</returns>
        public static implicit operator ScriptString(string value)
        {
            return string.IsNullOrEmpty(value) ? Empty : new ScriptString(value);
        }

        /// <summary>
        /// Gets length of the string.
        /// </summary>
        public int Length
        {
            get { return Value.Length; }
        }

        /// <summary>
        /// Gets a character at the specified position in the string.
        /// </summary>
        /// <param name="index">Zero-based position of the character.</param>
        /// <returns></returns>
        public char this[int index]
        {
            get { return Value[index]; }
        }

        /// <summary>
        /// Gets a string that represents a single characted at the specified position.
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
                    var index = indicies[0] as ScriptInteger;
                    if (index != null && index.IsInt32) return new ScriptString(this[(int)index]);
                    else throw new UnsupportedOperationException(state);
                }
                return base[indicies, state];
            }
            set { base[indicies, state] = value; }
        }

        /// <summary>
        /// Computes the sum, or union of the current object with the specified.
        /// </summary>
        /// <param name="right">The second operand of the addition operation.</param>
        /// <param name="state">Internal interpretation state.</param>
        /// <returns>The result of the binary operation interpretation.</returns>
        protected override IScriptObject Add(IScriptObject right, InterpreterState state)
        {
            if (right.OneOf<ScriptBoolean, ScriptInteger, ScriptReal, ScriptString>())
                return Add(Convert(right), state);
            else if (IsVoid(right))
                return Add(Empty, state);
            else if (state.Context == InterpretationContext.Unchecked)
                return ContractBinding.FromVoid(state);
            else throw new UnsupportedOperationException(state);
        }

        private ScriptString Add(string right, InterpreterState state)
        {
            return Value + right;
        }

        private ScriptString Subtract(string right, InterpreterState state)
        {
            return Value.Replace(right, string.Empty);
        }

        /// <summary>
        /// Returns a new string in which all occurences of a specified string in the current instance
        /// are replaced.
        /// </summary>
        /// <param name="right"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        protected override IScriptObject Subtract(IScriptObject right, InterpreterState state)
        {
            if (right.OneOf<ScriptBoolean, ScriptInteger, ScriptReal, ScriptString>())
                return Subtract(Convert(right), state);
            else if (IsVoid(right))
                return Subtract(Empty, state);
            else if (state.Context == InterpretationContext.Unchecked)
                return ContractBinding.FromVoid(state);
            else throw new UnsupportedOperationException(state);
        }

        private ScriptBoolean Equals(string right, InterpreterState state)
        {
            return Value.Equals(right, StringComparison.Ordinal);
        }

        /// <summary>
        /// Determines whether the two strings are equal.
        /// </summary>
        /// <param name="right"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        protected override IScriptObject Equals(IScriptObject right, InterpreterState state)
        {
            if (right is ScriptString)
                return Equals((ScriptString)right, state);
            else if (IsVoid(right))
                return Equals(Empty, state);
            if (state.Context == InterpretationContext.Unchecked)
                return ContractBinding.FromVoid(state);
            else throw new UnsupportedOperationException(state);
        }

        private ScriptBoolean NotEquals(string right, InterpreterState state)
        {
            return !Equals(right, state);
        }

        /// <summary>
        /// Determines whether this string is not equal to another.
        /// </summary>
        /// <param name="right"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        protected override IScriptObject NotEquals(IScriptObject right, InterpreterState state)
        {
            if (right is ScriptString)
                return NotEquals((ScriptString)right, state);
            else if (IsVoid(right))
                return NotEquals(Empty, state);
            if (state.Context == InterpretationContext.Unchecked)
                return ContractBinding.FromVoid(state);
            else throw new UnsupportedOperationException(state);
        }

        private ScriptString Multiply(long right, InterpreterState state)
        {
            switch (right > 0)
            {
                case true:
                    var result = new string[right];
                    Parallel.For(0, right, idx => result[idx] = Value);
                    return string.Concat(result);
                default: return this;
            }
        }

        /// <summary>
        /// Repeates this string the specified amount of times.
        /// </summary>
        /// <param name="right"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        protected override IScriptObject Multiply(IScriptObject right, InterpreterState state)
        {
            if (ScriptIntegerContract.Convert(ref right))
                return Multiply((ScriptInteger)right, state);
            else if (IsVoid(right))
                return Multiply(ScriptInteger.Zero, state);
            if (state.Context == InterpretationContext.Unchecked)
                return ContractBinding.FromVoid(state);
            else throw new UnsupportedOperationException(state);
        }

        private static IEnumerable<ScriptString> Split(string value, long count)
        {
            var iterations = (long)SystemMath.Ceiling((double)value.Length / count);
            var result = new StringBuilder(value.Length + 1);
            for (var i = 1; i <= value.Length; i++)
            {
                result.Append(value[i - 1]);
                if (i % iterations == 0 || i==value.Length)
                {
                    yield return new ScriptString(result.ToString());
                    result.Clear();
                }
            }
        }

        private ScriptArray Divide(long right, InterpreterState state)
        {
            var strings = Enumerable.ToArray(Split(Value, right));
            return strings.LongLength > 0L ? new ScriptArray(strings) : ScriptArray.Empty(ScriptStringContract.Instance);
        }

        /// <summary>
        /// Divides this string on the specified count of parts.
        /// </summary>
        /// <param name="right"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        protected override IScriptObject Divide(IScriptObject right, InterpreterState state)
        {
            if (ScriptIntegerContract.Convert(ref right))
                return Divide((ScriptInteger)right, state);
            else if (IsVoid(right))
                return Divide(ScriptInteger.Zero, state);
            if (state.Context == InterpretationContext.Unchecked)
                return ContractBinding.FromVoid(state);
            else throw new UnsupportedOperationException(state);
        }

        private ScriptBoolean GreaterThanOrEqual(string right, InterpreterState state)
        {
            return Value.CompareTo(right) >= 0;
        }

        /// <summary>
        /// Determines whether this string is greater than or equal to another string.
        /// </summary>
        /// <param name="right"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        protected override IScriptObject GreaterThanOrEqual(IScriptObject right, InterpreterState state)
        {
            if (right is ScriptString)
                return GreaterThanOrEqual((ScriptString)right, state);
            else if (IsVoid(right))
                return GreaterThanOrEqual(Empty, state);
            if (state.Context == InterpretationContext.Unchecked)
                return ContractBinding.FromVoid(state);
            else throw new UnsupportedOperationException(state);
        }

        private ScriptBoolean GreaterThan(string right, InterpreterState state)
        {
            return Value.CompareTo(right) > 0;
        }

        /// <summary>
        /// Determines whether this string is greater than another string.
        /// </summary>
        /// <param name="right"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        protected override IScriptObject GreaterThan(IScriptObject right, InterpreterState state)
        {
            if (right is ScriptString)
                return GreaterThan((ScriptString)right, state);
            else if (IsVoid(right))
                return GreaterThan(Empty, state);
            if (state.Context == InterpretationContext.Unchecked)
                return ContractBinding.FromVoid(state);
            else throw new UnsupportedOperationException(state);
        }

        private ScriptBoolean LessThan(string right, InterpreterState state)
        {
            return Value.CompareTo(right) < 0;
        }

        /// <summary>
        /// Determines whether this string is less than another string.
        /// </summary>
        /// <param name="right"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        protected override IScriptObject LessThan(IScriptObject right, InterpreterState state)
        {
            if (right is ScriptString)
                return LessThan((ScriptString)right, state);
            else if (IsVoid(right))
                return LessThan(Empty, state);
            if (state.Context == InterpretationContext.Unchecked)
                return ContractBinding.FromVoid(state);
            else throw new UnsupportedOperationException(state);
        }

        private ScriptBoolean LessThanOrEqual(string right, InterpreterState state)
        {
            return Value.CompareTo(right) <= 0;
        }

        /// <summary>
        /// Determines whether this string is less than or equal to another string.
        /// </summary>
        /// <param name="right"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        protected override IScriptObject LessThanOrEqual(IScriptObject right, InterpreterState state)
        {
            if (right is ScriptString)
                return LessThanOrEqual((ScriptString)right, state);
            else if (IsVoid(right))
                return LessThanOrEqual(Empty, state);
            if (state.Context == InterpretationContext.Unchecked)
                return ContractBinding.FromVoid(state);
            else throw new UnsupportedOperationException(state);
        }

        internal override ScriptObject Intern(InterpreterState state)
        {
            var obj = new ScriptString(string.Intern(Value));
            state.Intern(obj);
            return obj;
        }

        /// <summary>
        /// Determines whether this string
        /// contains the specified substring.
        /// </summary>
        /// <param name="str">The substring to check.</param>
        /// <returns></returns>
        public bool Contains(ScriptString str)
        {
            return str != null ? Value.Contains(str) : false;
        }

        bool IScriptContainer.Contains(IScriptObject obj, bool byref, InterpreterState state)
        {
            return byref ? false : Contains(obj as ScriptString);
        }

        private IEnumerator<IScriptObject> GetEnumerator()
        {
            switch (Value.Length)
            {
                case 0: yield break;
                case 1: yield return this; break;
                default:
                    foreach (var c in Value)
                        yield return new ScriptString(c);
                    break;
            }
        }

        IEnumerator<IScriptObject> IEnumerable<IScriptObject>.GetEnumerator()
        {
            return GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// Gets collection of aggregated slots.
        /// </summary>
        public override ICollection<string> Slots
        {
            get { return StaticSlots.Keys; }
        }

        /// <summary>
        /// Gets or sets value of the aggregated object.
        /// </summary>
        /// <param name="slotName"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        public override IScriptObject this[string slotName, InterpreterState state]
        {
            get { return StaticSlots.GetValue(this, slotName, state); }
            set { StaticSlots.SetValue(this, slotName, value, state); }
        }

        /// <summary>
        /// Returns metadata of the 
        /// </summary>
        /// <param name="slotName"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        protected override IScriptObject GetSlotMetadata(string slotName, InterpreterState state)
        {
            return StaticSlots.GetSlotMetadata(this, slotName, state);
        }

        /// <summary>
        /// Concatenates a collection of the specified objects.
        /// </summary>
        /// <param name="objects"></param>
        /// <returns></returns>
        public static ScriptString Concat(IEnumerable<IScriptObject> objects)
        {
            return string.Concat(objects);
        }

        /// <summary>
        /// Concatenates two script objects as string.
        /// </summary>
        /// <param name="obj1"></param>
        /// <param name="obj2"></param>
        /// <returns></returns>
        public static ScriptString Concat(IScriptObject obj1, IScriptObject obj2)
        {
            return string.Concat(obj1, obj2);
        }

        internal static MethodCallExpression Concat(Expression obj1, Expression obj2)
        {
            var call = LinqHelpers.BodyOf<IScriptObject, IScriptObject, ScriptString, MethodCallExpression>((o1, o2) => Concat(o1, o2));
            return call.Update(null, new[] { obj1, obj2 });
        }
    }
}
