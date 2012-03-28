using System;
using System.Dynamic;
using System.Linq.Expressions;
using System.Runtime.Serialization;
using System.Collections.Generic;

namespace DynamicScript.Runtime.Environment
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;
    using LinqExpression = System.Linq.Expressions.Expression;
    using SystemConverter = System.Convert;
    using Keyword = Compiler.Keyword;
    using InterpretationContext = Compiler.Ast.InterpretationContext;
    using SystemMath = System.Math;
    using InliningSourceAttribute = Compiler.Ast.Translation.LinqExpressions.InliningSourceAttribute;

    /// <summary>
    /// Represents integer contract.
    /// This class cannot be inherited.
    /// </summary>
    [ComVisible(false)]
    [Serializable]
    [WellKnownContractInfo(ScriptTypeCode.Integer)]
    public sealed class ScriptIntegerContract: ScriptBuiltinContract
    {
        #region Nested Types
        [ComVisible(false)]
        private sealed class IsInternedAction : ScriptFunc<ScriptInteger>
        {
            public const string Name = "isInterned";
            private const string FirstParamName = "int";

            public IsInternedAction()
                : base(FirstParamName, Instance, ScriptBooleanContract.Instance)
            {
            }

            protected override IScriptObject Invoke(ScriptInteger value, InterpreterState state)
            {
                return IsInterned(value, state);
            }
        }

        [ComVisible(false)]
        private sealed class EvenFunction : ScriptFunc<ScriptInteger>
        {
            public const string Name = "even";
            private const string FirstParamName = "int";

            public EvenFunction()
                : base(FirstParamName, Instance, ScriptBooleanContract.Instance)
            {
            }

            protected override IScriptObject Invoke(ScriptInteger arg0, InterpreterState state)
            {
                return Even(arg0, state);
            }
        }

        [ComVisible(false)]
        private sealed class OddFunction : ScriptFunc<ScriptInteger>
        {
            public const string Name = "odd";
            private const string FirstParamName = "int";

            public OddFunction()
                : base(FirstParamName, Instance, ScriptBooleanContract.Instance)
            {
            }

            protected override IScriptObject Invoke(ScriptInteger arg0, InterpreterState state)
            {
                return Odd(arg0, state);
            }
        }

        [ComVisible(false)]
        private sealed class AbsFunction : ScriptFunc<ScriptInteger>
        {
            public const string Name = "abs";
            private const string FirstParamName = "int";

            public AbsFunction()
                : base(FirstParamName, Instance, Instance)
            {
            }

            protected override IScriptObject Invoke(ScriptInteger arg0, InterpreterState state)
            {
                return Abs(arg0, state);
            }
        }

        [ComVisible(false)]
        private sealed class SumFunction : ScriptFunc<IScriptArray>
        {
            public const string Name = "sum";
            private const string FirstParamName = "ints";

            public SumFunction()
                : base(FirstParamName, new ScriptArrayContract(Instance), Instance)
            {
            }

            protected override IScriptObject Invoke(IScriptArray ints, InterpreterState state)
            {
                return Sum(ints, state);
            }
        }

        [ComVisible(false)]
        private sealed class RemFunction : ScriptFunc<IScriptArray>
        {
            public const string Name = "rem";
            private const string FirstParamName = "ints";

            public RemFunction()
                : base(FirstParamName, new ScriptArrayContract(Instance), Instance)
            {
            }

            protected override IScriptObject Invoke(IScriptArray ints, InterpreterState state)
            {
                return Rem(ints, state);
            }
        }
        #endregion

        private readonly AggregatedSlotCollection<ScriptIntegerContract> StaticSlots = new AggregatedSlotCollection<ScriptIntegerContract>()
            {
                {"size", () => new ScriptInteger(sizeof(long))},
                {"max", (owner, state) => ScriptInteger.MaxValue},
                {"min", (owner, state) => ScriptInteger.MinValue},
                {IsInternedAction.Name, (owner, state) => LazyField<IsInternedAction, IScriptFunction>(ref owner.m_interned)},
                {EvenFunction.Name, (owner, state) => LazyField<EvenFunction, IScriptFunction>(ref owner.m_even)},
                {OddFunction.Name, (owner, state) => LazyField<OddFunction, IScriptFunction>(ref owner.m_odd)},
                {AbsFunction.Name, (owner, state) => LazyField<AbsFunction, IScriptFunction>(ref owner.m_abs)},
                {SumFunction.Name, (owner, state) => LazyField<SumFunction, IScriptFunction>(ref owner.m_sum)},
                {RemFunction.Name, (owner, state) => LazyField<RemFunction, IScriptFunction>(ref owner.m_rem)}
            };

        private IScriptFunction m_interned;
        private IScriptFunction m_even;
        private IScriptFunction m_odd;
        private IScriptFunction m_abs;
        private IScriptFunction m_sum;
        private IScriptFunction m_rem;

        private ScriptIntegerContract(SerializationInfo info, StreamingContext context)
            :this()
        {
        }

        private ScriptIntegerContract()
        {
        }

        internal override Keyword Token
        {
            get { return Keyword.Integer; }
        }

        /// <summary>
        /// Represents singleton instance of the DynamicScript integer contract.
        /// </summary>
        public static readonly ScriptIntegerContract Instance = new ScriptIntegerContract();

        /// <summary>
        /// Returns empty value for the contract.
        /// </summary>
        [CLSCompliant(false)]
        public static new ScriptInteger Void
        {
            get { return ScriptInteger.Zero; }
        }

        /// <summary>
        /// Returns an integer default value.
        /// </summary>
        /// <param name="state">Internal interpreter state.</param>
        /// <returns>The integer default value.</returns>
        internal protected override ScriptObject FromVoid(InterpreterState state)
        {
            return Void;
        }

        /// <summary>
        /// Returns underlying contract of this contract.
        /// </summary>
        /// <returns></returns>
        public override IScriptContract GetContractBinding()
        {
            return ScriptFinSetContract.Instance;
        }

        /// <summary>
        /// Provides implicit conversion.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool TryConvert(ref IScriptObject value)
        {
            if (value is ScriptInteger)
                return true;
            else if (value is ScriptBoolean)
            {
                value = new ScriptInteger(SystemConverter.ToInt64(value));
                return true;
            }
            else if (IsVoid(value))
            {
                value = Void;
                return true;
            }
            else return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        public static ScriptInteger TryConvert(IScriptObject value, InterpreterState state)
        {
            if (TryConvert(ref value))
                return (ScriptInteger)value;
            else throw new ContractBindingException(value, Instance, state);
        }

        internal static Expression TryConvert(Expression value, ParameterExpression state)
        {
            return LinqHelpers.BodyOf<IScriptObject, InterpreterState, ScriptInteger, MethodCallExpression>((v, s) => TryConvert(v, s)).Update(null, new[] { value, state });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        protected override bool Mapping(ref IScriptObject value)
        {
            return TryConvert(ref value);
        }

        internal static LinqExpression Expression
        {
            get { return LinqHelpers.BodyOf<Func<ScriptIntegerContract>, MemberExpression>(() => Instance); }
        }

        private static ScriptInteger Convert(ScriptBoolean b)
        {
            return b ? ScriptInteger.One : ScriptInteger.Zero;
        }

        private static ScriptInteger Convert(ScriptReal r)
        {
            return new ScriptInteger((long)r);
        }

        /// <summary>
        /// Provides conversion to the integer-compliant object.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        public override IScriptObject Convert(IScriptObject value, InterpreterState state)
        {
            if(value is ScriptInteger)
                return value;
            if (value is ScriptBoolean)
                return Convert((ScriptBoolean)value);
            else if (value is ScriptReal)
                return Convert((ScriptReal)value);
            else if (IsVoid(value))
                return Void;
            else return new ScriptInteger(value.GetHashCode());
        }

        /// <summary>
        /// Returns relationship between the current contract and the specified.
        /// </summary>
        /// <param name="contract">The contract to compare. Cannot be <see langword="null"/>.</param>
        /// <returns>Relationship between the current contract and <paramref name="contract"/>.</returns>
        public override ContractRelationshipType GetRelationship(IScriptContract contract)
        {
            if (contract is ScriptIntegerContract)
                return ContractRelationshipType.TheSame;
            else if (contract.OneOf<ScriptRealContract, ScriptSuperContract>())
                return ContractRelationshipType.Subset;
            else if (contract.OneOf<ScriptBooleanContract, ScriptVoid>())
                return ContractRelationshipType.Superset;
            else if (contract.OneOf<IScriptComplementation, IScriptUnionContract>())
                return Inverse(contract.GetRelationship(this));
            else return ContractRelationshipType.None;
        }

        /// <summary>
        /// Tries to parse script integer value.
        /// </summary>
        /// <param name="value">The value to parse.</param>
        /// <returns>The parsed integer value; or <see langword="null"/> if parsing fails.</returns>
        [CLSCompliant(false)]
        public static ScriptInteger TryParse(string value)
        {
            switch (Keyword.Void.Equals(value))
            {
                case true: return Void;
                default:
                    var result = default(long);
                    return long.TryParse(value, out result) ? (ScriptInteger)result : null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        [InliningSource]
        public static ScriptBoolean IsInterned(ScriptInteger value, InterpreterState state)
        {
            if (value == null) throw new ContractBindingException(Instance, state);
            return state.IsInterned(value);
        }

        /// <summary>
        /// Determines whether the specified integer is even.
        /// </summary>
        /// <param name="int"></param>
        /// <param name="state">Internal interpreter state.</param>
        /// <returns></returns>
        [InliningSource]
        public static ScriptBoolean Even(ScriptInteger @int, InterpreterState state)
        {
            if (@int == null) throw new ContractBindingException(Instance, state);
            return @int % 2 == 0;
        }

        /// <summary>
        /// Determines whether the specified integer is odd.
        /// </summary>
        /// <param name="int"></param>
        /// <param name="state">Internal interpreter state.</param>
        /// <returns></returns>
        [InliningSource]
        public static ScriptBoolean Odd(ScriptInteger @int, InterpreterState state)
        {
            if (@int == null) throw new ContractBindingException(Instance, state);
            return @int % 2 != 0;
        }

        /// <summary>
        /// Returns absolute value.
        /// </summary>
        /// <param name="int"></param>
        /// <param name="state">Internal interpreter state.</param>
        /// <returns></returns>
        [InliningSource]
        public static ScriptInteger Abs(ScriptInteger @int, InterpreterState state)
        {
            if (@int == null) throw new ContractBindingException(Instance, state);
            return SystemMath.Abs(@int);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ints"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        [InliningSource]
        public static ScriptInteger Sum(IScriptArray ints, InterpreterState state)
        {
            if (ints == null) throw new ContractBindingException(new ScriptArrayContract(Instance), state);
            var result = 0L;
            var context = state.Context;
            var indicies = new long[1];
            for (var i = 0L; i < ints.GetLength(0); i++)
            {
                indicies[0] = i;
                var right = SystemConverter.ToInt64(ints[indicies, state]);
                result = context == InterpretationContext.Unchecked ? unchecked(result + right) : checked(result + right);
            }
            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ints"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        [InliningSource]
        public static ScriptInteger Rem(IScriptArray ints, InterpreterState state)
        {
            if (ints == null) throw new ContractBindingException(new ScriptArrayContract(Instance), state);
            var indicies = new long[1];
            switch (ints.GetLength(0) > 0L)
            {
                case true:
                    var result = SystemConverter.ToInt64(ints[indicies, state]);
                    var context = state.Context;
                    for (var i = 1L; i < ints.GetLength(0); i++)
                    {
                        indicies[0] = i;
                        var right = SystemConverter.ToInt64(ints[indicies, state]);
                        result = context == InterpretationContext.Unchecked ? unchecked(result - right) : checked(result - right);
                    }
                    return result;
                default:
                    return Void;
            }
        }

        /// <summary>
        /// Gets size of this data type, in bytes.
        /// </summary>
        public static ScriptInteger Size
        {
            get { return sizeof(long); }
        }

        /// <summary>
        /// Clears all internal data.
        /// </summary>
        public override void Clear()
        {
            m_abs = m_even = m_interned = m_odd = m_rem = m_sum = null;
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
        /// Gets collection of predefined slots.
        /// </summary>
        public override ICollection<string> Slots
        {
            get { return StaticSlots.Keys; }
        }

        /// <summary>
        /// Returns metadata of the aggregated slot.
        /// </summary>
        /// <param name="slotName"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        protected override IScriptObject GetSlotMetadata(string slotName, InterpreterState state)
        {
            return StaticSlots.GetSlotMetadata(this, slotName, state);
        }
    }
}
