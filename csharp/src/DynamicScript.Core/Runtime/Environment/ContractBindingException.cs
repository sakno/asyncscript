using System;

namespace DynamicScript.Runtime.Environment
{
    using Compiler;
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;

    /// <summary>
    /// Represents internal interpreter exception occured when object cannot bind to the target contract.
    /// This class cannot be inherited.
    /// </summary>
    [ComVisible(false)]
    
    public sealed class ContractBindingException: RuntimeException
    {
        private readonly IScriptObject m_obj;
        private readonly IScriptContract m_contract;

        internal ContractBindingException(IScriptObject obj, IScriptContract contract, InterpreterState state)
            : base(String.Format(ErrorMessages.ContractBinding1, ToString(obj), ToString(contract)), InterpreterErrorCode.FailedContractBinding, state)
        {
            m_obj = obj;
            m_contract = contract;
        }

        internal ContractBindingException(IScriptContract contract, InterpreterState state)
            : base(string.Format(ErrorMessages.ContractBinding2, ToString(contract)), InterpreterErrorCode.FailedContractBinding, state)
        {
        }
        
        /// <summary>
        /// Gets destination contract.
        /// </summary>
        public IScriptContract Target
        {
            get { return m_contract; }
        }

        /// <summary>
        /// Gets an object that is not satisified to the <see cref="Target"/> contract.
        /// </summary>
        public new IScriptObject Source
        {
            get { return m_obj; }
        }

        private static string ToString(IScriptObject obj)
        {
            return obj != null ? obj.ToString() : Keyword.Void;
        }
    }
}
