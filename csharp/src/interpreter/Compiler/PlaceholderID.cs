using System;

namespace DynamicScript.Compiler
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;
    using StringBuilder = System.Text.StringBuilder;

    /// <summary>
    /// Represents placeholder identifier.
    /// This class cannot be inherited.
    /// </summary>
    [ComVisible(false)]
    sealed class PlaceholderID: Lexeme
    {
        private readonly string m_id;

        public PlaceholderID(string id)
        {
            m_id = id ?? string.Empty;
        }

        public PlaceholderID(StringBuilder id)
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
    }
}
