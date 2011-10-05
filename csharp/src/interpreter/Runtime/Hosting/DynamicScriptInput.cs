using System;
using System.IO;

namespace DynamicScript.Runtime.Hosting
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;

    /// <summary>
    /// Represents DynamicScript input stream.
    /// </summary>
    
    [ComVisible(false)]
    public abstract class DynamicScriptInput: TextReader
    {
        /// <summary>
        /// Reads an object from the input stream.
        /// </summary>
        /// <param name="obj">The object returned from the input stream.</param>
        public abstract void ReadLine(out IScriptObject obj);
    }
}
