using System;
using System.ComponentModel;

namespace DynamicScript.Runtime.Environment
{
    using ScriptObjectConverter = Debugging.ScriptObjectConverter;
    using CultureInfo = System.Globalization.CultureInfo;

    sealed class ContractConverter: ScriptObjectConverter
    {
        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            return Equals(destinationType, typeof(string));
        }

        protected override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, IScriptObject value, Type destinationType)
        {
            return Equals(destinationType, typeof(string)) ? value.ToString() : null;
        }
    }
}
