using System;

namespace DynamicScript.Runtime.Environment
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;

    [ComVisible(false)]
    sealed class RealPool: InternPool<ScriptReal>
    {
        public RealPool(int capacity)
            : base(capacity)
        {
        }

        public static long MakeID(double value)
        {
            return BitConverter.DoubleToInt64Bits(value);
        }

        protected override long MakeID(ScriptReal obj)
        {
            return MakeID((double)(obj ?? ScriptReal.Zero));
        }
    }
}
