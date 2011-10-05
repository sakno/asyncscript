using System;
using System.Linq.Expressions;
using System.Dynamic;
using System.Runtime.Serialization;

namespace DynamicScript.Runtime.Environment
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;
    using LinqExpression = System.Linq.Expressions.Expression;
    using InterpretationContext = Compiler.Ast.InterpretationContext;
    using Keyword = Compiler.Keyword;

    /// <summary>
    /// Initializes a contract that accepts any action contracts.
    /// This class cannot be inherited.
    /// </summary>
    [ComVisible(false)]
    [Serializable]
    public sealed class ScriptCallableContract : ScriptBuiltinContract, IScriptMetaContract
    {
        private ScriptCallableContract(SerializationInfo info, StreamingContext context)
        {
        }

        private ScriptCallableContract()
        {
        }

        internal override Keyword Token
        {
            get { return Keyword.Callable; }
        }

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

        /// <summary>
        /// Represents singleton instance of the contract that accepts any other objects.
        /// </summary>
        public static readonly ScriptCallableContract Instance = new ScriptCallableContract();

        internal static LinqExpression Expression
        {
            get { return LinqHelpers.BodyOf<Func<ScriptCallableContract>, MemberExpression>(() => Instance); }
        }

        /// <summary>
        /// Returns relationship between the current contract and the specified.
        /// </summary>
        /// <param name="contract">The contract to compare. Cannot be <see langword="null"/>.</param>
        /// <returns>Relationship between the current contract and <paramref name="contract"/>.</returns>
        public override ContractRelationshipType GetRelationship(IScriptContract contract)
        {
            if (contract is ScriptCallableContract)
                return ContractRelationshipType.TheSame;
            else if (contract.OneOf<ScriptSuperContract, ScriptMetaContract>())
                return ContractRelationshipType.Subset;
            else if (contract is ScriptActionContract)
                return ContractRelationshipType.Superset;
            else if (contract.OneOf<IScriptComplementation, IScriptUnionContract>())
                return Inverse(contract.GetRelationship(this));
            else return ContractRelationshipType.None;
        }
    }
}
