using System;
using System.Collections.Generic;

namespace DynamicScript.Runtime.Environment.ObjectModel
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;
    using NativeGarbageCollector = System.GC;
    using InliningSourceAttribute = Compiler.Ast.Translation.LinqExpressions.InliningSourceAttribute;
    using InterpretationContext = Compiler.Ast.InterpretationContext;

    /// <summary> 
    /// Represents DynamicScript garbage collector.
    /// This class cannot be inherited.
    /// </summary>
    [ComVisible(false)]
    [CLSCompliant(false)]
    public sealed class VM: ScriptCompositeObject
    {
        internal const string Name = "vm";
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

        [ComVisible(false)]
        private sealed class GcFunction : ScriptAction<ScriptBoolean>
        {
            public const string Name = "gc";
            private const string FirstParamName = "wait";

            public GcFunction()
                : base(FirstParamName, ScriptBooleanContract.Instance)
            {
            }

            protected override void Invoke(ScriptBoolean wait, InterpreterState state)
            {
                GC(wait, state);
            }
        }

        [ComVisible(false)]
        private sealed class TotalMemSlot : RuntimeSlotBase, IStaticRuntimeSlot
        {
            public const string Name = "totalmem";

            public override IScriptObject GetValue(InterpreterState state)
            {
                return (ScriptInteger)VM.TotalMem;
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
        private sealed class ClearFunction : ScriptAction<ScriptObject>
        {
            public const string Name = "clear";
            private const string FirstParamName = "obj";

            public ClearFunction()
                : base(FirstParamName, ScriptSuperContract.Instance)
            {
            }

            protected override void Invoke(ScriptObject obj, InterpreterState state)
            {
                obj.Clear();
            }
        }
        #endregion

        private static new IEnumerable<KeyValuePair<string, IStaticRuntimeSlot>> Slots
        {
            get
            {
                yield return Constant(GcFunction.Name, new GcFunction());
                yield return new KeyValuePair<string, IStaticRuntimeSlot>(TotalMemSlot.Name, new TotalMemSlot());
                yield return new KeyValuePair<string, IStaticRuntimeSlot>(OmitVoidInLoops.Name, new OmitVoidInLoops());
            }
        }

        /// <summary>
        /// Initializes a new garbage collector object.
        /// </summary>
        private VM()
            : base(Slots)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        public static readonly VM Instance = new VM();

        /// <summary>
        /// Forces an immediate garbage collection.
        /// </summary>
        [InliningSource]
        public static IScriptObject GC(ScriptBoolean wait, InterpreterState state)
        {
            NativeGarbageCollector.Collect();
            if (wait) NativeGarbageCollector.WaitForPendingFinalizers();
            return Void;
        }

        /// <summary>
        /// Gets the number of bytes currently thought to be allocated.
        /// </summary>
        public static ScriptInteger TotalMem
        {
            get { return NativeGarbageCollector.GetTotalMemory(false); }
        }
    }
}
