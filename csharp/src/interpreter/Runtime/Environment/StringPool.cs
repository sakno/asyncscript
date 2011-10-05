using System;

namespace DynamicScript.Runtime.Environment
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;

    [ComVisible(false)]
    sealed class StringPool : InternPool<ScriptString>
    {
        public StringPool(int capacity)
            : base(capacity)
        {
        }

        public static long MakeID(string value)
        {
            return StringEqualityComparer.GetHashCodeLong(value, false);
        }

        protected override long MakeID(ScriptString obj)
        {
            return MakeID((string)(obj ?? ScriptString.Empty));
        }
    }
}
