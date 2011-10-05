using System;

namespace DynamicScript.Runtime.Environment
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;
    using CodeAnalysisException = Compiler.CodeAnalysisException;

    /// <summary>
    /// Represents an exception occured when unary, binary or invocation operation is performed on void operand.
    /// This class cannot nbe inherited.
    /// </summary>
    [ComVisible(false)]
    
    public sealed class VoidException: RuntimeException
    {
        internal VoidException(InterpreterState state)
            : base(ErrorMessages.VoidObjectDetected, InterpreterErrorCode.OperationOnVoidPerformed, state)
        {
        }
    }
}
