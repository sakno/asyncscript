using System;

namespace DynamicScript.Runtime.Environment
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;

    /// <summary>
    /// Represents runtime behavior of a script object.
    /// </summary>
    [ComVisible(false)]
    [Serializable]
    [Flags]
    enum ObjectBehavior : byte
    {
        /// <summary>
        /// No behavior is defined for script object.
        /// </summary>
        None = 0,

        /// <summary>
        /// Result of the member access binary operator should be unwrapped from runtime slot.
        /// </summary>
        UnwrapSlotValue = 0x01,
    }
}
