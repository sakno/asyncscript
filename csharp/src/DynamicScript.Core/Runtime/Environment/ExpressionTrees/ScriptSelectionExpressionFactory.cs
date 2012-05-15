using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace DynamicScript.Runtime.Environment.ExpressionTrees
{
    using Compiler.Ast;
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;

    [ComVisible(false)]
    [Serializable]
    sealed class ScriptSelectionExpressionFactory : ScriptExpressionFactory<ScriptCodeSelectionExpression, ScriptSelectionExpression>
    {
        #region Nested Types
        [ComVisible(false)]
        private sealed class GetCasesCountFunction : CodeElementPartProvider<ScriptInteger>
        {
            public const string Name = "getCases";

            public GetCasesCountFunction()
                : base(Instance, ScriptIntegerContract.Instance)
            {
            }

            protected override ScriptInteger Invoke(ScriptCodeSelectionExpression element, InterpreterState state)
            {
                return element.Cases.Count;
            }
        }

        [ComVisible(false)]
        private sealed class GetDefaultHandlerFunction : CodeElementPartProvider<IScriptCodeElement<ScriptCodeExpression>>
        {
            public const string Name = "getDefault";

            public GetDefaultHandlerFunction()
                : base(Instance, ScriptExpressionFactory.Instance)
            {
            }

            protected override IScriptCodeElement<ScriptCodeExpression> Invoke(ScriptCodeSelectionExpression element, InterpreterState state)
            {
                return Convert(element.DefaultHandler.Expression) as IScriptCodeElement<ScriptCodeExpression>;
            }
        }

        [ComVisible(false)]
        private sealed class SetDefaultHandlerFunction : ScriptAction<IScriptCodeElement<ScriptCodeSelectionExpression>, IScriptCodeElement<ScriptCodeExpression>>
        {
            public const string Name = "setDefault";
            private const string FirstParamName = "sel";
            private const string SecondParamName = "body";

            public SetDefaultHandlerFunction()
                : base(FirstParamName, Instance, SecondParamName, new ScriptArrayContract(ScriptStatementFactory.Instance))
            {
            }

            protected override void Invoke(IScriptCodeElement<ScriptCodeSelectionExpression> sel, IScriptCodeElement<ScriptCodeExpression> body, InterpreterState state)
            {
                sel.CodeObject.DefaultHandler.Expression = body != null ? body.CodeObject : ScriptCodeVoidExpression.Instance;
            }
        }

        [ComVisible(false)]
        private sealed class GetCaseValuesFunction : ScriptFunc<IScriptCodeElement<ScriptCodeSelectionExpression>, ScriptInteger>
        {
            public const string Name = "getCaseValues";
            private const string FirstParamName = "sel";
            private const string SecondParamName = "idx";

            public GetCaseValuesFunction()
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

            protected override IScriptObject Invoke(IScriptCodeElement<ScriptCodeSelectionExpression> sel, ScriptInteger index, InterpreterState state)
            {
                return GetCaseValues(sel.CodeObject.Cases, index, state);
            }
        }

        [ComVisible(false)]
        private sealed class SetCaseValuesFunction : ScriptAction<IScriptCodeElement<ScriptCodeSelectionExpression>, ScriptInteger, IScriptArray>
        {
            public const string Name = "setCaseValues";
            private const string FirstParamName = "sel";
            private const string SecondParamName = "idx";
            private const string ThirdParamName = "values";

            public SetCaseValuesFunction()
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

            protected override void Invoke(IScriptCodeElement<ScriptCodeSelectionExpression> sel, ScriptInteger index, IScriptArray values, InterpreterState state)
            {
                SetCaseValues(sel.CodeObject.Cases, index, values);
            }
        }
        
        [ComVisible(false)]
        private sealed class GetCaseBodyFunction: ScriptFunc<IScriptCodeElement<ScriptCodeSelectionExpression>, ScriptInteger>
        {
            public const string Name = "getCaseBody";
            private const string FirstParamName = "sel";
            private const string SecondParamName = "idx";

            public GetCaseBodyFunction()
                : base(FirstParamName, Instance, SecondParamName, ScriptIntegerContract.Instance, new ScriptArrayContract(ScriptStatementFactory.Instance))
            {
            }

            private static IScriptCodeElement<ScriptCodeExpression> GetCaseBody(IList<ScriptCodeSelectionExpression.SelectionCase> cases, long index, InterpreterState state)
            {
                switch (index.Between(0, cases.Count - 1))
                {
                    case true:
                        var c = cases[(int)index];
                        return Convert(c.Handler.Expression) as IScriptCodeElement<ScriptCodeExpression>;
                    default: return new ScriptConstantExpression(ScriptCodeVoidExpression.Instance);
                }
            }

            protected override IScriptObject Invoke(IScriptCodeElement<ScriptCodeSelectionExpression> sel, ScriptInteger index, InterpreterState state)
            {
                return GetCaseBody(sel.CodeObject.Cases, index, state);
            }
        }

        [ComVisible(false)]
        private sealed class SetCaseBodyFunction : ScriptAction<IScriptCodeElement<ScriptCodeSelectionExpression>, ScriptInteger, IScriptCodeElement<ScriptCodeExpression>>
        {
            public const string Name = "setCaseBody";
            private const string FirstParamName = "sel";
            private const string SecondParamName = "idx";
            private const string ThirdParamName = "body";

            public SetCaseBodyFunction()
                : base(FirstParamName, Instance, SecondParamName, ScriptIntegerContract.Instance, ThirdParamName, new ScriptArrayContract(ScriptStatementFactory.Instance))
            {
            }

            private static void SetCaseBody(IList<ScriptCodeSelectionExpression.SelectionCase> cases, long index, IScriptCodeElement<ScriptCodeExpression> body)
            {
                var c = index.Between(0, cases.Count - 1) ? cases[(int)index] : null;
                if (c != null)
                    c.Handler.Expression = body != null ? body.CodeObject : ScriptCodeVoidExpression.Instance;
            }

            protected override void Invoke(IScriptCodeElement<ScriptCodeSelectionExpression> sel, ScriptInteger index, IScriptCodeElement<ScriptCodeExpression> body, InterpreterState state)
            {
                SetCaseBody(sel.CodeObject.Cases, index, body);  
            }
        }
        #endregion

        private static readonly AggregatedSlotCollection<ScriptSelectionExpressionFactory> StaticSlots = new AggregatedSlotCollection<ScriptSelectionExpressionFactory>
        {
             {GetCasesCountFunction.Name, (owner, state) => LazyField<GetCasesCountFunction, IScriptFunction>(ref owner.m_getcases)},
             {GetDefaultHandlerFunction.Name, (owner, state) => LazyField<GetDefaultHandlerFunction, IScriptFunction>(ref owner.m_getdef)},
             {SetDefaultHandlerFunction.Name, (owner, state) => LazyField<SetDefaultHandlerFunction, IScriptFunction>(ref owner.m_setdef)},
             {GetCaseValuesFunction.Name, (owner, state) => LazyField<GetCaseValuesFunction, IScriptFunction>(ref owner.m_getcasevals)},
             {SetCaseValuesFunction.Name, (owner, state) => LazyField<SetCaseValuesFunction, IScriptFunction>(ref owner.m_setcasevals)},
             {GetCaseBodyFunction.Name, (owner, state) => LazyField<GetCaseBodyFunction, IScriptFunction>(ref owner.m_getcasebody)},
             {SetCaseBodyFunction.Name, (owner, state) => LazyField<SetCaseBodyFunction, IScriptFunction>(ref owner.m_setcasebody)},
        };

        public new const string Name = "selection";

        private IScriptFunction m_getcases;
        private IScriptFunction m_getdef;
        private IScriptFunction m_setdef;
        private IScriptFunction m_getcasevals;
        private IScriptFunction m_setcasevals;
        private IScriptFunction m_getcasebody;
        private IScriptFunction m_setcasebody;

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

        public override ICollection<string> Slots
        {
            get { return StaticSlots.Keys; }
        }

        public override IScriptObject this[string slotName, InterpreterState state]
        {
            get { return StaticSlots.GetValue(this, slotName, state); }
            set { StaticSlots.SetValue(this, slotName, value, state); }
        }

        protected override IScriptObject GetSlotMetadata(string slotName, InterpreterState state)
        {
            return StaticSlots.GetSlotMetadata(this, slotName, state);
        }
    }
}
