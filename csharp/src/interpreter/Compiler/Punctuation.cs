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
        #region Nested Types
        internal static class HashCodes
        {
#if DEBUG
            internal static void PrintPunctuationValues(System.IO.TextWriter output)
            {
                const BindingFlags PublicFields = BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly;
                var puncTokenType = typeof(Punctuation);
                foreach (var field in puncTokenType.GetFields(PublicFields))
                    if (field.FieldType.Equals(puncTokenType))
                    {
                        var p = field.GetValue(null);
                        output.WriteLine("/// <summary>");
                        output.WriteLine("/// Hash code of {0} token", p);
                        output.WriteLine("/// </summary>");
                        output.WriteLine("public const int lxm{0} = {1};", field.Name, p.GetHashCode());
                    }
                foreach (var field in puncTokenType.GetFields(PublicFields))
                    if (field.FieldType.Equals(puncTokenType))
                    {
                        output.WriteLine("case HashCodes.lxm{0}: return {1};", field.Name, field.Name);
                    }
            }
#endif
            /// <summary>
            /// Hash code of : token
            /// </summary>
            public const int lxmColon = 58;
            /// <summary>
            /// Hash code of ; token
            /// </summary>
            public const int lxmSemicolon = 59;
            /// <summary>
            /// Hash code of , token
            /// </summary>
            public const int lxmComma = 44;
            /// <summary>
            /// Hash code of { token
            /// </summary>
            public const int lxmLeftBrace = 123;
            /// <summary>
            /// Hash code of {{ token
            /// </summary>
            public const int lxmDoubleLeftBrace = 122016;
            /// <summary>
            /// Hash code of } token
            /// </summary>
            public const int lxmRightBrace = 125;
            /// <summary>
            /// Hash code of }} token
            /// </summary>
            public const int lxmDoubleRightBrace = 124000;
            /// <summary>
            /// Hash code of ( token
            /// </summary>
            public const int lxmLeftBracket = 40;
            /// <summary>
            /// Hash code of ) token
            /// </summary>
            public const int lxmRightBracket = 41;
            /// <summary>
            /// Hash code of [ token
            /// </summary>
            public const int lxmLeftSquareBracket = 91;
            /// <summary>
            /// Hash code of ] token
            /// </summary>
            public const int lxmRightSquareBracket = 93;
            /// <summary>
            /// Hash code of -> token
            /// </summary>
            public const int lxmArrow = 44657;
            /// <summary>
            /// Hash code of @ token
            /// </summary>
            public const int lxmDog = 64;
            /// <summary>
            /// Hash code of @@ token
            /// </summary>
            public const int lxmDoubleDog = 63488;
            /// <summary>
            /// Hash code of # token
            /// </summary>
            public const int lxmDiez = 35;
        }
        #endregion
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
        /// Represents double left brace.
        /// </summary>
        public static readonly Punctuation DoubleLeftBrace = new Punctuation(Lexeme.LeftBrace, Lexeme.LeftBrace);

        /// <summary>
        /// Represents right brace.
        /// </summary>
        public static readonly new Punctuation RightBrace = new Punctuation(Lexeme.RightBrace);

        /// <summary>
        /// Represents double right brace.
        /// </summary>
        public static readonly Punctuation DoubleRightBrace = new Punctuation(Lexeme.RightBrace, Lexeme.RightBrace);

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

        internal static Punctuation FromHashCode(int hashCode)
        {
            switch (hashCode)
            {
                case HashCodes.lxmColon: return Colon;
                case HashCodes.lxmSemicolon: return Semicolon;
                case HashCodes.lxmComma: return Comma;
                case HashCodes.lxmLeftBrace: return LeftBrace;
                case HashCodes.lxmDoubleLeftBrace: return DoubleLeftBrace;
                case HashCodes.lxmRightBrace: return RightBrace;
                case HashCodes.lxmDoubleRightBrace: return DoubleRightBrace;
                case HashCodes.lxmLeftBracket: return LeftBracket;
                case HashCodes.lxmRightBracket: return RightBracket;
                case HashCodes.lxmLeftSquareBracket: return LeftSquareBracket;
                case HashCodes.lxmRightSquareBracket: return RightSquareBracket;
                case HashCodes.lxmArrow: return Arrow;
                case HashCodes.lxmDog: return Dog;
                case HashCodes.lxmDoubleDog: return DoubleDog;
                case HashCodes.lxmDiez: return Diez;
                default: return null;
            }
        }
    }
}
