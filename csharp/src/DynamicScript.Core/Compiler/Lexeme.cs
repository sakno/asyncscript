using System;

namespace DynamicScript.Compiler
{
    using StringBuilder = System.Text.StringBuilder;
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;

    /// <summary>
    /// Represents an abstract class for buildig lexemes and tokens.
    /// </summary>
    [ComVisible(false)]
    [Serializable]
    abstract class Lexeme: IEquatable<string>, IEquatable<Lexeme>, IEquatable<int>
    {
        #region Nested Types
        /// <summary>
        /// Represents position of the lexeme in the source code.
        /// </summary>
        [Serializable]
        [ComVisible(false)]
        public struct Position
        {
            private readonly int m_column;
            private readonly int m_line;

            public Position(int line, int column)
            {
                m_column = column;
                m_line = line;
            }

            /// <summary>
            /// Gets column number of the lexeme beginning.
            /// </summary>
            public int Column
            {
                get { return m_column; }
            }
            
            /// <summary>
            /// Gets lexeme line number in the source code.
            /// </summary>
            public int Line
            {
                get { return m_line; }
            }
        }
        #endregion

        /// <summary>
        /// Represents percent character.
        /// </summary>
        public const char Percent = '%';

        /// <summary>
        /// Represents part of the name token.
        /// </summary>
        public const char Line = '_';

        /// <summary>
        /// Represents colon character.
        /// </summary>
        public const char Colon = ':';

        /// <summary>
        /// Represents semicolon character.
        /// </summary>
        public const char Semicolon = ';';

        /// <summary>
        /// Represents comma character.
        /// </summary>
        public const char Comma = ',';

        /// <summary>
        /// Represents '+' character.
        /// </summary>
        public const char Plus = '+';

        /// <summary>
        /// Represents asterisk character.
        /// </summary>
        public const char Asterisk = '*';

        /// <summary>
        /// Represents new line character.
        /// </summary>
        public const char NewLine = '\n';

        /// <summary>
        /// Represents carriage return character.
        /// </summary>
        public const char CarriageReturn = '\r';

        /// <summary>
        /// Represents tab space character.
        /// </summary>
        public const char TabSpace = '\t';

        /// <summary>
        /// Represents dot character.
        /// </summary>
        public const char Dot = '.';

        /// <summary>
        /// Represents division operator.
        /// </summary>
        public const char Slash = '/';

        /// <summary>
        /// Represents assignment operator.
        /// </summary>
        public const char Assignment = '=';

        /// <summary>
        /// Represents subtraction operator.
        /// </summary>
        public const char Minus = '-';

        /// <summary>
        /// Represents left brace.
        /// </summary>
        public const char LeftBrace = '{';

        /// <summary>
        /// Represents right brace.
        /// </summary>
        public const char RightBrace = '}';

        /// <summary>
        /// Represents left bracket.
        /// </summary>
        public const char LeftBracket = '(';

        /// <summary>
        /// Represents right bracket.
        /// </summary>
        public const char RightBracket = ')';

        /// <summary>
        /// Represents left square bracket.
        /// </summary>
        public const char LeftSquareBracket = '[';

        /// <summary>
        /// Represents right square bracket.
        /// </summary>
        public const char RightSquareBracket = ']';

        /// <summary>
        /// Represents exclamation.
        /// </summary>
        public const char Exclamation = '!';

        /// <summary>
        /// Represents ampersand.
        /// </summary>
        public const char Ampersand = '&';

        /// <summary>
        /// Represents vertical line.
        /// </summary>
        public const char Vertical = '|';

        /// <summary>
        /// Represents 'less than' operator.
        /// </summary>
        public const char LessThan = '<';

        /// <summary>
        /// Represents 'greater than' operator.
        /// </summary>
        public const char GreaterThan = '>';

        /// <summary>
        /// Represents ^ character.
        /// </summary>
        public const char Roof = '^';

        /// <summary>
        /// Represents " character.
        /// </summary>
        public const char SQuote = '"';

