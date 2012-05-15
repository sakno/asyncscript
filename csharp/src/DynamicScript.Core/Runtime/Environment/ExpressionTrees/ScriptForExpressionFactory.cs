using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace DynamicScript.Runtime.Environment.ExpressionTrees
{
    using Compiler.Ast;
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;

    [ComVisible(false)]
    sealed class ScriptForExpressionFactory : ScriptLoopExpressionFactory<ScriptCodeForLoopExpression, ScriptForExpression>
    {
        #region Nested Types
        [ComVisible(false)]
        private sealed class ModifyFunction : ModifyFunctionBase
        {
            private const string SecondParamName = "loopVar";
            private const string ThirdParamName = "condition";
            private const string FourthParamName = "grouping";
            private const string FifthParamName = "body";

            public ModifyFunction()
                : base(Instance,
                new ScriptFunctionContract.Parameter(SecondParamName, ScriptLoopVariableStatementFactory.Instance),
                new ScriptFunctionContract.Parameter(ThirdParamName, ScriptExpressionFactory.Instance),
                new ScriptFunctionContract.Parameter(FourthParamName, ScriptSuperContract.Instance),
                new ScriptFunctionContract.Parameter(FifthParamName, ScriptExpressionFactory.Instance))
            {
            }
        }

        [ComVisible(false)]
        private sealed class UseTemporaryVarFunction : CodeElementPartProvider<ScriptBoolean>
        {
            public const string Name = "tempvar";

            public UseTemporaryVarFunction()
                : base(Instance, ScriptBooleanContract.Instance)
            {
            }

            protected override ScriptBoolean Invoke(ScriptCodeForLoopExpression element, InterpreterState state)
            {
                return element.Variable != null && element.Variable.Temporary;
            }
        }

        [ComVisible(false)]
        private sealed class GetLoopVariableFunction : CodeElementPartProvider<IScriptStatement<ScriptCodeLoopWithVariableExpression.LoopVariable>>
        {
            public const string Name = "loopvar";

            public GetLoopVariableFunction()
                : base(Instance, ScriptVariableDeclarationFactory.Instance)
            {
            }

            protected override IScriptStatement<ScriptCodeLoopWithVariableExpression.LoopVariable> Invoke(ScriptCodeForLoopExpression element, InterpreterState state)
            {
                return element.Variable != null ? new ScriptLoopVariableStatement(element.Variable) : null;
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
        private sealed class GetConditionFunction : CodeElementPartProvider<IScriptExpression<ScriptCodeExpression>>
        {
            public const string Name = "condition";

            public GetConditionFunction()
                : base(Instance, ScriptExpressionFactory.Instance)
            {
            }

            protected override IScriptExpression<ScriptCodeExpression> Invoke(ScriptCodeForLoopExpression element, InterpreterState state)
            {
                return Convert(element.Condition) as IScriptExpression<ScriptCodeExpression>;
            }
        }
        #endregion

        private static readonly AggregatedSlotCollection<ScriptForExpressionFactory> StaticSlots = new AggregatedSlotCollection<ScriptForExpressionFactory>
        {
            {ModifyFunction.Name, (owner, state) =>LazyField<ModifyFunction, IScriptFunction>(ref owner.m_modify)},
            {GetLoopVariableFunction.Name, (owner, state) => LazyField<GetLoopVariableFunction, IScriptFunction>(ref owner.m_loopvar)},
            {GetBodyFunction.Name, (owner, state) => LazyField<GetBodyFunction, IScriptFunction>(ref owner.m_getbody)},
            {GetGroupingFunction.Name, (owner, state) => LazyField<GetGroupingFunction, IScriptFunction>(ref owner.m_grouping)},
            {GetConditionFunction.Name, (owner, state) => LazyField<GetConditionFunction, IScriptFunction>(ref owner.m_condition)}
        };

        public new const string Name = "`for";

        private IScriptFunction m_modify;
        private IScriptFunction m_grouping;
        private IScriptFunction m_getbody;
        private IScriptFunction m_loopvar;
        private IScriptFunction m_condition;

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
            m_condition =
                m_getbody =
                m_grouping =
                m_loopvar =
                m_modify = null;
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
