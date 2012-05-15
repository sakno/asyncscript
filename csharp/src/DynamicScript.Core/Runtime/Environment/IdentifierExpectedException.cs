using System;
using System.Linq.Expressions;

namespace DynamicScript.Runtime.Environment
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;

    /// <summary>
    /// Represents an exception ocurred when one of the operands of '.' or '::' operator is not identifier name.
    /// This class cannot be inherited.
    /// </summary>
    [ComVisible(false)]
    
    public sealed class IdentifierExpectedException: RuntimeException
    {
        internal IdentifierExpectedException(InterpreterState state)
            : base(ErrorMessages.IdentifierExpected, InterpreterErrorCode.IdentifierExpected, state)
        {
        }

        internal static UnaryExpression Bind(ParameterExpression stateVar)
        {
            var ctor = LinqHelpers.BodyOf<InterpreterState, IdentifierExpectedException, NewExpression>(state => new IdentifierExpectedException(state));
            return Expression.Throw(ctor.Update(new[] { stateVar }));
        }
    }
}
