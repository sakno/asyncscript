using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace DynamicScript.Runtime.Environment.ExpressionTrees
{
    using Compiler.Ast;
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;

    [ComVisible(false)]
    [Serializable]
    sealed class ScriptWhileExpressionFactory : ScriptLoopExpressionFactory<ScriptCodeWhileLoopExpression, ScriptWhileExpression>, IWhileExpressionFactorySlots
    {
        #region Nested Types
        [ComVisible(false)]
        private sealed class ModifyAction : ModifyActionBase
        {
            private const string SecondParamName = "postEval";
            private const string ThirdParamName = "condition";
            private const string FourthParamName = "grouping";
            private const string FifthParamName = "body";

            public ModifyAction()
                : base(Instance,
                new ScriptActionContract.Parameter(SecondParamName, ScriptBooleanContract.Instance),
                new ScriptActionContract.Parameter(ThirdParamName, ScriptExpressionFactory.Instance),
                new ScriptActionContract.Parameter(FourthParamName, ScriptSuperContract.Instance),
                new ScriptActionContract.Parameter(FifthParamName, new ScriptArrayContract(ScriptStatementFactory.Instance)))
            {
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
        private sealed class HasPostEvaluationAction : CodeElementPartProvider<ScriptBoolean>
        {
            public HasPostEvaluationAction()
                : base(Instance, ScriptBooleanContract.Instance)
            {
            }

            protected override ScriptBoolean Invoke(ScriptCodeWhileLoopExpression element, InterpreterState state)
            {
                return element.Style == ScriptCodeWhileLoopExpression.LoopStyle.EvaluateConditionAfterBody;
            }
        }

        [ComVisible(false)]
        private sealed class GetConditionAction : CodeElementPartProvider<IScriptExpression<ScriptCodeExpression>>
        {
            public GetConditionAction()
                : base(Instance, ScriptExpressionFactory.Instance)
            {
            }

            protected override IScriptExpression<ScriptCodeExpression> Invoke(ScriptCodeWhileLoopExpression element, InterpreterState state)
            {
                return Convert(element.Condition) as IScriptExpression<ScriptCodeExpression>;
            }
        }
        #endregion

        public new const string Name = "whileloop";

        private IRuntimeSlot m_modify;
        private IRuntimeSlot m_getbody;
        private IRuntimeSlot m_grouping;
        private IRuntimeSlot m_posteval;
        private IRuntimeSlot m_condition;

        private ScriptWhileExpressionFactory(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        private ScriptWhileExpressionFactory()
            : base(Name)
        {
        }

        public static readonly ScriptWhileExpressionFactory Instance = new ScriptWhileExpressionFactory();

        public static ScriptWhileExpression CreateExpression(ScriptBoolean postEval, IScriptCodeElement<ScriptCodeExpression> condition, IScriptObject grouping, IEnumerable<IScriptObject> body)
        {
            var expression = ScriptWhileExpression.CreateExpression(postEval, condition, grouping, body);
            return expression != null ? new ScriptWhileExpression(expression) : null;
        }

        public override ScriptWhileExpression CreateCodeElement(IList<IScriptObject> args, InterpreterState state)
        {
            return args.Count == 4 ? CreateExpression(args[0] as ScriptBoolean, args[1] as IScriptCodeElement<ScriptCodeExpression>, args[2], args[3] as IEnumerable<IScriptObject>) : null;
        }

        protected override IRuntimeSlot Modify
        {
            get { return CacheConst<ModifyAction>(ref m_modify); }
        }

        protected override IRuntimeSlot Grouping
        {
            get { return CacheConst<GetGroupingAction>(ref m_grouping); }
        }

        protected override IRuntimeSlot Body
        {
            get { return CacheConst<GetBodyAction>(ref m_getbody); }
        }

        IRuntimeSlot IWhileExpressionFactorySlots.Condition
        {
            get { return CacheConst<GetConditionAction>(ref m_condition); }
        }

        IRuntimeSlot IWhileExpressionFactorySlots.PostEval
        {
            get { return CacheConst<HasPostEvaluationAction>(ref m_posteval); }
        }

        public override void Clear()
        {
            m_condition =
                m_getbody =
                m_grouping =
                m_modify =
                m_posteval = null;
        }
    }
}
