using System;

namespace DynamicScript.Compiler
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;

    [Serializable]
    [ComVisible(false)]
    sealed class NameToken: Token
    {
        public NameToken(string name)
            : base(name)
        {
        }

        public static string Normalize(string name)
        {
            if (name == null) name = string.Empty;
            return StringEqualityComparer.IgnoreCase ? name.ToLower() : name;
        }
    }
}
