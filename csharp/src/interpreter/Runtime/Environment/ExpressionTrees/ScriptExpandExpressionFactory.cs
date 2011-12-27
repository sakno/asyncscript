using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace DynamicScript.Runtime.Environment.ExpressionTrees
{
    using Compiler.Ast;
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;

    [ComVisible(false)]
    [Serializable]
    sealed class ScriptExpandExpressionFactory : ScriptExpressionFactory<ScriptCodeExpandExpression, ScriptExpandExpression>
    {
        #region Nested Types
        [ComVisible(false)]
        private sealed class ModifyFunction : ModifyFunctionBase
        {
            private const string SecondParamName = "target";
            private const string ThirdParamName = "expressions";

            public ModifyFunction()
                : base(Instance, new ScriptFunctionContract.Parameter(SecondParamName, ScriptExpressionFactory.Instance),
                new ScriptFunctionContract.Parameter(ThirdParamName, new ScriptArrayContract(ScriptExpressionFactory.Instance)))
            {
            }
        }
        #endregion

        private static readonly AggregatedSlotCollection<ScriptExpandExpressionFactory> StaticSlots = new AggregatedSlotCollection<ScriptExpandExpressionFactory>
        {
            {ModifyFunction.Name, (owner, state) => LazyField<ModifyFunction, IScriptFunction>(ref owner.m_modify)}
        };

        public new const string Name = "`expandq";

        private IScriptFunction m_modify;

        private ScriptExpandExpressionFactory(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        private ScriptExpandExpressionFactory()
            : base(Name)
        {
        }

        public static readonly ScriptExpandExpressionFactory Instance = new ScriptExpandExpressionFactory();

        public static ScriptExpandExpression CreateExpression(IScriptObject target, IEnumerable<IScriptObject> substitutions)
        {
            return new ScriptExpandExpression(ScriptExpandExpression.CreateExpression(target, substitutions));
        }

        public override void Clear()
        {
            m_modify = null;
        }

        public override ScriptExpandExpression CreateCodeElement(IList<IScriptObject> args, InterpreterState state)
        {
            return args.Count == 2 ? CreateExpression(args[0], args[1] as IEnumerable<IScriptObject>) : null;
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
