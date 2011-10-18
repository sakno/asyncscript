using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace DynamicScript.Runtime.Environment.ExpressionTrees
{
    using Compiler.Ast;
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;

    [ComVisible(false)]
    [Serializable]
    sealed class ScriptForkExpressionFactory : ScriptExpressionFactory<ScriptCodeForkExpression, ScriptForkExpression>, IForkExpressionFactorySlots
    {
        #region Nested Types
        [ComVisible(false)]
        private sealed class ModifyAction : ModifyActionBase
        {
            private const string FirstParamName = "stmts";

            public ModifyAction()
                : base(Instance, new ScriptActionContract.Parameter(FirstParamName, ScriptExpressionFactory.Instance))
            {
            }
        }

        [ComVisible(false)]
        private sealed class GetBodyAction : CodeElementPartProvider<IScriptCodeElement<ScriptCodeExpression>>
        {
            public GetBodyAction()
                : base(Instance, new ScriptArrayContract(ScriptStatementFactory.Instance))
            {
            }

            protected override IScriptCodeElement<ScriptCodeExpression> Invoke(ScriptCodeForkExpression element, InterpreterState state)
            {
                return Convert(element.Body.Expression) as IScriptCodeElement<ScriptCodeExpression>;
            }
        }
        #endregion

        public new const string Name = "forkdef";

        private IRuntimeSlot m_modify;
        private IRuntimeSlot m_getbody;

        private ScriptForkExpressionFactory(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        private ScriptForkExpressionFactory()
            : base(Name)
        {
        }

        public static readonly ScriptForkExpressionFactory Instance = new ScriptForkExpressionFactory();

        public static ScriptForkExpression CreateExpression(IScriptCodeElement<ScriptCodeExpression> body)
        {
            return new ScriptForkExpression(ScriptForkExpression.CreateExpression(body));
        }

        public override ScriptForkExpression CreateCodeElement(IList<IScriptObject> args, InterpreterState state)
        {
            return args.Count == 1 ? CreateExpression(args[0] as IScriptCodeElement<ScriptCodeExpression>) : null;
        }

        protected override IRuntimeSlot Modify
        {
            get { return CacheConst<ModifyAction>(ref m_modify); }
        }

        public override void Clear()
        {
            m_getbody = m_modify = null;
        }

        IRuntimeSlot IForkExpressionFactorySlots.Body
        {
            get { return CacheConst<GetBodyAction>(ref m_getbody); }
        }
    }
}
