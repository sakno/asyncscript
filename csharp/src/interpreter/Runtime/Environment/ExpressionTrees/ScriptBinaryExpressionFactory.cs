using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace DynamicScript.Runtime.Environment.ExpressionTrees
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;
    using Compiler.Ast;

    [ComVisible(false)]
    [Serializable]
    sealed class ScriptBinaryExpressionFactory : ScriptExpressionFactory<ScriptCodeBinaryOperatorExpression, ScriptBinaryExpression>
    {
        #region Nested Types
        [ComVisible(false)]
        private sealed class CreateInvokerFunction : ScriptFunc<ScriptString>
        {
            public const string Name = "invoker";
            private const string FirstParamName = "oper";

            public CreateInvokerFunction()
                : base(FirstParamName, ScriptStringContract.Instance, ScriptSuperContract.Instance)
            {
            }

            protected override IScriptObject Invoke(ScriptString @operator, InterpreterState state)
            {
                var op = Parser.ParseOperator(@operator ?? ScriptString.Empty, true);
                IScriptObject result = op is ScriptCodeBinaryOperatorType ? new BinaryOperatorInvoker((ScriptCodeBinaryOperatorType)op) : null;
                return result ?? Void;
            }
        }

        [ComVisible(false)]
        private sealed class GetLeftOperandFunction : CodeElementPartProvider<IScriptExpression<ScriptCodeExpression>>
        {
            public const string Name = "left";

            public GetLeftOperandFunction()
                : base(Instance, ScriptExpressionFactory.Instance)
            {
            }

            protected override IScriptExpression<ScriptCodeExpression> Invoke(ScriptCodeBinaryOperatorExpression element, InterpreterState state)
            {
                return Convert(element.Left) as IScriptExpression<ScriptCodeExpression>;
            }
        }

        [ComVisible(false)]
        private sealed class ModifyFunction : ModifyFunctionBase
        {
            private const string SecondParamName = "left";
            private const string ThirdParamName = "operator";
            private const string FourthParamName = "right";

            public ModifyFunction()
                : base(Instance,
                new ScriptFunctionContract.Parameter(SecondParamName, ScriptExpressionFactory.Instance),
                new ScriptFunctionContract.Parameter(ThirdParamName, ScriptStringContract.Instance),
                new ScriptFunctionContract.Parameter(FourthParamName, ScriptExpressionFactory.Instance))
            {
            }
        }

        [ComVisible(false)]
        private sealed class GetRightOperandAction : CodeElementPartProvider<IScriptExpression<ScriptCodeExpression>>
        {
            public const string Name = "right";

            public GetRightOperandAction()
                : base(Instance, ScriptExpressionFactory.Instance)
            {
            }

            protected override IScriptExpression<ScriptCodeExpression> Invoke(ScriptCodeBinaryOperatorExpression element, InterpreterState state)
            {
                return Convert(element.Right) as IScriptExpression<ScriptCodeExpression>;
            }
        }

        [ComVisible(false)]
        private sealed class GetOperatorAction : CodeElementPartProvider<ScriptString>
        {
            public const string Name = "operator";

            public GetOperatorAction()
                : base(Instance, ScriptStringContract.Instance)
            {
            }

            protected override ScriptString Invoke(ScriptCodeBinaryOperatorExpression element, InterpreterState state)
            {
                return ScriptCodeBinaryOperatorExpression.ToString(element.Operator);
            }
        }
        #endregion

        public new const string Name = "binop";

        private static readonly AggregatedSlotCollection<ScriptBinaryExpressionFactory> StaticSlots = new AggregatedSlotCollection<ScriptBinaryExpressionFactory>
        {
            {CreateInvokerFunction.Name, (owner, state) => LazyField<CreateInvokerFunction, IScriptFunction>(ref owner.m_invoker)},
            {GetLeftOperandFunction.Name, (owner, state) => LazyField<GetLeftOperandFunction, IScriptFunction>(ref owner.m_left)},
            {GetRightOperandAction.Name, (owner, state) => LazyField<GetRightOperandAction, IScriptFunction>(ref owner.m_right)},
            {ModifyFunction.Name, (owner, state) => LazyField<ModifyFunction, IScriptFunction>(ref owner.m_modify)}
        };

        private IScriptFunction m_invoker;
        private IScriptFunction m_left;
        private IScriptFunction m_right;
        private IScriptFunction m_operator;
        private IScriptFunction m_modify;

        private ScriptBinaryExpressionFactory(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        private ScriptBinaryExpressionFactory()
            : base(Name)
        {
        }

        /// <summary>
        /// Represents a singleton instance of this contract.
        /// </summary>
        public static readonly ScriptBinaryExpressionFactory Instance = new ScriptBinaryExpressionFactory();

        public static ScriptBinaryExpression CreateExpression(IScriptObject left, ScriptCodeBinaryOperatorType @operator, IScriptObject right)
        {
            var expression = ScriptBinaryExpression.CreateExpression(left, @operator, right);
            return expression != null ? new ScriptBinaryExpression(expression) : null;
        }

        public static ScriptBinaryExpression CreateExpression(IScriptObject left, ScriptString @operator, IScriptObject right)
        {
            var expression = ScriptBinaryExpression.CreateExpression(left, @operator, right);
            return expression != null ? new ScriptBinaryExpression(expression) : null;
        }

        public override ScriptBinaryExpression CreateCodeElement(IList<IScriptObject> args, InterpreterState state)
        {
            return args.Count == 3 ? CreateExpression(args[0], args[1] as ScriptString, args[2]) : null;
        }

        public override void Clear()
        {
            m_invoker =
                m_left =
                m_modify =
                m_operator =
                m_right = null;
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
