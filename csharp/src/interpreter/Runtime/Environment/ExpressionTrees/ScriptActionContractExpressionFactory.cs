using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace DynamicScript.Runtime.Environment.ExpressionTrees
{
    using Compiler.Ast;
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;

    [ComVisible(false)]
    [Serializable]
    sealed class ScriptActionContractExpressionFactory : ScriptExpressionFactory<ScriptCodeActionContractExpression, ScriptActionContractExpression>, IActionContractExpressionFactorySlots
    {
        #region Nested Types
        [ComVisible(false)]
        private sealed class ModifyAction : ModifyActionBase
        {
            private const string SecondParamName = "params";
            private const string ThirdParamName = "retval";

            public ModifyAction()
                : base(Instance, new ScriptActionContract.Parameter(SecondParamName, new ScriptArrayContract(ScriptVariableDeclarationFactory.Instance)), new ScriptActionContract.Parameter(ThirdParamName, ScriptExpressionFactory.Instance))
            {
            }
        }

        [ComVisible(false)]
        private sealed class GetReturnTypeAction : CodeElementPartProvider<IScriptCodeElement<ScriptCodeExpression>>
        {
            public GetReturnTypeAction()
                : base(Instance)
            {
            }

            protected override IScriptCodeElement<ScriptCodeExpression> Invoke(ScriptCodeActionContractExpression element, InterpreterState state)
            {
                if (element.NoReturnValue) return null;
                else if (element.IsAsynchronous) return Convert(new ScriptCodeAsyncExpression(element.ReturnType)) as IScriptCodeElement<ScriptCodeExpression>;
                else return Convert(element.ReturnType) as IScriptCodeElement<ScriptCodeExpression>;
            }
        }

        [ComVisible(false)]
        private sealed class GetParametersAction : CodeElementPartProvider<IScriptArray>
        {
            public GetParametersAction()
                : base(Instance)
            {
            }

            private static IScriptCodeElement<ScriptCodeVariableDeclaration> CreateParameter(ScriptCodeActionContractExpression.Parameter p)
            {
                return new ScriptVariableDeclaration(p);
            }

            protected override IScriptArray Invoke(ScriptCodeActionContractExpression element, InterpreterState state)
            {
                return new ScriptArray(element.ParamList.ToArray(CreateParameter));
            }
        }
        #endregion

        public new const string Name = "signature";

        private ScriptActionContractExpressionFactory(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        private ScriptActionContractExpressionFactory()
            : base(Name)
        {
        }

        private IRuntimeSlot m_modify;
        private IRuntimeSlot m_retval;
        private IRuntimeSlot m_params;

        public static readonly ScriptActionContractExpressionFactory Instance = new ScriptActionContractExpressionFactory();

        public override ScriptActionContractExpression CreateCodeElement(IList<IScriptObject> args, InterpreterState state)
        {
            return args.Count == 2 ? CreateExpression(args[0] as IEnumerable<IScriptObject>, args[1] as IScriptCodeElement<ScriptCodeExpression>) : null;
        }

        public static ScriptActionContractExpression CreateExpression(IEnumerable<IScriptObject> parameters, IScriptCodeElement<ScriptCodeExpression> returnType)
        {
            return new ScriptActionContractExpression(ScriptActionContractExpression.CreateExpression(parameters, returnType));
        }

        public override void Clear()
        {
            m_modify = m_params = m_retval = null;
        }

        protected override IRuntimeSlot Modify
        {
            get { return CacheConst<ModifyAction>(ref m_modify); }
        }

        IRuntimeSlot IActionContractExpressionFactorySlots.Retval
        {
            get { return CacheConst<GetReturnTypeAction>(ref m_retval); }
        }

        IRuntimeSlot IActionContractExpressionFactorySlots.Params
        {
            get { return CacheConst<GetParametersAction>(ref m_params); }
        }
    }
}
