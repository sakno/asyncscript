using System;
using System.Collections.Generic;

namespace DynamicScript.Runtime.Environment.ObjectModel
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;
    using CallStack = Debugging.CallStack;
    using TransparentActionAttribute = Debugging.TransparentActionAttribute;
    using ScriptDebugger = Debugging.ScriptDebugger;
    using SystemConverter = System.Convert;
    using InterpretationContext = Compiler.Ast.InterpretationContext;
    using Resources = Properties.Resources;

    [ComVisible(false)]
    sealed class DebuggerModule : ScriptCompositeObject
    {
        #region Nested Types
        [ComVisible(false)]
        private sealed class IsEnabledSlot : RuntimeSlotBase, IEquatable<IsEnabledSlot>
        {
            public const string Name = "IsEnabled";

            public ScriptBoolean Value
            {
                get { return Monitoring.IsEnabled; }
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
                get { return ScriptBooleanContract.Instance; }
            }

            public override RuntimeSlotAttributes Attributes
            {
                get { return RuntimeSlotAttributes.Immutable; ; }
            }

            protected override ICollection<string> Slots
            {
                get { return Value.Slots; }
            }

            public override bool DeleteValue()
            {
                return false;
            }

            public bool Equals(IsEnabledSlot other)
            {
                return other != null;
            }

            public override bool Equals(IRuntimeSlot other)
            {
                return Equals(other as IsEnabledSlot);
            }

            public override bool Equals(object other)
            {
                return Equals(other as IsEnabledSlot);
            }

            public override int GetHashCode()
            {
                return GetType().MetadataToken;
            }
        }

        [ComVisible(false)]
        private sealed class EnableAction : ScriptAction
        {
            public const string Name = "enable";

            protected override void Invoke(InterpreterState state)
            {
                Monitoring.Enable();
            }
        }

        [ComVisible(false)]
        private sealed class AllocatedMemorySlot : RuntimeSlotBase, IEquatable<AllocatedMemorySlot>
        {
            public const string Name = "allocated";

            public ScriptInteger Value
            {
                get { return Monitoring.AllocatedMemory; }
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

            public bool Equals(AllocatedMemorySlot other)
            {
                return other != null;
            }

            public override bool Equals(IRuntimeSlot other)
            {
                return Equals(other as AllocatedMemorySlot);
            }

            public override bool Equals(object other)
            {
                return Equals(other as AllocatedMemorySlot);
            }

            public override int GetHashCode()
            {
                return GetType().MetadataToken;
            }
        }

        [ComVisible(false)]
        private sealed class SurvivedMemorySlot : RuntimeSlotBase, IEquatable<SurvivedMemorySlot>
        {
            public const string Name = "survived";

            public ScriptInteger Value
            {
                get { return Monitoring.SurvivedMemory; }
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

            public bool Equals(SurvivedMemorySlot other)
            {
                return other != null;
            }

            public override bool Equals(IRuntimeSlot other)
            {
                return Equals(other as SurvivedMemorySlot);
            }

            public override bool Equals(object other)
            {
                return Equals(other as SurvivedMemorySlot);
            }

            public override int GetHashCode()
            {
                return GetType().MetadataToken;
            }
        }

        [ComVisible(false)]
        private sealed class ProcessorTimeSlot : RuntimeSlotBase, IEquatable<ProcessorTimeSlot>
        {
            public const string Name = "cputime";

            public ScriptInteger Value
            {
                get { return Monitoring.ProcessorTime; }
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

            public bool Equals(ProcessorTimeSlot other)
            {
                return other != null;
            }

            public override bool Equals(IRuntimeSlot other)
            {
                return Equals(other as ProcessorTimeSlot);
            }

            public override bool Equals(object other)
            {
                return Equals(other as ProcessorTimeSlot);
            }

            public override int GetHashCode()
            {
                return GetType().MetadataToken;
            }
        }

        [ComVisible(false)]
        private sealed class LocalsSlot : ObservableSlot
        {
            public const string Name = "locals";

            public LocalsSlot()
                : base(ScriptArray.Empty(ScriptStringContract.Instance), true)
            {
            }

            protected override IScriptObject GetValue(IScriptObject value, InterpreterState state)
            {
                switch (CallStack.Current != null && CallStack.Current.Storages.Count > 0)
                {
                    case true:
                        var locals = new string[CallStack.Current.Storages.Count];
                        CallStack.Current.Storages.CopyTo(locals, 0);
                        return new ScriptArray(Array.ConvertAll<string, ScriptString>(locals, name => new ScriptString(name)));
                    default: return ScriptArray.Empty(ScriptStringContract.Instance);
                }
            }
        }

        [ComVisible(false)]
        [TransparentAction]
        private sealed class GetLocAction : ScriptFunc<ScriptString>
        {
            public const string Name = "getloc";
            private const string FirstParamName = "name";

            public GetLocAction()
                : base(FirstParamName, ScriptStringContract.Instance, ScriptSuperContract.Instance)
            {
            }

            protected override IScriptObject Invoke(ScriptString name, InterpreterState state)
            {
                switch (CallStack.Current != null && name != null)
                {
                    case true:
                        var slotHolder = CallStack.Current[name];
                        if (slotHolder != null)
                            return slotHolder.GetValue(state);
                        else if (state.Context == InterpretationContext.Unchecked)
                            return Void;
                        else throw new SlotNotFoundException(name, state);
                    default:
                        if (state.Context == InterpretationContext.Unchecked)
                            return Void;
                        else throw new ScriptFault(Resources.MonitoringNotEnabled, state);
                }
            }
        }

        [ComVisible(false)]
        [TransparentAction]
        private sealed class SetLocAction : ScriptAction<ScriptString, IScriptObject>
        {
            public const string Name = "setloc";
            private const string FirstParamName = "name";
            private const string SecondParamName = "value";

            public SetLocAction()
                : base(FirstParamName, ScriptStringContract.Instance, SecondParamName, ScriptSuperContract.Instance)
            {
            }

            protected override void Invoke(ScriptString name, IScriptObject value, InterpreterState state)
            {
                switch (CallStack.Current != null)
                {
                    case true:
                        var slotHolder = CallStack.Current[name];
                        if (slotHolder != null)
                            slotHolder.SetValue(value, state);
                        else if (state.Context != InterpretationContext.Unchecked)
                            throw new SlotNotFoundException(name, state);
                        return;
                    default:
                        if (state.Context == InterpretationContext.Unchecked)
                            return;
                        else throw new ScriptFault(Resources.MonitoringNotEnabled, state);
                }
            }
        }

        [ComVisible(false)]
        [TransparentAction]
        private sealed class BreakPointAction : ScriptAction<ScriptString>
        {
            public const string Name = "break";
            private const string FirstParamName = "comment";

            public BreakPointAction()
                : base(FirstParamName, ScriptStringContract.Instance)
            {
            }

            protected override void Invoke(ScriptString comment, InterpreterState state)
            {
                if (ScriptDebugger.CurrentDebugger != null)
                    ScriptDebugger.CurrentDebugger.OnBreakPoint(comment, state);
            }
        }

        [ComVisible(false)]
        [TransparentAction]
        private sealed class MarkFrameAction : ScriptAction<ScriptString>
        {
            public const string Name = "markf";
            private const string FirstParamName = "id";

            public MarkFrameAction()
                : base(FirstParamName, ScriptStringContract.Instance)
            {
            }

            protected override void Invoke(ScriptString id, InterpreterState state)
            {
                if (CallStack.Current != null) CallStack.Current.ID = id;
            }
        }

        [ComVisible(false)]
        private new sealed class Slots : ObjectSlotCollection
        {
            public Slots()
            {
                Add<IsEnabledSlot>(IsEnabledSlot.Name);
                AddConstant<EnableAction>(EnableAction.Name);
                Add<AllocatedMemorySlot>(AllocatedMemorySlot.Name);
                Add<SurvivedMemorySlot>(SurvivedMemorySlot.Name);
                Add<ProcessorTimeSlot>(ProcessorTimeSlot.Name);
                AddConstant<CallStackManager>(CallStackManager.Name);
                Add<LocalsSlot>(LocalsSlot.Name);
                AddConstant<GetLocAction>(GetLocAction.Name);
                AddConstant<SetLocAction>(SetLocAction.Name);
                AddConstant<MarkFrameAction>(MarkFrameAction.Name);
                AddConstant<BreakPointAction>(BreakPointAction.Name);
            }
        }
        #endregion
        public const string Name = "debugger";

        public DebuggerModule()
            : base(new Slots())
        {
        }
    }
}
