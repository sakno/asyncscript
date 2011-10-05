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
    sealed class RealLiteral : Literal<double>
    {
        private readonly NumberStyles m_style;

        private RealLiteral(string num, NumberStyles style)
            : base(num)
        {
            m_style = style;
        }

        public RealLiteral(double value)
            : this(value.ToString(CultureInfo.InvariantCulture), NumberStyles.Float)
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
        public override double Value
        {
            get
            {
                return double.Parse(ToString(), Style, CultureInfo.InvariantCulture);
            }
        }

        private static long Parse(string literal, NumberStyles style)
        {
            switch (style)
            {
                case NumberStyles.Float: return long.Parse(literal, style);
                default: return 0L;
            }
        }

        /// <summary>
        /// Creates a new lexeme that represents integer literal in the decimal form.
        /// </summary>
        /// <param name="floatValue">The string that represents integer literal. Cannot be <see langword="null"/> or empty.</param>
        /// <returns>A new instance of the lexeme that represents integer literal.</returns>
        /// <exception cref="System.ArgumentNullException"><paramref name="floatValue"/> is <see langword="null"/> or empty.</exception>
        public static RealLiteral CreateFloat(string floatValue)
        {
            if (floatValue == null) throw new ArgumentNullException("floatValue");
            return new RealLiteral(floatValue, NumberStyles.Float);
        }

        public static RealLiteral CreateFloat(StringBuilder decimalLit)
        {
            return CreateFloat(decimalLit != null ? decimalLit.ToString() : String.Empty);
        }

        public static implicit operator double(RealLiteral literal)
        {
            return literal != null ? literal.Value : default(long);
        }
    }
}
