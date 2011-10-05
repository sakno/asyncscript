using System;

namespace DynamicScript.Runtime.Environment
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;

    [ComVisible(false)]
    sealed class IntegerPool: InternPool<ScriptInteger>
    {
        public IntegerPool(int capacity)
            : base(capacity)
        {
        }

        /// <summary>
        /// Generates intern ID for the specified value.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static long MakeID(long value)
        {
            return value;
        }

        protected override long MakeID(ScriptInteger obj)
        {
            return MakeID((long)(obj ?? ScriptInteger.Zero));
        }
    }
}
