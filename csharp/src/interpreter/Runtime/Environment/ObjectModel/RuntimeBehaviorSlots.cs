using System;
using System.Collections.Generic;

namespace DynamicScript.Runtime.Environment.ObjectModel
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;
    using InterpretationContext = Compiler.Ast.InterpretationContext;

    [ComVisible(false)]
    sealed class RuntimeBehaviorSlots : ScriptCompositeObject
    {
        #region Nested Types
        [ComVisible(false)]
        private sealed class OmitVoidInLoops : RuntimeSlotBase, IStaticRuntimeSlot
        {
            public const string Name = "omitVoidInLoops";

            public override IScriptObject GetValue(InterpreterState state)
            {
                return (ScriptBoolean)state.Behavior.OmitVoidYieldInLoops;
            }

            public override IScriptObject SetValue(IScriptObject value, InterpreterState state)
            {
                if (ScriptBooleanContract.TryConvert(ref value))
                {
                    state.Behavior.OmitVoidYieldInLoops = (ScriptBoolean)value;
                    return value;
                }
                else if (state.Context == InterpretationContext.Unchecked)
                    return value;
                else throw new ContractBindingException(value, ScriptBooleanContract.Instance, state);
            }

            public IScriptContract ContractBinding
            {
                get { return ScriptBooleanContract.Instance; }
            }

            public override RuntimeSlotAttributes Attributes
            {
                get { return RuntimeSlotAttributes.None; }
            }

            public override bool DeleteValue()
            {
                return false;
            }

            public override bool HasValue
            {
                get { return true; }
                protected set { }
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
