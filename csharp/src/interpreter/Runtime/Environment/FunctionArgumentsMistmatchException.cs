using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace DynamicScript.Runtime.Environment
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;
    
    /// <summary>
    /// Represents an exception occured when an action cannot be called with the specified set of arguments.
    /// This class cannot be inherited.
    /// </summary>
    [ComVisible(false)]
    
    public sealed class FunctionArgumentsMistmatchException: RuntimeException
    {
        internal FunctionArgumentsMistmatchException(InterpreterState state)
            : base(ErrorMessages.ArgumentMistmatch, InterpreterErrorCode.ArgumentMistmatch, state)
        {
        }

        internal static Expression Throw(ParameterExpression state)
        {
            var ctor = LinqHelpers.BodyOf<InterpreterState, FunctionArgumentsMistmatchException, NewExpression>(s => new FunctionArgumentsMistmatchException(s));
            return Expression.Block(Expression.Throw(ctor.Update(new[] { state })), ScriptObject.Null);
        }
    }
}
