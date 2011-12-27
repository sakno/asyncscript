using System;
using System.Collections.Generic;
using System.Linq;

namespace DynamicScript
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;

    /// <summary>
    /// Represents implementation of the hash algorithm used by the Qube compiler.
    /// This class cannot be inherited.
    /// </summary>
    [ComVisible(false)]
    [Serializable]
    public sealed class StringEqualityComparer : IEqualityComparer<string>
    {
        /// <summary>
        /// Indicates that the comparer ignores literal case.
        /// </summary>
        public const bool IgnoreCase = true;

        /// <summary>
        /// Represents access to the singleton instance of the comparer.
        /// </summary>
        public static readonly IEqualityComparer<string> Instance = new StringEqualityComparer();

        /// <summary>
        /// Determines whether the two strings represents the same lexeme.
        /// </summary>
        /// <param name="lexeme1">The first lexeme to compare.</param>
        /// <param name="lexeme2">The second lexeme to compare.</param>
        /// <returns><see langword="true"/> if the two strings represents the same lexeme;
        /// otherwise, <see langword="false"/>.</returns>
        public static bool Equals(string lexeme1, string lexeme2)
        {
            return string.Equals(lexeme1, lexeme2, IgnoreCase ? StringComparison.OrdinalIgnoreCase: StringComparison.Ordinal);
        }

        #region IEqualityComparer<string> Members

        bool IEqualityComparer<string>.Equals(string lexeme1, string lexeme2)
        {
            return Equals(lexeme1, lexeme2);
        }

        int IEqualityComparer<string>.GetHashCode(string lexeme)
        {
            return GetHashCode(lexeme);
        }

        #endregion

        private static long GetHashCode(IEnumerable<long> tokens)
        {
            var checksum = 0L;
            foreach (var c in tokens)
                checksum = (checksum << 10) - (checksum << 5) - checksum + c;
            return checksum;
        }

        private static long ToInt64(char c)
        {
            return (long)c;
        }

        private static long ToLowerInt64(char c)
        {
            return ToInt64(char.ToLower(c));
        }

        private static long ToInt64(byte b)
        {
            return b;
        }

        private static long ToInt64(int i)
        {
            return i;
        }

        /// <summary>
        /// Returns a hash code for the particular lexeme.
        /// </summary>
        /// <param name="lexeme">A lexeme to be hashed.</param>
        /// <param name="ignoreCase">Specify that hash algorithm should ignore case sensivity.</param>
        /// <returns>A hash code for the particular lexeme.</returns>
        public static long GetHashCodeLong(string lexeme, bool ignoreCase = IgnoreCase)
        {
            if (lexeme == null) lexeme = string.Empty;
            return GetHashCode(Enumerable.Select(lexeme, ignoreCase ? new Func<char, long>(ToLowerInt64) : new Func<char, long>(ToInt64)));
        }

        /// <summary>
        /// Computes hash code for the specified sequence of bytes.
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static int GetHashCode(IEnumerable<byte> bytes)
        {
            return unchecked((int)GetHashCode(Enumerable.Select(bytes ?? Enumerable.Empty<byte>(), ToInt64)));
        }

        internal static int GetHashCode(IEnumerable<int> ints)
        {
            return unchecked((int)GetHashCode(Enumerable.Select(ints ?? Enumerable.Empty<int>(), ToInt64)));
        }

        /// <summary>
        /// Returns a hash code for the particular lexeme.
        /// </summary>
        /// <param name="lexeme">A lexeme to be hashed.</param>
        /// <param name="ignoreCase">Specify that hash algorithm should ignore case sensivity.</param>
        /// <returns>A hash code for the particular lexeme.</returns>
        public static int GetHashCode(string lexeme, bool ignoreCase = IgnoreCase)
        {
            return unchecked((int)GetHashCodeLong(lexeme, ignoreCase));
        }
    }
}
