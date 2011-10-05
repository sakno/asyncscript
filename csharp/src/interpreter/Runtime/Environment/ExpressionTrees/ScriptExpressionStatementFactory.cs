using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace DynamicScript.Runtime.Environment.ExpressionTrees
{
    using Compiler.Ast;
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;

    [ComVisible(false)]
    [Serializable]
    sealed class ScriptExpressionStatementFactory : ScriptStatementFactory<ScriptCodeExpressionStatement, ScriptExpressionStatement>, IExpressionStatementFactorySlots
    {
        #region Nested Types
        [ComVisible(false)]
        private sealed class ModifyAction : ModifyActionBase
        {
            private const string FirstParamName = "e";

            public ModifyAction()
                : base(Instance, new ScriptActionContract.Parameter(FirstParamName, ScriptExpressionFactory.Instance))
            {
            }
        }

        [ComVisible(false)]
        private sealed class ExtractAction : CodeElementPartProvider<IScriptExpression<ScriptCodeExpression>>
        {
            public ExtractAction()
                : base(Instance, ScriptExpressionFactory.Instance)
            {
            }

            protected override IScriptExpression<ScriptCodeExpression> Invoke(ScriptCodeExpressionStatement element, InterpreterState state)
            {
                return Convert(element.Expression) as IScriptExpression<ScriptCodeExpression>;
            }
        }
        #endregion

        public new const string Name = "expression";

        private IRuntimeSlot m_modify;
        private IRuntimeSlot m_extract;

        private ScriptExpressionStatementFactory(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        private ScriptExpressionStatementFactory()
            : base(Name)
        {
        }

        public static readonly ScriptExpressionStatementFactory Instance = new ScriptExpressionStatementFactory();

        public static ScriptExpressionStatement CreateStatement(IScriptObject expr)
        {
            var statement = ScriptExpressionStatement.CreateStatement(expr);
            return statement != null ? new ScriptExpressionStatement(statement) : null;
        }

        public override ScriptExpressionStatement CreateCodeElement(IList<IScriptObject> args, InterpreterState state)
        {
            return args.Count == 1 ? CreateStatement(args[0]) : null;
        }

        protected override IRuntimeSlot Modify
        {
            get { return CacheConst<ModifyAction>(ref m_modify); }
        }

        public override void Clear()
        {
            m_modify = m_extract = null;
        }

        IRuntimeSlot IExpressionStatementFactorySlots.Extract
        {
            get { return CacheConst<ExtractAction>(ref m_extract); }
        }
    }
}
