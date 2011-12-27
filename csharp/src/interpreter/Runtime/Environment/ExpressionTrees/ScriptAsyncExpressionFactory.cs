using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace DynamicScript.Runtime.Environment.ExpressionTrees
{
    using Compiler.Ast;
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;

    [ComVisible(false)]
    [Serializable]
    sealed class ScriptAsyncExpressionFactory: ScriptExpressionFactory<ScriptCodeAsyncExpression, ScriptAsyncExpression>
    {
        #region Nested Types
        [ComVisible(false)]
        private sealed class ModifyFunction : ModifyFunctionBase
        {
            private const string SecondParamName = "contract";

            public ModifyFunction()
                : base( Instance, new ScriptFunctionContract.Parameter(SecondParamName, ScriptExpressionFactory.Instance))
            {
            }
        }

        [ComVisible(false)]
        private sealed class GetContractFunction : CodeElementPartProvider<IScriptExpression<ScriptCodeExpression>>
        {
            public const string Name = "contract";

            public GetContractFunction()
                : base(Instance, ScriptExpressionFactory.Instance)
            {
            }

            protected override IScriptExpression<ScriptCodeExpression> Invoke(ScriptCodeAsyncExpression element, InterpreterState state)
            {
                return Convert(element.Contract) as IScriptExpression<ScriptCodeExpression>;
            }
        }
        #endregion

        private static readonly AggregatedSlotCollection<ScriptAsyncExpressionFactory> StaticSlots = new AggregatedSlotCollection<ScriptAsyncExpressionFactory>
        {
            {ModifyFunction.Name, (owner, state) => LazyField<ModifyFunction, IScriptFunction>(ref owner.m_modify)},
            {GetContractFunction.Name, (owner, state) => LazyField<GetContractFunction, IScriptFunction>(ref owner.m_contract)}
        };

        public new const string Name = "`async";

        private IScriptFunction m_modify;
        private IScriptFunction m_contract;

        private ScriptAsyncExpressionFactory(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        private ScriptAsyncExpressionFactory()
            : base(Name)
        {
        }

        public static readonly ScriptAsyncExpressionFactory Instance = new ScriptAsyncExpressionFactory();

        public static ScriptAsyncExpression CreateExpression(IScriptObject contractDef)
        {
            var value = ScriptAsyncExpression.CreateExpression(contractDef);
            return value != null ? new ScriptAsyncExpression(value) : null;
        }

        public override ScriptAsyncExpression CreateCodeElement(IList<IScriptObject> args, InterpreterState state)
        {
            return args.Count == 1 ? CreateExpression(args[0]) : null;
        }

        public override void Clear()
        {
            m_contract = m_modify = null;
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

        public override ICollection<string> Slots
        {
            get { return StaticSlots.Keys; }
        }
    }
}
