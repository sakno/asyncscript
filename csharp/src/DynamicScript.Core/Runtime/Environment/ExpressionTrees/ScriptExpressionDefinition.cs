using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace DynamicScript.Runtime.Environment.ExpressionTrees
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;
    using ScriptCodeExpression = Compiler.Ast.ScriptCodeExpression;
    using Punctuation = Compiler.Punctuation;

    /// <summary>
    /// Represents an abstract contract for all runtime expressions.
    /// </summary>
    /// <typeparam name="TExpressionKind">Type of the expression tree.</typeparam>
    /// <typeparam name="TRuntimeExpression">Runtime representation of expression tree.</typeparam>
    [ComVisible(false)]
    [Serializable]
    abstract class ScriptExpressionFactory<TExpressionKind, TRuntimeExpression> : ScriptCodeElementFactory<TExpressionKind, TRuntimeExpression>, 
        IScriptExpressionContract<TExpressionKind>
        where TExpressionKind : ScriptCodeExpression
        where TRuntimeExpression : ScriptObject, IScriptExpression<TExpressionKind>
    {
        /// <summary>
        /// Initializes a new runtime expression definition.
        /// </summary>
        /// <param name="contractName">The name of the contract.</param>
        protected ScriptExpressionFactory(string contractName)
            : base(contractName)
        {
        }

        protected ScriptExpressionFactory(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        /// <summary>
        /// Returns relationship with other
        /// </summary>
        /// <param name="contract"></param>
        /// <returns></returns>
        public sealed override ContractRelationshipType GetRelationship(IScriptContract contract)
        {
            if (contract is ScriptExpressionFactory<TExpressionKind, TRuntimeExpression>)
                return ContractRelationshipType.TheSame;
            else if (contract.OneOf<ScriptSuperContract, ScriptMetaContract, ScriptExpressionFactory>())
                return ContractRelationshipType.Subset;
            else if (contract.OneOf<IScriptComplementation, IScriptUnionContract, IScriptCartesianProduct>())
                return Inverse(contract.GetRelationship(this));
            else return ContractRelationshipType.None;
        }

       

        /// <summary>
        /// Returns a string representation of this contract.
        /// </summary>
        /// <returns></returns>
        public sealed override string ToString()
        {
            return string.Concat(ScriptExpressionFactory.Name, Punctuation.Dot, Name);
        }

        /// <summary>
        /// Gets contract binding of all expression factories.
        /// </summary>
        public static ScriptContract ContractBinding
        {
            get { return ScriptExpressionFactory.Instance; }
        }

        /// <summary>
        /// Returns contract binding of this expression factory.
        /// </summary>
        /// <returns>Underlying contract associated with this script object.</returns>
        public sealed override IScriptContract GetContractBinding()
        {
            return ContractBinding;
        }

        /// <summary>
        /// Provides implicit conversion to this contract.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        protected override bool Mapping(ref IScriptObject value)
        {
            return value is IScriptExpression<ScriptCodeExpression>;
        }

        IScriptExpression<TExpressionKind> IScriptCodeElementFactory<TExpressionKind, IScriptExpression<TExpressionKind>>.CreateCodeElement(IList<IScriptObject> args, InterpreterState state)
        {
            return CreateCodeElement(args, state);
        }
    }
}
