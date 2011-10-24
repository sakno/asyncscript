using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace DynamicScript.Runtime.Environment.ExpressionTrees
{
    using Compiler.Ast;
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;

    [ComVisible(false)]
    sealed class ScriptUnaryExpressionFactory : ScriptExpressionFactory<ScriptCodeUnaryOperatorExpression, ScriptUnaryExpression>, IUnaryExpressionFactorySlots
    {
        #region Nested Types
        [ComVisible(false)]
        private sealed class ModifyAction : ModifyActionBase
        {
            private const string SecondParamName = "operand";
            private const string ThirdParamName = "operator";

            public ModifyAction()
                : base( Instance,new ScriptActionContract.Parameter(SecondParamName, ScriptExpressionFactory.Instance),new ScriptActionContract.Parameter(ThirdParamName, ScriptStringContract.Instance))
            {
            }
        }

        [ComVisible(false)]
        private sealed class CreateInvokerAction : ScriptFunc<ScriptString>
        {
            private const string FirstParamName = "operator";

            public CreateInvokerAction()
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
        private sealed class GetOperandAction : CodeElementPartProvider<IScriptExpression<ScriptCodeExpression>>
        {
            public GetOperandAction()
                : base(Instance, ScriptExpressionFactory.Instance)
            {
            }

            protected override IScriptExpression<ScriptCodeExpression> Invoke(ScriptCodeUnaryOperatorExpression element, InterpreterState state)
            {
                return Convert(element.Operand) as IScriptExpression<ScriptCodeExpression>;
            }
        }

        [ComVisible(false)]
        private sealed class GetOperatorAction : CodeElementPartProvider<ScriptString>
        {
            private const string FirstParamName = "unop";

            public GetOperatorAction()
                : base( Instance, ScriptStringContract.Instance)
            {
            }

            protected override ScriptString Invoke(ScriptCodeUnaryOperatorExpression element, InterpreterState state)
            {
                return ScriptCodeUnaryOperatorExpression.ToString(element.Operator);
            }
        }
        #endregion

        private ScriptUnaryExpressionFactory(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        public new const string Name = "unop";

        private IRuntimeSlot m_modify;
        private IRuntimeSlot m_invoker;
        private IRuntimeSlot m_operator;
        private IRuntimeSlot m_operand;

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

        #region Runtime Slots

        protected override IRuntimeSlot Modify
        {
            get { return CacheConst<ModifyAction>(ref m_modify); }
        }

        IRuntimeSlot IUnaryExpressionFactorySlots.Invoker
        {
            get { return CacheConst < CreateInvokerAction>(ref m_invoker); }
        }

        IRuntimeSlot IUnaryExpressionFactorySlots.Operand
        {
            get { return CacheConst<GetOperandAction>(ref m_operand); }
        }

        IRuntimeSlot IUnaryExpressionFactorySlots.Operator
        {
            get { return CacheConst<GetOperatorAction>(ref m_operator); }
        }

        #endregion
    }
}
