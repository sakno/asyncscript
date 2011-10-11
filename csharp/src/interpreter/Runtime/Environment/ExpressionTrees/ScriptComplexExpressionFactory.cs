using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace DynamicScript.Runtime.Environment.ExpressionTrees
{
    using Compiler.Ast;
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;

    [ComVisible(false)]
    [Serializable]
    sealed class ScriptComplexExpressionFactory : ScriptExpressionFactory<ScriptCodeComplexExpression, ScriptComplexExpression>, IScriptComplexExpressionFactorySlots
    {
        #region Nested Types
        [ComVisible(false)]
        private sealed class ModifyAction : ModifyActionBase
        {
            private const string SecondParamName = "statements";

            public ModifyAction()
                : base(Instance, new ScriptActionContract.Parameter(SecondParamName, new ScriptArrayContract(ScriptStatementFactory.Instance)))
            {
            }
        }

        [ComVisible(false)]
        private sealed class GetStatementsAction : CodeElementPartProvider<IScriptArray>
        {
            public GetStatementsAction()
                : base(Instance, new ScriptArrayContract(ScriptStatementFactory.Instance))
            {
            }

            protected override IScriptArray Invoke(ScriptCodeComplexExpression element, InterpreterState state)
            {
                return ScriptStatementFactory.CreateStatements(element.Body, state);
            }
        }
        #endregion

        public new const string Name = "cplx";

        private IRuntimeSlot m_modify;
        private IRuntimeSlot m_statements;

        private ScriptComplexExpressionFactory(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        private ScriptComplexExpressionFactory()
            : base(Name)
        {
        }

        public static readonly ScriptComplexExpressionFactory Instance = new ScriptComplexExpressionFactory();

        public static ScriptComplexExpression CreateExpression(IEnumerable<IScriptObject> statements)
        {
            return new ScriptComplexExpression(ScriptComplexExpression.CreateExpression(statements));
        }

        public override ScriptComplexExpression CreateCodeElement(IList<IScriptObject> args, InterpreterState state)
        {
            switch (args.Count)
            {
                case 0: return null;
                case 1: return CreateExpression(args[0] as IEnumerable<IScriptObject> ?? args);
                default: return CreateExpression(args);
            }
        }

        public override void Clear()
        {
            m_statements = m_modify = null;
        }

        protected override IRuntimeSlot Modify
        {
            get { return CacheConst<ModifyAction>(ref m_modify); }
        }

        IRuntimeSlot IScriptComplexExpressionFactorySlots.Statements
        {
            get { return CacheConst<GetStatementsAction>(ref m_statements); }
        }
    }
}
