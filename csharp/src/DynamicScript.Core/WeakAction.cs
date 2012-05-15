using System;

namespace DynamicScript
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;

    [ComVisible(false)]
    sealed class WeakAction<T>: WeakDelegate
    {
        public WeakAction(Action<T> a)
            : base(a)
        {
        }

        public void Invoke(T arg)
        {
            DynamicInvoke(arg);
        }

        public new Action<T> CreateDelegate()
        {
            return base.CreateDelegate() as Action<T>;
        }
    }
}
