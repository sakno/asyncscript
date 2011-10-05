using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace DynamicScript.Runtime.Environment.ExpressionTrees
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;
    using Compiler.Ast;

    [ComVisible(false)]
    [Serializable]
    sealed class ScriptLoopVariableStatementFactory : ScriptStatementFactory<ScriptCodeLoopWithVariableExpression.LoopVariable, ScriptLoopVariableStatement>
    {
        #region Nested Types
        [ComVisible(false)]
        private sealed class ModifyAction : ModifyActionBase
        {
            private const string SecondParamName = "name";
            private const string ThirdParamName = "temp";
            private const string FourthParamName = "init";

            public ModifyAction()
                : base(Instance, new ScriptActionContract.Parameter(SecondParamName, ScriptStringContract.Instance),
                new ScriptActionContract.Parameter(ThirdParamName, ScriptBooleanContract.Instance),
                new ScriptActionContract.Parameter(FourthParamName, ScriptExpressionFactory.Instance))
            {
            }
        }
        #endregion

        public new const string Name = "loopvar";

        private IRuntimeSlot m_modify;

        private ScriptLoopVariableStatementFactory(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        private ScriptLoopVariableStatementFactory()
            : base(Name)
        {
        }

        public static readonly ScriptLoopVariableStatementFactory Instance = new ScriptLoopVariableStatementFactory();

        public static ScriptLoopVariableStatement CreateStatement(ScriptString name, ScriptBoolean temporary, IScriptCodeElement<ScriptCodeExpression> initExpr)
        {
            var loopvar = ScriptLoopVariableStatement.CreateStatement(name, temporary, initExpr);
            return loopvar != null ? new ScriptLoopVariableStatement(loopvar) : null;
        }

        public override ScriptLoopVariableStatement CreateCodeElement(IList<IScriptObject> args, InterpreterState state)
        {
            switch (args.Count)
            {
                case 1:
                    return CreateStatement(args[0] as ScriptString, ScriptBoolean.False, null);
                case 2:
                    return CreateStatement(args[0] as ScriptString, args[1] as ScriptBoolean, null);
                case 3:
                    return CreateStatement(args[0] as ScriptString, args[1] as ScriptBoolean, args[2] as IScriptCodeElement<ScriptCodeExpression>);
                default:
                    return null;
            }
        }

        public override void Clear()
        {
            m_modify = null;
        }

        protected override IRuntimeSlot Modify
        {
            get { return CacheConst<ModifyAction>(ref m_modify); }
        }
    }
}
