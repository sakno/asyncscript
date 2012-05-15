using System;

namespace DynamicScript.Runtime.Environment
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;

    /// <summary>
    /// Represents an exception that is occured when variable or constant statement
    /// cannot bind to the specified object.
    /// This class cannot be inherited.
    /// </summary>
    
    [ComVisible(false)]
    public sealed class ContractExpectedException: DynamicScriptException
    {
        internal ContractExpectedException()
            : base(ErrorMessages.ContractExpected)
        {
        }

        /// <summary>
        /// Gets error code.
        /// </summary>
        public override InterpreterErrorCode ErrorCode
        {
            get { return InterpreterErrorCode.ContractExpected; }
        }
    }
}
