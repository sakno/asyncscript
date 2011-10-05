using System;

namespace DynamicScript.Runtime
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;

    /// <summary>
    /// Represents runtime slot semantic attributes.
    /// </summary>
    
    [Serializable]
    [ComVisible(false)]
    [Flags]
    public enum RuntimeSlotAttributes
    {
        /// <summary>
        /// Represents variable runtime slot.
        /// </summary>
        None = 0,

        /// <summary>
        /// Represents read-only slot.
        /// </summary>
        Immutable = 0x01,

        /// <summary>
        /// Represents runtime slot that holds proxy or lazy object.
        /// </summary>
        Lazy = 0x02
    }
}
