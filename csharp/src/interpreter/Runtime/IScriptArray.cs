using System;
using System.Collections.Generic;

namespace DynamicScript.Runtime
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;

    /// <summary>
    /// Represents an interface for array object.
    /// </summary>
    [ComVisible(false)]
    public interface IScriptArray: IScriptObject, IEnumerable<IScriptObject>
    {
        /// <summary>
        /// Gets or sets element in the array.
        /// </summary>
        /// <param name="indicies">Position of the element.</param>
        /// <param name="state">Internal interpreter state.</param>
        /// <returns>The element of the array.</returns>
        IScriptObject this[long[] indicies, InterpreterState state]
        {
            get;
            set;
        }

        /// <summary>
        /// Gets size of the specified dimension.
        /// </summary>
        /// <param name="dimension">Zero-based dimension number.</param>
        /// <returns>The size of the specified dimension.</returns>
        long GetLength(int dimension);

        /// <summary>
        /// Gets array schema.
        /// </summary>
        /// <returns>An array schema.</returns>
        new IScriptArrayContract GetContractBinding();

        /// <summary>
        /// Converts this array to single dimensional array.
        /// </summary>
        /// <returns>A new single dimensional array that contains all elements from this array.</returns>
        IScriptArray ToSingleDimensional();
    }
}
