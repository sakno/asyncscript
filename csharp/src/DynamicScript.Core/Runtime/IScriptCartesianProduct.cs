using System;
using System.Collections.Generic;

namespace DynamicScript.Runtime
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;

    /// <summary>
    /// Represents cartesian product of two or more contracts.
    /// </summary>
    [ComVisible(false)]
    public interface IScriptCartesianProduct : IScriptContract, IEnumerable<IScriptContract>
    {
        /// <summary>
        /// Gets read-only collection of contracts combined into the cartesian product.
        /// </summary>
        IList<IScriptContract> Contracts { get; }
    }
}
