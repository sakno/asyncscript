using System;

namespace DynamicScript.Runtime.Environment.Threading
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;

    [ComVisible(false)]
    sealed class ScriptStaticQueue: ScriptCompositeObject, IScriptWorkItemQueue
    {
        #region Nested Types
        [ComVisible(false)]
        private sealed class ScriptAwaitFunction : ScriptFunc<ScriptReal, IScriptObject>
        {
            private const string FirstParamName = "timeout";
            private const string SecondParamName = "Failure";

            private readonly IWorkItemState<TimeSpan, IScriptObject> m_state;

            public ScriptAwaitFunction(IWorkItemState<TimeSpan, IScriptObject> workItemState)
                : base(FirstParamName, ScriptRealContract.Instance, SecondParamName, ScriptSuperContract.Instance, ScriptSuperContract.Instance)
            {
                m_state = workItemState;
            }

            private static IScriptObject Await(IWorkItemState<TimeSpan, IScriptObject> workItemState, double timeout, IScriptObject failure, InterpreterState state)
            {
                return workItemState.WaitOne(TimeSpan.FromMilliseconds(timeout)) ? workItemState.Result : failure;
            }

            protected override IScriptObject Invoke(ScriptReal timeout, IScriptObject failure, InterpreterState state)
            {
                return Await(m_state, timeout, failure, state);
            }
        }

        [ComVisible(false)]
        private sealed class ScriptEnqueueAction : ScriptFunc<IScriptObject, IScriptAction>
        {
            public const string Name = "enqueue";
            private const string FirstParamName = "target";
            private const string SecondParamName = "workItem";

            private readonly IScriptWorkItemQueue m_queue;

            public ScriptEnqueueAction(IScriptWorkItemQueue queue)
                : base(FirstParamName, ScriptSuperContract.Instance, SecondParamName, ScriptSuperContract.Instance, ScriptSuperContract.Instance)
            {
                m_queue = queue;
            }

            protected override IScriptObject Invoke(IScriptObject target, IScriptAction workItem, InterpreterState state)
            {
                return new ScriptAwaitFunction(m_queue.Enqueue(target, (t, s) => workItem.Invoke(new[] { t }, s), state));
            }
        }

        [ComVisible(false)]
        private new sealed class Slots : ObjectSlotCollection
        {
            public Slots(IScriptWorkItemQueue queue)
            {
                if (queue == null) throw new ArgumentNullException("implementation");
                AddConstant(ScriptEnqueueAction.Name, new ScriptEnqueueAction(queue));
            }
        }
        #endregion

        private readonly IScriptWorkItemQueue m_queue;

        public ScriptStaticQueue(IScriptWorkItemQueue queue)
            : base(new Slots(queue))
        {
            m_queue = queue;
        }

        IWorkItemState<TimeSpan, IScriptObject> IScriptWorkItemQueue.Enqueue(IScriptObject target, ScriptWorkItem workItem, InterpreterState state)
        {
            return m_queue.Enqueue(target, workItem, state);
        }
    }
}
