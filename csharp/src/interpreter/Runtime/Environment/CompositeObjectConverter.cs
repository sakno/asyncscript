using System;
using System.Linq;

namespace DynamicScript.Runtime.Environment
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;
    using CultureInfo = System.Globalization.CultureInfo;
    using ScriptObjectConverterAttribute = Debugging.ScriptObjectConverterAttribute;
    using IDebuggerEditable = Debugging.IDebuggerBrowsable;
    using Resources = Properties.Resources;
    using Punctuation = Compiler.Punctuation;

    [ComVisible(false)]
    [AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
    sealed class CompositeObjectConverterAttribute : ScriptObjectConverterAttribute
    {

        private static string ToString(string slotName, IScriptObject slot, InterpreterState state)
        {
            const string KeyValueFormat = "{0} = {1}";
            var value = default(object);
            return ConvertTo(slot, typeof(string), state, out value) ? string.Format(KeyValueFormat, slotName, value) : Resources.UnprintableValue;
        }

        private static string ToString(IScriptCompositeObject obj, InterpreterState state)
        {
            return string.Concat(Punctuation.LeftBrace, string.Join<string>(Punctuation.Comma, obj.Slots.Select(s => ToString(s, obj[s, state], state))), Punctuation.RightBrace);
        }

        public override bool TryConvertTo(IScriptObject value, Type destinationType, InterpreterState state, out object result)
        {
            if (value is IScriptCompositeObject && Type.GetTypeCode(destinationType) == TypeCode.String)
            {
                result = ToString((IScriptCompositeObject)value, state);
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
