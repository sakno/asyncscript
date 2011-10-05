using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace DynamicScript.Runtime.Environment.ExpressionTrees
{
    using Compiler.Ast;
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;

    [ComVisible(false)]
    [Serializable]
    sealed class ScriptForkExpressionFactory : ScriptExpressionFactory<ScriptCodeForkExpression, ScriptForkExpression>, IForkExpressionFactorySlots
    {
        #region Nested Types
        [ComVisible(false)]
        private sealed class ModifyAction : ModifyActionBase
        {
            private const string FirstParamName = "stmts";

            public ModifyAction()
                : base(Instance, new ScriptActionContract.Parameter(FirstParamName, new ScriptArrayContract(ScriptStatementFactory.Instance)))
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

            protected override IScriptArray Invoke(ScriptCodeForkExpression element, InterpreterState state)
            {
                return ScriptStatementFactory.CreateStatements(element.Body, state);
            }
        }
        #endregion

        public new const string Name = "forkdef";

        private IRuntimeSlot m_modify;
        private IRuntimeSlot m_getstmts;

        private ScriptForkExpressionFactory(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        private ScriptForkExpressionFactory()
            : base(Name)
        {
        }

        public static readonly ScriptForkExpressionFactory Instance = new ScriptForkExpressionFactory();

        public static ScriptForkExpression CreateExpression(IEnumerable<IScriptObject> statements)
        {
            return new ScriptForkExpression(ScriptForkExpression.CreateExpression(statements));
        }

        public override ScriptForkExpression CreateCodeElement(IList<IScriptObject> args, InterpreterState state)
        {
            return args.Count == 1 ? CreateExpression(args[0] as IEnumerable<IScriptObject>) : null;
        }

        protected override IRuntimeSlot Modify
        {
            get { return CacheConst<ModifyAction>(ref m_modify); }
        }

        public override void Clear()
        {
            m_getstmts = m_modify = null;
        }

        IRuntimeSlot IForkExpressionFactorySlots.GetStmts
        {
            get { return CacheConst<GetStatementsAction>(ref m_getstmts); }
        }
    }
}
