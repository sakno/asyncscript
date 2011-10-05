using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace DynamicScript.Runtime.Environment.ExpressionTrees
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;
    using Compiler.Ast;

    [ComVisible(false)]
    [Serializable]
    sealed class ScriptBinaryExpressionFactory : ScriptExpressionFactory<ScriptCodeBinaryOperatorExpression, ScriptBinaryExpression>, IBinaryExpressionFactorySlots
    {
        #region Nested Types
        [ComVisible(false)]
        private sealed class CreateInvokerAction : ScriptFunc<ScriptString>
        {
            private const string FirstParamName = "oper";

            public CreateInvokerAction()
                : base(FirstParamName, ScriptStringContract.Instance, ScriptSuperContract.Instance)
            {
            }

            protected override IScriptObject Invoke(InvocationContext ctx, ScriptString @operator)
            {
                var op = Parser.ParseOperator(@operator ?? ScriptString.Empty, true);
                IScriptObject result = op is ScriptCodeBinaryOperatorType ? new BinaryOperatorInvoker((ScriptCodeBinaryOperatorType)op) : null;
                return result ?? Void;
            }
        }

        [ComVisible(false)]
        private sealed class GetLeftOperandAction : CodeElementPartProvider<IScriptExpression<ScriptCodeExpression>>
        {
            public GetLeftOperandAction()
                : base(Instance, ScriptExpressionFactory.Instance)
            {
            }

            protected override IScriptExpression<ScriptCodeExpression> Invoke(ScriptCodeBinaryOperatorExpression element, InterpreterState state)
            {
                return Convert(element.Left) as IScriptExpression<ScriptCodeExpression>;
            }
        }

        [ComVisible(false)]
        private sealed class ModifyAction : ModifyActionBase
        {
            private const string SecondParamName = "left";
            private const string ThirdParamName = "operator";
            private const string FourthParamName = "right";

            public ModifyAction()
                : base(Instance,
                new ScriptActionContract.Parameter(SecondParamName, ScriptExpressionFactory.Instance),
                new ScriptActionContract.Parameter(ThirdParamName, ScriptStringContract.Instance),
                new ScriptActionContract.Parameter(FourthParamName, ScriptExpressionFactory.Instance))
            {
            }
        }

        [ComVisible(false)]
        private sealed class GetRightOperandAction : CodeElementPartProvider<IScriptExpression<ScriptCodeExpression>>
        {
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

        private IRuntimeSlot m_invoker;
        private IRuntimeSlot m_left;
        private IRuntimeSlot m_right;
        private IRuntimeSlot m_operator;
        private IRuntimeSlot m_modify;

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

        #region Runtime Slots

        IRuntimeSlot IBinaryExpressionFactorySlots.Invoker
        {
            get { return CacheConst<CreateInvokerAction>(ref m_invoker); }
        }

        IRuntimeSlot IBinaryExpressionFactorySlots.Left
        {
            get { return CacheConst<GetLeftOperandAction>(ref m_left); }
        }

        IRuntimeSlot IBinaryExpressionFactorySlots.Right
        {
            get { return CacheConst<GetRightOperandAction>(ref m_right); }
        }

        IRuntimeSlot IBinaryExpressionFactorySlots.Operator
        {
            get { return CacheConst<GetOperatorAction>(ref m_operator); }
        }

        protected override IRuntimeSlot Modify
        {
            get { return CacheConst<ModifyAction>(ref m_modify); }
        }

        #endregion
    }
}
