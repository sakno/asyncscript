using System;
using System.Linq.Expressions;
using System.Reflection;

namespace DynamicScript.Runtime.Environment
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;

    /// <summary>
    /// Represents runtime state of the action during invocation.
    /// This class cannot be inherited.
    /// </summary>
    /// <remarks>
    /// This class encapsulates internal interpreter state, 'this' reference and other sensitive data required
    /// for action invocation.
    /// </remarks>
    [ComVisible(false)]
    public static class InvocationContext
    {
        [ThreadStatic]
        private static IScriptFunction m_current;

        /// <summary>
        /// Gets current executing action.
        /// </summary>
        public static IScriptFunction Current
        {
            get { return m_current; }
            internal set { m_current = value; }
        }

        /// <summary>
        /// Sets the currently executing action.
        /// </summary>
        /// <param name="current"></param>
        /// <returns>An action located higher in the call stack.</returns>
        internal static IScriptFunction SetCurrent(IScriptFunction current)
        {
            var previous = m_current;
            m_current = current;
            return previous;
        }

        /// <summary>
        /// Gets action owner.
        /// </summary>
        public static IScriptObject This
        {
            get { return Current.This; }
        }

        internal static MemberExpression ThisRef
        {
            get { return LinqHelpers.BodyOf<Func<IScriptObject>, MemberExpression>(() => This); }
        }

        internal static MemberExpression ActionRef
        {
            get
            {
                return LinqHelpers.BodyOf<Func<IScriptFunction>, MemberExpression>(() => Current);
            }
        }
    }
}
