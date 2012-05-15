using System;
using System.Runtime.Serialization;

namespace DynamicScript.Runtime.Environment
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;

    /// <summary>
    /// Represents an exception occured when conversion from .NET Framework object to DynamicScript-compliant object
    /// is not supported.
    /// This class cannot be inherited.
    /// </summary>
    
    [ComVisible(false)]
    public sealed class ConversionNotSupportedException : NotSupportedException
    {
        private ConversionNotSupportedException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        /// <summary>
        /// Initializes a new instance of the exception.
        /// </summary>
        /// <param name="obj"></param>
        public ConversionNotSupportedException(object obj)
            : base(String.Format(ErrorMessages.UnsupportedConversion, obj != null ? obj.GetType() : typeof(void)))
        {
        }
    }
}
