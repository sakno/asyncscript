using System;
using System.ComponentModel;

namespace DynamicScript.Runtime.Environment
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;
    using ScriptObjectConverter = Debugging.ScriptObjectConverter;
    using Punctuation = Compiler.Punctuation;

    [ComVisible(false)]
    sealed class ScriptPrimitiveObjectConverter : ScriptObjectConverter
    {
        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            return Equals(destinationType, typeof(string));
        }

        protected override object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, IScriptObject value, Type destinationType)
        {
            switch (Equals(destinationType, typeof(string)))
            {
                case true:
                    return Convert.GetTypeCode(value) == TypeCode.String ? String.Concat(Punctuation.CQuote, Convert.ToString(value), Punctuation.CQuote) : Convert.ToString(value);
                default: return null;
            }
        }
    }
}
