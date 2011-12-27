using System;

namespace DynamicScript.Runtime.Environment
{
    using ScriptObjectConverterAttribute = Debugging.ScriptObjectConverterAttribute;
    using CultureInfo = System.Globalization.CultureInfo;
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;

    [ComVisible(false)]
    [AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
    sealed class ContractConverterAttribute : ScriptObjectConverterAttribute
    {

        public override bool TryConvertTo(IScriptObject value, Type destinationType, InterpreterState state, out object result)
        {
            if (Type.GetTypeCode(destinationType) == TypeCode.String && value is IScriptContract)
            {
                result = value.ToString();
                return true;
            }
            else
            {
                result = null;
                return false;
            }
        }
    }
}
