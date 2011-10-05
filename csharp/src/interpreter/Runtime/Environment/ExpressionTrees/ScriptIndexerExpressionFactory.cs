using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace DynamicScript.Runtime.Environment.ExpressionTrees
{
    using Compiler.Ast;
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;

    [ComVisible(false)]
    [Serializable]
    sealed class ScriptIndexerExpressionFactory : ScriptExpressionFactory<ScriptCodeIndexerExpression, ScriptIndexerExpression>, IInvocationExpressionFactorySlots
    {
        #region Nested Types
        [ComVisible(false)]
        private sealed class ModifyAction : ModifyActionBase
        {
            private const string SecondParamName = "args";

            public ModifyAction()
                : base(Instance, new ScriptActionContract.Parameter(SecondParamName, new ScriptArrayContract(ScriptExpressionFactory.Instance)))
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

            protected override IScriptArray Invoke(ScriptCodeIndexerExpression element, InterpreterState state)
            {
                return ScriptExpressionFactory.CreateExpressions(element.ArgList, state);
            }
        }
        #endregion

        public new const string Name = "indexer";
        private IRuntimeSlot m_modify;
        private IRuntimeSlot m_args;

        private ScriptIndexerExpressionFactory(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        private ScriptIndexerExpressionFactory()
            : base(Name)
        {
        }

        public static readonly ScriptIndexerExpressionFactory Instance = new ScriptIndexerExpressionFactory();

        public static ScriptIndexerExpression CreateExpression(IEnumerable<IScriptObject> args)
        {
            return new ScriptIndexerExpression(ScriptIndexerExpression.CreateExpression(args));
        }

        public override void Clear()
        {
            m_args = m_modify = null;
        }

        public override ScriptIndexerExpression CreateCodeElement(IList<IScriptObject> args, InterpreterState state)
        {
            switch (args.Count)
            {
                case 0: return CreateExpression(new IScriptObject[0]);
                case 1: return CreateExpression(args[0] as IEnumerable<IScriptObject> ?? args);
                default: return CreateExpression(args);
            }
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
