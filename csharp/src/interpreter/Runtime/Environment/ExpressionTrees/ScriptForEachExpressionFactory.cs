using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace DynamicScript.Runtime.Environment.ExpressionTrees
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;
    using Compiler.Ast;

    [ComVisible(false)]
    [Serializable]
    sealed class ScriptForEachExpressionFactory : ScriptLoopExpressionFactory<ScriptCodeForEachLoopExpression, ScriptForEachExpression>, IForEachExpressionFactorySlots
    {
        #region Nested Types
        [ComVisible(false)]
        private sealed class ModifyAction : ModifyActionBase
        {
            private const string SecondParamName = "loopVar";
            private const string ThirdParamName = "iterator";
            private const string FourthParamName = "grouping";
            private const string FifthParamName = "body";

            public ModifyAction()
                : base(Instance, new ScriptActionContract.Parameter(SecondParamName, ScriptLoopVariableStatementFactory.Instance),
                new ScriptActionContract.Parameter(ThirdParamName, ScriptExpressionFactory.Instance),
                new ScriptActionContract.Parameter(FourthParamName, ScriptExpressionFactory.Instance),
                new ScriptActionContract.Parameter(FifthParamName, ScriptExpressionFactory.Instance))
            {
            }
        }

        [ComVisible(false)]
        private sealed class GetLoopVariableAction : CodeElementPartProvider<IScriptStatement<ScriptCodeLoopWithVariableExpression.LoopVariable>>
        {
            public GetLoopVariableAction()
                : base(Instance, ScriptVariableDeclarationFactory.Instance)
            {
            }

            protected override IScriptStatement<ScriptCodeLoopWithVariableExpression.LoopVariable> Invoke(ScriptCodeForEachLoopExpression element, InterpreterState state)
            {
                return element.Variable != null ? new ScriptLoopVariableStatement(element.Variable) : null;
            }
        }

        

        [ComVisible(false)]
        private sealed class GetCollectionAction : CodeElementPartProvider<IScriptExpression<ScriptCodeExpression>>
        {
            public GetCollectionAction()
                : base(Instance, ScriptExpressionFactory.Instance)
            {
            }

            protected override IScriptExpression<ScriptCodeExpression> Invoke(ScriptCodeForEachLoopExpression element, InterpreterState state)
            {
                return Convert(element.Iterator != null ? element.Iterator : null) as IScriptExpression<ScriptCodeExpression>;
            }
        }

        [ComVisible(false)]
        private sealed class GetGroupingAction : GetGroupingActionBase
        {
            public GetGroupingAction()
                : base(Instance)
            {
            }
        }

        [ComVisible(false)]
        private sealed class GetBodyAction : GetBodyActionBase
        {
            public GetBodyAction()
                : base(Instance)
            {
            }
        }
        #endregion
        public new const string Name = "foreach";

        private IRuntimeSlot m_modify;
        private IRuntimeSlot m_getvar;
        private IRuntimeSlot m_getbody;
        private IRuntimeSlot m_grouping;
        private IRuntimeSlot m_getcollection;

        private ScriptForEachExpressionFactory(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        private ScriptForEachExpressionFactory()
            : base(Name)
        {
        }

        public static readonly ScriptForEachExpressionFactory Instance = new ScriptForEachExpressionFactory();

        public override void Clear()
        {
            m_getbody =
                m_getcollection =
                m_getvar =
                m_grouping =
                m_modify = null;
        }

        public static ScriptForEachExpression CreateExpression(IScriptObject declaration, IScriptObject iterator, IScriptObject grouping, IScriptObject body)
        {
            var expression = ScriptForEachExpression.CreateExpression(declaration, iterator, grouping, body);
            return expression != null ? new ScriptForEachExpression(expression) : null;
        }

        public override ScriptForEachExpression CreateCodeElement(IList<IScriptObject> args, InterpreterState state)
        {
            return args.Count == 4 ? CreateExpression(args[0], args[1], args[2], args[3]) : null;
        }

        protected override IRuntimeSlot Modify
        {
            get { return CacheConst<ModifyAction>(ref m_modify); }
        }

        protected override IRuntimeSlot Grouping
        {
            get { return CacheConst<GetGroupingAction>(ref m_grouping); }
        }

        IRuntimeSlot IForEachExpressionFactorySlots.LoopVar
        {
            get { return CacheConst<GetLoopVariableAction>(ref m_getvar); }
        }

        IRuntimeSlot IForEachExpressionFactorySlots.Collection
        {
            get { return CacheConst<GetCollectionAction>(ref m_getcollection); }
        }

        protected override IRuntimeSlot Body
        {
            get { return CacheConst<GetBodyAction>(ref m_getbody); }
        }
    }
}
