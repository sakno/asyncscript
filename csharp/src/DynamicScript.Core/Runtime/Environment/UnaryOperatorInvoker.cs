using System;

namespace DynamicScript.Runtime.Environment
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;
    using ScriptCodeUnaryOperatorType = Compiler.Ast.ScriptCodeUnaryOperatorType;

    /// <summary>
    /// Represents unary operator as action.
    /// This class cannot be inherited.
    /// </summary>
    [ComVisible(false)]
    public sealed class UnaryOperatorInvoker: ScriptFunc<IScriptObject>, IUnaryOperatorInvoker
    {
        private const string FirstParamName = "operand";
        /// <summary>
        /// Represents unary operator.
        /// </summary>
        public readonly ScriptCodeUnaryOperatorType Operator;

        /// <summary>
        /// Initializes a new unary operator invoker.
        /// </summary>
        /// <param name="operator">Operator type.</param>
        public UnaryOperatorInvoker(ScriptCodeUnaryOperatorType @operator)
            : base(FirstParamName, ScriptSuperContract.Instance, ScriptSuperContract.Instance)
        {
            Operator = @operator;
        }

        /// <summary>
        /// Executes unary operator.
        /// </summary>
        /// <param name="state">Internal interpreter state.</param>
        /// <param name="operand"></param>
        /// <returns></returns>
        protected override IScriptObject Invoke(IScriptObject operand, InterpreterState state)
        {
            return operand.UnaryOperation(Operator, state);
        }

        ScriptCodeUnaryOperatorType IUnaryOperatorInvoker.Operator
        {
            get { return Operator; }
        }
    }
}
