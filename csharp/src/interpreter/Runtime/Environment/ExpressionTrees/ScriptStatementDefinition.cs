using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace DynamicScript.Runtime.Environment.ExpressionTrees
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;
    using CodeStatement = System.CodeDom.CodeStatement;
    using Punctuation = Compiler.Punctuation;
    using Compiler.Ast;

    /// <summary>
    /// Represents runtime statement factory.
    /// </summary>
    /// <typeparam name="TStatementKind">Type of the script code statement.</typeparam>
    /// <typeparam name="TRuntimeStatement">Type of the statement runtime representation.</typeparam>
    [ComVisible(false)]
    abstract class ScriptStatementFactory<TStatementKind, TRuntimeStatement> : ScriptCodeElementFactory<TStatementKind, TRuntimeStatement>,
        IScriptStatementContract<TStatementKind>, ISerializable
        where TStatementKind : ScriptCodeStatement
        where TRuntimeStatement : ScriptObject, IScriptStatement<TStatementKind>
    {
        /// <summary>
        /// Deserializes this contract.
        /// </summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        protected ScriptStatementFactory(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        /// <summary>
        /// Initializes a new runtime statement factory.
        /// </summary>
        /// <param name="contractName">The name of the statement contract. Cannot be <see langword="null"/>.</param>
        /// <exception cref="System.ArgumentNullException"><paramref name="contractName"/> is <see langword="null"/>.</exception>
        protected ScriptStatementFactory(string contractName)
            : base(contractName)
        {
        }

        /// <summary>
        /// Returns relationship between this contract and the specified contract.
        /// </summary>
        /// <param name="contract"></param>
        /// <returns></returns>
        public sealed override ContractRelationshipType GetRelationship(IScriptContract contract)
        {
            if (contract is ScriptStatementFactory<TStatementKind, TRuntimeStatement>)
                return ContractRelationshipType.TheSame;
            if (contract is ScriptSuperContract || contract is ScriptMetaContract || contract is ScriptStatementFactory)
                return ContractRelationshipType.Subset;
            else return ContractRelationshipType.None;
        }

        /// <summary>
        /// Returns an underlying contract for this object.
        /// </summary>
        /// <returns>An underlying contract of this object.</returns>
        public sealed override IScriptContract GetContractBinding()
        {
            return ScriptStatementFactory.Instance;
        }

        /// <summary>
        /// Returns a string representation of this contract.
        /// </summary>
        /// <returns></returns>
        public sealed override string ToString()
        {
            return string.Concat(ScriptStatementFactory.Name, Punctuation.Dot, Name);
        }

        protected override bool Mapping(ref IScriptObject value)
        {
            return value is IScriptStatement<TStatementKind>;
        }

        public override IScriptObject Convert(IScriptObject value, InterpreterState state)
        {
            return value is IScriptExpression<ScriptCodeExpression> ?
                Convert(new ScriptCodeExpressionStatement(((IScriptExpression<ScriptCodeExpression>)value).CodeObject)) :
                base.Convert(value, state);
        }

        IScriptStatement<TStatementKind> IScriptCodeElementFactory<TStatementKind, IScriptStatement<TStatementKind>>.CreateCodeElement(IList<IScriptObject> args, InterpreterState state)
        {
            return CreateCodeElement(args, state);
        }
    }
}