        /// <summary>
        /// Represents ' character.
        /// </summary>
        public const char CQuote = '\'';

        /// <summary>
        /// Represents back-slash.
        /// </summary>
        public const char BackSlash = '\\';

        /// <summary>
        /// Represents @ character.
        /// </summary>
        public const char Dog = '@';

        /// <summary>
        /// Represents ? character.
        /// </summary>
        public const char Question = '?';

        /// <summary>
        /// Represents $ character.
        /// </summary>
        public const char Dollar = '$';

        /// <summary>
        /// Represents white space.
        /// </summary>
        public const char WhiteSpace = ' ';

        /// <summary>
        /// Represents diez.
        /// </summary>
        public const char Diez = '#';

        /// <summary>
        /// Represents backquote.
        /// </summary>
        public const char Backquote = '`';

        /// <summary>
        /// Represents tilda.
        /// </summary>
        public const char Tilda = '~';

        private int? m_hash;

        /// <summary>
        /// Initializes a new lexeme.
        /// </summary>
        protected Lexeme()
        {
            m_hash = null;
        }

        /// <summary>
        /// Gets a string that represents the current lexeme.
        /// </summary>
        protected abstract string Value
        {
            get;
        }

        /// <summary>
        /// Returns a string that represents the current lexeme.
        /// </summary>
        /// <returns>The string that represents the current lexeme.</returns>
        public sealed override string ToString()
        {
            return Value;
        }

        /// <summary>
        /// Determines whether the current lexeme represents the same
        /// string as other.
        /// </summary>
        /// <param name="other">Other string to compare.</param>
        /// <returns><see langword="true"/> if the current lexeme represents the same
        /// string as the specified string; otherwise, <see langword="false"/>.</returns>
        /// <remarks>
        /// DynamicScript language is not case-sensitive, therefore this method
        /// provides case insensitive comparison.
        /// </remarks>
        public bool Equals(string other)
        {
            return StringEqualityComparer.Equals(Value, other);
        }

        /// <summary>
        /// Determines whether the current lexeme is equal to one of the specified lexemes.
        /// </summary>
        /// <param name="lexemes">Other lexemes to compare.</param>
        /// <returns><see langword="true"/> if the current lexeme is equal to one of the specified lexemes; 
        /// otherwise, <see langword="false"/>.</returns>
        public bool OneOf(params Lexeme[] lexemes)
        {
            foreach (var lex in lexemes)
                if (Equals(lex)) return true;
            return false;
        }

        public bool OneOf<T1, T2>()
            where T1 : Lexeme
            where T2 : Lexeme
        {
            return this is T1 || this is T2;
        }

        public bool OneOf<T1, T2, T3>()
            where T1 : Lexeme
            where T2 : Lexeme
            where T3 : Lexeme
        {
            return OneOf<T1, T2>() || this is T3;
        }

        public bool OneOf<T1, T2, T3, T4>()
            where T1 : Lexeme
            where T2 : Lexeme
            where T3 : Lexeme
            where T4 : Lexeme
        {
            return OneOf<T1, T2, T3>() || this is T4;
        }

        public bool OneOf<T1, T2, T3, T4, T5>()
            where T1 : Lexeme
            where T2 : Lexeme
            where T3 : Lexeme
            where T4 : Lexeme
            where T5 : Lexeme
        {
            return OneOf<T1, T2, T3, T4>() || this is T5;
        }

        private bool Equals(int hashCode)
        {
            return GetHashCode()==hashCode;
        }

        private bool Equals(Lexeme other)
        {
            return Equals(other.GetHashCode());
        }

        bool IEquatable<int>.Equals(int hashCode)
        {
            return Equals(hashCode);
        }

        bool IEquatable<Lexeme>.Equals(Lexeme other)
        {
            return Equals(other);
        }

