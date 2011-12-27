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
    using CultureInfo = System.Globalization.CultureInfo;

    /// <summary>
    /// Represents real number contract.
    /// This class cannot be inherited.
    /// </summary>
    [ComVisible(false)]
    [Serializable]
    public sealed class ScriptRealContract : ScriptBuiltinContract
    {
        #region Nested Types
        [ComVisible(false)]
        private sealed class IsInternedAction : ScriptFunc<ScriptReal>
        {
            public const string Name = "isInterned";
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

        private static readonly AggregatedSlotCollection<ScriptRealContract> StaticSlots = new AggregatedSlotCollection<ScriptRealContract>
        {
            {"nan", (owner, state) => ScriptReal.NaN},
            {"max", (owner, state) => ScriptReal.MaxValue},
            {"min", (owner, state) => ScriptReal.MinValue},
            {"epsilon", (owner, state) => ScriptReal.Epsilon},
            {IsInternedAction.Name, (owner, state) => LazyField<IsInternedAction, IScriptFunction>(ref owner.m_interned)},
            {AbsAction.Name, (owner, state) => LazyField<AbsAction, IScriptFunction>(ref owner.m_abs)},
            {SumAction.Name, (owner, state) => LazyField<SumAction, IScriptFunction>(ref owner.m_sum)},
            {RemAction.Name, (owner, state) => LazyField<RemAction, IScriptFunction>(ref owner.m_rem)},
            {"pinf", (owner, state) => ScriptReal.PositiveInfinity},
            {"ninf", (owner, state) => ScriptReal.NegativeInfinity}
        };

        private IScriptFunction m_interned;
        private IScriptFunction m_abs;
        private IScriptFunction m_sum;
        private IScriptFunction m_rem;

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
        /// Clears all internal fields.
        /// </summary>
        public override void Clear()
        {
            m_abs = m_interned = m_rem = m_sum = null;
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

        /// <summary>
        /// Gets collection of aggregated slot.
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
