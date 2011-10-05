using System;
using System.Runtime.Serialization;

namespace DynamicScript
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;

    /// <summary>
    /// Represents a base class for internal interpreter errors.
    /// </summary>
    [ComVisible(false)]
    [Serializable]
    
    public abstract class DynamicScriptException : ApplicationException
    {
        internal DynamicScriptException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            
        }

        internal DynamicScriptException(string message, Exception innerException = null)
            : base(message, innerException)
        {
        }

        /// <summary>
        /// Gets error code.
        /// </summary>
        public abstract InterpreterErrorCode ErrorCode
        {
            get;
        }
    }
}
