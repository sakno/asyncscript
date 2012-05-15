using System;

namespace DynamicScript.Runtime.Environment
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;
    using IEnumerator = System.Collections.IEnumerator;

    /// <summary>
    /// Represents iterator provider.
    /// </summary>
    [ComVisible(false)]
    public interface IScriptIterable: IScriptObject
    {
        /// <summary>
        /// Returns an iterator through script objects.
        /// </summary>
        /// <param name="state"></param>
        /// <returns></returns>
        IEnumerator GetIterator(InterpreterState state);
    }
}
