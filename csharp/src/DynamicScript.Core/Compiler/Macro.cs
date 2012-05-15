using System;

namespace DynamicScript.Compiler
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;
    using StringBuilder = System.Text.StringBuilder;

    /// <summary>
    /// Represents macro command.
    /// This class cannot be inherited.
    /// </summary>
    [ComVisible(false)]
    sealed class Macro: Lexeme
    {
        private readonly string m_value;

        public Macro(string command)
        {
            m_value = command ?? string.Empty;
        }

        public Macro(StringBuilder command)
            : this(command != null ? command.ToString() : null)
        {
        }

        protected override string Value
        {
            get { return m_value; }
        }
    }
}
