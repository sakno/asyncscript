using System;
using System.ComponentModel;
using System.Linq;

namespace DynamicScript.Runtime.Environment
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;
    using CultureInfo = System.Globalization.CultureInfo;
    using ScriptObjectConverter = Debugging.ScriptObjectConverter;
    using IDebuggerEditable = Debugging.IDebuggerBrowsable;
    using Resources = Properties.Resources;
    using Punctuation = Compiler.Punctuation;

    [ComVisible(false)]
    sealed class CompositeObjectConverter: ScriptObjectConverter
    {
        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            return Equals(destinationType, typeof(string));
        }

        private static string ToString(string slotName, IRuntimeSlot slot, InterpreterState state)
        {
            const string KeyValueFormat = "{0} = {1}";
            switch (slot is IDebuggerEditable)
            {
                case true:
                    var value = default(string);
                    return String.Format(KeyValueFormat, slotName, ((IDebuggerEditable)slot).TryGetValue(out value, state) ? value : Resources.UnprintableValue);
                default: return String.Format(KeyValueFormat, slotName, Resources.UnprintableValue);
            }
        }

        private static string ToString(IScriptObject obj, InterpreterState state)
        {
            return string.Concat(Punctuation.LeftBrace, string.Join<string>(Punctuation.Comma, obj.Slots.Select(s => ToString(s, obj[s, state], state))), Punctuation.RightBrace);
        }

        protected override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, IScriptObject value, Type destinationType)
        {
            return Equals(destinationType, typeof(string)) && State != null ? ToString(value, State) : null;
        }
    }
}
