using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace DynamicScript.Runtime.Environment.ExpressionTrees
{
    using Compiler.Ast;
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;

    [ComVisible(false)]
    [Serializable]
    sealed class ScriptConditionalExpressionFactory : ScriptExpressionFactory<ScriptCodeConditionalExpression, ScriptConditionalExpression>
    {
        #region Nested Types
        [ComVisible(false)]
        private sealed class ModifyFunction : ModifyFunctionBase
        {
            private const string SecondParamName = "condition";
            private const string ThirdParamName = "thenBranch";
            private const string FourthParamName = "elseBranch";

            public ModifyFunction()
                : base(Instance, new ScriptFunctionContract.Parameter(SecondParamName, ScriptExpressionFactory.Instance), new ScriptFunctionContract.Parameter(ThirdParamName, ScriptExpressionFactory.Instance), new ScriptFunctionContract.Parameter(FourthParamName, ScriptExpressionFactory.Instance))
            {
            }
        }

        [ComVisible(false)]
        private sealed class GetConditionFunction : CodeElementPartProvider<IScriptExpression<ScriptCodeConditionalExpression>>
        {
            public const string Name = "condition";

            public GetConditionFunction()
                : base(Instance, ScriptExpressionFactory.Instance)
            {
            }

            protected override IScriptExpression<ScriptCodeConditionalExpression> Invoke(ScriptCodeConditionalExpression element, InterpreterState state)
            {
                return Convert(element.Condition) as IScriptExpression<ScriptCodeConditionalExpression>;
            }
        }

        [ComVisible(false)]
        private sealed class GetThenBranchFunction : CodeElementPartProvider<IScriptCodeElement<ScriptCodeExpression>>
        {
            public const string Name = "`then";

            public GetThenBranchFunction()
                : base(Instance, new ScriptArrayContract(ScriptStatementFactory.Instance))
            {
            }

            protected override IScriptCodeElement<ScriptCodeExpression> Invoke(ScriptCodeConditionalExpression element, InterpreterState state)
            {
                return Convert(element.ThenBranch) as IScriptCodeElement<ScriptCodeExpression>;
            }
        }

        [ComVisible(false)]
        private sealed class GetElseBranchFunction : CodeElementPartProvider<IScriptCodeElement<ScriptCodeExpression>>
        {
            public const string Name = "`else";

            public GetElseBranchFunction()
                : base(Instance, new ScriptArrayContract(ScriptStatementFactory.Instance))
            {
            }

            protected override IScriptCodeElement<ScriptCodeExpression> Invoke(ScriptCodeConditionalExpression element, InterpreterState state)
            {
                return Convert(element.ElseBranch) as IScriptCodeElement<ScriptCodeExpression>;
            }
        }
        #endregion
        public new const string Name = "conditional";

        private static readonly AggregatedSlotCollection<ScriptConditionalExpressionFactory> StaticSlots = new AggregatedSlotCollection<ScriptConditionalExpressionFactory>
        {
           {ModifyFunction.Name, (owner, state) => LazyField<ModifyFunction, IScriptFunction>(ref owner.m_modify)},
           {GetConditionFunction.Name, (owner, state) => LazyField<GetConditionFunction, IScriptFunction>(ref owner.m_condition)},
           {GetThenBranchFunction.Name, (owner, state) => LazyField<GetThenBranchFunction, IScriptFunction>(ref owner.m_then)},
           {GetElseBranchFunction.Name, (owner, state) => LazyField<GetElseBranchFunction, IScriptFunction>(ref owner.m_else)}
        };

        private IScriptFunction m_modify;
        private IScriptFunction m_condition;
        private IScriptFunction m_then;
        private IScriptFunction m_else;

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

        public override ICollection<string> Slots
        {
            get { return StaticSlots.Keys; }
        }

        protected override IScriptObject GetSlotMetadata(string slotName, InterpreterState state)
        {
            return StaticSlots.GetSlotMetadata(this, slotName, state);
        }

        public override IScriptObject this[string slotName, InterpreterState state]
        {
            get { return StaticSlots.GetValue(this, slotName, state); }
            set { StaticSlots.SetValue(this, slotName, value, state); }
        }
    }
}
