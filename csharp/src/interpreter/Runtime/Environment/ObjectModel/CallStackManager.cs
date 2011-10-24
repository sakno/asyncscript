using System;
using System.Collections.Generic;

namespace DynamicScript.Runtime.Environment.ObjectModel
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;
    using CallStack = Debugging.CallStack;
    using SystemConverter = System.Convert;

    [ComVisible(false)]
    sealed class CallStackManager : ScriptCompositeObject
    {
        public const string Name = "cstack";

        #region Nested Types
        [ComVisible(false)]
        private sealed class DepthSlot : RuntimeSlotBase, IEquatable<DepthSlot>
        {
            public const string Name = "depth";

            protected override IScriptContract GetValueContract()
            {
                return ContractBinding;
            }

            public ScriptInteger Value
            {
                get { return CallStack.Depth; }
            }

            public override IScriptObject GetValue(InterpreterState state)
            {
                return Value;
            }

            public override void SetValue(IScriptObject value, InterpreterState state)
            {
                throw new ConstantCannotBeChangedException(state);
            }

            public override IScriptContract ContractBinding
            {
                get { return ScriptIntegerContract.Instance; }
            }

            public override RuntimeSlotAttributes Attributes
            {
                get { return RuntimeSlotAttributes.Immutable; }
            }

            protected override ICollection<string> Slots
            {
                get { return Value.Slots; }
            }

            public override bool DeleteValue()
            {
                return false;
            }

            public bool Equals(DepthSlot other)
            {
                return other != null;
            }

            public override bool Equals(IRuntimeSlot other)
            {
                return Equals(other as DepthSlot);
            }

            public override bool Equals(object other)
            {
                return Equals(other as DepthSlot);
            }

            public override int GetHashCode()
            {
                return GetType().MetadataToken;
            }
        }

        [ComVisible(false)]
        private sealed class CallerSlot : RuntimeSlotBase, IEquatable<CallerSlot>
        {
            public const string Name = "caller";

            protected override IScriptContract GetValueContract()
            {
                return ContractBinding;
            }

            public IScriptAction Value
            {
                get { return CallStack.Caller; }
            }

            public override IScriptObject GetValue(InterpreterState state)
            {
                return Value ?? (IScriptObject)Void;
            }

            public override void SetValue(IScriptObject value, InterpreterState state)
            {
                throw new ConstantCannotBeChangedException(state);
            }

            public override IScriptContract ContractBinding
            {
                get { return ScriptSuperContract.Instance; }
            }

            public override RuntimeSlotAttributes Attributes
            {
                get { return RuntimeSlotAttributes.Immutable; }
            }

            protected override ICollection<string> Slots
            {
                get { return (Value ?? (IScriptObject)Void).Slots; }
            }

            public override bool DeleteValue()
            {
                return false;
            }

            public bool Equals(CallerSlot other)
            {
                return other != null;
            }

            public override bool Equals(IRuntimeSlot other)
            {
                return Equals(other as CallerSlot);
            }

            public override bool Equals(object other)
            {
                return Equals(other as CallerSlot);
            }

            public override int GetHashCode()
            {
                return GetType().MetadataToken;
            }
        }

        [ComVisible(false)]
        private sealed class GetFrameAction : ScriptFunc<ScriptInteger>
        {
            public const string Name = "getFrame";
            private const string FirstParamName = "frameNum";

            public GetFrameAction()
                : base(FirstParamName, ScriptIntegerContract.Instance, ScriptSuperContract.Instance)
            {
            }

            protected override IScriptObject Invoke(ScriptInteger frameNum, InterpreterState state)
            {
                return CallStack.GetFrame(SystemConverter.ToInt64(frameNum)).Action;
            }
        }

        [ComVisible(false)]
        private new sealed class Slots : ObjectSlotCollection
        {
            public Slots()
            {
                Add<DepthSlot>(DepthSlot.Name);
                Add<CallerSlot>(CallerSlot.Name);
                AddConstant<GetFrameAction>(GetFrameAction.Name);
            }
        }
        #endregion

        public CallStackManager()
            : base(new Slots())
        {
        }
    }
}
