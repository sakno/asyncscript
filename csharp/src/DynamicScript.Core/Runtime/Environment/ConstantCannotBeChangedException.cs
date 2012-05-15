using System;

namespace DynamicScript.Runtime.Environment
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;

    /// <summary>
    /// Represents an exception occured when user code attempts to overwrite constant value.
    /// This class cannot be inherited.
    /// </summary>
    [ComVisible(false)]
    
    public sealed class ConstantCannotBeChangedException : RuntimeException
    {
        internal ConstantCannotBeChangedException(InterpreterState state)
            : base(ErrorMessages.AttemptsToWriteConstant, InterpreterErrorCode.ConstantValueCannotBeChanged, state)
        {
        }
    }
}
