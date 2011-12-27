using System;
using System.Collections.Generic;
using System.Threading;

namespace DynamicScript.Runtime.Environment.Threading
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;
    using ClrEnvironment = System.Environment;
    using Enumerable = System.Linq.Enumerable;
    using InterpretationContext = Compiler.Ast.InterpretationContext;

    [ComVisible(false)]
    sealed class ThreadingLibrary: ScriptCompositeObject
    {
        public const string Name = "threading";
        #region Nested Types
        [ComVisible(false)]
        private sealed class IsLazyFunction : ScriptFunc<IScriptObject>
        {
            public const string Name = "is_lazy";
            private const string FirstParamName = "obj";

            public IsLazyFunction()
                : base(FirstParamName, ScriptSuperContract.Instance, ScriptBooleanContract.Instance)
            {
            }

            protected override IScriptObject Invoke(IScriptObject obj, InterpreterState state)
            {
                return (ScriptBoolean)(obj is IScriptProxyObject);
            }
        }

        [ComVisible(false)]
        private sealed class SleepFunction : ScriptAction<ScriptReal>
        {
            /// <summary>
            /// Represents name of the action.
            /// </summary>
            public const string Name = "sleep";
            private static string FirstParamName = "interval";

            /// <summary>
            /// Initializes a new 'puts' action stub.
            /// </summary>
            public SleepFunction()
                : base(FirstParamName, ScriptRealContract.Instance)
            {
            }

            protected override void Invoke(ScriptReal duration, InterpreterState state)
            {
                Thread.Sleep(TimeSpan.FromMilliseconds(duration));
            }
        }

        [ComVisible(false)]
        private sealed class CreateLazyQueueFunction : ScriptFunc
        {
            public const string Name = "create_lazy_queue";

            public CreateLazyQueueFunction()
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
        private sealed class UnwrapFunction : ScriptFunc<IScriptObject>
        {
            public const string Name = "unwrap";
            private const string FirstParamName = "asyncobj";

            public UnwrapFunction()
                : base(FirstParamName, ScriptSuperContract.Instance, ScriptSuperContract.Instance)
            {
            }

            protected override IScriptObject Invoke(IScriptObject asyncobj, InterpreterState state)
            {
                return asyncobj is IScriptProxyObject ? ((IScriptProxyObject)asyncobj).Unwrap(state) : asyncobj;
            }
        }

        [ComVisible(false)]
        private sealed class QueueSlot : RuntimeSlotBase, IStaticRuntimeSlot
        {
            public const string Name = "queue";

            public override IScriptObject GetValue(InterpreterState state)
            {
                var queue = ThreadManager.Queue;
                return queue is IScriptObject ? (IScriptObject)queue : new ScriptNativeQueue(queue);
            }

            public override IScriptObject SetValue(IScriptObject value, InterpreterState state)
            {
                ThreadManager.Queue = ThreadManager.CreateQueue(value);
                return value;
            }

            public IScriptContract ContractBinding
            {
                get { return ScriptNativeQueue.ContractBinding; }
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
        private sealed class ThreadName : RuntimeSlotBase, IStaticRuntimeSlot
        {
            public const string Name = "threadName";

            public override IScriptObject GetValue(InterpreterState state)
            {
                return new ScriptString(Thread.CurrentThread.Name ?? string.Empty);
            }

            public override IScriptObject SetValue(IScriptObject value, InterpreterState state)
            {
                if (ScriptStringContract.TryConvert(ref value))
                {
                    if (Thread.CurrentThread.Name == null) Thread.CurrentThread.Name = (ScriptString)value;
                    return value;
                }
                else if (state.Context == InterpretationContext.Unchecked)
                    return Void;
                else throw new ContractBindingException(value, ScriptStringContract.Instance, state);
            }

            public IScriptContract ContractBinding
            {
                get { return ScriptStringContract.Instance; }
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
        private new sealed class Slots : ObjectSlotCollection
        {
            private const string ProcessorsSlot = "processors";

            public Slots()
            {
                AddConstant(ProcessorsSlot, new ScriptInteger(ClrEnvironment.ProcessorCount));
                AddConstant<SleepFunction>(SleepFunction.Name);
                AddConstant<IsLazyFunction>(IsLazyFunction.Name);
                AddConstant("timeout", ScriptWorkItemQueue.Timeout);
                Add(QueueSlot.Name, new QueueSlot());
                AddConstant<CreateLazyQueueFunction>(CreateLazyQueueFunction.Name);
                AddConstant<UnwrapFunction>(UnwrapFunction.Name);
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
