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
                : base(Instance, new ScriptActionContract.Parameter(SecondParamName, ScriptExpressionFactory.Instance))
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
        private sealed class GetFinallyBodyAction : CodeElementPartProvider<IScriptCodeElement<ScriptCodeExpression>>
        {
            public GetFinallyBodyAction()
                : base(Instance, ScriptExpressionFactory.Instance)
            {
            }

            protected override IScriptCodeElement<ScriptCodeExpression> Invoke(ScriptCodeTryElseFinallyExpression element, InterpreterState state)
            {
                return Convert(element.Finally.Expression) as IScriptCodeElement<ScriptCodeExpression>;
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

            protected override IScriptObject Invoke(IScriptCodeElement<ScriptCodeTryElseFinallyExpression> seh, ScriptInteger idx, InterpreterState state)
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
                : base(FirstParamName, Instance, SecondParamName, ScriptIntegerContract.Instance, ScriptExpressionFactory.Instance)
            {
            }

            private static IScriptCodeElement<ScriptCodeExpression> GetTrapBody(IList<ScriptCodeTryElseFinallyExpression.FailureTrap> traps, long index, InterpreterState state)
            {
                switch (index.Between(0, traps.Count - 1))
                {
                    case true:
                        var v = traps[(int)index];
                        return v != null ? Convert(v.Handler.Expression) as IScriptCodeElement<ScriptCodeExpression> : null;
                    default: return null;
                }
            }

            protected override IScriptObject Invoke(IScriptCodeElement<ScriptCodeTryElseFinallyExpression> seh, ScriptInteger idx, InterpreterState state)
            {
                return GetTrapBody(seh.CodeObject.Traps, idx, state);
            }
        }

        [ComVisible(false)]
        private sealed class SetFinallyBodyAction : ScriptAction<IScriptCodeElement<ScriptCodeTryElseFinallyExpression>, IScriptCodeElement<ScriptCodeExpression>>
        {
            private const string FirstParamName = "seh";
            private const string SecondParamName = "body";

            public SetFinallyBodyAction()
                : base(FirstParamName, Instance, SecondParamName, ScriptExpressionFactory.Instance)
            {
            }

            protected override void Invoke(IScriptCodeElement<ScriptCodeTryElseFinallyExpression> seh, IScriptCodeElement<ScriptCodeExpression> body, InterpreterState state)
            {
                seh.CodeObject.Finally.Expression = body != null ? body.CodeObject : ScriptCodeVoidExpression.Instance;
            }
        }

        [ComVisible(false)]
        private sealed class SetTrapBodyAction : ScriptAction<IScriptCodeElement<ScriptCodeTryElseFinallyExpression>, ScriptInteger, IScriptCodeElement<ScriptCodeExpression>>
        {
            private const string FirstParamName = "seh";
            private const string SecondParamName = "idx";
            private const string ThirdParamName = "body";

            public SetTrapBodyAction()
                : base(FirstParamName, Instance, SecondParamName, ScriptIntegerContract.Instance, ThirdParamName, ScriptExpressionFactory.Instance)
            {
            }

            private static void SetTrapBody(IList<ScriptCodeTryElseFinallyExpression.FailureTrap> traps, long index, IScriptCodeElement<ScriptCodeExpression> body)
            {
                var v = index.Between(0, traps.Count - 1) ? traps[(int)index] : null;
                if (v != null)
                    v.Handler.Expression = body != null ? body.CodeObject : ScriptCodeVoidExpression.Instance;
            }

            protected override void Invoke(IScriptCodeElement<ScriptCodeTryElseFinallyExpression> seh, ScriptInteger idx, IScriptCodeElement<ScriptCodeExpression> body, InterpreterState state)
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

        public static ScriptSehExpression CreateExpression(IScriptCodeElement<ScriptCodeExpression> dangerousCode)
        {
            return new ScriptSehExpression(ScriptSehExpression.CreateExpression(dangerousCode));
        }

        public override ScriptSehExpression CreateCodeElement(IList<IScriptObject> args, InterpreterState state)
        {
            return args.Count == 1 ? CreateExpression(args[0] as IScriptCodeElement<ScriptCodeExpression>) : null;
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
