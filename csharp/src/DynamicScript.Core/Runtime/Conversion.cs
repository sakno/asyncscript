using System;

namespace DynamicScript.Runtime
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;

    /// <summary>
    /// Represents conversion type.
    /// </summary>
    [ComVisible(false)]
    [Serializable]
    public enum Conversion : byte
    {
        /// <summary>
        /// Represents implicit conversion.
        /// </summary>
        /// <remarks>Implicit conversion is transformation that saves internal state of the source object.
        /// Any changes applied to the conversion result should be reflected on the source object.</remarks>
        Implicit = 0,

        /// <summary>
        /// Represents explicit conversion.
        /// </summary>
        /// <remarks>The result of explicit conversion is newly created object which doesn't
        /// reflection internal state of the source object.</remarks>
        Explicit
    }
}
