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
        private sealed class WaitAllAction : ScriptFunc<IScriptArray, ScriptReal>
        {
            public const string Name = "waitall";
            private const string FirstParamName = "asyncObjects";
            private const string SecondParamName = "timeout";

            public WaitAllAction()
                : base(FirstParamName, new ScriptArrayContract(ScriptSuperContract.Instance), SecondParamName, ScriptRealContract.Instance, ScriptBooleanContract.Instance)
            {
            }

            private static bool WaitAll(WaitHandle[] handles, double timeout)
            {
                return handles.LongLength > 0L ? WaitHandle.WaitAll(handles, TimeSpan.FromMilliseconds(timeout)) : true;
            }

            protected override IScriptObject Invoke(IScriptArray objects, ScriptReal timeout, InterpreterState state)
            {
                return (ScriptBoolean)(objects != null ? WaitAll(Enumerable.ToArray(GetWaitHandles(objects, state)), timeout ?? ScriptReal.Zero) : false);
            }
        }

        [ComVisible(false)]
        private sealed class WaitAnyAction : ScriptFunc<IScriptArray, ScriptReal>
        {
            public const string Name = "waitany";
            private const string FirstParamName = "asyncObjects";
            private const string SecondParamName = "timeout";

            public WaitAnyAction()
                : base(FirstParamName, new ScriptArrayContract(ScriptSuperContract.Instance), SecondParamName, ScriptRealContract.Instance, ScriptIntegerContract.Instance)
            {
            }

            private static long WaitAny(WaitHandle[] handles, double timeout)
            {
                return handles.LongLength > 0L ? WaitHandle.WaitAny(handles, TimeSpan.FromMilliseconds(timeout)) : -1L;
            }

            protected override IScriptObject Invoke(IScriptArray objects, ScriptReal timeout, InterpreterState state)
            {
                return (ScriptInteger)(objects != null ? WaitAny(Enumerable.ToArray(GetWaitHandles(objects, state)), timeout ?? ScriptReal.Zero) : -1L);
            }
        }

        [ComVisible(false)]
        private sealed class CreateLazyQueueAction : ScriptFunc
        {
            public const string Name = "create_lazy_queue";

            public CreateLazyQueueAction()
                : base(ScriptCompositeContract.Empty)
            {
            }

            protected override IScriptObject Invoke(InterpreterState state)
            {
                return new ScriptStaticQueue(new LazyQueue());
            }
        }

        [ComVisible(false)]
        private new sealed class Slots : ObjectSlotCollection
        {
            private const string ProcessorsSlot = "processors";

            public Slots()
            {
                AddConstant(ProcessorsSlot, new ScriptInteger(ClrEnvironment.ProcessorCount));
                AddConstant<WaitAllAction>(WaitAllAction.Name);
                AddConstant<WaitAnyAction>(WaitAnyAction.Name);
                AddConstant<SleepAction>(SleepAction.Name);
                AddConstant<IsLazyAction>(IsLazyAction.Name);
                AddConstant("timeout", ScriptWorkItemQueue.Timeout);
                AddConstant("default_queue", new ScriptStaticQueue(DefaultQueue.Instance));
                AddConstant<CreateLazyQueueAction>(CreateLazyQueueAction.Name);
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
