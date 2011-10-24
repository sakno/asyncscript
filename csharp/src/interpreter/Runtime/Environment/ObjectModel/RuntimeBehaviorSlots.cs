using System;
using System.Collections.Generic;

namespace DynamicScript.Runtime.Environment.ObjectModel
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;

    [ComVisible(false)]
    sealed class RuntimeBehaviorSlots : ScriptCompositeObject
    {
        #region Nested Types
        [ComVisible(false)]
        private sealed class OmitVoidInLoops : RuntimeSlotBase
        {
            public const string Name = "omitVoidInLoops";

            protected override IScriptContract GetValueContract()
            {
                return ContractBinding;
            }

            public override IScriptObject GetValue(InterpreterState state)
            {
                return (ScriptBoolean)state.Behavior.OmitVoidYieldInLoops;
            }

            public override void SetValue(IScriptObject value, InterpreterState state)
            {
                state.Behavior.OmitVoidYieldInLoops = value as ScriptBoolean;
            }

            public override IScriptContract ContractBinding
            {
                get { return ScriptBooleanContract.Instance; }
            }

            public override RuntimeSlotAttributes Attributes
            {
                get { return RuntimeSlotAttributes.None; }
            }

            protected override ICollection<string> Slots
            {
                get { return ScriptBoolean.False.Slots; }
            }

            public override bool DeleteValue()
            {
                return false;
            }

            public override bool Equals(IRuntimeSlot other)
            {
                return other is OmitVoidInLoops;
            }
        }

        private new sealed class Slots : ObjectSlotCollection
        {
            public Slots()
            {
                Add(OmitVoidInLoops.Name, new OmitVoidInLoops());
            }
        }
        #endregion

        public const string Name = "behavior";

        public RuntimeBehaviorSlots()
            : base(new Slots())
        {
        }
    }
}
