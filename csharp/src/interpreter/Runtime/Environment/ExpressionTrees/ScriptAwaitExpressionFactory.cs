using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace DynamicScript.Runtime.Environment.ExpressionTrees
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;
    using Compiler.Ast;

    [ComVisible(false)]
    [Serializable]
    sealed class ScriptAwaitExpressionFactory : ScriptExpressionFactory<ScriptCodeAwaitExpression, ScriptAwaitExpression>, IAwaitExpressionFactorySlots
    {
        #region Nested Types
        [ComVisible(false)]
        private sealed class ModifyAction : ModifyActionBase
        {
            private const string FirstParamName = "ar";
            private const string SecondParamName = "s";

            public ModifyAction()
                : base(Instance, new ScriptActionContract.Parameter(FirstParamName, ScriptExpressionFactory.Instance), new ScriptActionContract.Parameter(SecondParamName, ScriptExpressionFactory.Instance))
            {
            }
        }

        [ComVisible(false)]
        private sealed class GetSynchronizerAction : CodeElementPartProvider<IScriptExpression<ScriptCodeExpression>>
        {
            public GetSynchronizerAction()
                : base(Instance, ScriptExpressionFactory.Instance)
            {
            }

            protected override IScriptExpression<ScriptCodeExpression> Invoke(ScriptCodeAwaitExpression element, InterpreterState state)
            {
                return element.Synchronizer != null ? Convert(element.Synchronizer) as IScriptExpression<ScriptCodeExpression> : null;
            }
        }

        [ComVisible(false)]
        private sealed class GetAsyncResultAction : CodeElementPartProvider<IScriptExpression<ScriptCodeExpression>>
        {
            public GetAsyncResultAction()
                : base(Instance, ScriptExpressionFactory.Instance)
            {
            }

            protected override IScriptExpression<ScriptCodeExpression> Invoke(ScriptCodeAwaitExpression element, InterpreterState state)
            {
                return element.AsyncResult != null ? Convert(element.AsyncResult) as IScriptExpression<ScriptCodeExpression> : null;
            }
        }
        #endregion
        public new const string Name = "awaitdef";

        private IRuntimeSlot m_modify;
        private IRuntimeSlot m_ar;
        private IRuntimeSlot m_sync;

        private ScriptAwaitExpressionFactory(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        private ScriptAwaitExpressionFactory()
            : base(Name)
        {
        }

        public static readonly ScriptAwaitExpressionFactory Instance = new ScriptAwaitExpressionFactory();

        public static ScriptAwaitExpression CreateExpression(IScriptExpression<ScriptCodeExpression> asyncResult, IScriptExpression<ScriptCodeExpression> synchronizer = null)
        {
            var expression = ScriptAwaitExpression.CreateExpression(asyncResult, synchronizer);
            return expression != null ? new ScriptAwaitExpression(expression) : null;
        }

        public override ScriptAwaitExpression CreateCodeElement(IList<IScriptObject> args, InterpreterState state)
        {
            switch (args.Count)
            {
                case 1: return CreateExpression(args[0] as IScriptExpression<ScriptCodeExpression>);
                case 2: return CreateExpression(args[0] as IScriptExpression<ScriptCodeExpression>, args[1] as IScriptExpression<ScriptCodeExpression>);
                default: return null;
            }
        }

        protected override IRuntimeSlot Modify
        {
            get { return CacheConst<ModifyAction>(ref m_modify); }
        }

        IRuntimeSlot IAwaitExpressionFactorySlots.AsyncResult
        {
            get { return CacheConst<GetAsyncResultAction>(ref m_ar); }
        }

        IRuntimeSlot IAwaitExpressionFactorySlots.Synchronizer
        {
            get { return CacheConst<GetSynchronizerAction>(ref m_sync); }
        }

        public override void Clear()
        {
            m_ar = m_modify = m_sync = null;
        }
    }
}
