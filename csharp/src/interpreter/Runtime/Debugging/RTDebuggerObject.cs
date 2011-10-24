using System;

namespace DynamicScript.Runtime.Debugging
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;
    using Resources = Properties.Resources;
    using Environment;
    using DynamicScriptIO = Hosting.DynamicScriptIO;
    using Keyword = Compiler.Keyword;
    using InterpretationContext = Compiler.Ast.InterpretationContext;

    [ComVisible(false)]
    sealed class RTDebuggerObject: ScriptCompositeObject
    {
        #region Nested Types
        [ComVisible(false)]
        [TransparentAction]
        private sealed class LocalsAction : ScriptAction
        {
            public const string Name = "locals";

            protected override void Invoke(InterpreterState state)
            {
                if (CallStack.Current != null)
                    foreach (var slot in CallStack.Current)
                    {
                        DynamicScriptIO.Output.WriteLine(Resources.StorageName, slot.Key);
                        DynamicScriptIO.Output.WriteLine(Resources.StorageSemantics, slot.Value.Attributes);
                        DynamicScriptIO.Output.WriteLine(Resources.StorageContract, slot.Value.GetContractBinding());
                        var value = default(string);
                        DynamicScriptIO.Output.WriteLine(Resources.StorageValue, slot.Value.TryGetValue(out value, state) ? value : Resources.UnprintableValue);
                        DynamicScriptIO.Output.WriteLine();
                    }
            }
        }

        [ComVisible(false)]
        [TransparentAction]
        private sealed class CallStackAction : ScriptAction
        {
            public const string Name = "cstack";

            public CallStackAction()
                : base()
            {
            }

            protected override void Invoke(InterpreterState state)
            {
                foreach (var frame in CallStack.GetSnapshot())
                    DynamicScriptIO.Output.WriteLine(Resources.CallStackFrameFormat, frame.ID, frame.Storages.Count);
            }
        }

        [ComVisible(false)]
        [TransparentAction]
        private sealed class ModulesAction : ScriptAction
        {
            public const string Name = "modules";

            protected override void Invoke(InterpreterState state)
            {
                if (ScriptDebugger.CurrentDebugger != null)
                    foreach (var module in ((IScriptDebuggerSession)ScriptDebugger.CurrentDebugger).Modules)
                        DynamicScriptIO.Output.WriteLine(module);
            }
        }

        [ComVisible(false)]
        [TransparentAction]
        private sealed class GetContextAction : ScriptAction
        {
            public const string Name = "getctx";

            protected override void Invoke(InterpreterState state)
            {
                switch (state.Context)
                {
                    case InterpretationContext.Checked:
                        DynamicScriptIO.Output.WriteLine(Keyword.Checked);
                        return;
                    case InterpretationContext.Unchecked:
                        DynamicScriptIO.Output.WriteLine(Keyword.Unchecked);
                        return;
                }
            }
        }

        [ComVisible(false)]
        [TransparentAction]
        private sealed class ClearScreenAction : ScriptAction
        {
            public const string Name = "clrscr";

            protected override void Invoke(InterpreterState state)
            {
                Console.Clear();
            }
        }
        
        [ComVisible(false)]
        private new sealed class Slots : ObjectSlotCollection
        {
            public Slots()
            {
                AddConstant<LocalsAction>(LocalsAction.Name);
                AddConstant<CallStackAction>(CallStackAction.Name);
                AddConstant<ModulesAction>(ModulesAction.Name);
                AddConstant<GetContextAction>(GetContextAction.Name);
                AddConstant<ClearScreenAction>(ClearScreenAction.Name);
            }
        }
        #endregion

        public const string Name = "debugger";

        private RTDebuggerObject()
            : base(new Slots())
        {
        }

        public static readonly RTDebuggerObject DebuggerRuntime = new RTDebuggerObject();

        public static readonly ScriptConstant Storage = new ScriptConstant(DebuggerRuntime);
    }
}
