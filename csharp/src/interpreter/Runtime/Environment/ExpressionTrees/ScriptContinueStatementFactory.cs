using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace DynamicScript.Runtime.Environment.ExpressionTrees
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;
    using Compiler.Ast;

    [ComVisible(false)]
    [Serializable]
    sealed class ScriptContinueStatementFactory : ScriptStatementFactory<ScriptCodeContinueStatement, ScriptContinueStatement>
    {
        #region Nested Types
        [ComVisible(false)]
        private sealed class ModifyFunction : ModifyFunctionBase
        {
            private const string SecondParamName = "values";

            public ModifyFunction()
                : base( Instance, new ScriptFunctionContract.Parameter(SecondParamName, new ScriptArrayContract()))
            {
            }
        }
        #endregion

        private static readonly AggregatedSlotCollection<ScriptContinueStatementFactory> StaticSlots = new AggregatedSlotCollection<ScriptContinueStatementFactory>
        {
             {ModifyFunction.Name, (owner, state) => LazyField<ModifyFunction, IScriptFunction>(ref owner.m_modify)},
             {FlowControlStatementArgumentsFunction<ScriptCodeContinueStatement>.Name, (owner, state) => {if(owner.m_args == null)owner.m_args = new FlowControlStatementArgumentsFunction<ScriptCodeContinueStatement>(owner); return owner.m_args;}},
        };

        public new const string Name = "`continue";

        private IScriptFunction m_args;
        private IScriptFunction m_modify;

        private ScriptContinueStatementFactory(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        private ScriptContinueStatementFactory()
            : base(Name)
        {
        }

        public static ScriptContinueStatementFactory Instance = new ScriptContinueStatementFactory();

        public static ScriptContinueStatement CreateStatement(IEnumerable<IScriptObject> args = null)
        {
            return new ScriptContinueStatement(ScriptContinueStatement.CreateStatement(args));
        }

        public override ScriptContinueStatement CreateCodeElement(IList<IScriptObject> args, InterpreterState state)
        {
            switch (args.Count)
            {
                default:
                case 0: return CreateStatement();
                case 1: return CreateStatement(args[0] as IEnumerable<IScriptObject>);
            }
        }

        public override void Clear()
        {
            m_args = m_modify = null;
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
