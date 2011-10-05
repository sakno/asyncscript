using System;

namespace DynamicScript
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;

    [ComVisible(false)]
    sealed class WeakFunc<TResult> : WeakDelegate
    {
        public WeakFunc(Func<TResult> func)
            : base(func)
        {
        }

        public TResult Invoke()
        {
            return (TResult)DynamicInvoke();
        }

        public new Func<TResult> CreateDelegate()
        {
            return base.CreateDelegate() as Func<TResult>;
        }

        public static Func<TResult> CreateWeakDelegate(Func<TResult> func)
        {
            return func != null ? new Func<TResult>(new WeakFunc<TResult>(func).Invoke) : null;
        }
    }
}
