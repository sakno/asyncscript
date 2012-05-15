using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DynamicScript.Compiler
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;

    /// <summary>
    /// Represents string literal. This class cannot be inherited.
    /// </summary>
    [Serializable]
    [ComVisible(false)]
    sealed class StringLiteral : Literal<string>
    {
        private readonly string m_value;

        public StringLiteral(string value)
            : base(String.Concat(Lexeme.CQuote, value, Lexeme.CQuote))
        {
            m_value = value;
        }

        public override string Value
        {
            get { return m_value; }
        }

        public static implicit operator string(StringLiteral literal)
        {
            return literal != null ? literal.Value : null;
        }
    }
}
