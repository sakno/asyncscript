using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace DynamicScript.Runtime.Environment.ExpressionTrees
{
    using Compiler.Ast;
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;

    [ComVisible(false)]
    sealed class ScriptArrayContractExpressionFactory : ScriptExpressionFactory<ScriptCodeArrayContractExpression, ScriptArrayContractExpression>
    {
        #region Nested Types
        [ComVisible(false)]
        private sealed class GetElementContractFunction : CodeElementPartProvider<IScriptExpression<ScriptCodeExpression>>
        {
            public const string Name = "elem";

            public GetElementContractFunction()
                : base(Instance, ScriptExpressionFactory.Instance)
            {
            }

            protected override IScriptExpression<ScriptCodeExpression> Invoke(ScriptCodeArrayContractExpression expression, InterpreterState state)
            {
                return Convert(expression.ElementContract) as IScriptExpression<ScriptCodeExpression>;
            }
        }

        [ComVisible(false)]
        private sealed class GetRankFunction : CodeElementPartProvider<ScriptInteger>
        {
            public const string Name = "rank";

            public GetRankFunction()
                : base(Instance, ScriptIntegerContract.Instance)
            {
            }

            protected override ScriptInteger Invoke(ScriptCodeArrayContractExpression expression, InterpreterState state)
            {
                return expression.Rank;
            }
        }

        [ComVisible(false)]
        private sealed class ModifyFunction : ModifyFunctionBase
        {
            private const string SecondParamName = "contract";
            private const string ThirdParamName = "rank";

            public ModifyFunction()
                : base(Instance, new ScriptFunctionContract.Parameter(SecondParamName, ScriptExpressionFactory.Instance), new ScriptFunctionContract.Parameter(ThirdParamName, ScriptIntegerContract.Instance))
            {
            }
        }
        #endregion

        private static readonly AggregatedSlotCollection<ScriptArrayContractExpressionFactory> StaticSlots = new AggregatedSlotCollection<ScriptArrayContractExpressionFactory>
        {
            {ModifyFunction.Name, (owner, state) => LazyField<ModifyFunction, IScriptFunction>(ref owner.m_modify)},
            {GetElementContractFunction.Name, (owner, state) => LazyField<GetElementContractFunction, IScriptFunction>(ref owner.m_elem)},
            {GetRankFunction.Name, (owner, state) => LazyField<GetRankFunction, IScriptFunction>(ref owner.m_rank)}
        };

        public new const string Name = "array_type";
        private IScriptFunction m_elem;
        private IScriptFunction m_rank;
        private IScriptFunction m_modify;

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


        public override void Clear()
        {
            m_elem = m_modify = m_rank = null;
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

        public override ICollection<string> Slots
        {
            get { return StaticSlots.Keys; }
        }
    }
}
