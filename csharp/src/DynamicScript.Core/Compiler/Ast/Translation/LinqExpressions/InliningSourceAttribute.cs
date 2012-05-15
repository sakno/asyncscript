using System;
using System.Reflection;

namespace DynamicScript.Compiler.Ast.Translation.LinqExpressions
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;

    /// <summary>
    /// Marks the static method, field or property that represents built-in function.
    /// This class cannot be inherited.
    /// </summary>
    [ComVisible(false)]
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public sealed class InliningSourceAttribute: Attribute
    {
        /// <summary>
        /// Determines whether the specified method is marked as built-in function.
        /// </summary>
        /// <param name="mi"></param>
        /// <returns></returns>
        public static bool IsDefined(MethodInfo mi)
        {
            return IsDefined(mi, typeof(InliningSourceAttribute), true);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fi"></param>
        /// <returns></returns>
        public static bool IsDefined(FieldInfo fi)
        {
            return IsDefined(fi, typeof(InliningSourceAttribute), true);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pi"></param>
        /// <returns></returns>
        public static bool IsDefined(PropertyInfo pi)
        {
            return IsDefined(pi, typeof(InliningSourceAttribute), true);
        }
    }
}
