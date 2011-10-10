using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace DynamicScript.Compiler
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;
    using StringBuilder = System.Text.StringBuilder;
    using BindingFlags = System.Reflection.BindingFlags;

    /// <summary>
    /// Represents DynamicScript lexical analyzer.
    /// This class cannot be inherited.
    /// </summary>
    [ComVisible(false)]
    sealed class LexemeAnalyzer : IEnumerator<KeyValuePair<Lexeme.Position, Lexeme>>
    {
        private static readonly IDictionary<string, Keyword> m_keywordMap;

        static LexemeAnalyzer()
        {
            const BindingFlags PublicFields = BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly;
            var keywordTokenType = typeof(Keyword);
            var keyWordTokenFields = keywordTokenType.GetFields(PublicFields);
            m_keywordMap = new Dictionary<string, Keyword>(keyWordTokenFields.Length, new StringEqualityComparer());
            foreach (var field in keywordTokenType.GetFields(PublicFields))
                if (field.FieldType.Equals(keywordTokenType))
                    m_keywordMap.Add((Keyword)field.GetValue(null));
        }

        private readonly IEnumerator<char> m_characters;
        private bool m_disposed;
        private KeyValuePair<Lexeme.Position, Lexeme> m_current;
        private int m_column;
        private int m_line;
        private bool m_next;

        private LexemeAnalyzer()
        {
            m_disposed = false;
            m_column = 0;
            m_line = 1;
            m_next = false;
        }

        /// <summary>
        /// Initializes a new lexical analyzer.
        /// </summary>
        /// <param name="characters">An enumerator through characters in the source code. Cannot be <see langword="null"/>.</param>
        /// <exception cref="System.ArgumentNullException"><paramref name="characters"/> is <see langword="null"/>.</exception>
        public LexemeAnalyzer(IEnumerator<char> characters)
            :this()
        {
            if (characters == null) throw new ArgumentNullException("characters");
            m_characters = characters;
        }

        public LexemeAnalyzer(IEnumerable<char> sourceCode)
            : this(sourceCode != null ? sourceCode.GetEnumerator() : String.Empty.GetEnumerator())
        {
        }

        private string ObjectName
        {
            get { return GetType().Name; }
        }

        /// <summary>
        /// Gets hash table of the DynamicScript keywords.
        /// </summary>
        public static IDictionary<string, Keyword> KeywordTable
        {
            get { return m_keywordMap; }
        }

        [DebuggerNonUserCode]
        [DebuggerHidden]
        private void VerifyOnDisposed()
        {
            if (m_disposed) throw new ObjectDisposedException(ObjectName);
        }

        /// <summary>
        /// Gets the currently parsed lexeme.
        /// </summary>
        /// <exception cref="System.ObjectDisposedException">The analyzer is closed.</exception>
        public KeyValuePair<Lexeme.Position, Lexeme> Current
        {
            get 
            {
                VerifyOnDisposed();
                return m_current;
            }
        }

        object System.Collections.IEnumerator.Current
        {
            get { return Current; }
        }

        /// <summary>
        /// Moves to the next lexeme in the source code.
        /// </summary>
        /// <returns><see langword="true"/> if the end of the source code is not reached;
        /// otherwise, <see langword="false"/>.</returns>
        public bool MoveNext()
        {
            VerifyOnDisposed();
            if (!m_next) m_next = m_characters.MoveNext();
            return m_next ? MoveNext(m_characters, ref m_column, ref m_line, out m_current, out m_next) : false;
        }

        private static bool MoveNext(IEnumerator<char> characters, ref int column, ref int line, out KeyValuePair<Lexeme.Position, Lexeme> state, out bool nextRequired)
        {
            var lexeme = Parse(characters, ref column, ref line, out nextRequired);
            if (lexeme != null)
            {
                state = new KeyValuePair<Lexeme.Position, Lexeme>(new Lexeme.Position(line, column), lexeme);
                return true;
            }
            else if (characters.MoveNext())
                return MoveNext(characters, ref column, ref line, out state, out nextRequired);
            else
            {
                state = new KeyValuePair<Lexeme.Position, Lexeme>(new Lexeme.Position(line, column), lexeme);
                return false;
            }
        }

        private static Lexeme Parse(IEnumerator<char> characters, ref int column, ref int line, out bool hasNext)
        {
            hasNext = false;
            switch (characters.Current)
            {
                #region Punctuation
                case Lexeme.Diez:
                    return ParseMacro(characters, ref column, out hasNext);
                case Lexeme.Dog:
                    return ParseDog(characters, ref column, out hasNext);
                case Lexeme.Comma:
                    column++;
                    return Punctuation.Comma;
                case Lexeme.Colon:
                    return ParseColon(characters, ref column, out hasNext);
                case Lexeme.LeftBrace:
                    column++;
                    return Punctuation.LeftBrace;
                case Lexeme.RightBrace:
                    column++;
                    return Punctuation.RightBrace;
                case Lexeme.SQuote: //parse Unicode string
                    return ParseStringLiteral(characters, ref column, ref line, out hasNext);
                case Lexeme.CQuote:
                    return ParseVerbatimStringLiteral(characters, ref column, ref line, out hasNext);
                case Lexeme.LeftBracket:
                    column++;
                    return Punctuation.LeftBracket;
                case Lexeme.RightBracket:
                    column++;
                    return Punctuation.RightBracket;
                case Lexeme.Semicolon:
                    column++;
                    return Punctuation.Semicolon;
                case Lexeme.LeftSquareBracket:
                    column++;
                    return Punctuation.LeftSquareBracket;
                case Lexeme.RightSquareBracket:
                    column++;
                    return Punctuation.RightSquareBracket;
                #endregion
                #region Operators
                case Lexeme.Percent:
                    return ParseModuloOperator(characters, ref column, out hasNext);
                case Lexeme.Question:
                    return ParseQuestion(characters, ref column, out hasNext);
                case Lexeme.Dollar:
                    column++;
                    return Operator.TypeOf;
                case Lexeme.Roof:
                    return ParseExclusion(characters, ref column, out hasNext);
                case Lexeme.LessThan:
                    return ParseLT(characters, ref column, out hasNext);
                case Lexeme.GreaterThan:
                    return ParseGT(characters, ref column, out hasNext);
                case Lexeme.Vertical:
                    return ParseVertical(characters, ref column, out hasNext);
                case Lexeme.Ampersand:
                    return ParseAmpersand(characters, ref column, out hasNext);
                case Lexeme.Assignment:
                    return ParseAssignmentOperator(characters, ref column, out hasNext);
                case Lexeme.Exclamation:
                    return ParseExclamation(characters, ref column, out hasNext);
                case Lexeme.Dot:
                    column++;
                    return Operator.MemberAccess;
                case Lexeme.Minus:
                    return ParseSubtractionOperator(characters, ref column, out hasNext);
                case Lexeme.Asterisk:
                    return ParseMultiplicationOperator(characters, ref column, out hasNext);
                case Lexeme.Plus:
                    return ParsePlusOperator(characters, ref column, out hasNext);
                case Lexeme.Slash:
                    return ParseDivision(characters, ref column, ref line, out hasNext);
                #endregion
                #region White spaces
                case Lexeme.TabSpace:
                    column++;
                    return null;
                case Lexeme.NewLine:
                    column = 1;
                    line++;
                    return null;
                case Lexeme.CarriageReturn: return null;
                #endregion
                default:
                    if (char.IsDigit(characters.Current))
                        return ParseNumber(characters, ref column, line, out hasNext);
                    if (char.IsWhiteSpace(characters.Current))
                    {
                        column++;
                        return null;
                    }
                    else if (char.IsLetter(characters.Current) || characters.Current == Lexeme.Line)
                        return ParseToken(characters, ref column, out hasNext);
                    break;
            }
            throw CodeAnalysisException.UnknownCharacter(characters.Current, column++, line);
        }

        private static bool ParseEscapeSequence(IEnumerator<char> characters, ref int column, out char result)
        {
            result = default(char);
            switch (characters.Current)
            {
                case Lexeme.BackSlash:
                    if (!characters.MoveNext()) return false; //Invalid escape sequence
                    column++;
                    switch (characters.Current)
                    {
                        case 'r': result = '\r'; return true;   //carriage return
                        case 'n': result = '\n'; return true;   //new line
                        case 't': result = '\t'; return true;   //horizontal tab
                        case 'v': result = '\v'; return true;   //vertical tab
                        case 'f': result = '\f'; return true;   //form feed
                        case 'b': result = '\b'; return true;   //backspace
                        case 'a': result = '\a'; return true;   //alert
                        case '0': result = '\0'; return true;   //terminator
                        case '\'': result = '\''; return true;  //verbatim string quote
                        case '\"': result = '\"'; return true;  //string quote
                        case '\\': result = '\\'; return true;  //back slash
                        default: result = characters.Current; return true;
                    }
                default:
                    result = characters.Current;
                    return true;
            }
        }

        #region Token Handlers

        private static StringLiteral ParseVerbatimStringLiteral(IEnumerator<char> characters, ref int column, ref int line, out bool hasNext)
        {
            column++;
            var literal = new StringBuilder();
            hasNext = false;
            while (characters.MoveNext() && characters.Current != Lexeme.CQuote)
            {
                column++;
                if (characters.Current == '\n') { column = 0; line++; }
                literal.Append(characters.Current);
            }
            return new StringLiteral(literal.ToString());
        }

        private static StringLiteral ParseStringLiteral(IEnumerator<char> characters, ref int column, ref int line, out bool hasNext)
        {
            column++;
            var literal = new StringBuilder();
            hasNext = false;
            while (characters.MoveNext() && characters.Current!=Lexeme.SQuote)
            {
                column++;
                var c = default(char);
                switch(ParseEscapeSequence(characters, ref column, out c))
                {
                    case true:
                        if (c == '\n') { column = 0; line++; }
                        literal.Append(c);
                        continue;
                    default:
                        throw CodeAnalysisException.WrongEscapeSequence(column, line);
                }
            }
            return new StringLiteral(literal.ToString());
        }

        private static PlaceholderID ParsePlaceholder(IEnumerator<char> characters, ref int column, out bool hasNext)
        {
            column++;
            hasNext = false;
            var builder = new StringBuilder();
            while ((hasNext = characters.MoveNext()) && char.IsDigit(characters.Current))
                {
                    builder.Append(characters.Current);
                    column++;
                }
            return new PlaceholderID(builder);
        }

        private static Lexeme ParseModuloOperator(IEnumerator<char> characters, ref int column, out bool hasNext)
        {
            column++;
            hasNext = false;
            if (characters.MoveNext())
            {
                column++;
                switch (characters.Current)
                {
                    case Lexeme.Assignment:
                        return Operator.ModuloAssign;
                    case Lexeme.Percent:
                        return ParsePlaceholder(characters, ref column, out hasNext);
                    default:
                        hasNext = true;
                        break;
                }
            }
            return Operator.Modulo;
        }

        private static Operator ParsePlusOperator(IEnumerator<char> characters, ref int column, out bool hasNext)
        {
            column++;
            hasNext = false;
            if (characters.MoveNext())
            {
                column++;
                switch (characters.Current)
                {
                    case Lexeme.Plus:
                        return Operator.DoublePlus;
                    case Lexeme.Assignment:
                        return Operator.PlusAssignment;
                    default:
                        hasNext = true;
                        break;
                }
            }
            return Operator.Plus;
        }

        private static Operator ParseAmpersand(IEnumerator<char> characters, ref int column, out bool hasNext)
        {
            column++;
            hasNext = false;
            if (characters.MoveNext())
            {
                column++;
                switch (characters.Current)
                {
                    case Lexeme.Assignment:
                        return Operator.IntersectionAssignment;
                    case Lexeme.Ampersand:
                        return Operator.AndAlso;
                    default:
                        hasNext = true;
                        break;
                }
            }
            return Operator.Intersection;
        }

        private static Token ParseColon(IEnumerator<char> characters, ref int column, out bool hasNext)
        {
            column++;
            hasNext = false;
            if (characters.MoveNext())
            {
                column++;
                switch (characters.Current)
                {
                    case Lexeme.Colon:
                        return Operator.MetadataDiscovery;
                    default:
                        hasNext = true;
                        break;
                }
            }
            return Punctuation.Colon;
        }

        private static Punctuation ParseDog(IEnumerator<char> characters, ref int column, out bool hasNext)
        {
            column++;
            hasNext = false;
            if (characters.MoveNext())
            {
                column++;
                switch (characters.Current)
                {
                    case Lexeme.Dog:
                        return Punctuation.DoubleDog;
                    default:
                        hasNext = true;
                        break;
                }
            }
            return Punctuation.Dog;
        }

        private static Operator ParseQuestion(IEnumerator<char> characters, ref int column, out bool hasNext)
        {
            column++;
            hasNext = false;
            if (characters.MoveNext())
            {
                column++;
                switch (characters.Current)
                {
                    case Lexeme.Question:
                        return Operator.Coalesce;
                    default:
                        hasNext = true;
                        break;
                }
            }
            return Operator.VoidCheck;
        }

        private static Operator ParseVertical(IEnumerator<char> characters, ref int column, out bool hasNext)
        {
            column++;
            hasNext = false;
            if (characters.MoveNext())
            {
                column++;
                switch (characters.Current)
                {
                    case Lexeme.Assignment:
                        return Operator.UnionAssignment;
                    case Lexeme.Vertical:
                        return Operator.OrElse;
                    default:
                        hasNext = true;
                        break;
                }
            }
            return Operator.Union;
        }

        private static Operator ParseExclusion(IEnumerator<char> characters, ref int column, out bool hasNext)
        {
            column++;
            hasNext = false;
            if (characters.MoveNext())
            {
                column++;
                switch (characters.Current)
                {
                    case Lexeme.Assignment:
                        return Operator.ExclusionAssignment;
                    default:
                        hasNext = true;
                        break;
                }
            }
            return Operator.Exclusion;
        }

        private static Operator ParseLT(IEnumerator<char> characters, ref int column, out bool hasNext)
        {
            column++;
            hasNext = false;
            if (characters.MoveNext())
            {
                column++;
                switch (characters.Current)
                {
                    case Lexeme.Assignment:
                        return Operator.LessThanOrEqual;
                    default:
                        hasNext = true;
                        break;
                }
            }
            return Operator.LessThan;
        }

        private static Operator ParseGT(IEnumerator<char> characters, ref int column, out bool hasNext)
        {
            column++;
            hasNext = false;
            if (characters.MoveNext())
            {
                column++;
                switch (characters.Current)
                {
                    case Lexeme.Assignment:
                        return Operator.GreaterThanOrEqual;
                    default:
                        hasNext = true;
                        break;
                }
            }
            return Operator.GreaterThan;
        }

        private static Operator ParseMultiplicationOperator(IEnumerator<char> characters, ref int column, out bool hasNext)
        {
            column++;
            hasNext = false;
            if (characters.MoveNext())
            {
                column++;
                switch (characters.Current)
                {
                    case Lexeme.Asterisk:
                        return Operator.DoubleAsterisk;
                    case Lexeme.Assignment:
                        return Operator.AsteriskAssignment;
                    default:
                        hasNext = true;
                        break;
                }
            }
            return Operator.Asterisk;
        }

        private static Operator ParseExclamation(IEnumerator<char> characters, ref int column, out bool hasNext)
        {
            column++;
            switch (characters.MoveNext())
            {
                case true:
                    column++;
                    switch (characters.Current)
                    {
                        case Lexeme.Assignment:
                            column++;
                            switch (characters.MoveNext())
                            {
                                case true:
                                    column++;
                                    switch (characters.Current)
                                    {
                                        case Lexeme.Assignment:
                                            hasNext = false;
                                            return Operator.ReferenceInequality;
                                        default:
                                            hasNext = true;
                                            return Operator.ValueInequality;
                                    }
                                default:
                                    hasNext = false;
                                    return Operator.ValueInequality;
                            }
                        default:
                            hasNext = true;
                            return Operator.Negotiation;
                    }
                default:
                    hasNext = false;
                    return Operator.Negotiation;
            }
        }

        private static Operator ParseAssignmentOperator(IEnumerator<char> characters, ref int column, out bool hasNext)
        {
            column++;
            switch (characters.MoveNext())
            {
                case true:
                    column++;
                    switch (characters.Current)
                    {
                        case Lexeme.Assignment:
                            column++;
                            switch (characters.MoveNext())
                            {
                                case true:
                                    column++;
                                    switch (characters.Current)
                                    {
                                        case Lexeme.Assignment:
                                            hasNext = false;
                                            return Operator.ReferenceEquality;
                                        default:
                                            hasNext = true;
                                            return Operator.ValueEquality;
                                    }
                                default:
                                    hasNext = false;
                                    return Operator.ValueEquality;
                            }
                        default:
                            hasNext = true;
                            return Operator.Assignment;
                    }
                default:
                    hasNext = false;
                    return Operator.Assignment;
            }
        }

        private static Token ParseSubtractionOperator(IEnumerator<char> characters, ref int column, out bool hasNext)
        {
            hasNext = false;
            column++;
            if (characters.MoveNext())
            {
                column++;
                switch (characters.Current)
                {
                    case Lexeme.Minus:
                        return Operator.DoubleMinus;
                    case Lexeme.Assignment:
                        return Operator.MinusAssignment;
                    case Lexeme.GreaterThan:
                        return Punctuation.Arrow;
                    default:
                        hasNext = true;
                        break;
                }
            }
            return Operator.Minus;
        }

        private static Token ParseDivision(IEnumerator<char> characters, ref int column, ref int line, out bool hasNext)
        {
            hasNext = false;
            column++;
            if (characters.MoveNext())
            {
                column++;
                switch (characters.Current)
                {
                    case Lexeme.Slash:
                        return ParseSingleLineComment(characters, ref column, out hasNext);
                    case Lexeme.Asterisk:
                        return ParseMultiLineComment(characters, ref column, ref line, out hasNext);
                    case Lexeme.Assignment:
                        hasNext = false;
                        return Operator.SlashAssignment;
                    default:
                        hasNext = true;
                        break;
                }
            }
            return Operator.Slash;
        }

        private static Comment ParseMultiLineComment(IEnumerator<char> characters, ref int column, ref int line, out bool hasNext)
        {
            var comment = new StringBuilder();
            while (hasNext = characters.MoveNext())
                switch (characters.Current)
                {
                    case Lexeme.Asterisk:
                        column++;
                        if (hasNext = characters.MoveNext())
                            if (characters.Current == Lexeme.Slash)
                            {
                                column++;
                                hasNext = false;
                                return Comment.CreateMultiLineComment(comment.ToString());
                            }
                            else comment.Append(new[] { Lexeme.Asterisk, characters.Current });
                        continue;
                    case Lexeme.NewLine:
                        column = 0;
                        line++;
                        comment.Append(Environment.NewLine);
                        continue;
                    case Lexeme.CarriageReturn: column++; continue;
                    default:
                        column++;
                        comment.Append(characters.Current);
                        continue;
                }
            throw CodeAnalysisException.EndOfCommentExpected(column, line);
        }

        private static Comment ParseSingleLineComment(IEnumerator<char> characters, ref int column, out bool hasNext)
        {
            var comment = new StringBuilder();
            while (hasNext = characters.MoveNext())
                if (characters.Current == Lexeme.NewLine)
                {
                    column++;
                    hasNext = false;
                    break;
                }
                else
                {
                    column++;
                    comment.Append(characters.Current);
                }
            return Comment.CreateSingleLineComment(comment.ToString());
        }

        private static Macro ParseMacro(IEnumerator<char> characters, ref int column, out bool hasNext)
        {
            var command = new StringBuilder();
            while (hasNext = characters.MoveNext())
                if (characters.Current == Lexeme.NewLine)
                {
                    column++;
                    hasNext = false;
                    break;
                }
                else
                {
                    column++;
                    command.Append(characters.Current);
                }
            return new Macro(command);
        }

        private static Token ParseNumber(IEnumerator<char> characters, ref int column, int line, out bool hasNext)
        {
            var builder = new StringBuilder();
            var isFloat = false;
            do
            {
                if (characters.Current == Lexeme.Dot)
                    switch (isFloat)
                    {
                        case true:
                            hasNext = false;
                            throw CodeAnalysisException.InvalidFloatingPointNumberFormat(column, line);
                        default:
                            isFloat = true;
                            break;
                    }
                builder.Append(characters.Current);
                column++;
            } while ((hasNext = characters.MoveNext()) && (char.IsDigit(characters.Current) || characters.Current == Lexeme.Dot));
            return isFloat ? (Token)RealLiteral.CreateFloat(builder) : IntegerLiteral.CreateDecimal(builder);
        }

        private static Token ParseToken(IEnumerator<char> characters, ref int column, out bool hasNext)
        {
            var builder = new StringBuilder();
            do
            {
                column++;
                builder.Append(characters.Current);
            } while ((hasNext = characters.MoveNext()) && (char.IsLetterOrDigit(characters.Current) || characters.Current == Lexeme.Line));
            var lexeme = builder.ToString();
            var kw = default(Keyword);
            switch (KeywordTable.TryGetValue(lexeme, out kw))
            {
                case true:
                    return kw;
                default:
                    return new NameToken(lexeme);
            }
        }

        #endregion

        /// <summary>
        /// Sets analyzer to the first character in the source code.
        /// </summary>
        public void Reset()
        {
            VerifyOnDisposed();
            m_characters.Reset();
            m_line = 1;
            m_column = 1;
            m_next = false;
        }

        /// <summary>
        /// Releases all resources associated with the analyzer.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (!m_disposed && disposing)
            {
                m_characters.Dispose();
            }
            m_disposed = true;
        }

        ~LexemeAnalyzer()
        {
            Dispose(false);
        }

        public static explicit operator LexemeAnalyzer(string source)
        {
            return source != null ? new LexemeAnalyzer(source) : null;
        }
    }
}
