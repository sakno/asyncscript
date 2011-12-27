using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace DynamicScript.Runtime.Environment.ExpressionTrees
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;
    using ScriptCodePlaceholderExpression = Compiler.Ast.ScriptCodePlaceholderExpression;

    [ComVisible(false)]
    [Serializable]
    sealed class ScriptPlaceholderExpressionFactory: ScriptExpressionFactory<ScriptCodePlaceholderExpression, ScriptPlaceholderExpression>
    {
        #region Nested Types
        [ComVisible(false)]
        private sealed class ModifyFunction : ModifyFunctionBase
        {
            private const string SecondParamName = "id";

            public ModifyFunction()
                : base(Instance, new ScriptFunctionContract.Parameter(SecondParamName, ScriptIntegerContract.Instance))
            {
            }
        }
        #endregion

        private static readonly AggregatedSlotCollection<ScriptPlaceholderExpressionFactory> StaticSlots = new AggregatedSlotCollection<ScriptPlaceholderExpressionFactory>
        {
             {ModifyFunction.Name, (owner, state) => LazyField<ModifyFunction, IScriptFunction>(ref owner.m_modify)},
        };

        public new const string Name = "placeholder";

        private IScriptFunction m_modify;

        private ScriptPlaceholderExpressionFactory(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        private ScriptPlaceholderExpressionFactory()
            : base(Name)
        {
        }

        public static readonly ScriptPlaceholderExpressionFactory Instance = new ScriptPlaceholderExpressionFactory();

        public static ScriptPlaceholderExpression CreateExpression(ScriptInteger id)
        {
            return new ScriptPlaceholderExpression(id);
        }

        public override ScriptPlaceholderExpression CreateCodeElement(IList<IScriptObject> args, InterpreterState state)
        {
            return args.Count == 1 ? CreateExpression(args[0] as ScriptInteger) : null;
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
