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
        private sealed class CollectAction : ScriptAction
        {
            public const string Name = "collect";

            protected override void Invoke(InvocationContext ctx)
            {
                Collect(ctx);
            }
        }

        [ComVisible(false)]
        private sealed class WaitAction : ScriptAction
        {
            public const string Name = "wait";

            protected override void Invoke(InvocationContext ctx)
            {
                Wait(ctx);
            }
        }

        [ComVisible(false)]
        private sealed class TotalMemSlot : RuntimeSlotBase, IEquatable<TotalMemSlot>
        {
            public const string Name = "totalmem";

            public ScriptInteger Value
            {
                get { return GC.TotalMem; }
            }

            protected override IScriptContract GetValueContract()
            {
                return ContractBinding;
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

            public bool Equals(TotalMemSlot other)
            {
                return other != null;
            }

            public override bool Equals(IRuntimeSlot other)
            {
                return Equals(other as TotalMemSlot);
            }

            public override bool Equals(object other)
            {
                return Equals(other as TotalMemSlot);
            }

            public override int GetHashCode()
            {
                return GetType().MetadataToken;
            }
        }

        #endregion

        private static new IEnumerable<KeyValuePair<string, IRuntimeSlot>> Slots
        {
            get
            {
                yield return Constant(CollectAction.Name, new CollectAction());
                yield return Constant(WaitAction.Name, new WaitAction());
                yield return new KeyValuePair<string, IRuntimeSlot>(TotalMemSlot.Name, new TotalMemSlot());
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
        /// <param name="ctx">The invocation context.</param>
        public static void Collect(InvocationContext ctx)
        {
            NativeGarbageCollector.Collect();
        }

        /// <summary>
        /// Suspends the current thread until the thread that is processing the queue of finalizers has emptied the queue.
        /// </summary>
        /// <param name="ctx"></param>
        public static void Wait(InvocationContext ctx)
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
