using System;

namespace DynamicScript.Runtime.Environment.Threading
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;

    [ComVisible(false)]
    sealed class ScriptNativeQueue: ScriptCompositeObject, IScriptWorkItemQueue
    {
        public const string EnqueueActionName = "enqueue";
        #region Nested Types
        [ComVisible(false)]
        private sealed class ScriptAwaitFunction : ScriptFunc<ScriptReal, IScriptObject>
        {
            private readonly IWorkItemState<TimeSpan, IScriptObject> m_state;

            public ScriptAwaitFunction(IWorkItemState<TimeSpan, IScriptObject> workItemState)
                : base(AwaitFunctionContract.FirstParamName, ScriptRealContract.Instance, AwaitFunctionContract.SecondParamName, ScriptSuperContract.Instance, ScriptSuperContract.Instance)
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
        private sealed class ScriptEnqueueAction : ScriptFunc<IScriptFunction>
        {
            public const string Name = EnqueueActionName;
            private const string FirstParamName = "workItem";

            private readonly IScriptWorkItemQueue m_queue;

            public ScriptEnqueueAction(IScriptWorkItemQueue queue)
                : base(FirstParamName, ScriptSuperContract.Instance, ScriptSuperContract.Instance)
            {
                m_queue = queue;
            }

            protected override IScriptObject Invoke(IScriptFunction workItem, InterpreterState state)
            {
                return new ScriptAwaitFunction(m_queue.Enqueue(workItem.This, (t, s) => workItem.Invoke(EmptyArray, s), state));
            }
        }

        [ComVisible(false)]
        private new sealed class Slots : ObjectSlotCollection
        {
            public Slots(IScriptWorkItemQueue queue)
            {
                AddConstant(ScriptEnqueueAction.Name, new ScriptEnqueueAction(queue));
            }
        }

        #endregion

        private readonly IScriptWorkItemQueue m_queue;
        private static IScriptContract m_contract;

        public ScriptNativeQueue(IScriptWorkItemQueue queue)
            : base(new Slots(queue))
        {
            m_queue = queue;
        }

        IWorkItemState<TimeSpan, IScriptObject> IScriptWorkItemQueue.Enqueue(IScriptObject target, ScriptWorkItem workItem, InterpreterState state)
        {
            return m_queue.Enqueue(target, workItem, state);
        }

        public static ScriptFunc<ScriptReal, IScriptObject> CreateAwaitLambda(IWorkItemState<TimeSpan, IScriptObject> workItemState)
        {
            return new ScriptAwaitFunction(workItemState);
        }

        public static IScriptContract ContractBinding
        {
            get
            {
                if (m_contract == null) m_contract = new ScriptNativeQueue(null).GetContractBinding();
                return m_contract;
            }
        }
    }
}
