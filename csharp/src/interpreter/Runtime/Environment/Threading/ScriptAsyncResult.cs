using System;

namespace DynamicScript.Runtime.Environment.Threading
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;
    using WaitHandle = System.Threading.WaitHandle;

    /// <summary>
    /// Represents DynamicScript-compliant wrapper of <see cref="IScriptAsyncResult"/> interface.
    /// This class cannot be inherited.
    /// </summary>
    [ComVisible(false)]
    sealed class ScriptAsyncResult: ScriptCompositeObject, ISynchronizable, IAsyncResult
    {
        #region Nested Types
        [ComVisible(false)]
        private sealed class CompletedSlot : RuntimeSlotBase<IScriptAsyncResult, ScriptBoolean>
        {
            public const string Name = ScriptAsyncResultContract.CompletedSlot.Name;

            public CompletedSlot(IScriptAsyncResult ar)
                : base(ar, ScriptAsyncResultContract.CompletedSlot.ContractBinding, RuntimeSlotAttributes.Immutable)
            {
            }

            public override ScriptBoolean Value
            {
                get { return Target.IsCompleted; }
                set { }
            }
        }

        [ComVisible(false)]
        private sealed class CancelAction : ScriptAction
        {
            public const string Name = ScriptAsyncResultContract.CancelSlot.Name;
            private readonly IScriptAsyncResult m_ar;

            public CancelAction(IScriptAsyncResult ar)
            {
                m_ar = ar;
            }

            protected override void Invoke(InvocationContext ctx)
            {
                m_ar.Cancel();
            }
        }

        [ComVisible(false)]
        private sealed class ResultSlot : RuntimeSlotBase<IScriptAsyncResult, IScriptObject>
        {
            public const string Name = ScriptAsyncResultContract.ResultSlot.Name;

            public ResultSlot(IScriptAsyncResult ar, IScriptContract contract)
                : base(ar, contract, RuntimeSlotAttributes.Immutable)
            {
            }

            public override IScriptObject Value
            {
                get { return Target.Result; }
                set { }
            }
        }

        [ComVisible(false)]
        private sealed class NotifierSlot : RuntimeSlotBase<IScriptAsyncResult, IScriptAction>
        {
            public const string Name = ScriptAsyncResultContract.NotifierSlot.Name;

            public NotifierSlot(IScriptAsyncResult ar)
                : base(ar, ScriptAsyncResultContract.NotifierSlot.ContractBinding, RuntimeSlotAttributes.Immutable)
            {
            }

            public override IScriptAction Value
            {
                get { return Target.Notifier; }
                set { Target.Notifier = value; }
            }
        }

        [ComVisible(false)]
        private sealed class WaitAction : ScriptFunc<ScriptReal>
        {
            public const string Name = ScriptAsyncResultContract.WaitSlot.Name;

            private readonly IScriptAsyncResult m_ar;

            public WaitAction(IScriptAsyncResult ar)
                : base(ScriptAsyncResultContract.WaitSlot.ContractBinding.Parameters[0], ScriptAsyncResultContract.WaitSlot.ContractBinding.ReturnValueContract)
            {
                if (ar == null) throw new ArgumentNullException("ar");
                m_ar = ar;
            }

            protected override IScriptObject Invoke(InvocationContext ctx, ScriptReal timeout)
            {
                return (ScriptBoolean)m_ar.AsyncWaitHandle.WaitOne(TimeSpan.FromMilliseconds(timeout), true);
            }
        }

        [ComVisible(false)]
        private new sealed class Slots : ObjectSlotCollection
        {
            public Slots(IScriptAsyncResult ar, IScriptContract contract)
            {
                Add(CompletedSlot.Name, new CompletedSlot(ar));
                AddConstant(CancelAction.Name, new CancelAction(ar));
                Add(ResultSlot.Name, new ResultSlot(ar, contract));
                Add(NotifierSlot.Name, new NotifierSlot(ar));
                AddConstant(WaitAction.Name, new WaitAction(ar));
            }
        }
        #endregion

        private readonly IScriptAsyncResult m_ar;

        public ScriptAsyncResult(IScriptAsyncResult ar, IScriptContract contract)
            : base(new Slots(ar, contract))
        {
            m_ar = ar;
        }

        bool ISynchronizable.Await(WaitHandle handle, TimeSpan timeout)
        {
            return WaitHandle.WaitAny(new[] { m_ar.AsyncWaitHandle, handle }, timeout) == 0;
        }

        object IAsyncResult.AsyncState
        {
            get { return m_ar.AsyncState; }
        }

        WaitHandle IAsyncResult.AsyncWaitHandle
        {
            get { return m_ar.AsyncWaitHandle; }
        }

        bool IAsyncResult.CompletedSynchronously
        {
            get { return m_ar.CompletedSynchronously; }
        }

        bool IAsyncResult.IsCompleted
        {
            get { return m_ar.IsCompleted; }
        }
    }
}
