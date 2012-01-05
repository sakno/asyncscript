using System;

namespace DynamicScript
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;

    /// <summary>
    /// Represents statically known type of the script object.
    /// </summary>
    [ComVisible(false)]
    [Serializable]
    public enum ScriptTypeCode : byte
    {
        /// <summary>
        /// Represents unknown type.
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// Represents object with slots.
        /// </summary>
        Object = 1,

        /// <summary>
        /// Represents signature of the function
        /// </summary>
        Function = 2,

        /// <summary>
        /// Represents 64-bit signed integer.
        /// </summary>
        Integer = 3,

        /// <summary>
        /// Represents double precision floating-point number.
        /// </summary>
        Real = 4,

        /// <summary>
        /// Represents boolean data type.
        /// </summary>
        Boolean = 5,

        /// <summary>
        /// Represents ultimate type for all functional types.
        /// </summary>
        Callable = 6,

        /// <summary>
        /// Represents a root in the type hierarchy.
        /// </summary>
        Super = 7,

        /// <summary>
        /// Represents meta type that references all other types.
        /// </summary>
        Meta = 8,

        /// <summary>
        /// Represents string.
        /// </summary>
        String = 9,

        /// <summary>
        /// Represents expression tree factory.
        /// </summary>
        Expression = 10,

        /// <summary>
        /// Represents statement factory.
        /// </summary>
        Statement = 11,

        /// <summary>
        /// Represents void object.
        /// </summary>
        Void = 12,

        /// <summary>
        /// Represents array type.
        /// </summary>
        Array = 1 << 4
    }
}
