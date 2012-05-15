using System;
using System.Linq.Expressions;

namespace DynamicScript
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;
    using MethodInfo = System.Reflection.MethodInfo;

    [ComVisible(false)]
    [Serializable]
    abstract class WeakDelegate: WeakReference
    {
        private readonly MethodInfo m_implementation;
        private readonly Type m_delegate;

        protected WeakDelegate(Delegate d)
            : base(d.Target, false)
        {
            m_implementation = d.Method;
            m_delegate = d.GetType();
        }

        public Type DelegateType
        {
            get { return m_delegate; }
        }

        public MethodInfo Method
        {
            get { return m_implementation; }
        }

        public object DynamicInvoke(params object[] args)
        {
            return Method.Invoke(Target, args);
        }

        public Delegate CreateDelegate()
        {
            return Delegate.CreateDelegate(DelegateType, Target, Method, true);
        }

        public static explicit operator Delegate(WeakDelegate d)
        {
            return d != null ? d.CreateDelegate() : null;
        }
    }
}
