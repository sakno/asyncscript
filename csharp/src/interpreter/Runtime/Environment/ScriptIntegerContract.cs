using System;
using System.Dynamic;
using System.Linq.Expressions;
using System.Runtime.Serialization;

namespace DynamicScript.Runtime.Environment
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;
    using LinqExpression = System.Linq.Expressions.Expression;
    using SystemConverter = System.Convert;
    using Keyword = Compiler.Keyword;
    using InterpretationContext = Compiler.Ast.InterpretationContext;
    using SystemMath = System.Math;

    /// <summary>
    /// Represents integer contract.
    /// This class cannot be inherited.
    /// </summary>
    [ComVisible(false)]
    [Serializable]
    public sealed class ScriptIntegerContract: ScriptBuiltinContract, IIntegerContractSlots
    {
        #region Nested Types
        [ComVisible(false)]
        private sealed class IsInternedAction : ScriptFunc<ScriptInteger>
        {
            private const string FirstParamName = "int";

            public IsInternedAction()
                : base(FirstParamName, Instance, ScriptBooleanContract.Instance)
            {
            }

            protected override IScriptObject Invoke(InvocationContext ctx, ScriptInteger arg0)
            {
                return (ScriptBoolean)IsInterned(ctx, arg0);
            }
        }

        [ComVisible(false)]
        private sealed class EvenAction : ScriptFunc<ScriptInteger>
        {
            private const string FirstParamName = "int";

            public EvenAction()
                : base(FirstParamName, Instance, ScriptBooleanContract.Instance)
            {
            }

            protected override IScriptObject Invoke(InvocationContext ctx, ScriptInteger @int)
            {
                return Even(ctx, @int);
            }
        }

        [ComVisible(false)]
        private sealed class OddAction : ScriptFunc<ScriptInteger>
        {
            private const string FirstParamName = "int";

            public OddAction()
                : base(FirstParamName, Instance, ScriptBooleanContract.Instance)
            {
            }

            protected override IScriptObject Invoke(InvocationContext ctx, ScriptInteger arg0)
            {
                return Odd(ctx, arg0);
            }
        }

        [ComVisible(false)]
        private sealed class AbsAction : ScriptFunc<ScriptInteger>
        {
            private const string FirstParamName = "int";

            public AbsAction()
                : base(FirstParamName, Instance, Instance)
            {
            }

            protected override IScriptObject Invoke(InvocationContext ctx, ScriptInteger arg0)
            {
                return Abs(ctx, arg0);
            }
        }

        [ComVisible(false)]
        private sealed class SumAction : ScriptFunc<IScriptArray>
        {
            private const string FirstParamName = "ints";

            public SumAction()
                : base(FirstParamName, new ScriptArrayContract(Instance), Instance)
            {
            }

            protected override IScriptObject Invoke(InvocationContext ctx, IScriptArray ints)
            {
                if (ints == null) return Void;
                var result = 0L;
                var context = ctx.RuntimeState.Context;
                var indicies = new long[1];
                for (var i = 0L; i < ints.GetLength(0); i++)
                {
                    indicies[0] = i;
                    var right = SystemConverter.ToInt64(ints[indicies, ctx.RuntimeState]);
                    result = context == InterpretationContext.Unchecked ? unchecked(result + right) : checked(result + right);
                }
                return new ScriptInteger(result);
            }
        }

        [ComVisible(false)]
        private sealed class RemAction : ScriptFunc<IScriptArray>
        {
            private const string FirstParamName = "ints";

            public RemAction()
                : base(FirstParamName, new ScriptArrayContract(Instance), Instance)
            {
            }

            protected override IScriptObject Invoke(InvocationContext ctx, IScriptArray ints)
            {
                var indicies = new long[1];
                switch (ints !=null&& ints.GetLength(0)>0L)
                {
                    case true:
                        var result = SystemConverter.ToInt64(ints[indicies, ctx.RuntimeState]);
                        var context = ctx.RuntimeState.Context;
                        for (var i = 1L; i < ints.GetLength(0); i++)
                        {
                            indicies[0] = i;
                            var right = SystemConverter.ToInt64(ints[indicies, ctx.RuntimeState]);
                            result = context == InterpretationContext.Unchecked ? unchecked(result - right) : checked(result - right);
                        }
                        return new ScriptInteger(result);
                    default:
                        return Void;
                }
            }
        }
        #endregion

        private IRuntimeSlot m_size;
        private IRuntimeSlot m_max;
        private IRuntimeSlot m_min;
        private IRuntimeSlot m_even;
        private IRuntimeSlot m_odd;
        private IRuntimeSlot m_abs;
        private IRuntimeSlot m_sum;
        private IRuntimeSlot m_rem;
        private IRuntimeSlot m_isint;

        private ScriptIntegerContract(SerializationInfo info, StreamingContext context)
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
        public static bool Convert(ref IScriptObject value)
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
        /// <returns></returns>
        protected override bool Mapping(ref IScriptObject value)
        {
            return Convert(ref value);
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
                return this;
            if (value is ScriptBoolean)
                return Convert((ScriptBoolean)value);
            else if (value is ScriptReal)
                return Convert((ScriptReal)value);
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
        /// Determines whether the specified integer is even.
        /// </summary>
        /// <param name="ctx">Invocation context.</param>
        /// <param name="int"></param>
        /// <returns></returns>
        public static ScriptBoolean Even(InvocationContext ctx, ScriptInteger @int)
        {
            return @int % 2 == 0;
        }

        /// <summary>
        /// Determines whether the specified integer is odd.
        /// </summary>
        /// <param name="ctx">Invocation context.</param>
        /// <param name="int"></param>
        /// <returns></returns>
        public static ScriptBoolean Odd(InvocationContext ctx, ScriptInteger @int)
        {
            return @int % 2 != 0;
        }

        /// <summary>
        /// Returns absolute value.
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="int"></param>
        /// <returns></returns>
        public static ScriptInteger Abs(InvocationContext ctx, ScriptInteger @int)
        {
            return SystemMath.Abs(@int);
        }

        /// <summary>
        /// Gets size of this data type, in bytes.
        /// </summary>
        public static ScriptInteger Size
        {
            get { return sizeof(long); }
        }

        /// <summary>
        /// Determines whether the specified integer is interned.
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool IsInterned(InvocationContext ctx, ScriptInteger value)
        {
            return ctx.RuntimeState.IsInterned(value);
        }

        #region Runtime Slots

        IRuntimeSlot IIntegerContractSlots.IsInterned
        {
            get { return CacheConst<IsInternedAction>(ref m_isint); }
        }

        IRuntimeSlot IIntegerContractSlots.Rem
        {
            get { return CacheConst<RemAction>(ref m_rem); }
        }

        IRuntimeSlot IIntegerContractSlots.Sum
        {
            get { return CacheConst<SumAction>(ref m_sum); }
        }

        IRuntimeSlot IIntegerContractSlots.Size
        {
            get { return CacheConst(ref m_size, () => Size); }
        }

        IRuntimeSlot IIntegerContractSlots.Max
        {
            get { return CacheConst(ref m_max, () => ScriptInteger.MaxValue); }
        }

        IRuntimeSlot IIntegerContractSlots.Min
        {
            get { return CacheConst(ref m_min, () => ScriptInteger.MinValue); }
        }

        IRuntimeSlot IIntegerContractSlots.Even
        {
            get { return CacheConst<EvenAction>(ref m_even); }
        }

        IRuntimeSlot IIntegerContractSlots.Odd
        {
            get { return CacheConst<OddAction>(ref m_odd); }
        }

        IRuntimeSlot IIntegerContractSlots.Abs
        {
            get { return CacheConst<AbsAction>(ref m_abs); }
        }
        #endregion
    }
}
