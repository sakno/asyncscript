using System;
using System.Collections.Generic;

namespace DynamicScript.Runtime
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;

    /// <summary>
    /// Represents DynamicScript-compliant representation of the specified type.
    /// </summary>
    /// <typeparam name="T">A type to wrap into DynamicScript-compliant representation.</typeparam>
    [ComVisible(false)]
    public abstract class DynamicScriptProxy<T>: IEnumerable<KeyValuePair<string, IScriptAction>>
        where T : class
    {
        private readonly T m_obj;

        /// <summary>
        /// Initializes a new proxy object.
        /// </summary>
        /// <param name="obj">An object to wrap.</param>
        protected DynamicScriptProxy(T obj)
        {
            if (obj == null) throw new ArgumentNullException("obj");
            m_obj = obj;
        }

        /// <summary>
        /// Gets target object.
        /// </summary>
        protected T Target
        {
            get { return m_obj; }
        }

        /// <summary>
        /// Combines action name and its implementation.
        /// </summary>
        /// <param name="slotName"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        protected static KeyValuePair<string, IScriptAction> DefineAction(string slotName, IScriptAction action)
        {
            if (string.IsNullOrEmpty(slotName)) throw new ArgumentNullException("slotName");
            if (action == null) throw new ArgumentNullException("action");
            return new KeyValuePair<string, IScriptAction>(slotName, action);
        }

        /// <summary>
        /// Returns an enumerator through all actions.
        /// </summary>
        /// <returns></returns>
        public abstract IEnumerator<KeyValuePair<string, IScriptAction>> GetEnumerator();

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
