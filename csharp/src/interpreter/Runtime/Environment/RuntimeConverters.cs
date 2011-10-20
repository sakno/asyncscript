using System;

namespace DynamicScript.Runtime.Environment
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;
    using IRuntimeConverters = System.Collections.Generic.ISet<IRuntimeConverter>;

    [ComVisible(false)]
    static class RuntimeConverters
    {
        public static bool RegisterConverter<TConverter>(this IRuntimeConverters converters)
            where TConverter : IRuntimeConverter, new()
        {
            return converters.Add(new TConverter());
        }

        public static void RegisterConverters(IRuntimeConverters converters)
        {
            RegisterConverter<ScriptInteger.Int64Converter>(converters);
            RegisterConverter<ScriptInteger.Int32Converter>(converters);
            RegisterConverter<ScriptInteger.Int16Converter>(converters);
            RegisterConverter<ScriptInteger.Int8Converter>(converters);
            RegisterConverter<ScriptInteger.UInt8Converter>(converters);
            RegisterConverter<ScriptInteger.UInt16Converter>(converters);
            RegisterConverter<ScriptInteger.UInt32Converter>(converters);
            RegisterConverter<ScriptBoolean.BooleanConverter>(converters);
            RegisterConverter<ScriptReal.DoubleConverter>(converters);
            RegisterConverter<ScriptReal.SingleConverter>(converters);
            RegisterConverter<ScriptSlotMetadata.SlotMetaConverter>(converters);
            RegisterConverter<ScriptString.StringConverter>(converters);
            RegisterConverter<ScriptIterable.ScriptIterableContractDefConverter>(converters);
        }
    }
}
