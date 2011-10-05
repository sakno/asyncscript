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
        private sealed class ModifyAction : ModifyActionBase
        {
            private const string SecondParamName = "id";

            public ModifyAction()
                : base(Instance, new ScriptActionContract.Parameter(SecondParamName, ScriptIntegerContract.Instance))
            {
            }
        }
        #endregion

        public new const string Name = "placeholder";

        private IRuntimeSlot m_modify;

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

        protected override IRuntimeSlot Modify
        {
            get { return CacheConst<ModifyAction>(ref m_modify); }
        }
    }
}
