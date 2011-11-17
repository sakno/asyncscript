using System;
using System.Collections.Generic;
using System.Threading;

namespace DynamicScript.Runtime.Environment.Threading
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;
    using ClrEnvironment = System.Environment;
    using Enumerable = System.Linq.Enumerable;

    [ComVisible(false)]
    sealed class ThreadingLibrary: ScriptCompositeObject
    {
        public const string Name = "threading";
        #region Nested Types
        [ComVisible(false)]
        private sealed class IsLazyAction : ScriptFunc<IScriptObject>
        {
            public const string Name = "is_lazy";
            private const string FirstParamName = "obj";

            public IsLazyAction()
                : base(FirstParamName, ScriptSuperContract.Instance, ScriptBooleanContract.Instance)
            {
            }

            protected override IScriptObject Invoke(IScriptObject obj, InterpreterState state)
            {
                return (ScriptBoolean)(obj is IScriptProxyObject);
            }
        }

        [ComVisible(false)]
        private sealed class SleepAction : ScriptAction<ScriptReal>
        {
            /// <summary>
            /// Represents name of the action.
            /// </summary>
            public const string Name = "sleep";
            private static string FirstParamName = "interval";

            /// <summary>
            /// Initializes a new 'puts' action stub.
            /// </summary>
            public SleepAction()
                : base(FirstParamName, ScriptRealContract.Instance)
            {
            }

            protected override void Invoke(ScriptReal duration, InterpreterState state)
            {
                Thread.Sleep(TimeSpan.FromMilliseconds(duration));
            }
        }

        [ComVisible(false)]
        private sealed class CreateLazyQueueAction : ScriptFunc
        {
            public const string Name = "create_lazy_queue";

            public CreateLazyQueueAction()
                : base(ScriptNativeQueue.ContractBinding)
            {
            }

            protected override IScriptObject Invoke(InterpreterState state)
            {
                return new ScriptNativeQueue(new LazyQueue());
            }
        }

        [ComVisible(false)]
        private sealed class CreateDefaultQueue : ScriptFunc
        {
            public const string Name = "create_default_queue";

            public CreateDefaultQueue()
                : base(ScriptNativeQueue.ContractBinding)
            {
            }

            protected override IScriptObject Invoke(InterpreterState state)
            {
                return new ScriptNativeQueue(new DefaultQueue());
            }
        }

        [ComVisible(false)]
        private sealed class CreateParallelQueue : ScriptFunc
        {
            public const string Name = "create_parallel_queue";

            public CreateParallelQueue()
                : base(ScriptNativeQueue.ContractBinding)
            {
            }

            protected override IScriptObject Invoke(InterpreterState state)
            {
                return new ScriptNativeQueue(ParallelQueue.Instance);
            }
        }

        [ComVisible(false)]
        private sealed class UnwrapAction : ScriptFunc<IScriptObject>
        {
            public const string Name = "unwrap";
            private const string FirstParamName = "asyncobj";

            public UnwrapAction()
                : base(FirstParamName, ScriptSuperContract.Instance, ScriptSuperContract.Instance)
            {
            }

            protected override IScriptObject Invoke(IScriptObject asyncobj, InterpreterState state)
            {
                return asyncobj is IScriptProxyObject ? ((IScriptProxyObject)asyncobj).Unwrap(state) : asyncobj;
            }
        }

        [ComVisible(false)]
        private sealed class QueueSlot : RuntimeSlotBase
        {
            public const string Name = "queue";

            public override IScriptObject GetValue(InterpreterState state)
            {
                var queue = ThreadManager.Queue;
                return queue is IScriptObject ? (IScriptObject)queue : new ScriptNativeQueue(queue);
            }

            public override void SetValue(IScriptObject value, InterpreterState state)
            {
                ThreadManager.Queue = ThreadManager.CreateQueue(value);
            }

            public override IScriptContract ContractBinding
            {
                get { return ScriptNativeQueue.ContractBinding; }
            }

            public override RuntimeSlotAttributes Attributes
            {
                get { return RuntimeSlotAttributes.None; }
            }

            protected override ICollection<string> Slots
            {
                get { return new string[0]; }
            }

            public override bool DeleteValue()
            {
                return false;
            }

            public override bool Equals(IRuntimeSlot other)
            {
                return other is QueueSlot;
            }
        }

        [ComVisible(false)]
        private sealed class ThreadName : RuntimeSlotBase, IEquatable<ThreadName>
        {
            public const string Name = "threadName";
            /// <summary>
            /// Gets or sets thread name.
            /// </summary>
            public static ScriptString Value
            {
                get { return Thread.CurrentThread.Name ?? string.Empty; }
                set { if (Thread.CurrentThread.Name == null)Thread.CurrentThread.Name = value; }
            }

            public override IScriptObject GetValue(InterpreterState state)
            {
                return Value;
            }

            public override void SetValue(IScriptObject value, InterpreterState state)
            {
                Value = value.ToString();
            }

            public override IScriptContract ContractBinding
            {
                get { return ScriptStringContract.Instance; }
            }

            public override RuntimeSlotAttributes Attributes
            {
                get { return RuntimeSlotAttributes.None; }
            }

            protected override ICollection<string> Slots
            {
                get { return Value.Slots; }
            }

            public override bool DeleteValue()
            {
                return false;
            }

            public bool Equals(ThreadName other)
            {
                return other != null;
            }

            public override bool Equals(IRuntimeSlot other)
            {
                return Equals(other as ThreadName);
            }

            public override int GetHashCode()
            {
                return Value.GetHashCode();
            }
        }

        [ComVisible(false)]
        private new sealed class Slots : ObjectSlotCollection
        {
            private const string ProcessorsSlot = "processors";

            public Slots()
            {
                AddConstant(ProcessorsSlot, new ScriptInteger(ClrEnvironment.ProcessorCount));
                AddConstant<SleepAction>(SleepAction.Name);
                AddConstant<IsLazyAction>(IsLazyAction.Name);
                AddConstant("timeout", ScriptWorkItemQueue.Timeout);
                Add(QueueSlot.Name, new QueueSlot());
                AddConstant<CreateLazyQueueAction>(CreateLazyQueueAction.Name);
                AddConstant<UnwrapAction>(UnwrapAction.Name);
                AddConstant<CreateDefaultQueue>(CreateDefaultQueue.Name);
                AddConstant<CreateParallelQueue>(CreateParallelQueue.Name);
                Add(ThreadName.Name, new ThreadName());
            }
        }
        #endregion

        private static IEnumerable<WaitHandle> GetWaitHandles(IScriptArray handles, InterpreterState state)
        {
            var indicies=new long[1];
            for (var i = 0L; i < ScriptArray.GetTotalLength(handles); i++)
            {
                indicies[0] = i;
                var obj = handles[indicies, state];
                if (obj is IAsyncResult) yield return ((IAsyncResult)obj).AsyncWaitHandle;
            }
        }

        public ThreadingLibrary()
            : base(new Slots())
        {
        }
    }
}
