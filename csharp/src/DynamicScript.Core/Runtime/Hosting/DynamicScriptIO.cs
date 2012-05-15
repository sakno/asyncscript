using System;

namespace DynamicScript.Runtime.Hosting
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;
    using ScriptObject = Environment.ScriptObject;

    /// <summary>
    /// Represents DynamicScript I/O routines.
    /// </summary>
    [ComVisible(false)]
    
    public static class DynamicScriptIO
    {
        private static DynamicScriptOutput m_output = new ScriptConsoleOutput();
        private static DynamicScriptInput m_input = new ScriptConsoleInput();
        private static ScriptErrorOutput m_error;

        /// <summary>
        /// Gets output subsystem.
        /// </summary>
        public static DynamicScriptOutput Output
        {
            get { return m_output; }
            private set { m_output = value; }
        }

        /// <summary>
        /// Gets input subsystem.
        /// </summary>
        public static DynamicScriptInput Input
        {
            get { return m_input; }
            private set { m_input = value; }
        }

        /// <summary>
        /// Gets error output subsystem.
        /// </summary>
        public static ScriptErrorOutput Error
        {
            get { return m_error; }
            private set { m_error = value; }
        }

        /// <summary>
        /// Redirects script I/O.
        /// </summary>
        /// <param name="input">The input stream.</param>
        /// <param name="output">The output stream.</param>
        /// <param name="error">The error output stream.</param>
        public static void Redirect(DynamicScriptInput input = null, DynamicScriptOutput output = null, ScriptErrorOutput error = null)
        {
            Input = input ?? new ScriptConsoleInput();
            Output = output ?? new ScriptConsoleOutput();
            Error = error;
        }

        /// <summary>
        /// Writes DynamicScript object to the output stream.
        /// </summary>
        /// <param name="value">The value to be written to the output stream.</param>
        public static void Write(IScriptObject value)
        {
            Output.Write(value);
        }

        /// <summary>
        /// Writes DynamicScript object to the output stream and breaks line.
        /// </summary>
        /// <param name="value">The value to be written to the output stream.</param>
        public static void WriteLine(IScriptObject value)
        {
            Output.WriteLine(value);
        }

        /// <summary>
        /// Reads DynamicScript object from the input stream.
        /// </summary>
        /// <returns></returns>
        public static IScriptObject ReadLine()
        {
            var result = default(IScriptObject);
            Input.ReadLine(out result);
            return result ?? ScriptObject.Void;
        }
    }
}
