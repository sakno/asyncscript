using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace DynamicScript.Runtime.Environment.ExpressionTrees
{
    using Compiler.Ast;
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;

    [ComVisible(false)]
    [Serializable]
    sealed class ScriptAsyncExpressionFactory: ScriptExpressionFactory<ScriptCodeAsyncExpression, ScriptAsyncExpression>, IAsyncExpressionFactorySlots
    {
        #region Nested Types
        [ComVisible(false)]
        private sealed class ModifyAction : ModifyActionBase
        {
            private const string SecondParamName = "contract";

            public ModifyAction()
                : base( Instance, new ScriptActionContract.Parameter(SecondParamName, ScriptExpressionFactory.Instance))
            {
            }
        }

        [ComVisible(false)]
        private sealed class GetContractAction : CodeElementPartProvider<IScriptExpression<ScriptCodeExpression>>
        {
            public GetContractAction()
                : base(Instance, ScriptExpressionFactory.Instance)
            {
            }

            protected override IScriptExpression<ScriptCodeExpression> Invoke(ScriptCodeAsyncExpression element, InterpreterState state)
            {
                return Convert(element.Contract) as IScriptExpression<ScriptCodeExpression>;
            }
        }
        #endregion

        private new const string Name = "asyncdef";

        private IRuntimeSlot m_modify;
        private IRuntimeSlot m_contract;

        private ScriptAsyncExpressionFactory(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        private ScriptAsyncExpressionFactory()
            : base(Name)
        {
        }

        public static readonly ScriptAsyncExpressionFactory Instance = new ScriptAsyncExpressionFactory();

        public static ScriptAsyncExpression CreateExpression(IScriptObject contractDef)
        {
            var value = ScriptAsyncExpression.CreateExpression(contractDef);
            return value != null ? new ScriptAsyncExpression(value) : null;
        }

        public override ScriptAsyncExpression CreateCodeElement(IList<IScriptObject> args, InterpreterState state)
        {
            return args.Count == 1 ? CreateExpression(args[0]) : null;
        }

        public override void Clear()
        {
            m_contract = m_modify = null;
        }

        #region Runtime Slots

        protected override IRuntimeSlot Modify
        {
            get { return CacheConst<ModifyAction>(ref m_modify); }
        }

        IRuntimeSlot IAsyncExpressionFactorySlots.Contract
        {
            get { return CacheConst<GetContractAction>(ref m_contract); }
        }

        #endregion
    }
}
