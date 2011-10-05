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
    sealed class ScriptBreakStatementFactory: ScriptStatementFactory<ScriptCodeBreakLexicalScopeStatement, ScriptBreakStatement>, IFlowControlStatementFactorySlots
    {
        #region Nested Types
        [ComVisible(false)]
        private sealed class ModifyAction : ModifyActionBase
        {
            private const string SecondParamName = "values";

            public ModifyAction()
                : base( Instance, new ScriptActionContract.Parameter(SecondParamName, new ScriptArrayContract(ScriptSuperContract.Instance)))
            {
            }
        }
        #endregion

        public new const string Name = "leavedef";

        private IRuntimeSlot m_args;
        private IRuntimeSlot m_modify;

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

        IRuntimeSlot IFlowControlStatementFactorySlots.Args
        {
            get { return CacheConst(ref m_args, () => new FlowControlStatementArgumentsAction<ScriptCodeBreakLexicalScopeStatement>(Instance)); }
        }

        protected override IRuntimeSlot Modify
        {
            get { return CacheConst<ModifyAction>(ref m_modify); }
        }
    }
}
