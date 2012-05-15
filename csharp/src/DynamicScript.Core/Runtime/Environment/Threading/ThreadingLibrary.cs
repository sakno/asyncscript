using System;
using System.Collections.Generic;
using System.Threading;

namespace DynamicScript.Runtime.Environment.Threading
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;
    using ClrEnvironment = System.Environment;
    using Enumerable = System.Linq.Enumerable;
    using InterpretationContext = Compiler.Ast.InterpretationContext;
    using InliningSourceAttribute = Compiler.Ast.Translation.LinqExpressions.InliningSourceAttribute;

    [ComVisible(false)]
    sealed class ThreadingLibrary: ScriptCompositeObject
    {
        internal const string Name = "threading";
        #region Nested Types
        [ComVisible(false)]
        private sealed class IsLazyFunction : ScriptFunc<IScriptObject>
        {
            public const string Name = "islazy";
            private const string FirstParamName = "obj";

            public IsLazyFunction()
                : base(FirstParamName, ScriptSuperContract.Instance, ScriptBooleanContract.Instance)
            {
            }

            protected override IScriptObject Invoke(IScriptObject obj, InterpreterState state)
            {
                return IsLazy(obj, state);
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
                Sleep(duration, state);
            }
        }

        [ComVisible(false)]
        private sealed class CreateLazyQueueFunction : ScriptFunc
        {
            public const string Name = "createLazyQueue";

            public CreateLazyQueueFunction()
                : base(ScriptNativeQueue.ContractBinding)
            {
            }

            protected override IScriptObject Invoke(InterpreterState state)
            {
                return CreateLazyQueue(state);
            }
        }

        [ComVisible(false)]
        private sealed class CreateDefaultQueueFunction : ScriptFunc
        {
            public const string Name = "createDefaultQueue";

            public CreateDefaultQueueFunction()
                : base(ScriptNativeQueue.ContractBinding)
            {
            }

            protected override IScriptObject Invoke(InterpreterState state)
            {
                return CreateDefaultQueue(state);
            }
        }

        [ComVisible(false)]
        private sealed class CreateParallelQueueFunction : ScriptFunc
        {
            public const string Name = "createParallelQueue";

            public CreateParallelQueueFunction()
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
                return ThreadingLibrary.Unwrap(asyncobj, state);
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
                AddConstant<CreateDefaultQueueFunction>(CreateDefaultQueueFunction.Name);
                AddConstant<CreateParallelQueueFunction>(CreateParallelQueueFunction.Name);
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

        private ThreadingLibrary()
            : base(new Slots())
        {
        }

        /// <summary>
        /// Represents singleton instance of the threading library.
        /// </summary>
        public static readonly ThreadingLibrary Instance = new ThreadingLibrary();

        /// <summary>
        /// Suspends this thread for a specified time.
        /// </summary>
        /// <param name="duration"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        [InliningSource]
        public static IScriptObject Sleep(ScriptReal duration, InterpreterState state)
        {
            Thread.Sleep(TimeSpan.FromMilliseconds(duration));
            return Void;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        [InliningSource]
        public static ScriptBoolean IsLazy(IScriptObject obj, InterpreterState state)
        {
            return obj is IScriptProxyObject;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="state"></param>
        /// <returns></returns>
        [InliningSource]
        public static IScriptCompositeObject CreateLazyQueue(InterpreterState state)
        {
            return new ScriptNativeQueue(new LazyQueue());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="state"></param>
        /// <returns></returns>
        [InliningSource]
        public static IScriptCompositeObject CreateDefaultQueue(InterpreterState state)
        {
            return new ScriptNativeQueue(new DefaultQueue());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="state"></param>
        /// <returns></returns>
        [InliningSource]
        public static IScriptCompositeObject CreateParallelQueue(InterpreterState state)
        {
            return new ScriptNativeQueue(ParallelQueue.Instance);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="asyncobj"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        [InliningSource]
        public static IScriptObject Unwrap(IScriptObject asyncobj, InterpreterState state)
        {
            return asyncobj is IScriptProxyObject ? ((IScriptProxyObject)asyncobj).Unwrap(state) : asyncobj;
        }
    }
}
