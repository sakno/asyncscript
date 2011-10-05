using System;
using System.Runtime.Serialization;

namespace DynamicScript.Runtime.Environment
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;
    using Keyword = Compiler.Keyword;
    using InterpretationContext = Compiler.Ast.InterpretationContext;
    using LinqExpression = System.Linq.Expressions.Expression;
    using MemberExpression = System.Linq.Expressions.MemberExpression;

    /// <summary>
    /// Represents contract for all finite sets.
    /// This class cannot be inherited.
    /// </summary>
    [ComVisible(false)]
    
    [Serializable]
    public sealed class ScriptFinSetContract: ScriptBuiltinContract, IScriptMetaContract
    {
        private ScriptFinSetContract(SerializationInfo info, StreamingContext context)
        {
        }

        private ScriptFinSetContract()
        {
        }

        /// <summary>
        /// Represents singleton instance of this class.
        /// </summary>
        public static readonly ScriptFinSetContract Instance = new ScriptFinSetContract();

        internal override Keyword Token
        {
            get { return Keyword.FinSet; }
        }

        /// <summary>
        /// Returns relationship with the specified contract.
        /// </summary>
        /// <param name="contract"></param>
        /// <returns></returns>
        public override ContractRelationshipType GetRelationship(IScriptContract contract)
        {
            if (contract is ScriptFinSetContract)
                return ContractRelationshipType.TheSame;
            else if (contract.OneOf<ScriptSuperContract, ScriptMetaContract>())
                return ContractRelationshipType.Subset;
            else if (contract.OneOf<IScriptSet, ScriptIntegerContract, ScriptRealContract, ScriptBooleanContract>())
                return ContractRelationshipType.Superset;
            else if (IsVoid(contract))
                return ContractRelationshipType.Superset;
            else return ContractRelationshipType.None;
        }

        /// <summary>
        /// Returns default value that satisfies to this contract.
        /// </summary>
        /// <param name="state">Internal interpreter state.</param>
        /// <returns></returns>
        /// <exception cref="UnsupportedOperationException">This method is not supported.</exception>
        protected internal override ScriptObject FromVoid(InterpreterState state)
        {
            if (state.Context == InterpretationContext.Unchecked)
                return Void;
            else throw new UnsupportedOperationException(state);
        }

        internal static LinqExpression Expression
        {
            get { return LinqHelpers.BodyOf<Func<ScriptFinSetContract>, MemberExpression>(() => Instance); }
        }
    }
}
