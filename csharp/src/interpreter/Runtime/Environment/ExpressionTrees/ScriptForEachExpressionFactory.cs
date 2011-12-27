using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace DynamicScript.Runtime.Environment.ExpressionTrees
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;
    using Compiler.Ast;

    [ComVisible(false)]
    [Serializable]
    sealed class ScriptForEachExpressionFactory : ScriptLoopExpressionFactory<ScriptCodeForEachLoopExpression, ScriptForEachExpression>
    {
        #region Nested Types
        [ComVisible(false)]
        private sealed class ModifyFunction : ModifyFunctionBase
        {
            private const string SecondParamName = "loopVar";
            private const string ThirdParamName = "iterator";
            private const string FourthParamName = "grouping";
            private const string FifthParamName = "body";

            public ModifyFunction()
                : base(Instance, new ScriptFunctionContract.Parameter(SecondParamName, ScriptLoopVariableStatementFactory.Instance),
                new ScriptFunctionContract.Parameter(ThirdParamName, ScriptExpressionFactory.Instance),
                new ScriptFunctionContract.Parameter(FourthParamName, ScriptExpressionFactory.Instance),
                new ScriptFunctionContract.Parameter(FifthParamName, ScriptExpressionFactory.Instance))
            {
            }
        }

        [ComVisible(false)]
        private sealed class GetLoopVariableFunction : CodeElementPartProvider<IScriptStatement<ScriptCodeLoopWithVariableExpression.LoopVariable>>
        {
            public const string Name = "loopvar";
            public GetLoopVariableFunction()
                : base(Instance, ScriptVariableDeclarationFactory.Instance)
            {
            }

            protected override IScriptStatement<ScriptCodeLoopWithVariableExpression.LoopVariable> Invoke(ScriptCodeForEachLoopExpression element, InterpreterState state)
            {
                return element.Variable != null ? new ScriptLoopVariableStatement(element.Variable) : null;
            }
        }

        [ComVisible(false)]
        private sealed class GetCollectionFunction : CodeElementPartProvider<IScriptExpression<ScriptCodeExpression>>
        {
            public const string Name = "collection";

            public GetCollectionFunction()
                : base(Instance, ScriptExpressionFactory.Instance)
            {
            }

            protected override IScriptExpression<ScriptCodeExpression> Invoke(ScriptCodeForEachLoopExpression element, InterpreterState state)
            {
                return Convert(element.Iterator != null ? element.Iterator : null) as IScriptExpression<ScriptCodeExpression>;
            }
        }

        [ComVisible(false)]
        private sealed class GetGroupingFunction : GetGroupingFunctionBase
        {
            public GetGroupingFunction()
                : base(Instance)
            {
            }
        }

        [ComVisible(false)]
        private sealed class GetBodyFunction : GetBodyFunctionBase
        {
            public GetBodyFunction()
                : base(Instance)
            {
            }
        }
        #endregion

        private static readonly AggregatedSlotCollection<ScriptForEachExpressionFactory> StaticSlots = new AggregatedSlotCollection<ScriptForEachExpressionFactory>
        {
            {ModifyFunction.Name, (owner, state) => LazyField<ModifyFunction, IScriptFunction>(ref owner.m_modify)},
            {GetLoopVariableFunction.Name, (owner, state) => LazyField<GetLoopVariableFunction, IScriptFunction>(ref owner.m_loopvar)},
            {GetBodyFunction.Name, (owner, state) => LazyField<GetBodyFunction, IScriptFunction>(ref owner.m_getbody)},
            {GetGroupingFunction.Name, (owner, state) => LazyField<GetGroupingFunction, IScriptFunction>(ref owner.m_grouping)},
            {GetCollectionFunction.Name, (owner, state) => LazyField<GetCollectionFunction, IScriptFunction>(ref owner.m_getcollection)}
        };

        public new const string Name = "foreach";

        private IScriptFunction m_modify;
        private IScriptFunction m_loopvar;
        private IScriptFunction m_getbody;
        private IScriptFunction m_grouping;
        private IScriptFunction m_getcollection;

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
                m_loopvar =
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

        public override ICollection<string> Slots
        {
            get { return StaticSlots.Keys; }
        }

        protected override IScriptObject GetSlotMetadata(string slotName, InterpreterState state)
        {
            return StaticSlots.GetSlotMetadata(this, slotName, state);
        }

        public override IScriptObject this[string slotName, InterpreterState state]
        {
            get { return StaticSlots.GetValue(this, slotName, state); }
            set { StaticSlots.SetValue(this, slotName, value, state); }
        }
    }
}
