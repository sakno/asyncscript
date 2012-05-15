using System;

namespace DynamicScript.Runtime
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;

    /// <summary>
    /// Represents statically typed runtime slot.
    /// </summary>
    [ComVisible(false)]
    public interface IStaticRuntimeSlot: IRuntimeSlot
    {
        /// <summary>
        /// Gets static contract binding.
        /// </summary>
        IScriptContract ContractBinding { get; }
    }
}
