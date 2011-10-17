using System;

namespace DynamicScript
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;

    [ComVisible(false)]
    static class MonoRuntime
    {
        public static readonly bool Available;

        static MonoRuntime()
        {
            Available = Type.GetType("Mono.Runtime", false) != null;
        }
    }
}
