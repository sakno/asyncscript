using System;
using System.Collections.Generic;

namespace DynamicScript.Runtime.Environment.ObjectModel
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;
    using NativeGarbageCollector = System.GC;

    /// <summary> 
    /// Represents DynamicScript garbage collector.
    /// This class cannot be inherited.
    /// </summary>
    [ComVisible(false)]
    [CLSCompliant(false)]
    public sealed class GC: ScriptCompositeObject
    {
        internal const string Name = "gc";
        #region Nested Types
        [ComVisible(false)]
        private sealed class CollectFunction : ScriptAction
        {
            public const string Name = "collect";

            protected override void Invoke(InterpreterState state)
            {
                Collect();
            }
        }

        [ComVisible(false)]
        private sealed class WaitFunction : ScriptAction
        {
            public const string Name = "wait";

            protected override void Invoke(InterpreterState state)
            {
                Wait();
            }
        }

        [ComVisible(false)]
        private sealed class TotalMemSlot : RuntimeSlotBase, IStaticRuntimeSlot
        {
            public const string Name = "totalmem";

            public override IScriptObject GetValue(InterpreterState state)
            {
                return (ScriptInteger)GC.TotalMem;
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
                yield return Constant(CollectFunction.Name, new CollectFunction());
                yield return Constant(WaitFunction.Name, new WaitFunction());
                yield return new KeyValuePair<string, IStaticRuntimeSlot>(TotalMemSlot.Name, new TotalMemSlot());
            }
        }

        /// <summary>
        /// Initializes a new garbage collector object.
        /// </summary>
        public GC()
            : base(Slots)
        {
        }

        /// <summary>
        /// Forces an immediate garbage collection.
        /// </summary>
        public static void Collect()
        {
            NativeGarbageCollector.Collect();
        }

        /// <summary>
        /// Suspends the current thread until the thread that is processing the queue of finalizers has emptied the queue.
        /// </summary>
        public static void Wait()
        {
            NativeGarbageCollector.WaitForPendingFinalizers();
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
