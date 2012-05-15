using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace DynamicScript.Runtime.Environment.ExpressionTrees
{
    using Compiler.Ast;
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;

    [ComVisible(false)]
    sealed class ScriptUnaryExpressionFactory : ScriptExpressionFactory<ScriptCodeUnaryOperatorExpression, ScriptUnaryExpression>
    {
        #region Nested Types
        [ComVisible(false)]
        private sealed class ModifyFunction : ModifyFunctionBase
        {
            private const string SecondParamName = "operand";
            private const string ThirdParamName = "operator";

            public ModifyFunction()
                : base( Instance,new ScriptFunctionContract.Parameter(SecondParamName, ScriptExpressionFactory.Instance),new ScriptFunctionContract.Parameter(ThirdParamName, ScriptStringContract.Instance))
            {
            }
        }

        [ComVisible(false)]
        private sealed class CreateInvokerFunction : ScriptFunc<ScriptString>
        {
            public const string Name = "invoker";
            private const string FirstParamName = "operator";

            public CreateInvokerFunction()
                : base(FirstParamName, ScriptStringContract.Instance, ScriptSuperContract.Instance)
            {
            }

            protected override IScriptObject Invoke(ScriptString @operator, InterpreterState state)
            {
                var op = Parser.ParseOperator(@operator ?? ScriptString.Empty, false);
                IScriptObject result = op is ScriptCodeUnaryOperatorType ? new UnaryOperatorInvoker((ScriptCodeUnaryOperatorType)op) : null;
                return result ?? Void;
            }
        }

        [ComVisible(false)]
        private sealed class GetOperandFunction : CodeElementPartProvider<IScriptExpression<ScriptCodeExpression>>
        {
            public const string Name = "operand";
            public GetOperandFunction()
                : base(Instance, ScriptExpressionFactory.Instance)
            {
            }

            protected override IScriptExpression<ScriptCodeExpression> Invoke(ScriptCodeUnaryOperatorExpression element, InterpreterState state)
            {
                return Convert(element.Operand) as IScriptExpression<ScriptCodeExpression>;
            }
        }

        [ComVisible(false)]
        private sealed class GetOperatorFunction : CodeElementPartProvider<ScriptString>
        {
            public const string Name = "operator";
            private const string FirstParamName = "unop";

            public GetOperatorFunction()
                : base( Instance, ScriptStringContract.Instance)
            {
            }

            protected override ScriptString Invoke(ScriptCodeUnaryOperatorExpression element, InterpreterState state)
            {
                return ScriptCodeUnaryOperatorExpression.ToString(element.Operator);
            }
        }
        #endregion

        private static readonly AggregatedSlotCollection<ScriptUnaryExpressionFactory> StaticSlots = new AggregatedSlotCollection<ScriptUnaryExpressionFactory>
        {
             {ModifyFunction.Name, (owner, state) => LazyField<ModifyFunction, IScriptFunction>(ref owner.m_modify)},
             {CreateInvokerFunction.Name, (owner, state) => LazyField<CreateInvokerFunction, IScriptFunction>(ref owner.m_invoker)},
             {GetOperatorFunction.Name, (owner, state) => LazyField<GetOperatorFunction, IScriptFunction>(ref owner.m_operator)},
             {GetOperandFunction.Name, (owner, state) => LazyField<GetOperandFunction, IScriptFunction>(ref owner.m_operand)}
        };

        private ScriptUnaryExpressionFactory(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        public new const string Name = "unop";

        private IScriptFunction m_modify;
        private IScriptFunction m_invoker;
        private IScriptFunction m_operator;
        private IScriptFunction m_operand;

        private ScriptUnaryExpressionFactory()
            : base(Name)
        {
        }

        public static readonly ScriptUnaryExpressionFactory Instance = new ScriptUnaryExpressionFactory();

        public static ScriptUnaryExpression CreateExpression(IScriptObject operand, ScriptCodeUnaryOperatorType @operator)
        {
            var expression = ScriptUnaryExpression.CreateExpression(operand, @operator);
            return expression != null ? new ScriptUnaryExpression(expression) : null;
        }

        public static ScriptUnaryExpression CreateExpression(IScriptObject operand, ScriptString @operator)
        {
            var expression = ScriptUnaryExpression.CreateExpression(operand, @operator);
            return expression != null ? new ScriptUnaryExpression(expression) : null;
        }

        public override ScriptUnaryExpression CreateCodeElement(IList<IScriptObject> args, InterpreterState state)
        {
            return args.Count == 2 ? CreateExpression(args[0], args[1] as ScriptString) : null;
        }

        public override void Clear()
        {
            m_invoker =
                m_modify =
                m_operand =
                m_operator = null;
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
