using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace DynamicScript.Runtime.Environment.ExpressionTrees
{
    using Compiler.Ast;
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;
    using Enumerable = System.Linq.Enumerable;

    [ComVisible(false)]
    sealed class ScriptSehExpressionFactory : ScriptExpressionFactory<ScriptCodeTryElseFinallyExpression, ScriptSehExpression>, ISehExpressionFactorySlots
    {
        #region Nested Types
        [ComVisible(false)]
        private sealed class ModifyAction : ModifyActionBase
        {
            private const string SecondParamName = "dangerouseCode";

            public ModifyAction()
                : base(Instance, new ScriptActionContract.Parameter(SecondParamName, new ScriptArrayContract(ScriptStatementFactory.Instance)))
            {
            }
        }

        [ComVisible(false)]
        private sealed class GetTrapCountAction : CodeElementPartProvider<ScriptInteger>
        {
            public GetTrapCountAction()
                : base(Instance, ScriptIntegerContract.Instance)
            {
            }

            protected override ScriptInteger Invoke(ScriptCodeTryElseFinallyExpression element, InterpreterState state)
            {
                return element.Traps.Count;
            }
        }

        [ComVisible(false)]
        private sealed class GetFinallyBodyAction : CodeElementPartProvider<IScriptArray>
        {
            public GetFinallyBodyAction()
                : base(Instance, new ScriptArrayContract(ScriptStatementFactory.Instance))
            {
            }

            protected override IScriptArray Invoke(ScriptCodeTryElseFinallyExpression element, InterpreterState state)
            {
                return ScriptStatementFactory.CreateStatements(element.Finally, state);
            }
        }

        [ComVisible(false)]
        private sealed class GetTrapVarAction : ScriptFunc<IScriptCodeElement<ScriptCodeTryElseFinallyExpression>, ScriptInteger>
        {
            private const string FirstParamName = "seh";
            private const string SecondParamName = "idx";

            public GetTrapVarAction()
                : base(FirstParamName, Instance, SecondParamName, ScriptIntegerContract.Instance, ScriptVariableDeclarationFactory.Instance)
            {
            }

            private static ScriptVariableDeclaration GetTrapVar(IList<ScriptCodeTryElseFinallyExpression.FailureTrap> traps, long index)
            {
                switch (index.Between(0, traps.Count - 1))
                {
                    case true:
                        var v = traps[(int)index];
                        return v != null && v.Filter != null ? new ScriptVariableDeclaration(v.Filter) : null;
                    default: return null;
                }
            }

            protected override IScriptObject Invoke(InvocationContext ctx, IScriptCodeElement<ScriptCodeTryElseFinallyExpression> seh, ScriptInteger idx)
            {
                return GetTrapVar(seh.CodeObject.Traps, idx);
            }
        }

        [ComVisible(false)]
        private sealed class GetTrapBodyAction : ScriptFunc<IScriptCodeElement<ScriptCodeTryElseFinallyExpression>, ScriptInteger>
        {
            private const string FirstParamName = "seh";
            private const string SecondParamName = "idx";

            public GetTrapBodyAction()
                : base(FirstParamName, Instance, SecondParamName, ScriptIntegerContract.Instance, new ScriptArrayContract(ScriptStatementFactory.Instance))
            {
            }

            private static IScriptArray GetTrapBody(IList<ScriptCodeTryElseFinallyExpression.FailureTrap> traps, long index, InterpreterState state)
            {
                switch (index.Between(0, traps.Count - 1))
                {
                    case true:
                        var v = traps[(int)index];
                        return v != null ? ScriptStatementFactory.CreateStatements(v.Handler, state) : null;
                    default: return null;
                }
            }

            protected override IScriptObject Invoke(InvocationContext ctx, IScriptCodeElement<ScriptCodeTryElseFinallyExpression> seh, ScriptInteger idx)
            {
                return GetTrapBody(seh.CodeObject.Traps, idx, ctx.RuntimeState);
            }
        }

        [ComVisible(false)]
        private sealed class SetFinallyBodyAction : ScriptAction<IScriptCodeElement<ScriptCodeTryElseFinallyExpression>, IScriptArray>
        {
            private const string FirstParamName = "seh";
            private const string SecondParamName = "body";

            public SetFinallyBodyAction()
                : base(FirstParamName, Instance, SecondParamName, new ScriptArrayContract(ScriptStatementFactory.Instance))
            {
            }

            protected override void Invoke(InvocationContext ctx, IScriptCodeElement<ScriptCodeTryElseFinallyExpression> seh, IScriptArray body)
            {
                seh.CodeObject.Finally.Clear();
                ScriptStatementFactory.CreateStatements(body, seh.CodeObject.Finally);
            }
        }

        [ComVisible(false)]
        private sealed class SetTrapBodyAction : ScriptAction<IScriptCodeElement<ScriptCodeTryElseFinallyExpression>, ScriptInteger, IScriptArray>
        {
            private const string FirstParamName = "seh";
            private const string SecondParamName = "idx";
            private const string ThirdParamName = "body";

            public SetTrapBodyAction()
                : base(FirstParamName, Instance, SecondParamName, ScriptIntegerContract.Instance, ThirdParamName, new ScriptArrayContract(ScriptStatementFactory.Instance))
            {
            }

            private static void SetTrapBody(IList<ScriptCodeTryElseFinallyExpression.FailureTrap> traps, long index, IScriptArray body)
            {
                var v = index.Between(0, traps.Count - 1) ? traps[(int)index] : null;
                if (v != null)
                {
                    v.Handler.Clear();
                    ScriptStatementFactory.CreateStatements(body, v.Handler);
                }
            }

            protected override void Invoke(InvocationContext ctx, IScriptCodeElement<ScriptCodeTryElseFinallyExpression> seh, ScriptInteger idx, IScriptArray body)
            {
                SetTrapBody(seh.CodeObject.Traps, idx, body);
            }
        }
        #endregion

        public new const string Name = "seh";

        private IRuntimeSlot m_modify;
        private IRuntimeSlot m_trapcount;
        private IRuntimeSlot m_getfinally;
        private IRuntimeSlot m_gettrapvar;
        private IRuntimeSlot m_gettrapbody;
        private IRuntimeSlot m_setfinally;
        private IRuntimeSlot m_settrapbody;

        private ScriptSehExpressionFactory(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        private ScriptSehExpressionFactory()
            : base(Name)
        {
        }

        public static readonly ScriptSehExpressionFactory Instance = new ScriptSehExpressionFactory();

        public static ScriptSehExpression CreateExpression(IEnumerable<IScriptObject> dangerousCode)
        {
            return new ScriptSehExpression(ScriptSehExpression.CreateExpression(dangerousCode));
        }

        public override ScriptSehExpression CreateCodeElement(IList<IScriptObject> args, InterpreterState state)
        {
            switch (args.Count)
            {
                case 0: return CreateExpression(Enumerable.Empty<IScriptObject>());
                case 1: return CreateExpression(args[0] as IEnumerable<IScriptObject> ?? args);
                default: return CreateExpression(args);
            }
        }

        public override void Clear()
        {
            m_getfinally = m_gettrapbody = m_gettrapvar = m_modify = m_setfinally = m_settrapbody = m_trapcount = null;
        }

        protected override IRuntimeSlot Modify
        {
            get { return CacheConst<ModifyAction>(ref m_modify); }
        }

        IRuntimeSlot ISehExpressionFactorySlots.GetFinallyBody
        {
            get { return CacheConst<GetFinallyBodyAction>(ref m_getfinally); }
        }

        IRuntimeSlot ISehExpressionFactorySlots.SetFinallyBody
        {
            get { return CacheConst<SetFinallyBodyAction>(ref m_setfinally); }
        }

        IRuntimeSlot ISehExpressionFactorySlots.GetTrapBody
        {
            get { return CacheConst<GetTrapBodyAction>(ref m_gettrapbody); }
        }

        IRuntimeSlot ISehExpressionFactorySlots.SetTrapBody
        {
            get { return CacheConst<SetTrapBodyAction>(ref m_settrapbody); }
        }

        IRuntimeSlot ISehExpressionFactorySlots.GetTrapVar
        {
            get { return CacheConst<GetTrapVarAction>(ref m_gettrapvar); }
        }

        IRuntimeSlot ISehExpressionFactorySlots.Traps
        {
            get { return CacheConst<GetTrapCountAction>(ref m_trapcount); }
        }
    }
}
