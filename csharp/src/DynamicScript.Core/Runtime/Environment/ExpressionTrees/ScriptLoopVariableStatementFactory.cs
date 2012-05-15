using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace DynamicScript.Runtime.Environment.ExpressionTrees
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;
    using Compiler.Ast;

    [ComVisible(false)]
    [Serializable]
    sealed class ScriptLoopVariableStatementFactory : ScriptStatementFactory<ScriptCodeLoopWithVariableExpression.LoopVariable, ScriptLoopVariableStatement>
    {
        #region Nested Types
        [ComVisible(false)]
        private sealed class ModifyFunction : ModifyFunctionBase
        {
            private const string SecondParamName = "name";
            private const string ThirdParamName = "temp";
            private const string FourthParamName = "init";

            public ModifyFunction()
                : base(Instance, new ScriptFunctionContract.Parameter(SecondParamName, ScriptStringContract.Instance),
                new ScriptFunctionContract.Parameter(ThirdParamName, ScriptBooleanContract.Instance),
                new ScriptFunctionContract.Parameter(FourthParamName, ScriptExpressionFactory.Instance))
            {
            }
        }
        #endregion

        public new const string Name = "loopvar";

        private IScriptFunction m_modify;

        private static readonly AggregatedSlotCollection<ScriptLoopVariableStatementFactory> StaticSlots = new AggregatedSlotCollection<ScriptLoopVariableStatementFactory>
        {
             {ModifyFunction.Name, (owner, state) => LazyField<ModifyFunction, IScriptFunction>(ref owner.m_modify)},
        };

        private ScriptLoopVariableStatementFactory(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        private ScriptLoopVariableStatementFactory()
            : base(Name)
        {
        }

        public static readonly ScriptLoopVariableStatementFactory Instance = new ScriptLoopVariableStatementFactory();

        public static ScriptLoopVariableStatement CreateStatement(ScriptString name, ScriptBoolean temporary, IScriptCodeElement<ScriptCodeExpression> initExpr)
        {
            var loopvar = ScriptLoopVariableStatement.CreateStatement(name, temporary, initExpr);
            return loopvar != null ? new ScriptLoopVariableStatement(loopvar) : null;
        }

        public override ScriptLoopVariableStatement CreateCodeElement(IList<IScriptObject> args, InterpreterState state)
        {
            switch (args.Count)
            {
                case 1:
                    return CreateStatement(args[0] as ScriptString, ScriptBoolean.False, null);
                case 2:
                    return CreateStatement(args[0] as ScriptString, args[1] as ScriptBoolean, null);
                case 3:
                    return CreateStatement(args[0] as ScriptString, args[1] as ScriptBoolean, args[2] as IScriptCodeElement<ScriptCodeExpression>);
                default:
                    return null;
            }
        }

        public override void Clear()
        {
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
