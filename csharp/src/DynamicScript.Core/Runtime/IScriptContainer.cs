using System;
using System.Collections.Generic;

namespace DynamicScript.Runtime
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;

    /// <summary>
    /// Represents script object that encapsulates a collection of script objects.
    /// </summary>
    [ComVisible(false)]
    public interface IScriptContainer: IScriptObject, IEnumerable<IScriptObject>
    {
        /// <summary>
        /// Determines whether the current collection contains the specified object.
        /// </summary>
        /// <param name="obj">An object to check.</param>
        /// <param name="byref">Specifies that search algorithm should provide reference comparison.</param>
        /// <param name="state">Internal interpreter state.</param>
        /// <returns><see langword="true"/> if the current collection contains the specified object; otherwise, <see langword="false"/></returns>
        bool Contains(IScriptObject obj, bool byref, InterpreterState state);
    }
}
