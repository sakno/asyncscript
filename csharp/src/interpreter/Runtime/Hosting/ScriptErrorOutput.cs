using System;
using System.IO;

namespace DynamicScript.Runtime.Hosting
{
    using Encoding = System.Text.Encoding;
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;

    /// <summary>
    /// Represents DynamicScript error output.
    /// </summary>
    
    [ComVisible(false)]
    public abstract class ScriptErrorOutput: TextWriter
    {
        /// <summary>
        /// Gets error output encoding.
        /// </summary>
        public override Encoding Encoding
        {
            get { return Encoding.Unicode; }
        }

        /// <summary>
        /// Writes DynamicScript-compliant exception.
        /// </summary>
        /// <param name="error">The exception to be written to the error output.</param>
        public abstract void Write(DynamicScriptException error);
    }
}
