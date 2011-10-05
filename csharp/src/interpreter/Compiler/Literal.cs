using System;

namespace DynamicScript.Compiler
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;

    [Serializable]
    [ComVisible(false)]
    abstract class Literal<T>: Token
        where T:IConvertible
    {
        protected Literal(string value)
            : base(value)
        {
        }

        public new abstract T Value
        {
            get;
        }

        public static implicit operator T(Literal<T> lit)
        {
            return lit != null ? lit.Value : default(T);
        }
    }
}
