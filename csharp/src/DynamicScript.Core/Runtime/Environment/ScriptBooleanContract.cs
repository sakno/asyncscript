﻿using System;
using System.Dynamic;
using System.Linq.Expressions;
using System.Runtime.Serialization;

namespace DynamicScript.Runtime.Environment
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;
    using LinqExpression = System.Linq.Expressions.Expression;
    using SystemConverter = System.Convert;
    using Keyword = Compiler.Keyword;
    using CultureInfo = System.Globalization.CultureInfo;

    /// <summary>
    /// Represents boolean contract.
    /// This class cannot be inherited.
    /// </summary>
    [ComVisible(false)]
    [Serializable]
    public sealed class ScriptBooleanContract: ScriptBuiltinContract
    {
        private ScriptBooleanContract(SerializationInfo info, StreamingContext context)
        {
        }

        private ScriptBooleanContract()
        {
        }

        internal override Keyword Token
        {
            get { return Keyword.Boolean; }
        }

        /// <summary>
        /// Represents singleton instance of the contract.
        /// </summary>
        public static readonly ScriptBooleanContract Instance = new ScriptBooleanContract();

        /// <summary>
        /// Gets empty value for the contract.
        /// </summary>
        [CLSCompliant(false)]
        public static new ScriptBoolean Void
        {
            get { return ScriptBoolean.False; }
        }

        /// <summary>
        /// Returns a default boolean value.
        /// </summary>
        /// <param name="state">Internal interpreter state.</param>
        /// <returns>The default boolean value.</returns>
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
        /// Attempts to convert the specified script object to boolean implicitly.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool TryConvert(ref IScriptObject value)
        {
            if (value is ScriptBoolean)
                return true;
            else if (value is ScriptInteger)
            {
                value = (ScriptBoolean)SystemConverter.ToBoolean(value);
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
        public static ScriptBoolean TryConvert(IScriptObject value, InterpreterState state)
        {
            if (TryConvert(ref value))
                return (ScriptBoolean)value;
            else throw new ContractBindingException(value, Instance, state);
        }

        internal static Expression TryConvert(Expression value, ParameterExpression state)
        {
            return LinqHelpers.BodyOf<IScriptObject, InterpreterState, ScriptBoolean, MethodCallExpression>((v, s) => TryConvert(v, s)).Update(null, new[] { value, state });
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
            get { return LinqHelpers.BodyOf<Func<ScriptBooleanContract>, MemberExpression>(() => Instance); }
        }

        /// <summary>
        /// Returns relationship between the current contract and the specified.
        /// </summary>
        /// <param name="contract">The contract to compare. Cannot be <see langword="null"/>.</param>
        /// <returns>Relationship between the current contract and <paramref name="contract"/>.</returns>
        public override ContractRelationshipType GetRelationship(IScriptContract contract)
        {
            if (contract is ScriptBooleanContract)
                return ContractRelationshipType.TheSame;
            else if (contract.OneOf<ScriptSuperContract, ScriptIntegerContract>())
                return ContractRelationshipType.Subset;
            else if (contract is ScriptVoid)
                return ContractRelationshipType.Superset;
            else if (contract.OneOf<IScriptComplementation, IScriptUnionContract, IScriptCartesianProduct>())
                return Inverse(contract.GetRelationship(this));
            else return ContractRelationshipType.None;
        }

        /// <summary>
        /// Tries to parse script boolean value.
        /// </summary>
        /// <param name="value">The value to parse.</param>
        /// <returns>The parsed boolean value; or <see langword="null"/> if parsing fails.</returns>
        [CLSCompliant(false)]
        public static ScriptBoolean TryParse(string value)
        {
            var result = default(bool);
            return bool.TryParse(value, out result) ? (ScriptBoolean)result : null;
        }
    }
}