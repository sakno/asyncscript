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
        private sealed class DepthSlot : RuntimeSlotBase, IStaticRuntimeSlot
        {
            public const string Name = "depth";


            public override IScriptObject GetValue(InterpreterState state)
            {
                return (ScriptInteger)CallStack.Depth;
            }

            public override IScriptObject SetValue(IScriptObject value, InterpreterState state)
            {
                throw new ConstantCannotBeChangedException(state);
            }

            public IScriptContract ContractBinding
            {
                get { return ScriptIntegerContract.Instance; }
            }

            public override RuntimeSlotAttributes Attributes
            {
                get { return RuntimeSlotAttributes.Immutable; }
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

        [ComVisible(false)]
        private sealed class CallerSlot : RuntimeSlotBase, IStaticRuntimeSlot
        {
            public const string Name = "caller";

            public override IScriptObject GetValue(InterpreterState state)
            {
                return CallStack.Caller ?? (IScriptObject)Void;
            }

            public override IScriptObject SetValue(IScriptObject value, InterpreterState state)
            {
                throw new ConstantCannotBeChangedException(state);
            }

            public IScriptContract ContractBinding
            {
                get { return ScriptSuperContract.Instance; }
            }

            public override RuntimeSlotAttributes Attributes
            {
                get { return RuntimeSlotAttributes.Immutable; }
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

        [ComVisible(false)]
        private sealed class GetFrameFunction : ScriptFunc<ScriptInteger>
        {
            public const string Name = "getFrame";
            private const string FirstParamName = "frameNum";

            public GetFrameFunction()
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
                AddConstant<GetFrameFunction>(GetFrameFunction.Name);
            }
        }
        #endregion

        public CallStackManager()
            : base(new Slots())
        {
        }
    }
}
