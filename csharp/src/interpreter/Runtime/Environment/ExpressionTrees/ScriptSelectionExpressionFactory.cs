using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace DynamicScript.Runtime.Environment.ExpressionTrees
{
    using Compiler.Ast;
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;

    [ComVisible(false)]
    [Serializable]
    sealed class ScriptSelectionExpressionFactory : ScriptExpressionFactory<ScriptCodeSelectionExpression, ScriptSelectionExpression>, ISelectionExpressionFactorySlots
    {
        #region Nested Types
        [ComVisible(false)]
        private sealed class GetCasesCountAction : CodeElementPartProvider<ScriptInteger>
        {
            public GetCasesCountAction()
                : base(Instance, ScriptIntegerContract.Instance)
            {
            }

            protected override ScriptInteger Invoke(ScriptCodeSelectionExpression element, InterpreterState state)
            {
                return element.Cases.Count;
            }
        }

        [ComVisible(false)]
        private sealed class GetDefaultHandlerAction : CodeElementPartProvider<IScriptArray>
        {
            public GetDefaultHandlerAction()
                : base(Instance, new ScriptArrayContract(ScriptStatementFactory.Instance))
            {
            }

            protected override IScriptArray Invoke(ScriptCodeSelectionExpression element, InterpreterState state)
            {
                return ScriptStatementFactory.CreateStatements(element.DefaultHandler, state);
            }
        }

        [ComVisible(false)]
        private sealed class SetDefaultHandlerAction : ScriptAction<IScriptCodeElement<ScriptCodeSelectionExpression>, IScriptArray>
        {
            private const string FirstParamName = "sel";
            private const string SecondParamName = "body";

            public SetDefaultHandlerAction()
                : base(FirstParamName, Instance, SecondParamName, new ScriptArrayContract(ScriptStatementFactory.Instance))
            {
            }

            protected override void Invoke(InvocationContext ctx, IScriptCodeElement<ScriptCodeSelectionExpression> sel, IScriptArray body)
            {
                sel.CodeObject.DefaultHandler.Clear();
                ScriptStatementFactory.CreateStatements(body, sel.CodeObject.DefaultHandler);
            }
        }

        [ComVisible(false)]
        private sealed class GetCaseValuesAction : ScriptFunc<IScriptCodeElement<ScriptCodeSelectionExpression>, ScriptInteger>
        {
            private const string FirstParamName = "sel";
            private const string SecondParamName = "idx";

            public GetCaseValuesAction()
                : base(FirstParamName, Instance, SecondParamName, ScriptIntegerContract.Instance, new ScriptArrayContract(ScriptExpressionFactory.Instance))
            {
            }

            private static IScriptArray GetCaseValues(IList<ScriptCodeSelectionExpression.SelectionCase> cases, long index, InterpreterState state)
            {
                switch (index.Between(0, cases.Count - 1))
                {
                    case true:
                        var c = cases[(int)index];
                        return ScriptExpressionFactory.CreateExpressions(c.Values, state);
                    default: return ScriptArray.Empty(ScriptExpressionFactory.Instance);
                }
            }

            protected override IScriptObject Invoke(InvocationContext ctx, IScriptCodeElement<ScriptCodeSelectionExpression> sel, ScriptInteger index)
            {
                return GetCaseValues(sel.CodeObject.Cases, index, ctx.RuntimeState);
            }
        }

        [ComVisible(false)]
        private sealed class SetCaseValuesAction : ScriptAction<IScriptCodeElement<ScriptCodeSelectionExpression>, ScriptInteger, IScriptArray>
        {
            private const string FirstParamName = "sel";
            private const string SecondParamName = "idx";
            private const string ThirdParamName = "values";

            public SetCaseValuesAction()
                : base(FirstParamName, Instance, SecondParamName, ScriptIntegerContract.Instance, ThirdParamName, new ScriptArrayContract(ScriptExpressionFactory.Instance))
            {
            }

            private static void SetCaseValues(IList<ScriptCodeSelectionExpression.SelectionCase> cases, long index, IScriptArray values)
            {
                var c = index.Between(0, cases.Count - 1) ? cases[(int)index] : null;
                if (c != null)
                {
                    c.Values.Clear();
                    ScriptExpressionFactory.CreateExpressions(values, c.Values);
                }
            }

            protected override void Invoke(InvocationContext ctx, IScriptCodeElement<ScriptCodeSelectionExpression> sel, ScriptInteger index, IScriptArray values)
            {
                throw new NotImplementedException();
            }
        }
        
        [ComVisible(false)]
        private sealed class GetCaseBodyAction: ScriptFunc<IScriptCodeElement<ScriptCodeSelectionExpression>, ScriptInteger>
        {
            private const string FirstParamName = "sel";
            private const string SecondParamName = "idx";

            public GetCaseBodyAction()
                : base(FirstParamName, Instance, SecondParamName, ScriptIntegerContract.Instance, new ScriptArrayContract(ScriptStatementFactory.Instance))
            {
            }

            private static IScriptArray GetCaseBody(IList<ScriptCodeSelectionExpression.SelectionCase> cases, long index, InterpreterState state)
            {
                switch (index.Between(0, cases.Count - 1))
                {
                    case true:
                        var c = cases[(int)index];
                        return ScriptStatementFactory.CreateStatements(c.Handler, state);
                    default: return ScriptArray.Empty(ScriptExpressionFactory.Instance);
                }
            }

            protected override IScriptObject Invoke(InvocationContext ctx, IScriptCodeElement<ScriptCodeSelectionExpression> sel, ScriptInteger index)
            {
                return GetCaseBody(sel.CodeObject.Cases, index, ctx.RuntimeState);
            }
        }

        [ComVisible(false)]
        private sealed class SetCaseBodyAction : ScriptAction<IScriptCodeElement<ScriptCodeSelectionExpression>, ScriptInteger, IScriptArray>
        {
            private const string FirstParamName = "sel";
            private const string SecondParamName = "idx";
            private const string ThirdParamName = "body";

            public SetCaseBodyAction()
                : base(FirstParamName, Instance, SecondParamName, ScriptIntegerContract.Instance, ThirdParamName, new ScriptArrayContract(ScriptStatementFactory.Instance))
            {
            }

            private static void SetCaseBody(IList<ScriptCodeSelectionExpression.SelectionCase> cases, long index, IScriptArray body)
            {
                var c = index.Between(0, cases.Count - 1) ? cases[(int)index] : null;
                if (c != null)
                {
                    c.Handler.Clear();
                    ScriptStatementFactory.CreateStatements(body, c.Handler);
                }
            }

            protected override void Invoke(InvocationContext ctx, IScriptCodeElement<ScriptCodeSelectionExpression> sel, ScriptInteger index, IScriptArray body)
            {
                SetCaseBody(sel.CodeObject.Cases, index, body);  
            }
        }
        #endregion

        public new const string Name = "selection";

        private IRuntimeSlot m_getcases;
        private IRuntimeSlot m_getdef;
        private IRuntimeSlot m_setdef;
        private IRuntimeSlot m_getcasevals;
        private IRuntimeSlot m_setcasevals;
        private IRuntimeSlot m_getcasebody;
        private IRuntimeSlot m_setcasebody;

        private ScriptSelectionExpressionFactory(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        private ScriptSelectionExpressionFactory()
            : base(Name)
        {
        }

        public static readonly ScriptSelectionExpressionFactory Instance = new ScriptSelectionExpressionFactory();

        public override ScriptSelectionExpression CreateCodeElement(IList<IScriptObject> args, InterpreterState state)
        {
            return new ScriptSelectionExpression();
        }

        public override void Clear()
        {
            m_getcasebody =
                m_getcases =
                m_getcasevals =
                m_getdef =
                m_setcasebody =
                m_setcasevals =
                m_setdef = null;
        }

        protected override IRuntimeSlot Modify
        {
            get { return null; }
        }

        IRuntimeSlot ISelectionExpressionFactorySlots.GetDef
        {
            get { return CacheConst<GetDefaultHandlerAction>(ref m_getdef); }
        }

        IRuntimeSlot ISelectionExpressionFactorySlots.SetDef
        {
            get { return CacheConst<SetDefaultHandlerAction>(ref m_setdef); }
        }

        IRuntimeSlot ISelectionExpressionFactorySlots.GetCaseValues
        {
            get { return CacheConst<GetCaseValuesAction>(ref m_getcasevals); }
        }

        IRuntimeSlot ISelectionExpressionFactorySlots.SetCaseValues
        {
            get { return CacheConst<SetCaseValuesAction>(ref m_setcasevals); }
        }

        IRuntimeSlot ISelectionExpressionFactorySlots.GetCaseBody
        {
            get { return CacheConst<GetCaseBodyAction>(ref m_getcasebody); }
        }

        IRuntimeSlot ISelectionExpressionFactorySlots.SetCaseBody
        {
            get { return CacheConst<SetCaseBodyAction>(ref m_setcasebody); }
        }

        IRuntimeSlot ISelectionExpressionFactorySlots.Cases
        {
            get { return CacheConst<GetCasesCountAction>(ref m_getcases); }
        }
    }
}
