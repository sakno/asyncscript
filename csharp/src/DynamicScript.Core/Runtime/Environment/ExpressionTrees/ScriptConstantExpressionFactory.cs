using System;
using System.Collections.Generic;

namespace DynamicScript.Runtime.Environment.ExpressionTrees
{
    using Compiler.Ast;
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;

    [ComVisible(false)]
    sealed class ScriptConstantExpressionFactory : ScriptExpressionFactory<ScriptCodePrimitiveExpression, ScriptConstantExpression>
    {
        #region Nested Types
        [ComVisible(false)]
        private sealed class ModifyFunction : ModifyFunctionBase
        {
            private const string SecondParamName = "value";

            public ModifyFunction()
                : base(Instance, new ScriptFunctionContract.Parameter(SecondParamName, ScriptSuperContract.Instance))
            {
            }
        }
        #endregion

        private static readonly AggregatedSlotCollection<ScriptConstantExpressionFactory> StaticSlots = new AggregatedSlotCollection<ScriptConstantExpressionFactory>
        {
             {ModifyFunction.Name, (owner, state) => LazyField<ModifyFunction, IScriptFunction>(ref owner.m_modify)}
        };

        public new const string Name = "constant";

        private IScriptFunction m_modify;
        
        private ScriptConstantExpressionFactory()
            : base(Name)
        {
        }

        public static ScriptConstantExpressionFactory Instance = new ScriptConstantExpressionFactory();

        public static ScriptConstantExpression CreateExpression(IScriptObject value, bool emitDefault = true)
        {
            var expression = ScriptConstantExpression.CreateExpression(value);
            return expression != null || emitDefault ?
                new ScriptConstantExpression(expression ?? ScriptCodeVoidExpression.Instance) : null;
        }

        public override ScriptConstantExpression CreateCodeElement(IList<IScriptObject> args, InterpreterState state)
        {
            switch (args.Count)
            {
                case 1: return CreateExpression(args[0]);
                default: throw new FunctionArgumentsMistmatchException(state);
            }
        }

        protected override bool Mapping(ref IScriptObject value)
        {
            return value is ScriptConstantExpression;
        }

        public override IScriptObject Convert(IScriptObject value, InterpreterState state)
        {
            switch (value is ScriptConstantExpression)
            {
                case true: return value;
                default:
                    value = CreateExpression(value, false);
                    return value ?? Void;
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
