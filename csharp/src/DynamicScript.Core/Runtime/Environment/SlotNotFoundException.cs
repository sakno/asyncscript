using System;
using System.Linq.Expressions;

namespace DynamicScript.Runtime.Environment
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;

    /// <summary>
    /// Represents an exception occured when slot with the specified name is not existed in the object.
    /// This class cannot be inherited.
    /// </summary>
    [ComVisible(false)]
    
    public sealed class SlotNotFoundException: RuntimeException
    {
        /// <summary>
        /// Initializes a new instance of the exception.
        /// </summary>
        /// <param name="slotName">The name of the missing slot.</param>
        /// <param name="state">Internal interpreter state.</param>
        public SlotNotFoundException(string slotName, InterpreterState state)
            : base(String.Format(ErrorMessages.UndeclaredIdentifier, slotName), InterpreterErrorCode.UndeclaredIdentifier, state)
        {
        }

        internal static Expression Bind(ConstantExpression slotName, ParameterExpression stateVar)
        {
            var ctor = LinqHelpers.BodyOf<string, InterpreterState, SlotNotFoundException, NewExpression>((name, state) => new SlotNotFoundException(name, state));
            return Expression.Block(ctor.Update(new Expression[] { slotName, stateVar }), ScriptObject.MakeVoid());
        }

        internal static Expression Bind(string slotName, ParameterExpression stateVar)
        {
            return Bind(LinqHelpers.Constant(slotName), stateVar);
        }
    }
}
