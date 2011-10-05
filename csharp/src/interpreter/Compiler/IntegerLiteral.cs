using System;
using System.Globalization;

namespace DynamicScript.Compiler
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;
    using StringBuilder = System.Text.StringBuilder;

    /// <summary>
    /// Represents parsed integer literal.
    /// This class cannot be inherited.
    /// </summary>
    [Serializable]
    [ComVisible(false)]
    sealed class IntegerLiteral: Literal<long>
    {
        private readonly NumberStyles m_style;

        private IntegerLiteral(string num, NumberStyles style)
            : base(num)
        {
            m_style = style;
        }

        public IntegerLiteral(long value)
            : this(value.ToString(CultureInfo.InvariantCulture), NumberStyles.Integer)
        {
        }

        /// <summary>
        /// Gets style of the parsed integer.
        /// </summary>
        public NumberStyles Style
        {
            get { return m_style; }
        }

        /// <summary>
        /// Gets parsed integer value.
        /// </summary>
        public override long Value
        {
            get 
            {
                return long.Parse(ToString(), Style);
            }
        }

        private static long Parse(string literal, NumberStyles style)
        {
            switch (style)
            {
                case NumberStyles.Integer: return long.Parse(literal, style);
                default: return 0L;
            }
        }

        /// <summary>
        /// Creates a new lexeme that represents integer literal in the decimal form.
        /// </summary>
        /// <param name="decimalLit">The string that represents integer literal. Cannot be <see langword="null"/> or empty.</param>
        /// <returns>A new instance of the lexeme that represents integer literal.</returns>
        /// <exception cref="System.ArgumentNullException"><paramref name="decimalLit"/> is <see langword="null"/> or empty.</exception>
        public static IntegerLiteral CreateDecimal(string decimalLit)
        {
            if (decimalLit == null) throw new ArgumentNullException("decimalLit");
            return new IntegerLiteral(decimalLit, NumberStyles.Integer);
        }

        public static IntegerLiteral CreateDecimal(StringBuilder decimalLit)
        {
            return CreateDecimal(decimalLit != null ? decimalLit.ToString() : String.Empty);
        }

        public static implicit operator long(IntegerLiteral literal)
        {
            return literal != null ? literal.Value : default(long);
        }
    }
}
