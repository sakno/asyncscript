using System;

namespace DynamicScript.Runtime.Environment
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;
    using Compiler.Ast;

    [ComVisible(false)]
    static class RuntimeConverters
    {
        static RuntimeConverters()
        {
            RuntimeHelpers.RegisterConverter<ScriptInteger.Int64Converter>();
            RuntimeHelpers.RegisterConverter<ScriptInteger.Int32Converter>();
            RuntimeHelpers.RegisterConverter<ScriptInteger.Int16Converter>();
            RuntimeHelpers.RegisterConverter<ScriptInteger.Int8Converter>();
            RuntimeHelpers.RegisterConverter<ScriptInteger.UInt8Converter>();
            RuntimeHelpers.RegisterConverter<ScriptInteger.UInt16Converter>();
            RuntimeHelpers.RegisterConverter<ScriptInteger.UInt32Converter>();
            RuntimeHelpers.RegisterConverter<ScriptBoolean.BooleanConverter>();
            RuntimeHelpers.RegisterConverter<ScriptReal.DoubleConverter>();
            RuntimeHelpers.RegisterConverter<ScriptReal.SingleConverter>();
            RuntimeHelpers.RegisterConverter<ScriptSlotMetadata.SlotMetaConverter>();
            RuntimeHelpers.RegisterConverter<ScriptString.StringConverter>();
            RuntimeHelpers.RegisterConverter<ScriptIterable.ScriptIterableContractDefConverter>();
        }
    }
}
