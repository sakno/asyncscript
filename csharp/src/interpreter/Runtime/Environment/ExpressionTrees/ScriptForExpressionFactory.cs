using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace DynamicScript.Runtime.Environment.ExpressionTrees
{
    using Compiler.Ast;
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;

    [ComVisible(false)]
    sealed class ScriptForExpressionFactory : ScriptLoopExpressionFactory<ScriptCodeForLoopExpression, ScriptForExpression>, IForExpressionFactorySlots
    {
        #region Nested Types
        [ComVisible(false)]
        private sealed class ModifyAction : ModifyActionBase
        {
            private const string SecondParamName = "loopVar";
            private const string ThirdParamName = "condition";
            private const string FourthParamName = "grouping";
            private const string FifthParamName = "body";

            public ModifyAction()
                : base(Instance,
                new ScriptActionContract.Parameter(SecondParamName, ScriptLoopVariableStatementFactory.Instance),
                new ScriptActionContract.Parameter(ThirdParamName, ScriptExpressionFactory.Instance),
                new ScriptActionContract.Parameter(FourthParamName, ScriptSuperContract.Instance),
                new ScriptActionContract.Parameter(FifthParamName, ScriptExpressionFactory.Instance))
            {
            }
        }

        [ComVisible(false)]
        private sealed class UseTemporaryVarAction : CodeElementPartProvider<ScriptBoolean>
        {
            public UseTemporaryVarAction()
                : base(Instance, ScriptBooleanContract.Instance)
            {
            }

            protected override ScriptBoolean Invoke(ScriptCodeForLoopExpression element, InterpreterState state)
            {
                return element.Variable != null && element.Variable.Temporary;
            }
        }

        [ComVisible(false)]
        private sealed class GetLoopVariableAction : CodeElementPartProvider<IScriptStatement<ScriptCodeLoopWithVariableExpression.LoopVariable>>
        {
            public GetLoopVariableAction()
                : base(Instance, ScriptVariableDeclarationFactory.Instance)
            {
            }

            protected override IScriptStatement<ScriptCodeLoopWithVariableExpression.LoopVariable> Invoke(ScriptCodeForLoopExpression element, InterpreterState state)
            {
                return element.Variable != null ? new ScriptLoopVariableStatement(element.Variable) : null;
            }
        }

        [ComVisible(false)]
        private sealed class GetGroupingAction : GetGroupingActionBase
        {
            public GetGroupingAction()
                : base(Instance)
            {
            }
        }

        [ComVisible(false)]
        private sealed class GetBodyAction : GetBodyActionBase
        {
            public GetBodyAction()
                : base(Instance)
            {
            }
        }

        [ComVisible(false)]
        private sealed class GetConditionAction : CodeElementPartProvider<IScriptExpression<ScriptCodeExpression>>
        {
            public GetConditionAction()
                : base(Instance, ScriptExpressionFactory.Instance)
            {
            }

            protected override IScriptExpression<ScriptCodeExpression> Invoke(ScriptCodeForLoopExpression element, InterpreterState state)
            {
                return Convert(element.Condition) as IScriptExpression<ScriptCodeExpression>;
            }
        }
        #endregion

        public new const string Name = "forloop";

        private IRuntimeSlot m_modify;
        private IRuntimeSlot m_getgrouping;
        private IRuntimeSlot m_getbody;
        private IRuntimeSlot m_loopvar;
        private IRuntimeSlot m_getcond;

        private ScriptForExpressionFactory(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        private ScriptForExpressionFactory()
            : base(Name)
        {
        }

        public static readonly ScriptForExpressionFactory Instance = new ScriptForExpressionFactory();

        public static ScriptForExpression CreateExpression(IScriptCodeElement<ScriptCodeLoopWithVariableExpression.LoopVariable> declaration, IScriptCodeElement<ScriptCodeExpression> condition, IScriptObject grouping, IScriptObject body)
        {
            var expression = ScriptForExpression.CreateExpression(declaration, condition, grouping, body);
            return expression != null ? new ScriptForExpression(expression) : null;
        }

        public override ScriptForExpression CreateCodeElement(IList<IScriptObject> args, InterpreterState state)
        {
            return args.Count == 4 ? CreateExpression(args[0] as IScriptCodeElement<ScriptCodeLoopWithVariableExpression.LoopVariable>, args[1] as IScriptCodeElement<ScriptCodeExpression>, args[2], args[3]) : null;
        }

        public override void Clear()
        {
            m_getcond =
                m_getbody =
                m_getgrouping =
                m_loopvar =
                m_modify = null;
        }

        protected override IRuntimeSlot Modify
        {
            get { return CacheConst<ModifyAction>(ref m_modify); }
        }

        protected override IRuntimeSlot Grouping
        {
            get { return CacheConst<GetGroupingAction>(ref m_getgrouping); }
        }

        protected override IRuntimeSlot Body
        {
            get { return CacheConst<GetBodyAction>(ref m_getbody); }
        }

        IRuntimeSlot IForExpressionFactorySlots.LoopVar
        {
            get { return CacheConst<GetLoopVariableAction>(ref m_loopvar); }
        }


        IRuntimeSlot IForExpressionFactorySlots.Condition
        {
            get { return CacheConst<GetConditionAction>(ref m_getcond); }
        }
    }
}
