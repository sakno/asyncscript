using System;

namespace DynamicScript.Compiler.Ast.Translation
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;

    /// <summary>
    /// Represents information about file with DynamicScript source code.
    /// </summary>
    /// <typeparam name="TDebugSource">Type of the debug information.</typeparam>
    
    [ComVisible(false)]
    public abstract class SourceCodeInfo<TDebugSource>
    {
        private readonly string m_fileName;
        private readonly TDebugSource m_debug;

        /// <summary>
        /// Initializes a new information about source code.
        /// </summary>
        /// <param name="fileName">The path to the source file. Cannot be <see langword="null"/> or empty.</param>
        /// <param name="debugSource">The debugging information source.</param>
        /// <exception cref="System.ArgumentNullException"><paramref name="fileName"/> is <see langword="null"/> or empty.</exception>
        protected SourceCodeInfo(string fileName, TDebugSource debugSource)
        {
            if (string.IsNullOrEmpty(fileName)) throw new ArgumentNullException("fileName");
            m_debug = debugSource;
            m_fileName = fileName;
        }

        /// <summary>
        /// Gets path to the DynamicScript source file.
        /// </summary>
        public string FileName
        {
            get { return m_fileName; }
        }

        /// <summary>
        /// Gets debugging information source.
        /// </summary>
        public TDebugSource DebugSource
        {
            get { return m_debug; }
        }
    }
}
