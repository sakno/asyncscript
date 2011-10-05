using System;

namespace DynamicScript.Runtime
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;
    using IScopeVariable = Microsoft.Scripting.IScopeVariable;

    /// <summary>
    /// Represents runtime slot that represents proxy object for another object.
    /// </summary>
    [ComVisible(false)]
    public interface IRuntimeSlot : IScriptObject, IScopeVariable, IEquatable<IRuntimeSlot>
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
        void SetValue(IScriptObject value, InterpreterState state);

        /// <summary>
        /// Returns static contract binding for the slot that is not changed
        /// during changing of the stored object.
        /// </summary>
        /// <returns>The static contract binding for the slot.</returns>
        new IScriptContract GetContractBinding();

        /// <summary>
        /// Gets semantic of the runtime slot.
        /// </summary>
        RuntimeSlotAttributes Attributes { get; }
    }
}
