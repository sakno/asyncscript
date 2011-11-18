using System;

namespace DynamicScript.Compiler
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;
    using StringBuilder = System.Text.StringBuilder;

    /// <summary>
    /// Represents argument reference.
    /// This class cannot be inherited.
    /// </summary>
    [ComVisible(false)]
    sealed class ArgRef : Lexeme
    {
        private readonly string m_id;

        public ArgRef(string id)
        {
            m_id = id ?? string.Empty;
        }

        public ArgRef(StringBuilder id)
            : this(id != null ? id.ToString() : null)
        {
        }

        protected override string Value
        {
            get { return m_id; }
        }

        public long Parse()
        {
            var result = default(long);
            return long.TryParse(Value, out result) ? result : 0L;
        }

        internal static string MakeName(long paramIndex)
        {
            return string.Concat('_', Math.Abs(paramIndex));
        }
    }
}
