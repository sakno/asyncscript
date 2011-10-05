using System;
using System.Dynamic;
using System.Linq.Expressions;
using System.Runtime.Serialization;
using System.Collections.Generic;

namespace DynamicScript.Runtime.Environment
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;
    using InterpretationContext = Compiler.Ast.InterpretationContext;
    using QCodeUnaryOperatorType = Compiler.Ast.ScriptCodeUnaryOperatorType;
    using Keyword = Compiler.Keyword;

    /// <summary>
    /// Represents void object.
    /// This class cannot be inherited.
    /// </summary>
    [ComVisible(false)]
    
    [Serializable]
    public sealed class ScriptVoid : ScriptContract, ISerializable
    {
        private ScriptVoid(SerializationInfo info, StreamingContext context)
        {
        }

        private ScriptVoid()
        {
            
        }

        /// <summary>
        /// Represents an instance of the void object.
        /// </summary>
        public static ScriptVoid Instance = new ScriptVoid();

        /// <summary>
        /// Returns contract binding for void object.
        /// </summary>
        /// <returns>Returns contract binding for void object.</returns>
        public override IScriptContract GetContractBinding()
        {
            return this;
        }

        /// <summary>
        /// Returns coalesce result.
        /// </summary>
        /// <param name="right">The right operand of coalescing operation.</param>
        /// <param name="state">Internal interpreter state.</param>
        /// <returns>The operation result.</returns>
        protected override IScriptObject Coalesce(IScriptObject right, InterpreterState state)
        {
            return right;
        }

        /// <summary>
        /// Determines whether the current object is void.
        /// </summary>
        /// <param name="state"></param>
        /// <returns></returns>
        protected override IScriptObject IsVoid(InterpreterState state)
        {
            return ScriptBoolean.True;
        }

        /// <summary>
        /// Returns string representation of the void object.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return Keyword.Void;
        }

        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
        {
            
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="right"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        protected override IScriptObject Add(IScriptObject right, InterpreterState state)
        {
            return IsVoid(right) ? Void : right.Add(this, state);
        }

        internal override IScriptContract Intersect(IScriptContract right, InterpreterState state)
        {
            return this;
        }

        internal override IScriptContract Unite(IScriptContract right, InterpreterState state)
        {
            return right;
        }

        internal override IScriptObject Complement(InterpreterState state)
        {
            return ScriptSuperContract.Instance;
        }

        /// <summary>
        /// Returns relationship between the current contract and the specified.
        /// </summary>
        /// <param name="contract">The contract to compare. Cannot be <see langword="null"/>.</param>
        /// <returns>Relationship between the current contract and <paramref name="contract"/>.</returns>
        public override ContractRelationshipType GetRelationship(IScriptContract contract)
        {
            return contract is ScriptVoid ? ContractRelationshipType.TheSame : ContractRelationshipType.Subset;
        }


        /// <summary>
        /// Always throws <see cref="VoidException"/> exception.
        /// </summary>
        /// <param name="args"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        public override ScriptObject CreateObject(IList<IScriptObject> args, InterpreterState state)
        {
            throw new VoidException(state);
        }
    }
}
