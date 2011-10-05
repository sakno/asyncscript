using System;

namespace DynamicScript.Runtime.Environment
{
    using Compiler;
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;

    /// <summary>
    /// Represents an exception that is thrown when DynamicScript program attempts to use unary, binary or application
    /// operator with unsupported operands.
    /// This class cannot be inherited.
    /// </summary>
    [ComVisible(false)]
    
    public sealed class UnsupportedOperationException: RuntimeException
    {
        internal UnsupportedOperationException(InterpreterState state)
            : base(ErrorMessages.UnsupportedOperation, InterpreterErrorCode.UnsupportedOperation, state)
        {
        }
    }
}
