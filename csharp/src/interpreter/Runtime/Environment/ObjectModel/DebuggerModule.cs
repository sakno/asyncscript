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
        private sealed class IsEnabledSlot : RuntimeSlotBase, IStaticRuntimeSlot
        {
            public const string Name = "IsEnabled";

            public override IScriptObject GetValue(InterpreterState state)
            {
                return (ScriptBoolean)Monitoring.IsEnabled;
            }

            public override IScriptObject SetValue(IScriptObject value, InterpreterState state)
            {
                throw new ConstantCannotBeChangedException(state);
            }

            public IScriptContract ContractBinding
            {
                get { return ScriptBooleanContract.Instance; }
            }

            public override RuntimeSlotAttributes Attributes
            {
                get { return RuntimeSlotAttributes.Immutable; ; }
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
        private sealed class EnableFunction : ScriptAction
        {
            public const string Name = "enable";

            protected override void Invoke(InterpreterState state)
            {
                Monitoring.Enable();
            }
        }

        [ComVisible(false)]
        private sealed class AllocatedMemorySlot : RuntimeSlotBase, IStaticRuntimeSlot
        {
            public const string Name = "allocated";

            public override IScriptObject GetValue(InterpreterState state)
            {
                return (ScriptInteger)Monitoring.AllocatedMemory;
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
        private sealed class SurvivedMemorySlot : RuntimeSlotBase, IStaticRuntimeSlot
        {
            public const string Name = "survived";

            public override IScriptObject GetValue(InterpreterState state)
            {
                return (ScriptInteger)Monitoring.SurvivedMemory;
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
        private sealed class ProcessorTimeSlot : RuntimeSlotBase, IStaticRuntimeSlot
        {
            public const string Name = "cputime";

            public override IScriptObject GetValue(InterpreterState state)
            {
                return (ScriptInteger)Monitoring.ProcessorTime;
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
        private sealed class LocalsSlot : RuntimeSlotBase, IStaticRuntimeSlot
        {
            public const string Name = "locals";

            public readonly ScriptArrayContract ContractBinding = new ScriptArrayContract(ScriptStringContract.Instance);

            public override bool DeleteValue()
            {
                return false;
            }

            public override IScriptObject GetValue(InterpreterState state)
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

            public override IScriptObject SetValue(IScriptObject value, InterpreterState state)
            {
                throw new ConstantCannotBeChangedException(state);
            }

            public override RuntimeSlotAttributes Attributes
            {
                get { return RuntimeSlotAttributes.Immutable; }
            }

            IScriptContract IStaticRuntimeSlot.ContractBinding
            {
                get { return ContractBinding; }
            }

            public override bool HasValue
            {
                get{  return true; }
                protected set { }
            }
        }

        [ComVisible(false)]
        [TransparentAction]
        private sealed class GetLocFunction : ScriptFunc<ScriptString>
        {
            public const string Name = "getloc";
            private const string FirstParamName = "name";

            public GetLocFunction()
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
        private sealed class SetLocFunction : ScriptAction<ScriptString, IScriptObject>
        {
            public const string Name = "setloc";
            private const string FirstParamName = "name";
            private const string SecondParamName = "value";

            public SetLocFunction()
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
        private sealed class BreakPointFunction : ScriptAction<ScriptString>
        {
            public const string Name = "break";
            private const string FirstParamName = "comment";

            public BreakPointFunction()
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
        private sealed class MarkFrameFunction : ScriptAction<ScriptString>
        {
            public const string Name = "markf";
            private const string FirstParamName = "id";

            public MarkFrameFunction()
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
                AddConstant<EnableFunction>(EnableFunction.Name);
                Add<AllocatedMemorySlot>(AllocatedMemorySlot.Name);
                Add<SurvivedMemorySlot>(SurvivedMemorySlot.Name);
                Add<ProcessorTimeSlot>(ProcessorTimeSlot.Name);
                AddConstant<CallStackManager>(CallStackManager.Name);
                Add<LocalsSlot>(LocalsSlot.Name);
                AddConstant<GetLocFunction>(GetLocFunction.Name);
                AddConstant<SetLocFunction>(SetLocFunction.Name);
                AddConstant<MarkFrameFunction>(MarkFrameFunction.Name);
                AddConstant<BreakPointFunction>(BreakPointFunction.Name);
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
