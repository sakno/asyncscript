using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace DynamicScript.Runtime.Environment.ExpressionTrees
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;
    using Compiler.Ast;

    [ComVisible(false)]
    [Serializable]
    sealed class ScriptReturnStatementFactory : ScriptStatementFactory<ScriptCodeReturnStatement, ScriptReturnStatement>, IReturnStatementFactorySlots
    {
        #region Nested Types
        [ComVisible(false)]
        private sealed class ModifyAction : ModifyActionBase
        {
            private const string SecondParamName = "retval";

            public ModifyAction()
                : base(Instance, new ScriptActionContract.Parameter(SecondParamName, ScriptExpressionFactory.Instance))
            {
            }
        }

        [ComVisible(false)]
        private sealed class GetValueAction : CodeElementPartProvider<IScriptExpression<ScriptCodeExpression>>
        {
            public GetValueAction()
                : base(Instance, ScriptSuperContract.Instance)
            {
            }

            protected override IScriptExpression<ScriptCodeExpression> Invoke(ScriptCodeReturnStatement element, InterpreterState state)
            {
                return Convert(element.Value) as IScriptExpression<ScriptCodeExpression>;
            }
        }
        #endregion

        public new const string Name = "returndef";
        private IRuntimeSlot m_retval;
        private IRuntimeSlot m_modify;

        private ScriptReturnStatementFactory()
            : base(Name)
        {
        }

        private ScriptReturnStatementFactory(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        public static readonly ScriptReturnStatementFactory Instance = new ScriptReturnStatementFactory();

        public static ScriptReturnStatement CreateStatement(IScriptObject retObj = null)
        {
            return new ScriptReturnStatement(ScriptReturnStatement.CreateStatement(retObj));
        }

        public override ScriptReturnStatement CreateCodeElement(IList<IScriptObject> args, InterpreterState state)
        {
            switch (args.Count)
            {
                case 0: return CreateStatement();
                case 1: return CreateStatement(args[0]);
                default: return null;
            }
        }

        public override void Clear()
        {
            m_retval = m_modify = null;
        }

        #region Runtime Slots

        IRuntimeSlot IReturnStatementFactorySlots.Value
        {
            get { return CacheConst<GetValueAction>(ref m_retval); }
        }

        protected override IRuntimeSlot Modify
        {
            get { return CacheConst<ModifyAction>(ref m_modify); }
        }

        #endregion

        
    }
}
