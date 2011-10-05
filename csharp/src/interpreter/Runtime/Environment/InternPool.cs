using System;

namespace DynamicScript.Runtime.Environment
{
    using LongComparer = System.Collections.Generic.EqualityComparer<long>;
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;

    [ComVisible(false)]
    abstract class InternPool<TConvertible> : InternPool<long, TConvertible>
        where TConvertible : ScriptObject, IConvertible
    {
        protected InternPool(int capacity)
            : base(capacity, LongComparer.Default)
        {
        }
    }
}
