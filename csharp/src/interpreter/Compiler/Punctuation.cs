using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DynamicScript.Compiler
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;
    using BindingFlags = System.Reflection.BindingFlags;

    /// <summary>
    /// Represents punctutation token.
    /// This class cannot be inherited.
    /// </summary>
    [Serializable]
    [ComVisible(false)]
    sealed class Punctuation : Token
    {
        private Punctuation(params char[] pun)
            : base(new string(pun))
        {
        }

        /// <summary>
        /// Represents colon.
        /// </summary>
        public static new readonly Punctuation Colon = new Punctuation(Lexeme.Colon);

        /// <summary>
        /// Represents semicolon.
        /// </summary>
        public static readonly new Punctuation Semicolon = new Punctuation(Lexeme.Semicolon);

        /// <summary>
        /// Represents comma.
        /// </summary>
        public static readonly new Punctuation Comma = new Punctuation(Lexeme.Comma);

        /// <summary>
        /// Represents left brace.
        /// </summary>
        public static readonly new Punctuation LeftBrace = new Punctuation(Lexeme.LeftBrace);

        /// <summary>
        /// Represents right brace.
        /// </summary>
        public static readonly new Punctuation RightBrace = new Punctuation(Lexeme.RightBrace);

        /// <summary>
        /// Represents left bracket.
        /// </summary>
        public static readonly new Punctuation LeftBracket = new Punctuation(Lexeme.LeftBracket);

        /// <summary>
        /// Represents right bracket.
        /// </summary>
        public static readonly new Punctuation RightBracket = new Punctuation(Lexeme.RightBracket);

        /// <summary>
        /// Represents [ bracket.
        /// </summary>
        public static readonly new Punctuation LeftSquareBracket = new Punctuation(Lexeme.LeftSquareBracket);

        /// <summary>
        /// Represents ] bracket.
        /// </summary>
        public static readonly new Punctuation RightSquareBracket = new Punctuation(Lexeme.RightSquareBracket);

        /// <summary>
        /// Represents -> token that is used to separate argument list and return type of the action.
        /// </summary>
        public static readonly Punctuation Arrow = new Punctuation(Lexeme.Minus, Lexeme.GreaterThan);

        /// <summary>
        /// Represents @ punctuation that is used to declare action type or implementation.
        /// </summary>
        public static readonly new Punctuation Dog = new Punctuation(Lexeme.Dog);

        /// <summary>
        /// Represents @@ punctuation that is used to defined quoted action.
        /// </summary>
        public static readonly Punctuation DoubleDog = new Punctuation(Lexeme.Dog, Lexeme.Dog);

        /// <summary>
        /// Represents diez characted that is reserved for macro commands.
        /// </summary>
        public static readonly new Punctuation Diez = new Punctuation(Lexeme.Diez);

        /// <summary>
        /// Gets punctuation character.
        /// </summary>
        public new char Value
        {
            get { return base.Value[0]; }
        }

        /// <summary>
        /// Converts punctuation token to the character.
        /// </summary>
        /// <param name="p">The punctuation token to be converted.</param>
        /// <returns>The character that represents punctuation.</returns>
        public static implicit operator char(Punctuation p)
        {
            return p != null ? p.Value : default(char);
        }

        /// <summary>
        /// Returns all punctuation characters.
        /// </summary>
        /// <returns>Punctuation characters.</returns>
        public static IEnumerable<Punctuation> GetPunctuationCharacters()
        {
            const BindingFlags PublicFields = BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly;
            foreach (var field in typeof(Punctuation).GetFields(PublicFields))
                yield return field.GetValue(null) as Punctuation;
        }
    }
}
