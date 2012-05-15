using System;
using System.ComponentModel;

namespace DynamicScript.Runtime.Environment
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;
    using ScriptObjectConverterAttribute = Debugging.ScriptObjectConverterAttribute;
    using Punctuation = Compiler.Punctuation;

    [ComVisible(false)]
    [AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
    sealed class ScriptPrimitiveObjectConverterAttribute : ScriptObjectConverterAttribute
    {

        public override bool TryConvertTo(IScriptObject value, Type destinationType, InterpreterState state, out object result)
        {
            switch (Type.GetTypeCode(destinationType))
            {
                case TypeCode.Int64:
                case TypeCode.Int32:
                case TypeCode.Boolean:
                case TypeCode.Single:
                case TypeCode.Double:
                case TypeCode.Int16:
                    result =  value.ToString();
                    return true;
                case TypeCode.String:
                    result = string.Concat(Punctuation.CQuote, value, Punctuation.CQuote);
                    return true;
                default:
                    result = null;
                    return false;
            }
        }
    }
}
