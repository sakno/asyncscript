using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace DynamicScript.Runtime.Environment.ExpressionTrees
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;
    using Compiler.Ast;

    /// <summary>
    /// Represents a factory of the BREAK statement.
    /// This class cannot be inherited.
    /// </summary>
    [ComVisible(false)]
    sealed class ScriptBreakStatementFactory: ScriptStatementFactory<ScriptCodeBreakLexicalScopeStatement, ScriptBreakStatement>
    {
        #region Nested Types
        [ComVisible(false)]
        private sealed class ModifyFunction : ModifyFunctionBase
        {
            private const string SecondParamName = "values";

            public ModifyFunction()
                : base( Instance, new ScriptFunctionContract.Parameter(SecondParamName, new ScriptArrayContract(ScriptSuperContract.Instance)))
            {
            }
        }
        #endregion

        private static readonly AggregatedSlotCollection<ScriptBreakStatementFactory> StaticSlots = new AggregatedSlotCollection<ScriptBreakStatementFactory>
        {
            {ModifyFunction.Name, (owner, state) => LazyField<ModifyFunction, IScriptFunction>(ref owner.m_modify)},
            {FlowControlStatementArgumentsFunction<ScriptCodeBreakLexicalScopeStatement>.Name, (owner, state) => {if(owner.m_args == null) owner.m_args = new FlowControlStatementArgumentsFunction<ScriptCodeBreakLexicalScopeStatement>(owner); return owner.m_args;}}
        };

        public new const string Name = "`leave";

        private IScriptFunction m_args;
        private IScriptFunction m_modify;

        private ScriptBreakStatementFactory(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        private ScriptBreakStatementFactory()
            : base(Name)
        {
        }

        /// <summary>
        /// Represents a singleton instance of this script contract.
        /// </summary>
        public static ScriptBreakStatementFactory Instance = new ScriptBreakStatementFactory();

        public static ScriptBreakStatement CreateStatement(IEnumerable<IScriptObject> args = null)
        {
            return new ScriptBreakStatement(ScriptBreakStatement.CreateStatement(args));
        }

        public override ScriptBreakStatement CreateCodeElement(IList<IScriptObject> args, InterpreterState state)
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
            m_modify = m_args = null;
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
