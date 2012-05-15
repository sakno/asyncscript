using System;
using System.Linq.Expressions;

namespace DynamicScript.Runtime.Environment
{
    using Compiler;
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;

    /// <summary>
    /// Represents an exception that is thrown when DynamicScript program attempts to use unary, binary or application
    /// operator with unsupported operands.
    /// This class cannot be inherited.
    /// </summary>
    [ComVisible(false)]
    public sealed class UnsupportedOperationException: RuntimeException
    {
        /// <summary>
        /// Initializes a new instance of the exception.
        /// </summary>
        /// <param name="state">Internal interpreter state.</param>
        public UnsupportedOperationException(InterpreterState state)
            : base(ErrorMessages.UnsupportedOperation, InterpreterErrorCode.UnsupportedOperation, state)
        {
        }

        internal static Expression Throw(ParameterExpression state)
        {
            var ctor = LinqHelpers.BodyOf<InterpreterState, UnsupportedOperationException, NewExpression>(s => new UnsupportedOperationException(s));
            return Expression.Block(Expression.Throw(ctor.Update(new[] { state })), ScriptObject.Null);
        }
    }
}
