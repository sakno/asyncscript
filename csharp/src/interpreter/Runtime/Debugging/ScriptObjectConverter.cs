using System;
using System.ComponentModel;

namespace DynamicScript.Runtime.Debugging
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;
    using CultureInfo = System.Globalization.CultureInfo;

    /// <summary>
    /// Represents an abstract class for script object converters.
    /// </summary>
    [ComVisible(false)]
    [AttributeUsage(AttributeTargets.Class, Inherited=true, AllowMultiple=false)]
    public abstract class ScriptObjectConverterAttribute: Attribute
    {
        /// <summary>
        /// Converts the specified script object to the given type.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="destinationType"></param>
        /// <param name="state"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public abstract bool TryConvertTo(IScriptObject value, Type destinationType, InterpreterState state, out object result);

        internal static bool ConvertTo(IScriptObject value, Type destinationType, InterpreterState state, out object result)
        {
            var attributes = GetCustomAttributes(value.GetType(), typeof(ScriptObjectConverterAttribute), true) as ScriptObjectConverterAttribute[];
            foreach (var attr in attributes)
                if (attr.TryConvertTo(value, destinationType, state, out result))
                    return true;
            result = null;
            return false;
        }

        /// <summary>
        /// Converts the specified script object to .NET-compliant object.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <param name="state"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public static bool ConvertTo<T>(IScriptObject value, InterpreterState state, out T result)
        {
            var obj = default(object);
            if (ConvertTo(value, typeof(T), state, out obj))
            {
                result = (T)obj;
                return true;
            }
            else
            {
                result = default(T);
                return false;
            }
        }

        
    }
}
