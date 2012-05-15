using System;

namespace DynamicScript.Compiler
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;

    /// <summary>
    /// Represents single-line comment.
    /// This class cannot be inherited.
    /// </summary>
    [Serializable]
    [ComVisible(false)]
    sealed class Comment : Token
    {
        private readonly bool m_multiline;

        private Comment(string comment, bool multiline)
            : base(comment)
        {
            m_multiline = multiline;
        }

        /// <summary>
        /// Gets a value indicating that the comment is multi-line.
        /// </summary>
        public bool IsMultiline
        {
            get { return m_multiline; }
        }

        public static Comment CreateSingleLineComment(string comment)
        {
            return new Comment(comment, false);
        }

        public static Comment CreateMultiLineComment(string comment)
        {
            return new Comment(comment, true);
        }

        public static readonly string SingleLineComment = new string(new[] { Slash, Slash });
    }
}
