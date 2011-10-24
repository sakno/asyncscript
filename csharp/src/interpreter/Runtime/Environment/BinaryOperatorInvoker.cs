using System;
using System.Linq.Expressions;

namespace DynamicScript.Runtime.Environment
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;
    using ScriptCodeBinaryOperatorType = Compiler.Ast.ScriptCodeBinaryOperatorType;

    /// <summary>
    /// Represents action that implements binary operation invocation.
    /// This class cannot be inherited.
    /// </summary>
    [ComVisible(false)]
    public sealed class BinaryOperatorInvoker: ScriptFunc<IScriptObject, IScriptObject>, IBinaryOperatorInvoker
    {
        private const string FirstParamName = "left";
        private const string SecondParamName = "right";
        /// <summary>
        /// Represents binary operator.
        /// </summary>
        public readonly ScriptCodeBinaryOperatorType Operator;

        /// <summary>
        /// Initializes a new binary operator invoker.
        /// </summary>
        /// <param name="operator">The operator to be invoked.</param>
        public BinaryOperatorInvoker(ScriptCodeBinaryOperatorType @operator)
            : base(FirstParamName, ScriptSuperContract.Instance, SecondParamName, ScriptSuperContract.Instance, ScriptSuperContract.Instance)
        {
            Operator = @operator;
        }

        internal static NewExpression Bind(ConstantExpression @operator)
        {
            var ctor = LinqHelpers.BodyOf<ScriptCodeBinaryOperatorType, BinaryOperatorInvoker, NewExpression>(op => new BinaryOperatorInvoker(op));
            return ctor.Update(new[] { @operator });
        }

        internal static NewExpression New(ScriptCodeBinaryOperatorType @operator)
        {
            return Bind(LinqHelpers.Constant(@operator));
        }

        /// <summary>
        /// Invokes a binary operator.
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        protected override IScriptObject Invoke(IScriptObject left, IScriptObject right, InterpreterState state)
        {
            return left.BinaryOperation(Operator, right, state);
        }

        ScriptCodeBinaryOperatorType IBinaryOperatorInvoker.Operator
        {
            get { return Operator; }
        }
    }
}
