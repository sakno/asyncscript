using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace DynamicScript.Runtime.Environment.ExpressionTrees
{
    using Compiler.Ast;
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;

    [ComVisible(false)]
    [Serializable]
    sealed class ScriptWhileExpressionFactory : ScriptLoopExpressionFactory<ScriptCodeWhileLoopExpression, ScriptWhileExpression>
    {
        #region Nested Types
        [ComVisible(false)]
        private sealed class ModifyFunction : ModifyFunctionBase
        {
            private const string SecondParamName = "postEval";
            private const string ThirdParamName = "condition";
            private const string FourthParamName = "grouping";
            private const string FifthParamName = "body";

            public ModifyFunction()
                : base(Instance,
                new ScriptFunctionContract.Parameter(SecondParamName, ScriptBooleanContract.Instance),
                new ScriptFunctionContract.Parameter(ThirdParamName, ScriptExpressionFactory.Instance),
                new ScriptFunctionContract.Parameter(FourthParamName, ScriptSuperContract.Instance),
                new ScriptFunctionContract.Parameter(FifthParamName, ScriptExpressionFactory.Instance))
            {
            }
        }

        [ComVisible(false)]
        private sealed class GetGroupingFunction : GetGroupingFunctionBase
        {
            public GetGroupingFunction()
                : base(Instance)
            {
            }
        }

        [ComVisible(false)]
        private sealed class GetBodyFunction : GetBodyFunctionBase
        {
            public GetBodyFunction()
                : base(Instance)
            {
            }
        }

        [ComVisible(false)]
        private sealed class HasPostEvaluationAction : CodeElementPartProvider<ScriptBoolean>
        {
            public const string Name = "post_condition";

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
        private sealed class GetConditionFunction : CodeElementPartProvider<IScriptExpression<ScriptCodeExpression>>
        {
            public const string Name = "condition";

            public GetConditionFunction()
                : base(Instance, ScriptExpressionFactory.Instance)
            {
            }

            protected override IScriptExpression<ScriptCodeExpression> Invoke(ScriptCodeWhileLoopExpression element, InterpreterState state)
            {
                return Convert(element.Condition) as IScriptExpression<ScriptCodeExpression>;
            }
        }
        #endregion

        private static readonly AggregatedSlotCollection<ScriptWhileExpressionFactory> StaticSlots = new AggregatedSlotCollection<ScriptWhileExpressionFactory>
        {
             {ModifyFunction.Name, (owner, state) => LazyField<ModifyFunction, IScriptFunction>(ref owner.m_modify)},
             {GetBodyFunction.Name, (owner, state) => LazyField<GetBodyFunction, IScriptFunction>(ref owner.m_getbody)},
             {GetGroupingFunction.Name, (owner, state) => LazyField<GetGroupingFunction, IScriptFunction>(ref owner.m_grouping)},
             {HasPostEvaluationAction.Name, (owner, state) => LazyField<HasPostEvaluationAction, IScriptFunction>(ref owner.m_posteval)},
             {GetConditionFunction.Name, (owner, state) => LazyField<GetConditionFunction, IScriptFunction>(ref owner.m_condition)}
        };

        public new const string Name = "`while";

        private IScriptFunction m_modify;
        private IScriptFunction m_getbody;
        private IScriptFunction m_grouping;
        private IScriptFunction m_posteval;
        private IScriptFunction m_condition;

        private ScriptWhileExpressionFactory(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        private ScriptWhileExpressionFactory()
            : base(Name)
        {
        }

        public static readonly ScriptWhileExpressionFactory Instance = new ScriptWhileExpressionFactory();

        public static ScriptWhileExpression CreateExpression(ScriptBoolean postEval, IScriptCodeElement<ScriptCodeExpression> condition, IScriptObject grouping, IScriptObject body)
        {
            var expression = ScriptWhileExpression.CreateExpression(postEval, condition, grouping, body);
            return expression != null ? new ScriptWhileExpression(expression) : null;
        }

        public override ScriptWhileExpression CreateCodeElement(IList<IScriptObject> args, InterpreterState state)
        {
            return args.Count == 4 ? CreateExpression(args[0] as ScriptBoolean, args[1] as IScriptCodeElement<ScriptCodeExpression>, args[2], args[3]) : null;
        }

        public override void Clear()
        {
            m_condition =
                m_getbody =
                m_grouping =
                m_modify =
                m_posteval = null;
        }

        public override ICollection<string> Slots
        {
            get { return StaticSlots.Keys; }
        }

        public override IScriptObject this[string slotName, InterpreterState state]
        {
            get { return StaticSlots.GetValue(this, slotName, state); }
            set { StaticSlots.SetValue(this, slotName, value, state); }
        }

        protected override IScriptObject GetSlotMetadata(string slotName, InterpreterState state)
        {
            return StaticSlots.GetSlotMetadata(this, slotName, state);
        }
    }
}
