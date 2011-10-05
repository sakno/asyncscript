using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace DynamicScript.Runtime.Environment.ExpressionTrees
{
    using Compiler.Ast;
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;

    [Serializable]
    [ComVisible(false)]
    sealed class ScriptInvocationExpressionFactory : ScriptExpressionFactory<ScriptCodeInvocationExpression, ScriptInvocationExpression>, IInvocationExpressionFactorySlots
    {
        #region Nested Types
        [ComVisible(false)]
        private sealed class ModifyAction : ModifyActionBase
        {
            private const string FirstParamName = "args";

            public ModifyAction()
                : base(Instance, new ScriptActionContract.Parameter(FirstParamName, new ScriptArrayContract(ScriptExpressionFactory.Instance)))
            {
            }
        }

        [ComVisible(false)]
        private sealed class GetArgumentsAction : CodeElementPartProvider<IScriptArray>
        {
            public GetArgumentsAction()
                : base(Instance, new ScriptArrayContract(ScriptExpressionFactory.Instance))
            {
            }

            protected override IScriptArray Invoke(ScriptCodeInvocationExpression element, InterpreterState state)
            {
                return ScriptExpressionFactory.CreateExpressions(element.ArgList, state);
            }
        }
        #endregion

        public new const string Name = "inv";

        private IRuntimeSlot m_modify;
        private IRuntimeSlot m_args;

        private ScriptInvocationExpressionFactory(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        private ScriptInvocationExpressionFactory()
            : base(Name)
        {
        }

        public static readonly ScriptInvocationExpressionFactory Instance = new ScriptInvocationExpressionFactory();

        public static ScriptInvocationExpression CreateExpression(IEnumerable<IScriptObject> args)
        {
            return new ScriptInvocationExpression(ScriptInvocationExpression.CreateExpression(args));
        }

        public override ScriptInvocationExpression CreateCodeElement(IList<IScriptObject> args, InterpreterState state)
        {
            switch (args.Count)
            {
                case 0: return CreateExpression(new IScriptObject[0]);
                case 1: return CreateExpression(args[0] as IEnumerable<IScriptObject> ?? args);
                default: return CreateExpression(args);
            }
        }

        public override void Clear()
        {
            m_modify = m_args = null;
        }

        protected override IRuntimeSlot Modify
        {
            get { return CacheConst<ModifyAction>(ref m_modify); }
        }

        IRuntimeSlot IInvocationExpressionFactorySlots.Args
        {
            get { return CacheConst<GetArgumentsAction>(ref m_args); }
        }

    }
}
