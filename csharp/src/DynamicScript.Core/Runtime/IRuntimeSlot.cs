using System;

namespace DynamicScript.Runtime
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;
    using IScopeVariable = Microsoft.Scripting.IScopeVariable;

    /// <summary>
    /// Represents runtime slot that represents proxy object for another object.
    /// </summary>
    [ComVisible(false)]
    public interface IRuntimeSlot : IScopeVariable
    {
        /// <summary>
        /// Unwraps object from the container.
        /// </summary>
        /// <param name="state">Internal interpreter state.</param>
        /// <returns>The object extract from the slot.</returns>
        IScriptObject GetValue(InterpreterState state);

        /// <summary>
        /// Wraps the specified object to the
        /// </summary>
        /// <param name="value">The value to store in the slot.</param>
        /// <param name="state">Internal interpreter state.</param>
        /// <returns>Script object stored to the slot.</returns>
        IScriptObject SetValue(IScriptObject value, InterpreterState state);

        /// <summary>
        /// Gets semantic of the runtime slot.
        /// </summary>
        RuntimeSlotAttributes Attributes { get; }
    }
}
