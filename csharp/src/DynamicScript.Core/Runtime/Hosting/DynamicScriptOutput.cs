using System;
using System.IO;

namespace DynamicScript.Runtime.Hosting
{
    using Encoding = System.Text.Encoding;
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;

    /// <summary>
    /// Represents an abstract class for DynamicScript output stream.
    /// </summary>
    
    [ComVisible(false)]
    public abstract class DynamicScriptOutput : TextWriter
    {
        /// <summary>
        /// Gets encoding of the output strings.
        /// </summary>
        public override Encoding Encoding
        {
            get { return Encoding.Unicode; }
        }

        /// <summary>
        /// Writes DynamicScript object to the output stream.
        /// </summary>
        /// <param name="value">The object to be written.</param>
        public abstract void Write(IScriptObject value);

        /// <summary>
        /// Writes DynamicScript object and inserts line break.
        /// </summary>
        /// <param name="value">The object to be written.</param>
        public abstract void WriteLine(IScriptObject value);
    }
}
