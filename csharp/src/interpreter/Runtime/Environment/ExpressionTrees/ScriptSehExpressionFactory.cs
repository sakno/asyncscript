using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace DynamicScript.Runtime.Environment.ExpressionTrees
{
    using Compiler.Ast;
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;
    using Enumerable = System.Linq.Enumerable;

    [ComVisible(false)]
    sealed class ScriptSehExpressionFactory : ScriptExpressionFactory<ScriptCodeTryElseFinallyExpression, ScriptSehExpression>
    {
        #region Nested Types
        [ComVisible(false)]
        private sealed class ModifyFunction : ModifyFunctionBase
        {
            private const string SecondParamName = "dangerouseCode";

            public ModifyFunction()
                : base(Instance, new ScriptFunctionContract.Parameter(SecondParamName, ScriptExpressionFactory.Instance))
            {
            }
        }

        [ComVisible(false)]
        private sealed class GetTrapCountFunction : CodeElementPartProvider<ScriptInteger>
        {
            public const string Name = "trapCount";

            public GetTrapCountFunction()
                : base(Instance, ScriptIntegerContract.Instance)
            {
            }

            protected override ScriptInteger Invoke(ScriptCodeTryElseFinallyExpression element, InterpreterState state)
            {
                return element.Traps.Count;
            }
        }

        [ComVisible(false)]
        private sealed class GetFinallyBodyFunction : CodeElementPartProvider<IScriptCodeElement<ScriptCodeExpression>>
        {
            public const string Name = "getFinally";

            public GetFinallyBodyFunction()
                : base(Instance, ScriptExpressionFactory.Instance)
            {
            }

            protected override IScriptCodeElement<ScriptCodeExpression> Invoke(ScriptCodeTryElseFinallyExpression element, InterpreterState state)
            {
                return Convert(element.Finally.Expression) as IScriptCodeElement<ScriptCodeExpression>;
            }
        }

        [ComVisible(false)]
        private sealed class GetTrapVarFunction : ScriptFunc<IScriptCodeElement<ScriptCodeTryElseFinallyExpression>, ScriptInteger>
        {
            public const string Name = "trapvar";
            private const string FirstParamName = "seh";
            private const string SecondParamName = "idx";

            public GetTrapVarFunction()
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
        private sealed class GetTrapBodyFunction : ScriptFunc<IScriptCodeElement<ScriptCodeTryElseFinallyExpression>, ScriptInteger>
        {
            public const string Name = "getTrap";
            private const string FirstParamName = "seh";
            private const string SecondParamName = "idx";

            public GetTrapBodyFunction()
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
        private sealed class SetFinallyBodyFunction : ScriptAction<IScriptCodeElement<ScriptCodeTryElseFinallyExpression>, IScriptCodeElement<ScriptCodeExpression>>
        {
            public const string Name = "setFinally";
            private const string FirstParamName = "seh";
            private const string SecondParamName = "body";

            public SetFinallyBodyFunction()
                : base(FirstParamName, Instance, SecondParamName, ScriptExpressionFactory.Instance)
            {
            }

            protected override void Invoke(IScriptCodeElement<ScriptCodeTryElseFinallyExpression> seh, IScriptCodeElement<ScriptCodeExpression> body, InterpreterState state)
            {
                seh.CodeObject.Finally.Expression = body != null ? body.CodeObject : ScriptCodeVoidExpression.Instance;
            }
        }

        [ComVisible(false)]
        private sealed class SetTrapBodyFunction : ScriptAction<IScriptCodeElement<ScriptCodeTryElseFinallyExpression>, ScriptInteger, IScriptCodeElement<ScriptCodeExpression>>
        {
            public const string Name = "setTrap";
            private const string FirstParamName = "seh";
            private const string SecondParamName = "idx";
            private const string ThirdParamName = "body";

            public SetTrapBodyFunction()
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

        private static readonly AggregatedSlotCollection<ScriptSehExpressionFactory> StaticSlots = new AggregatedSlotCollection<ScriptSehExpressionFactory>
        {
             {ModifyFunction.Name, (owner, state) => LazyField<ModifyFunction, IScriptFunction>(ref owner.m_modify)},
             {GetTrapCountFunction.Name, (owner, state) => LazyField<GetTrapCountFunction, IScriptFunction>(ref owner.m_trapcount)},
             {GetFinallyBodyFunction.Name, (owner, state) => LazyField<GetFinallyBodyFunction, IScriptFunction>(ref owner.m_getfinally)},
             {GetTrapVarFunction.Name, (owner, state) => LazyField<GetTrapVarFunction, IScriptFunction>(ref owner.m_gettrapvar)},
             {GetTrapBodyFunction.Name, (owner, state) => LazyField<GetTrapBodyFunction, IScriptFunction>(ref owner.m_gettrapbody)},
             {SetFinallyBodyFunction.Name, (owner, state) => LazyField<SetFinallyBodyFunction, IScriptFunction>(ref owner.m_setfinally)},
             {SetTrapBodyFunction.Name, (owner, state) => LazyField<SetTrapBodyFunction, IScriptFunction>(ref owner.m_settrapbody)},
        };

        public new const string Name = "seh";

        private IScriptFunction m_modify;
        private IScriptFunction m_trapcount;
        private IScriptFunction m_getfinally;
        private IScriptFunction m_gettrapvar;
        private IScriptFunction m_gettrapbody;
        private IScriptFunction m_setfinally;
        private IScriptFunction m_settrapbody;

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
