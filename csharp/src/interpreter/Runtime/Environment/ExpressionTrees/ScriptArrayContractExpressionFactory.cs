using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace DynamicScript.Runtime.Environment.ExpressionTrees
{
    using Compiler.Ast;
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;

    [ComVisible(false)]
    sealed class ScriptArrayContractExpressionFactory : ScriptExpressionFactory<ScriptCodeArrayContractExpression, ScriptArrayContractExpression>, IArrayContractExpressionFactorySlots
    {
        #region Nested Types
        [ComVisible(false)]
        private sealed class GetElementContractAction : CodeElementPartProvider<IScriptExpression<ScriptCodeExpression>>
        {
            public GetElementContractAction()
                : base(Instance, ScriptExpressionFactory.Instance)
            {
            }

            protected override IScriptExpression<ScriptCodeExpression> Invoke(ScriptCodeArrayContractExpression expression, InterpreterState state)
            {
                return Convert(expression.ElementContract) as IScriptExpression<ScriptCodeExpression>;
            }
        }

        [ComVisible(false)]
        private sealed class GetRankAction : CodeElementPartProvider<ScriptInteger>
        {
            public GetRankAction()
                : base(Instance, ScriptIntegerContract.Instance)
            {
            }

            protected override ScriptInteger Invoke(ScriptCodeArrayContractExpression expression, InterpreterState state)
            {
                return expression.Rank;
            }
        }

        [ComVisible(false)]
        private sealed class ModifyAction : ModifyActionBase
        {
            private const string SecondParamName = "contract";
            private const string ThirdParamName = "rank";

            public ModifyAction()
                : base(Instance, new ScriptActionContract.Parameter(SecondParamName, ScriptExpressionFactory.Instance), new ScriptActionContract.Parameter(ThirdParamName, ScriptIntegerContract.Instance))
            {
            }
        }
        #endregion

        public new const string Name = "arcon";
        private IRuntimeSlot m_elem;
        private IRuntimeSlot m_rank;
        private IRuntimeSlot m_modify;

        private ScriptArrayContractExpressionFactory(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        private ScriptArrayContractExpressionFactory()
            : base(Name)
        {
        }

        public static ScriptArrayContractExpressionFactory Instance = new ScriptArrayContractExpressionFactory();

        public static ScriptArrayContractExpression CreateExpression(IScriptObject elementContract, ScriptInteger rank)
        {
            var expression = ScriptArrayContractExpression.CreateExpression(elementContract, rank);
            return expression != null ? new ScriptArrayContractExpression(expression) : null;
        }

        public override ScriptArrayContractExpression CreateCodeElement(IList<IScriptObject> args, InterpreterState state)
        {
            return args.Count == 2 ? CreateExpression(args[0], args[1] as ScriptInteger) : null;
        }

        protected override IRuntimeSlot Modify
        {
            get { return CacheConst<ModifyAction>(ref m_modify); }
        }

        public override void Clear()
        {
            m_elem = m_modify = m_rank = null;
        }

        IRuntimeSlot IArrayContractExpressionFactorySlots.Elem
        {
            get { return CacheConst<GetElementContractAction>(ref m_elem); }
        }

        IRuntimeSlot IArrayContractExpressionFactorySlots.Rank
        {
            get { return CacheConst<GetElementContractAction>(ref m_rank); }
        }
    }
}
