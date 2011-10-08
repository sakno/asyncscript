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
            private const string SecondParamName = "condition";
            private const string ThirdParamName = "thenBranch";
            private const string FourthParamName = "elseBranch";

            public ModifyAction()
                : base(Instance, new ScriptActionContract.Parameter(SecondParamName, ScriptExpressionFactory.Instance), new ScriptActionContract.Parameter(ThirdParamName, ScriptExpressionFactory.Instance), new ScriptActionContract.Parameter(FourthParamName, ScriptExpressionFactory.Instance))
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
        private sealed class GetThenBranchAction : CodeElementPartProvider<IScriptCodeElement<ScriptCodeExpression>>
        {
            public GetThenBranchAction()
                : base(Instance, new ScriptArrayContract(ScriptStatementFactory.Instance))
            {
            }

            protected override IScriptCodeElement<ScriptCodeExpression> Invoke(ScriptCodeConditionalExpression element, InterpreterState state)
            {
                return Convert(element.ThenBranch) as IScriptCodeElement<ScriptCodeExpression>;
            }
        }

        [ComVisible(false)]
        private sealed class GetElseBranchAction : CodeElementPartProvider<IScriptCodeElement<ScriptCodeExpression>>
        {
            public GetElseBranchAction()
                : base(Instance, new ScriptArrayContract(ScriptStatementFactory.Instance))
            {
            }

            protected override IScriptCodeElement<ScriptCodeExpression> Invoke(ScriptCodeConditionalExpression element, InterpreterState state)
            {
                return Convert(element.ElseBranch) as IScriptCodeElement<ScriptCodeExpression>;
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

        public static ScriptConditionalExpression CreateExpression(IScriptObject condition, IScriptObject thenBranch, IScriptObject elseBranch = null)
        {
            return new ScriptConditionalExpression(ScriptConditionalExpression.CreateExpression(condition, thenBranch, elseBranch));
        }

        public override ScriptConditionalExpression CreateCodeElement(IList<IScriptObject> args, InterpreterState state)
        {
            switch (args.Count)
            {
                case 2: return CreateExpression(args[0], args[1]);
                case 3: return CreateExpression(args[0], args[1], args[2]);
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
