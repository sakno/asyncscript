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
    using CultureInfo = System.Globalization.CultureInfo;

    /// <summary>
    /// Represents real number contract.
    /// This class cannot be inherited.
    /// </summary>
    [ComVisible(false)]
    [Serializable]
    public sealed class ScriptRealContract : ScriptBuiltinContract, IRealContractSlots
    {
        #region Nested Types
        [ComVisible(false)]
        private sealed class IsInternedAction : ScriptFunc<ScriptReal>
        {
            private const string FirstParamName = "r";

            public IsInternedAction()
                : base(FirstParamName, Instance, ScriptBooleanContract.Instance)
            {
            }

            protected override IScriptObject Invoke(ScriptReal value, InterpreterState state)
            {
                return (ScriptBoolean)IsInterned(value, state);
            }
        }

        [ComVisible(false)]
        private sealed class AbsAction : ScriptFunc<ScriptReal>
        {
            public const string Name = "abs";
            private const string FirstParamName = "value";

            public AbsAction()
                : base(FirstParamName, Instance, Instance)
            {
            }

            protected override IScriptObject Invoke(ScriptReal value, InterpreterState state)
            {
                return Abs(value);
            }
        }

        [ComVisible(false)]
        private sealed class SumAction : ScriptFunc<IScriptArray>
        {
            public const string Name = "sum";
            private const string FirstParamName = "floats";

            public SumAction()
                : base(FirstParamName, new ScriptArrayContract(Instance), Instance)
            {
            }

            protected override IScriptObject Invoke(IScriptArray floats, InterpreterState state)
            {
                if (floats == null) return Void;
                var result = 0.0;
                var indicies = new long[1];
                for (var i = 0L; i < floats.GetLength(0); i++)
                {
                    indicies[0] = i;
                    result += SystemConverter.ToDouble(floats[indicies, state]);
                }
                return new ScriptReal(result);
            }
        }

        [ComVisible(false)]
        private sealed class RemAction : ScriptFunc<IScriptArray>
        {
            public const string Name = "rem";
            private const string FirstParamName = "floats";

            public RemAction()
                : base(FirstParamName, new ScriptArrayContract(Instance), Instance)
            {
            }

            protected override IScriptObject Invoke(IScriptArray floats, InterpreterState state)
            {
                if (floats == null) return Void;
                var indicies = new long[1];
                switch (floats.GetLength(0))
                {
                    case 0: return ScriptRealContract.Void;
                    default:
                        var result = SystemConverter.ToDouble(floats[indicies, state]);
                        for (var i = 1L; i < floats.GetLength(0); i++)
                        {
                            indicies[0] = i;
                            result -= SystemConverter.ToDouble(floats[indicies, state]);
                        }
                        return new ScriptReal(result);
                }
            }
        }
        #endregion

        private IRuntimeSlot m_nan;
        private IRuntimeSlot m_max;
        private IRuntimeSlot m_min;
        private IRuntimeSlot m_epsilon;
        private IRuntimeSlot m_rem;
        private IRuntimeSlot m_sum;
        private IRuntimeSlot m_abs;
        private IRuntimeSlot m_isint;
        private IRuntimeSlot m_pinf;
        private IRuntimeSlot m_ninf;

        private ScriptRealContract(SerializationInfo info, StreamingContext context)
        {
        }

        private ScriptRealContract()
        {
        }

        internal override Keyword Token
        {
            get { return Keyword.Real; }
        }

        /// <summary>
        /// Represents singleton instance of the contract.
        /// </summary>
        public static readonly ScriptRealContract Instance = new ScriptRealContract();

        /// <summary>
        /// Returns underlying contract of this contract.
        /// </summary>
        /// <returns></returns>
        public override IScriptContract GetContractBinding()
        {
            return ScriptFinSetContract.Instance;
        }

        /// <summary>
        /// Gets default value for the contract.
        /// </summary>
        [CLSCompliant(false)]
        public static new ScriptReal Void
        {
            get { return ScriptReal.Zero; }
        }

        /// <summary>
        /// Returns a default real value.
        /// </summary>
        /// <param name="state">Internal interpreter state.</param>
        /// <returns>The default real value.</returns>
        internal protected override ScriptObject FromVoid(InterpreterState state)
        {
            return Void;
        }

        internal static LinqExpression Expression
        {
            get { return LinqHelpers.BodyOf<Func<ScriptRealContract>, MemberExpression>(() => Instance); }
        }

        /// <summary>
        /// Provides implicit conversion from script object to real.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static bool Convert(ref IScriptObject obj)
        {
            if (obj is ScriptReal)
                return true;
            else if (obj.OneOf<ScriptInteger, ScriptBoolean>())
            {
                obj = new ScriptReal(SystemConverter.ToDouble(obj));
                return true;
            }
            else if (IsVoid(obj))
            {
                obj = Void;
                return true;
            }
            else return false;
        }

        /// <summary>
        /// Provides mapping between DynamicScript object and Real data type.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        protected override bool Mapping(ref IScriptObject obj)
        {
            return Convert(ref obj);
        }

        /// <summary>
        /// Returns relationship between the current contract and the specified.
        /// </summary>
        /// <param name="contract">The contract to compare. Cannot be <see langword="null"/>.</param>
        /// <returns>Relationship between the current contract and <paramref name="contract"/>.</returns>
        public override ContractRelationshipType GetRelationship(IScriptContract contract)
        {
            if (contract is ScriptRealContract)
                return ContractRelationshipType.TheSame;
            else if (contract is ScriptSuperContract)
                return ContractRelationshipType.Subset;
            else if (contract.OneOf<ScriptVoid, ScriptIntegerContract, ScriptBooleanContract>())
                return ContractRelationshipType.Superset;
            else if (contract.OneOf<IScriptComplementation, IScriptUnionContract, IScriptCartesianProduct>())
                return Inverse(contract.GetRelationship(this));
            else return ContractRelationshipType.None;
        }

        /// <summary>
        /// Tries to parse script real value.
        /// </summary>
        /// <param name="value">The value to parse.</param>
        /// <param name="formatProvider"></param>
        /// <returns>The parsed real value; or <see langword="null"/> if parsing fails.</returns>
        [CLSCompliant(false)]
        public static ScriptReal TryParse(string value, CultureInfo formatProvider)
        {
            var result = default(double);
            return double.TryParse(value, System.Globalization.NumberStyles.Any, formatProvider, out result) ? new ScriptReal(result) : null;
        }

        /// <summary>
        /// Returns an absoule value.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static ScriptReal Abs(ScriptReal value)
        {
            return SystemMath.Abs(value);
        }

        /// <summary>
        /// Determines whether the specified value is interned.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        public static bool IsInterned(ScriptReal value, InterpreterState state)
        {
            return state.IsInterned(value);
        }

        #region Runtime Slots

        IRuntimeSlot IRealContractSlots.PINF
        {
            get { return CacheConst(ref m_pinf, () => ScriptReal.PositiveInfinity); }
        }

        IRuntimeSlot IRealContractSlots.NINF
        {
            get { return CacheConst(ref m_ninf, () => ScriptReal.NegativeInfinity); }
        }

        IRuntimeSlot IRealContractSlots.IsInterned
        {
            get { return CacheConst<IsInternedAction>(ref m_isint); }
        }

        IRuntimeSlot IRealContractSlots.Abs
        {
            get { return CacheConst<AbsAction>(ref m_abs); }
        }

        IRuntimeSlot IRealContractSlots.Sum
        {
            get { return CacheConst<SumAction>(ref m_sum); }
        }

        IRuntimeSlot IRealContractSlots.Rem
        {
            get { return CacheConst<RemAction>(ref m_rem); }
        }

        IRuntimeSlot IRealContractSlots.NaN
        {
            get { return CacheConst(ref m_nan, () => ScriptReal.NaN); }
        }

        IRuntimeSlot IRealContractSlots.Max
        {
            get { return CacheConst(ref m_max, () => ScriptReal.MaxValue); }
        }

        IRuntimeSlot IRealContractSlots.Min
        {
            get { return CacheConst(ref m_min, () => ScriptReal.MinValue); }
        }

        IRuntimeSlot IRealContractSlots.Epsilon
        {
            get { return CacheConst(ref m_epsilon, () => ScriptReal.Epsilon); }
        }

        #endregion


        
    }
}
