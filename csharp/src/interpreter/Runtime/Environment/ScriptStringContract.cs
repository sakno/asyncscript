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

    /// <summary>
    /// Represents string contract.
    /// This class cannot be inherited.
    /// </summary>
    [ComVisible(false)]
    [Serializable]
    public sealed class ScriptStringContract : ScriptBuiltinContract, IStringContractSlots
    {
        #region Nested Types
        [ComVisible(false)]
        private sealed class IsInternedAction : ScriptFunc<ScriptString>
        {
            private const string FirstParamName = "str";

            public IsInternedAction()
                : base(FirstParamName, Instance, ScriptBooleanContract.Instance)
            {
            }

            protected override IScriptObject Invoke(InvocationContext ctx, ScriptString arg0)
            {
                return (ScriptBoolean)IsInterned(ctx, arg0);
            }
        }

        [ComVisible(false)]
        private sealed class ConcatAction : ScriptFunc<IScriptArray>
        {
            private const string FirstParamName = "strings";

            public ConcatAction()
                : base(FirstParamName, new ScriptArrayContract(ScriptSuperContract.Instance), Instance)
            {
            }

            protected override IScriptObject Invoke(InvocationContext ctx, IScriptArray strings)
            {
                if (strings == null) return Void;
                var result = new StringBuilder();
                var indicies = new long[1];
                for (var i = 0L; i < strings.GetLength(0); i++)
                {
                    indicies[0] = i;
                    result.Append(strings[indicies, ctx.RuntimeState]);
                }
                return new ScriptString(result);
            }
        }

        [ComVisible(false)]
        private sealed class LanguageSlot : RuntimeSlotBase
        {
            private static ScriptString Value
            {
                get { return new ScriptString(Thread.CurrentThread.CurrentCulture.IetfLanguageTag); }
                set { Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfoByIetfLanguageTag(value); }
            }

            protected override IScriptContract GetValueContract()
            {
                return Value.GetContractBinding();
            }

            public override IScriptObject GetValue(InterpreterState state)
            {
                return Value;
            }

            public override void SetValue(IScriptObject value, InterpreterState state)
            {
                Value = (ScriptString)value;
            }

            public override IScriptContract ContractBinding
            {
                get { return Instance; }
            }

            public override RuntimeSlotAttributes Attributes
            {
                get { return RuntimeSlotAttributes.None; }
            }

            protected override ICollection<string> Slots
            {
                get { return Value.Slots; }
            }

            public override bool DeleteValue()
            {
                return false;
            }

            public override bool Equals(IRuntimeSlot other)
            {
                return other is LanguageSlot;
            }
        }

        [ComVisible(false)]
        private sealed class EqualityAction : ScriptFunc<ScriptString, ScriptString, ScriptString>
        {
            private const string FirstParamName = "a";
            private const string SecondParamName = "b";
            private const string ThirdParamName = "lang";

            public EqualityAction()
                : base(FirstParamName, Instance, SecondParamName, Instance, ThirdParamName, Instance, ScriptBooleanContract.Instance)
            {
            }

            public override IScriptObject Invoke(InvocationContext ctx, ScriptString str1, ScriptString str2, ScriptString language)
            {
                return (ScriptBoolean)ScriptStringContract.Equals(ctx, str1, str2, language);
            }
        }

        [ComVisible(false)]
        private sealed class ComparisonAction : ScriptFunc<ScriptString, ScriptString, ScriptString>
        {
            private const string FirstParamName = "a";
            private const string SecondParamName = "b";
            private const string ThirdParamName = "lang";

            public ComparisonAction()
                : base(FirstParamName, Instance, SecondParamName, Instance, ThirdParamName, Instance, ScriptIntegerContract.Instance)
            {
            }

            public override IScriptObject Invoke(InvocationContext ctx, ScriptString a, ScriptString b, ScriptString lang)
            {
                return (ScriptInteger)Compare(ctx, a, b, lang);
            }
        }
        #endregion

        private IRuntimeSlot m_empty;
        private IRuntimeSlot m_concat;
        private IRuntimeSlot m_language;
        private IRuntimeSlot m_isint;
        private IRuntimeSlot m_equ;
        private IRuntimeSlot m_cmp;

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
        /// Returns a string default value.
        /// </summary>
        /// <param name="state">Internal interpreter state.</param>
        /// <returns>The string default value.</returns>
        internal protected override ScriptObject FromVoid(InterpreterState state)
        {
            return Void;
        }

        private static bool Convert(ref IScriptObject value)
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
        /// Provides implicit conversion.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        protected override bool Mapping(ref IScriptObject value)
        {
            return Convert(ref value);
        }

        /// <summary>
        /// Converts the specified object to string.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public new static ScriptString ToString(IScriptObject value)
        {
            if (Convert(ref value))
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
            else if (contract.OneOf<IScriptComplementation, IScriptUnionContract>())
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
        /// Determines whether string is interned.
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool IsInterned(InvocationContext ctx, ScriptString value)
        {
            return ctx.RuntimeState.IsInterned(value);
        }

        /// <summary>
        /// Compares two strings.
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="language"></param>
        /// <returns></returns>
        public static int Compare(InvocationContext ctx, string a, string b, string language)
        {
            switch (string.IsNullOrEmpty(language))
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
        /// <param name="ctx"></param>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="language"></param>
        /// <returns></returns>
        public static bool Equals(InvocationContext ctx, string a, string b, string language)
        {
            return Compare(ctx, a, b, language) == 0;
        }

        #region Runtime Slots

        IRuntimeSlot IStringContractSlots.Equ
        {
            get { return CacheConst<EqualityAction>(ref m_equ); }
        }

        IRuntimeSlot IStringContractSlots.Cmp
        {
            get { return CacheConst<ComparisonAction>(ref m_cmp); }
        }

        IRuntimeSlot IStringContractSlots.IsInterned
        {
            get { return CacheConst<IsInternedAction>(ref m_isint); }
        }

        IRuntimeSlot IStringContractSlots.Empty
        {
            get { return CacheConst(ref m_empty, () => ScriptString.Empty); }
        }

        IRuntimeSlot IStringContractSlots.Concat
        {
            get { return CacheConst<ConcatAction>(ref m_concat); }
        }

        IRuntimeSlot IStringContractSlots.Language
        {
            get { return Cache<LanguageSlot>(ref m_language); }
        }

        #endregion
    }
}
