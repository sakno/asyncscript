using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace DynamicScript.Runtime.Environment.ExpressionTrees
{
    using Compiler.Ast;
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;

    [ComVisible(false)]
    [Serializable]
    sealed class ScriptActionExpressionFactory : ScriptExpressionFactory<ScriptCodeActionImplementationExpression, ScriptActionExpression>, IActionExpressionFactorySlots
    {
        #region Nested Types

        [ComVisible(false)]
        private sealed class ModifyAction : ModifyActionBase
        {
            private const string SecondParamName = "sig";
            private const string ThirdParamName = "body";

            public ModifyAction()
                : base(Instance, new ScriptActionContract.Parameter(SecondParamName, ScriptActionContractExpressionFactory.Instance),
                new ScriptActionContract.Parameter(ThirdParamName, new ScriptArrayContract(ScriptStatementFactory.Instance)))
            {
            }
        }

        [ComVisible(false)]
        private sealed class GetBodyAction : CodeElementPartProvider<IScriptArray>
        {
            public GetBodyAction()
                :base(Instance, new ScriptArrayContract(ScriptStatementFactory.Instance))
            {
            }

            protected override IScriptArray Invoke(ScriptCodeActionImplementationExpression element, InterpreterState state)
            {
                return ScriptStatementFactory.CreateStatements(element.Body, state);
            }
        }

        [ComVisible(false)]
        private sealed class GetSignatureAction : CodeElementPartProvider<IScriptCodeElement<ScriptCodeActionContractExpression>>
        {
            public GetSignatureAction()
                : base(Instance, ScriptActionContractExpressionFactory.Instance)
            {
            }

            protected override IScriptCodeElement<ScriptCodeActionContractExpression> Invoke(ScriptCodeActionImplementationExpression element, InterpreterState state)
            {
                return new ScriptActionContractExpression(element.Signature);
            }
        }
        #endregion

        public new const string Name = "action";

        private IRuntimeSlot m_modify;
        private IRuntimeSlot m_sig;
        private IRuntimeSlot m_body; 

        private ScriptActionExpressionFactory(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        private ScriptActionExpressionFactory()
            : base(Name)
        {
        }

        public static readonly ScriptActionExpressionFactory Instance = new ScriptActionExpressionFactory();

        public static ScriptActionExpression CreateExpression(IScriptCodeElement<ScriptCodeActionContractExpression> signature, IEnumerable<IScriptObject> body)
        {
            return new ScriptActionExpression(ScriptActionExpression.CreateExpression(signature, body));
        }

        public override ScriptActionExpression CreateCodeElement(IList<IScriptObject> args, InterpreterState state)
        {
            return args.Count == 2 ? CreateExpression(args[0] as IScriptCodeElement<ScriptCodeActionContractExpression>, args[1] as IEnumerable<IScriptObject>) : null;
        }

        public override void Clear()
        {
            m_body = m_modify = m_sig = null;
        }

        protected override IRuntimeSlot Modify
        {
            get { return CacheConst<ModifyAction>(ref m_modify); }
        }

        IRuntimeSlot IActionExpressionFactorySlots.Sig
        {
            get { return CacheConst<GetSignatureAction>(ref m_sig); }
        }

        IRuntimeSlot IActionExpressionFactorySlots.Body
        {
            get { return CacheConst<GetBodyAction>(ref m_body); }
        }
    }
}
