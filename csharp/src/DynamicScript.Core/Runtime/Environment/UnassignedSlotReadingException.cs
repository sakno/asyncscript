using System;

namespace DynamicScript.Runtime.Environment
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;
    using Compiler;

    /// <summary>
    /// Represents an exception occured when user code attempts to read value
    /// from unassigned variable or slot.
    /// This class cannot be inherited.
    /// </summary>
    [ComVisible(false)]
    
    public sealed class UnassignedSlotReadingException: RuntimeException
    {
        internal UnassignedSlotReadingException(InterpreterState state)
            : base(ErrorMessages.ReadingFromUnassigedSlot, InterpreterErrorCode.UnassignedSlotReading, state)
        {
        }
    }
}
