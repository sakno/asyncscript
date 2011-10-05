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
    
    public abstract class ScriptObjectConverter: TypeConverter
    {
        /// <summary>
        /// Gets interpreter state used for conversion.
        /// </summary>
        protected InterpreterState State
        {
            get;
            private set;
        }

        internal void SetState(InterpreterState s)
        {
            State = s;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <param name="culture"></param>
        /// <param name="value"></param>
        /// <param name="destinationType"></param>
        /// <returns></returns>
        protected abstract object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, IScriptObject value, Type destinationType);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <param name="culture"></param>
        /// <param name="value"></param>
        /// <param name="destinationType"></param>
        /// <returns></returns>
        public sealed override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            return value is IScriptObject ? ConvertTo(context, culture, (IScriptObject)value, destinationType) : null;
        }
    }
}
