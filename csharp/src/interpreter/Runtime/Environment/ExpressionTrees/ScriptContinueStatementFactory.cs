using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace DynamicScript.Runtime.Environment.ExpressionTrees
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;
    using Compiler.Ast;

    [ComVisible(false)]
    [Serializable]
    sealed class ScriptContinueStatementFactory : ScriptStatementFactory<ScriptCodeContinueStatement, ScriptContinueStatement>, IFlowControlStatementFactorySlots
    {
        #region Nested Types
        [ComVisible(false)]
        private sealed class ModifyAction : ModifyActionBase
        {
            private const string SecondParamName = "values";

            public ModifyAction()
                : base( Instance, new ScriptActionContract.Parameter(SecondParamName, new ScriptArrayContract()))
            {
            }
        }
        #endregion

        public new const string Name = "continuedef";

        private IRuntimeSlot m_args;
        private IRuntimeSlot m_modify;

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

        #region Runtime Slots

        IRuntimeSlot IFlowControlStatementFactorySlots.Args
        {
            get { return CacheConst(ref m_args, () => new FlowControlStatementArgumentsAction<ScriptCodeContinueStatement>(Instance)); }
        }

        protected override IRuntimeSlot Modify
        {
            get { return CacheConst<ModifyAction>(ref m_modify); }
        }

        #endregion
    }
}
