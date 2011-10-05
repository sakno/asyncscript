using System;
using System.Text;

namespace DynamicScript.Compiler
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;

    /// <summary>
    /// Represents string token.
    /// </summary>
    [Serializable]
    [ComVisible(false)]
    abstract class Token : Lexeme
    {
        private readonly string m_token;

        /// <summary>
        /// Initializes a new string token.
        /// </summary>
        /// <param name="token">A string that represents the token. Cannot be <see langword="null"/> or empty.</param>
        /// <exception cref="System.ArgumentNullException"><paramref name="token"/> is <see langword="null"/>.</exception>
        protected Token(string token)
            : base(true)
        {
            if (string.IsNullOrEmpty(token))
                throw new ArgumentNullException("token");
            m_token = token;
        }

        /// <summary>
        /// Gets token value.
        /// </summary>
        protected sealed override string Value
        {
            get { return m_token; }
        }
    }
}
