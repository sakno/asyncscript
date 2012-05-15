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
    /// Initializes a contract that accepts any other objects.
    /// This class cannot be inherited.
    /// </summary>
    [ComVisible(false)]
    
    [Serializable]
    public sealed class ScriptSuperContract: ScriptBuiltinContract
    {
        private ScriptSuperContract(SerializationInfo info, StreamingContext context)
        {
        }

        private ScriptSuperContract()
        {
        }

        internal override Keyword Token
        {
            get { return Keyword.Object; }
        }

        /// <summary>
        /// Creates an object that represents void value according with the contract.
        /// </summary>
        /// <param name="state">Internal interpreter state.</param>
        /// <returns>The object that represents void value according with the contract.</returns>
        internal protected override ScriptObject FromVoid(InterpreterState state)
        {
            return Void;
        }

        /// <summary>
        /// Represents singleton instance of the contract that accepts any other objects.
        /// </summary>
        public static readonly ScriptSuperContract Instance = new ScriptSuperContract();

        internal static LinqExpression Expression
        {
            get { return LinqHelpers.BodyOf<Func<ScriptSuperContract>, MemberExpression>(() => Instance); }
        }

        internal override IScriptObject Complement(InterpreterState state)
        {
            return Void;
        }

        internal override IScriptContract Unite(IScriptContract right, InterpreterState state)
        {
            return this;
        }

        internal override IScriptContract Intersect(IScriptContract right, InterpreterState state)
        {
            return right;
        }

        /// <summary>
        /// Returns relationship between the current contract and the specified.
        /// </summary>
        /// <param name="contract">The contract to compare. Cannot be <see langword="null"/>.</param>
        /// <returns>Relationship between the current contract and <paramref name="contract"/>.</returns>
        public override ContractRelationshipType GetRelationship(IScriptContract contract)
        {
            return contract is ScriptSuperContract ? ContractRelationshipType.TheSame : ContractRelationshipType.Superset;
        }

        /// <summary>
        /// Returns contract of this object.
        /// </summary>
        /// <returns></returns>
        public override IScriptContract GetContractBinding()
        {
            return ScriptMetaContract.Instance;
        }
    }
}
