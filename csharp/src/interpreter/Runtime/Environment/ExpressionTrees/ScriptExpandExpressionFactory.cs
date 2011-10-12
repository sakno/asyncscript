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
        private sealed class ModifyAction : ModifyActionBase
        {
            private const string SecondParamName = "target";
            private const string ThirdParamName = "expressions";

            public ModifyAction()
                : base(Instance, new ScriptActionContract.Parameter(SecondParamName, ScriptExpressionFactory.Instance),
                new ScriptActionContract.Parameter(ThirdParamName, new ScriptArrayContract(ScriptExpressionFactory.Instance)))
            {
            }
        }
        #endregion
        public new const string Name = "expand";

        private IRuntimeSlot m_modify;

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

        protected override IRuntimeSlot Modify
        {
            get { return CacheConst<ModifyAction>(ref m_modify); }
        }
    }
}
