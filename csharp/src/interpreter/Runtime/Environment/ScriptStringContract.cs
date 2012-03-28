using System;
using System.Dynamic;
using System.Linq.Expressions;
using System.Runtime.Serialization;
using System.Collections.Generic;

namespace DynamicScript.Runtime.Environment
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;
    using LinqExpression = System.Linq.Expressions.Expression;
    using Keyword = Compiler.Keyword;
    using InterpretationContext = Compiler.Ast.InterpretationContext;
    using Enumerable = System.Linq.Enumerable;
    using SystemConverter = System.Convert;
    using CultureInfo = System.Globalization.CultureInfo;
    using Thread = System.Threading.Thread;
    using StringBuilder = System.Text.StringBuilder;
    using InliningSourceAttribute = Compiler.Ast.Translation.LinqExpressions.InliningSourceAttribute;

    /// <summary>
    /// Represents string contract.
    /// This class cannot be inherited.
    /// </summary>
    [ComVisible(false)]
    [Serializable]
    public sealed class ScriptStringContract : ScriptBuiltinContract
    {
        #region Nested Types

        [ComVisible(false)]
        private sealed class InsertFunction : ScriptFunc<ScriptString, ScriptInteger, ScriptString>
        {
            public const string Name = "insert";
            private const string FirstParamName = "str";
            private const string SecondParamName = "index";
            private const string ThirdParamName = "value";

            public InsertFunction()
                : base(FirstParamName, Instance, SecondParamName, ScriptIntegerContract.Instance, ThirdParamName, Instance, Instance)
            {
            }

            public override IScriptObject Invoke(ScriptString str, ScriptInteger index, ScriptString value, InterpreterState state)
            {
                return Insert(str, index, value, state);
            }
        }

        [ComVisible(false)]
        private sealed class IndexOfFunction : ScriptFunc<ScriptString, ScriptString, ScriptInteger>
        {
            public const string Name = "indexof";
            private const string FirstParamName = "str";
            private const string SecondParamName = "value";
            private const string ThirdParamName = "startIndex";

            public IndexOfFunction()
                : base(FirstParamName, Instance, SecondParamName, Instance, ThirdParamName, ScriptIntegerContract.Instance, ScriptIntegerContract.Instance)
            {
            }

            public override IScriptObject Invoke(ScriptString str, ScriptString value, ScriptInteger startIndex, InterpreterState state)
            {
                return IndexOf(str, value, startIndex, state);
            }
        }

        [ComVisible(false)]
        private sealed class SubstringFunction : ScriptFunc<ScriptString, ScriptInteger, ScriptInteger>
        {
            public const string Name = "substr";
            private const string FirstParamName = "str";
            private const string SecondParamName = "startIndex";
            private const string ThirdParamName = "length";

            public SubstringFunction()
                : base(FirstParamName, Instance, SecondParamName, ScriptIntegerContract.Instance, ThirdParamName, ScriptIntegerContract.Instance, Instance)
            {
            }

            public override IScriptObject Invoke(ScriptString str, ScriptInteger startIndex, ScriptInteger length, InterpreterState state)
            {
                return Substring(str, startIndex, length, state);
            }
        }

        [ComVisible(false)]
        private sealed class IsInternedFunction : ScriptFunc<ScriptString>
        {
            public const string Name = "isinterned";
            private const string FirstParamName = "str";

            public IsInternedFunction()
                : base(FirstParamName, Instance, ScriptBooleanContract.Instance)
            {
            }

            protected override IScriptObject Invoke(ScriptString value, InterpreterState state)
            {
                return IsInterned(value, state);
            }
        }

        [ComVisible(false)]
        private sealed class ConcatAction : ScriptFunc<IScriptArray>
        {
            public const string Name = "concat";
            private const string FirstParamName = "strings";

            public ConcatAction()
                : base(FirstParamName, new ScriptArrayContract(ScriptSuperContract.Instance), Instance)
            {
            }

            protected override IScriptObject Invoke(IScriptArray strings, InterpreterState state)
            {
                return Concat(strings, state);
            }
        }

        [ComVisible(false)]
        private sealed class LanguageSlot : AggregatedSlot<ScriptStringContract, ScriptString>
        {
            public const string Name = "language";

            public override ScriptString GetValue(ScriptStringContract owner, InterpreterState state)
            {
                return new ScriptString(Thread.CurrentThread.CurrentCulture.IetfLanguageTag);
            }

            public override void SetValue(ScriptStringContract owner, ScriptString value, InterpreterState state)
            {
                Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfoByIetfLanguageTag(value);
            }

            public override IScriptContract GetContractBinding(ScriptStringContract owner, InterpreterState state)
            {
                return Instance;
            }
        }

        [ComVisible(false)]
        private sealed class EqualityFunction : ScriptFunc<ScriptString, ScriptString, ScriptString>
        {
            public const string Name = "equ";
            private const string FirstParamName = "a";
            private const string SecondParamName = "b";
            private const string ThirdParamName = "lang";

            public EqualityFunction()
                : base(FirstParamName, Instance, SecondParamName, Instance, ThirdParamName, Instance, ScriptBooleanContract.Instance)
            {
            }

            public override IScriptObject Invoke(ScriptString a, ScriptString b, ScriptString language, InterpreterState state)
            {
                return Equ(a, b, language, state);
            }
        }

        [ComVisible(false)]
        private sealed class ComparisonFunction : ScriptFunc<ScriptString, ScriptString, ScriptString>
        {
            public const string Name = "cmp";
            private const string FirstParamName = "a";
            private const string SecondParamName = "b";
            private const string ThirdParamName = "lang";

            public ComparisonFunction()
                : base(FirstParamName, Instance, SecondParamName, Instance, ThirdParamName, Instance, ScriptIntegerContract.Instance)
            {
            }

            public override IScriptObject Invoke(ScriptString a, ScriptString b, ScriptString language, InterpreterState state)
            {
                return Cmp(a, b, language, state);
            }
        }
        #endregion

        private static AggregatedSlotCollection<ScriptStringContract> StaticSlots = new AggregatedSlotCollection<ScriptStringContract>
        {
            {"empty", (owner, state) => ScriptString.Empty},
            {InsertFunction.Name, (owner, state) => LazyField<InsertFunction, IScriptFunction>(ref owner.m_insert)},
            {IndexOfFunction.Name, (owner, state) => LazyField<IndexOfFunction, IScriptFunction>(ref owner.m_indexof)},
            {SubstringFunction.Name, (owner, state) => LazyField<SubstringFunction, IScriptFunction>(ref owner.m_substr)},
            {IsInternedFunction.Name, (owner, state) => LazyField<IsInternedFunction, IScriptFunction>(ref owner.m_interned)},
            {ConcatAction.Name, (owner, state) => LazyField<ConcatAction, IScriptFunction>(ref owner.m_concat)},
            {LanguageSlot.Name, new LanguageSlot()},
            {EqualityFunction.Name, (owner, state) => LazyField<EqualityFunction, IScriptFunction>(ref owner.m_equ)},
            {ComparisonFunction.Name, (owner, state) => LazyField<ComparisonFunction, IScriptFunction>(ref owner.m_cmp)}
        };

        private IScriptFunction m_insert;
        private IScriptFunction m_indexof;
        private IScriptFunction m_substr;
        private IScriptFunction m_interned;
        private IScriptFunction m_concat;
        private IScriptFunction m_equ;
        private IScriptFunction m_cmp;

        private ScriptStringContract(SerializationInfo info, StreamingContext context)
        {
        }

        private ScriptStringContract()
        {
        }

        internal override Keyword Token
        {
            get { return Keyword.String; }
        }

        /// <summary>
        /// Represents singleton instance of the contract.
        /// </summary>
        public static readonly ScriptStringContract Instance = new ScriptStringContract();

        internal static MemberExpression Expression
        {
            get
            {
                return LinqHelpers.BodyOf<Func<ScriptStringContract>, MemberExpression>(() => Instance);
            }
        }

        /// <summary>
        /// Represents empty string.
        /// </summary>
        [CLSCompliant(false)]
        public new static ScriptString Void
        {
            get { return ScriptString.Empty; }
        }

        /// <summary>
        /// Clears all internal fields.
        /// </summary>
        public override void Clear()
        {
            m_cmp = m_concat = m_equ = m_indexof = m_insert = m_interned = m_substr = null;
        }

        /// <summary>
        /// Returns a string default value.
        /// </summary>
        /// <param name="state">Internal interpreter state.</param>
        /// <returns>The string default value.</returns>
        internal protected override ScriptObject FromVoid(InterpreterState state)
        {
            return Void;
        }

        /// <summary>
        /// Attempts to convert the specified script object to the string.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool TryConvert(ref IScriptObject value)
        {
            if (value is ScriptString)
                return true;
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
        public static ScriptString TryConvert(IScriptObject value, InterpreterState state)
        {
            if (TryConvert(ref value))
                return (ScriptString)value;
            else throw new ContractBindingException(value, Instance, state);
        }

        internal static Expression TryConvert(Expression value, ParameterExpression state)
        {
            return LinqHelpers.BodyOf<IScriptObject, InterpreterState, ScriptString, MethodCallExpression>((v, s) => TryConvert(v, s)).Update(null, new[] { value, state });
        }

        /// <summary>
        /// Provides implicit conversion.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        protected override bool Mapping(ref IScriptObject value)
        {
            return TryConvert(ref value);
        }

        /// <summary>
        /// Converts the specified object to string.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public new static ScriptString ToString(IScriptObject value)
        {
            if (TryConvert(ref value))
                return (ScriptString)value;
            else if (value is IConvertible)
                return new ScriptString(SystemConverter.ToString((IConvertible)value));
            else return new ScriptString(value.ToString());
        }

        /// <summary>
        /// Converts the specified object to string contract.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        public override IScriptObject Convert(IScriptObject value, InterpreterState state)
        {
            return ToString(value);
        }

        /// <summary>
        /// Returns relationship between the current contract and the specified.
        /// </summary>
        /// <param name="contract">The contract to compare. Cannot be <see langword="null"/>.</param>
        /// <returns>Relationship between the current contract and <paramref name="contract"/>.</returns>
        public override ContractRelationshipType GetRelationship(IScriptContract contract)
        {
            if (contract is ScriptStringContract)
                return ContractRelationshipType.TheSame;
            else if (contract is ScriptVoid)
                return ContractRelationshipType.Superset;
            else if (contract is ScriptSuperContract)
                return ContractRelationshipType.Subset;
            else if (contract.OneOf<IScriptComplementation, IScriptUnionContract, IScriptCartesianProduct>())
                return Inverse(contract.GetRelationship(this));
            else return ContractRelationshipType.None;
        }

        /// <summary>
        /// Returns contract binding.
        /// </summary>
        /// <returns></returns>
        public override IScriptContract GetContractBinding()
        {
            return ScriptMetaContract.Instance;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="str"></param>
        /// <param name="index"></param>
        /// <param name="value"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        [InliningSource]
        public static ScriptString Insert(ScriptString str, ScriptInteger index, ScriptString value, InterpreterState state)
        {
            if (str == null || value == null) throw new ContractBindingException(Instance, state);
            if (index == null) throw new ContractBindingException(ScriptIntegerContract.Instance, state);
            return str.Insert((int)index, value);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="str"></param>
        /// <param name="value"></param>
        /// <param name="startIndex"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        [InliningSource]
        public static ScriptInteger IndexOf(ScriptString str, ScriptString value, ScriptInteger startIndex, InterpreterState state)
        {
            if (str == null || value == null) throw new ContractBindingException(Instance, state);
            if (startIndex == null) throw new ContractBindingException(ScriptIntegerContract.Instance, state);
            return str.IndexOf(value, (int)startIndex);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="str"></param>
        /// <param name="startIndex"></param>
        /// <param name="length"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        [InliningSource]
        public static ScriptString Substring(ScriptString str, ScriptInteger startIndex, ScriptInteger length, InterpreterState state)
        {
            if (str == null) throw new ContractBindingException(Instance, state);
            if (startIndex == null || length == null) throw new ContractBindingException(ScriptIntegerContract.Instance, state);
            return str.Substring((int)startIndex, (int)length);
        }

        /// <summary>
        /// Determines whether the specified string is interned.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        [InliningSource]
        public static ScriptBoolean IsInterned(ScriptString value, InterpreterState state)
        {
            if (value == null) throw new ContractBindingException(ScriptStringContract.Instance, state);
            return state.IsInterned(value);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="strings"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        [InliningSource]
        public static ScriptString Concat(IScriptArray strings, InterpreterState state)
        {
            if (strings == null) throw new ContractBindingException(new ScriptArrayContract(), state);
            var result = new StringBuilder();
            var indicies = new long[1];
            for (var i = 0L; i < strings.GetLength(0); i++)
            {
                indicies[0] = i;
                result.Append(strings[indicies, state]);
            }
            return result.ToString();
        }

        /// <summary>
        /// Compares two strings.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="language"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        [InliningSource]
        public static ScriptInteger Cmp(ScriptString a, ScriptString b, ScriptString language, InterpreterState state)
        {
            if (a == null || b == null) throw new ContractBindingException(Instance, state);
            switch (string.IsNullOrWhiteSpace(language))
            {
                case true: return string.CompareOrdinal(a, b);
                default:
                    var culture = CultureInfo.GetCultureInfoByIetfLanguageTag(language);
                    return culture.CompareInfo.Compare(a, b);
            }
        }

        /// <summary>
        /// Determines whether the two strings are equal.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="language"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        [InliningSource]
        public static ScriptBoolean Equ(ScriptString a, ScriptString b, ScriptString language, InterpreterState state)
        {
            if (a == null || b == null) throw new ContractBindingException(Instance, state);
            return Cmp(a, b, language, state) == 0L;
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
