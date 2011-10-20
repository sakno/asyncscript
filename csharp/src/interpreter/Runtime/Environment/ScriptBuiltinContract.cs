using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace DynamicScript.Runtime.Environment
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;
    using Expression = System.Linq.Expressions.Expression;
    using BindingRestrictions = System.Dynamic.BindingRestrictions;
    using InterpretationContext = Compiler.Ast.InterpretationContext;
    using Keyword = Compiler.Keyword;

    /// <summary>
    /// Represents DynamicScript predefined contract.
    /// </summary>
    [ComVisible(false)]
    [Serializable]
    public abstract class ScriptBuiltinContract: ScriptContract, ISerializable
    {
#if USE_REL_MATRIX
        private int? m_hashCode;
#endif

        internal ScriptBuiltinContract()
        {
        }

        internal abstract Keyword Token
        {
            get;
        }

        /// <summary>
        /// Returns a string representation of the current contract.
        /// </summary>
        /// <returns></returns>
        public sealed override string ToString()
        {
            return Token.ToString();
        }

        /// <summary>
        /// Returns a contract binding for the current contract.
        /// </summary>
        /// <returns>The contract binding for the current contract.</returns>
        /// <remarks>Contract binding for the DynamicScript contracts is represented by meta contract('type').</remarks>
        public override IScriptContract GetContractBinding()
        {
            return ScriptMetaContract.Instance;
        }

        /// <summary>
        /// Creates a clone of the built-in contract.
        /// </summary>
        /// <returns>The clone of the built-in contract.</returns>
        /// <remarks>All built-in contracts are represented by immutable object therefore, it is not necessary to create a clone version.</remarks>
        protected sealed override ScriptObject Clone()
        {
            return this;
        }

        /// <summary>
        /// Creates default object according with this built-in contract.
        /// </summary>
        /// <param name="args"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        public override ScriptObject CreateObject(IList<IScriptObject> args, InterpreterState state)
        {
            if (args.Count == 0)
                return FromVoid(state);
            else throw new ActionArgumentsMistmatchException(state);
        }

        /// <summary>
        /// Serializes the built-in contract.
        /// </summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        protected virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
        }

        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
        {
            GetObjectData(info, context);   
        }

        /// <summary>
        /// Computes a hash code for this contract.
        /// </summary>
        /// <returns></returns>
        public sealed override int GetHashCode()
        {
#if USE_REL_MATRIX
            if (m_hashCode == null) m_hashCode = GetType().MetadataToken;
            return m_hashCode.Value;
#else
            return GetType().MetadataToken;
#endif
        }
    }
}
