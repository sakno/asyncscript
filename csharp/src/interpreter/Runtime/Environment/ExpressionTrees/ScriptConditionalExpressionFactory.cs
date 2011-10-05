using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace DynamicScript.Runtime.Environment.ExpressionTrees
{
    using Compiler.Ast;
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;

    [ComVisible(false)]
    [Serializable]
    sealed class ScriptConditionalExpressionFactory : ScriptExpressionFactory<ScriptCodeConditionalExpression, ScriptConditionalExpression>, IConditionalExpressionFactorySlots
    {
        #region Nested Types
        [ComVisible(false)]
        private sealed class ModifyAction : ModifyActionBase
        {
            private const string FirstParamName = "condition";
            private const string SecondParamName = "thenBranch";
            private const string ThirdParamName = "elseBranch";

            private ModifyAction(ScriptArrayContract branchContract)
                : base(Instance, new ScriptActionContract.Parameter(FirstParamName, ScriptExpressionFactory.Instance), new ScriptActionContract.Parameter(SecondParamName, branchContract), new ScriptActionContract.Parameter(ThirdParamName, branchContract))
            {
            }

            public ModifyAction()
                : this(new ScriptArrayContract(ScriptStatementFactory.Instance))
            {
            }
        }

        [ComVisible(false)]
        private sealed class GetConditionAction : CodeElementPartProvider<IScriptExpression<ScriptCodeConditionalExpression>>
        {
            public GetConditionAction()
                : base(Instance, ScriptExpressionFactory.Instance)
            {
            }

            protected override IScriptExpression<ScriptCodeConditionalExpression> Invoke(ScriptCodeConditionalExpression element, InterpreterState state)
            {
                return Convert(element.Condition) as IScriptExpression<ScriptCodeConditionalExpression>;
            }
        }

        [ComVisible(false)]
        private sealed class GetThenBranchAction : CodeElementPartProvider<IScriptArray>
        {
            public GetThenBranchAction()
                : base(Instance, new ScriptArrayContract(ScriptStatementFactory.Instance))
            {
            }

            protected override IScriptArray Invoke(ScriptCodeConditionalExpression element, InterpreterState state)
            {
                return ScriptStatementFactory.CreateStatements(element.ThenBranch, state);
            }
        }

        [ComVisible(false)]
        private sealed class GetElseBranchAction : CodeElementPartProvider<IScriptArray>
        {
            public GetElseBranchAction()
                : base(Instance, new ScriptArrayContract(ScriptStatementFactory.Instance))
            {
            }

            protected override IScriptArray Invoke(ScriptCodeConditionalExpression element, InterpreterState state)
            {
                return ScriptStatementFactory.CreateStatements(element.ElseBranch, state);
            }
        }
        #endregion
        public new const string Name = "cond";
        private IRuntimeSlot m_modify;
        private IRuntimeSlot m_condition;
        private IRuntimeSlot m_then;
        private IRuntimeSlot m_else;

        private ScriptConditionalExpressionFactory(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        private ScriptConditionalExpressionFactory()
            : base(Name)
        {
        }

        public static readonly ScriptConditionalExpressionFactory Instance = new ScriptConditionalExpressionFactory();

        public static ScriptConditionalExpression CreateExpression(IScriptObject condition, IEnumerable<IScriptObject> thenBranch, IEnumerable<IScriptObject> elseBranch = null)
        {
            var expr = ScriptConditionalExpression.CreateExpression(condition, thenBranch, elseBranch);
            return expr != null ? new ScriptConditionalExpression(expr) : null;
        }

        public override ScriptConditionalExpression CreateCodeElement(IList<IScriptObject> args, InterpreterState state)
        {
            switch (args.Count)
            {
                case 2: return CreateExpression(args[0], args[1] as IEnumerable<IScriptObject>);
                case 3: return CreateExpression(args[0], args[1] as IEnumerable<IScriptObject>, args[2] as IEnumerable<IScriptObject>);
                default: return null;
            }
        }

        public override void Clear()
        {
            m_condition = m_else = m_modify = m_then = null;
        }

        protected override IRuntimeSlot Modify
        {
            get { return CacheConst<ModifyAction>(ref m_modify); }
        }

        IRuntimeSlot IConditionalExpressionFactorySlots.IfTrue
        {
            get { return CacheConst<GetThenBranchAction>(ref m_then); }
        }

        IRuntimeSlot IConditionalExpressionFactorySlots.IfFalse
        {
            get { return CacheConst<GetElseBranchAction>(ref m_else); }
        }

        IRuntimeSlot IConditionalExpressionFactorySlots.Cond
        {
            get { return CacheConst<GetConditionAction>(ref m_condition); }
        }
    }
}
