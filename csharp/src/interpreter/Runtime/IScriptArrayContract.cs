using System;

namespace DynamicScript.Runtime
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;

    /// <summary>
    /// Represents array contract.
    /// </summary>
    [ComVisible(false)]
    public interface IScriptArrayContract : IScriptContract
    {
        /// <summary>
        /// Gets number of dimensions.
        /// </summary>
        long Rank { get; }
        
        /// <summary>
        /// Gets contract for each element in array.
        /// </summary>
        IScriptContract ElementContract { get; }
    }
}
