using System;

namespace DynamicScript.Runtime.Debugging
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;

    /// <summary>
    /// Marks an action that is should not be stored in the call stack.
    /// This class cannot be inherited.
    /// </summary>
    [ComVisible(false)]
    
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public sealed class TransparentActionAttribute : Attribute
    {
        internal static bool IsDefined(Type t)
        {
            return IsDefined(t, typeof(TransparentActionAttribute), false);
        }
    }
}