        /// <summary>
        /// Determines whether the current lexeme is equal to the specified lexeme.
        /// </summary>
        /// <param name="other">Other lexeme to compare.</param>
        /// <returns><see langword="true"/> if the current lexeme  is equal to the specified lexeme; 
        /// otherwise, <see langword="false"/>.</returns>
        /// <remarks>
        /// DynamicScript language is not case-sensitive, therefore this method
        /// provides case insensitive comparison.
        /// </remarks>
        public sealed override bool Equals(object other)
        {
            if (other is string)
                return Equals((string)other);
            else if (other is Lexeme)
                return Equals((Lexeme)other);
            else if (other is int)
                return Equals((int)other);
            else return false;
        }

        /// <summary>
        /// Returns a hash code that uniquely identifies
        /// the current lexeme.
        /// </summary>
        /// <returns>A hash code that uniquely identifies
        /// the current lexeme.</returns>
        public sealed override int GetHashCode()
        {
            if (m_hash == null) m_hash = StringEqualityComparer.GetHashCode(Value);
            return m_hash.Value;
        }

        /// <summary>
        /// Determines whether the two lexemes are equal.
        /// </summary>
        /// <param name="lex1">The first lexeme to compare.</param>
        /// <param name="lex2">The second lexeme to compare.</param>
        /// <returns><see langword="true"/> if the two lexemes are equal;
        /// otherwise, <see langword="false"/>.</returns>
        public static bool operator ==(Lexeme lex1, Lexeme lex2)
        {
            return Equals(lex1, lex2);
        }

        /// <summary>
        /// Determines whether the two lexemes are not equal.
        /// </summary>
        /// <param name="lex1">The first lexeme to compare.</param>
        /// <param name="lex2">The second lexeme to compare.</param>
        /// <returns><see langword="true"/> if the two lexemes are not equal;
        /// otherwise, <see langword="false"/>.</returns>
        public static bool operator !=(Lexeme lex1, Lexeme lex2)
        {
            return !Equals(lex1, lex2);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="lex1"></param>
        /// <param name="hashCode"></param>
        /// <returns></returns>
        public static bool operator ==(Lexeme lex1, int hashCode)
        {
            return lex1 != null && lex1.Equals(hashCode);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="lex1"></param>
        /// <param name="hashCode"></param>
        /// <returns></returns>
        public static bool operator !=(Lexeme lex1, int hashCode)
        {
            return lex1 != null && !lex1.Equals(hashCode);
        }

        /// <summary>
        /// Converts lexeme to its textual representation.
        /// </summary>
        /// <param name="lex">The lexeme to convert.</param>
        /// <returns>Textual representation of the lexeme.</returns>
        public static implicit operator string(Lexeme lex)
        {
            return lex != null ? lex.Value : String.Empty;
        }

        /// <summary>
        /// Concatenates two lexemes into the array.
        /// </summary>
        /// <param name="lex1">The first lexeme to concatenate.</param>
        /// <param name="lex2">The second lexeme to concatenate.</param>
        /// <returns></returns>
        public static Lexeme[] operator +(Lexeme lex1, Lexeme lex2)
        {
            switch ((lex1 != null ? 0x01 : 0) + (lex2 != null ? 0x02 : 0))
            {
                case 0x03:
                    return new[] { lex1, lex2 };
                case 0x02:
                    return new[] { lex2 };
                case 0x01:
                    return new[] { lex1 };
                default:
                    return new Lexeme[0];
            }
        }

        /// <summary>
        /// Adds a new lexeme to the array.
        /// </summary>
        /// <param name="lexemes"></param>
        /// <param name="lex"></param>
        /// <returns></returns>
        public static Lexeme[] operator +(Lexeme[] lexemes, Lexeme lex)
        {
            if (lexemes == null) lexemes = new Lexeme[0];
            switch (lex != null)
            {
                case true:
                    var initialSize = lexemes.Length;
                    Array.Resize(ref lexemes, initialSize + 1);
                    lexemes[initialSize] = lex;
                    return lexemes;
                default: return lexemes;
            }
        }
    }
}
