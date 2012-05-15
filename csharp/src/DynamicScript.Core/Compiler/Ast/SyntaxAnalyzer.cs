using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Diagnostics;

namespace DynamicScript.Compiler.Ast
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;

    /// <summary>
    /// Represents syntax analyzer.
    /// This class cannot be inherited.
    /// </summary>
    [Serializable]
    [ComVisible(false)]
    sealed class SyntaxAnalyzer : IEnumerator<ScriptCodeStatement>
    {
        private readonly IEnumerator<KeyValuePair<Lexeme.Position, Lexeme>> m_lexer;
        private ScriptCodeStatement m_current;
        private bool m_disposed;
        private readonly string m_sourceFile;

        private SyntaxAnalyzer()
        {
            m_disposed = false;
            m_current = null;
        }

        /// <summary>
        /// Initializes a new syntax analyzer.
        /// </summary>
        /// <param name="lexer">An enumerator through lexemes. This class cannot be inherited.</param>
        /// <param name="sourceFile">The path to the source file.</param>
        /// <exception cref="System.ArgumentNullException"><paramref name="lexer"/> is <see langword="null"/>.</exception>
        public SyntaxAnalyzer(IEnumerator<KeyValuePair<Lexeme.Position, Lexeme>> lexer, string sourceFile = null)
            :this()
        {
            if (lexer == null) throw new ArgumentNullException("lexer");
            m_lexer = lexer;
            m_sourceFile = sourceFile;
        }

        /// <summary>
        /// Initializes a new syntax analyzer.
        /// </summary>
        /// <param name="sourceCode">The source code.</param>
        /// <param name="sourceFile">The path to the source file.</param>
        public SyntaxAnalyzer(IEnumerable<char> sourceCode, string sourceFile = null)
            : this(new LexemeAnalyzer(sourceCode), sourceFile)
        {
        }

        public static IEnumerable<ScriptCodeStatement> Parse(IEnumerable<char> sourceCode)
        {
            using (var analyzer = new SyntaxAnalyzer(sourceCode))
                while (analyzer.MoveNext())
                    yield return analyzer.Current;
        }

        public static void Parse(IEnumerable<char> sourceCode, ICollection<ScriptCodeStatement> statements)
        {
            foreach (var stmt in Parse(sourceCode))
                statements.Add(stmt);
        }

        /// <summary>
        /// Gets currently parsed statement or expression.
        /// </summary>
        public ScriptCodeStatement Current
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
        /// Parses the next expression or statement.
        /// </summary>
        /// <returns><see langword="true"/> if the end of the lexeme stream is not reached;
        /// otherwise, <see langword="false"/>.</returns>
        public bool MoveNext()
        {
            VerifyOnDisposed();
            m_current = Parser.ParseStatement(m_lexer, Punctuation.Semicolon);
            switch (m_current != null)
            {
                case true:
                    if (m_current.LinePragma != null) m_current.LinePragma.FileName = m_sourceFile;
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>
        /// Resets syntax analyzer to its initial state.
        /// </summary>
        public void Reset()
        {
            VerifyOnDisposed();
            m_lexer.Reset();
        }

        private string ObjectName
        {
            get { return GetType().Name; }
        }

        [DebuggerNonUserCode]
        [DebuggerHidden]
        private void VerifyOnDisposed()
        {
            if (m_disposed) throw new ObjectDisposedException(ObjectName);
        }

        /// <summary>
        /// Releases all resources associated with the syntax analyzer.
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
                m_current = null;
                m_lexer.Dispose();
            }
            m_disposed = true;
        }

        ~SyntaxAnalyzer()
        {
            Dispose(false);
        }
    }
}
