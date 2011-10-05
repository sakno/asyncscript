using System;

namespace DynamicScript.Runtime.Environment
{
    using Compiler;
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;

    /// <summary>
    /// Represents an abstract class for DynamicScript runtime exceptions.
    /// </summary>
    [ComVisible(false)]
    public abstract class RuntimeException: DynamicScriptException
    {
        private readonly InterpreterState m_state;
        private readonly InterpreterErrorCode m_code;

        internal RuntimeException(string message, InterpreterErrorCode errorCode, InterpreterState state)
            : base(message)
        {
            m_state = state;
            m_code = errorCode;
        }

        /// <summary>
        /// Gets internal state of the interpreter.
        /// </summary>
        public InterpreterState State
        {
            get { return m_state; }
        }
        
        /// <summary>
        /// Gets error code associated with the exception.
        /// </summary>
        public sealed override InterpreterErrorCode ErrorCode
        {
            get { return m_code; }
        }
    }
}
