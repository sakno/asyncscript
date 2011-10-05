using System;
using System.Collections.Generic;

namespace DynamicScript.Runtime.Environment
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;

    [ComVisible(false)]
    sealed class ScriptSlotMetadata : ScriptCompositeObject
    {
        #region Nested Types
        [ComVisible(false)]
        public sealed class SlotMetaConverter : RuntimeConverter<KeyValuePair<string, ScriptCompositeContract.SlotMeta>>
        {
            public override bool Convert(KeyValuePair<string, ScriptCompositeContract.SlotMeta> input, out IScriptObject result)
            {
                result = new ScriptSlotMetadata(input);
                return true;
            }
        }
        #endregion

        private const string NameSlot = "name";
        private const string VariableSlot = "variable";
        private const string ContractSlot = "contract";

        public ScriptSlotMetadata(string slotName, ScriptCompositeContract.SlotMeta slot)
            : base(Slots(slotName, slot))
        {
        }

        public ScriptSlotMetadata(KeyValuePair<string, ScriptCompositeContract.SlotMeta> slot)
            : this(slot.Key, slot.Value)
        {
            
        }

        private static new IEnumerable<KeyValuePair<string, IRuntimeSlot>> Slots(string slotName, ScriptCompositeContract.SlotMeta slot)
        {
            yield return Constant(NameSlot, new ScriptString(slotName), ScriptStringContract.Instance);
            yield return Constant(VariableSlot, (ScriptBoolean)!slot.IsConstant, ScriptBooleanContract.Instance);
            yield return Constant(ContractSlot, slot.ContractBinding, ScriptMetaContract.Instance);
        }
    }
}
