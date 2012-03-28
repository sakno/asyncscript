using System;

namespace DynamicScript.Runtime.Environment
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;

    /// <summary>
    /// Represents dynamic property.
    /// </summary>
    [ComVisible(false)]
    public interface IScriptProperty: IStaticRuntimeSlot
    {
        /// <summary>
        /// Gets property getter.
        /// </summary>
        IScriptFunction Getter { get; }

        /// <summary>
        /// Gets property setter.
        /// </summary>
        IScriptFunction Setter { get; }
    }
}
