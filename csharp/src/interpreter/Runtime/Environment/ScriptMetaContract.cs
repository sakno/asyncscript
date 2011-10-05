using System;
using System.Linq.Expressions;
using System.Dynamic;
using System.Collections.Generic;

namespace DynamicScript.Runtime.Environment
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;
    using InterpretationContext = Compiler.Ast.InterpretationContext;
    using LinqExpression = System.Linq.Expressions.Expression;
    using Keyword = Compiler.Keyword;

    /// <summary>
    /// Represents meta contract that is used to identify built-in contracts and custom composite contracts.
    /// This class cannot be inherited.
    /// </summary>
    [ComVisible(false)]
    
    public sealed class ScriptMetaContract: ScriptBuiltinContract
    {
        private ScriptMetaContract()
        {
        }

        internal override Keyword Token
        {
            get { return Keyword.Type; }
        }

        /// <summary>
        /// Represents singleton instance of the contract.
        /// </summary>
        public static readonly ScriptMetaContract Instance = new ScriptMetaContract();

        /// <summary>
        /// Creates a void object according with the current contract.
        /// </summary>
        /// <param name="state">Internal interpreter state.</param>
        /// <returns>The void object according with the current contract.</returns>
        internal protected override ScriptObject FromVoid(InterpreterState state)
        {
            switch (state.Context)
            {
                case InterpretationContext.Unchecked: return Void;
                default: throw new UnsupportedOperationException(state);
            }
        }

        internal static LinqExpression Expression
        {
            get { return LinqHelpers.BodyOf<Func<ScriptMetaContract>, MemberExpression>(() => Instance); }
        }

        /// <summary>
        /// Returns relationship between the current contract and the specified.
        /// </summary>
        /// <param name="contract">The contract to compare. Cannot be <see langword="null"/>.</param>
        /// <returns>Relationship between the current contract and <paramref name="contract"/>.</returns>
        public override ContractRelationshipType GetRelationship(IScriptContract contract)
        {
            if (contract is ScriptMetaContract)
                return ContractRelationshipType.TheSame;
            else if (contract is IScriptMetaContract)
                return ContractRelationshipType.Superset;
            else if (IsVoid(contract))
                return ContractRelationshipType.Superset;
            else if (contract is ScriptSuperContract)
                return ContractRelationshipType.Subset;
            else if (contract.OneOf<IScriptComplementation, IScriptUnionContract>())
                return Inverse(contract.GetRelationship(this));
            return ContractRelationshipType.None;
        }

        /// <summary>
        /// Provides implicit conversion of the specified object.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        protected override bool Mapping(ref IScriptObject value)
        {
            return value is IScriptContract;
        }

        /// <summary>
        /// Provides explicit conversion of the specified object.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        public override IScriptObject Convert(IScriptObject value, InterpreterState state)
        {
            if (Mapping(ref value))
                return value;
            else if (value is IScriptSetFactory)
                return ((IScriptSetFactory)value).CreateSet(state);
            else if (state.Context == InterpretationContext.Unchecked)
                return Void;
            else throw new UnsupportedOperationException(state);
        }

        /// <summary>
        /// Returns contract of this object.
        /// </summary>
        /// <returns></returns>
        public override IScriptContract GetContractBinding()
        {
            return this;
        }
    }
}
