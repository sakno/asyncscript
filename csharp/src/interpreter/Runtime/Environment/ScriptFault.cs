using System;
using System.Linq.Expressions;
using System.Diagnostics;

namespace DynamicScript.Runtime.Environment
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;
    using RuntimeWrappedException = System.Runtime.CompilerServices.RuntimeWrappedException;

    /// <summary>
    /// Represents .NET-compliant exception that wraps script fault.
    /// This class cannot be inherited.
    /// </summary>
    [CLSCompliant(false)]
    [ComVisible(false)]
    public sealed class ScriptFault: RuntimeException
    {
        private readonly IScriptObject m_fault;

        /// <summary>
        /// Initializes script fault wrapper.
        /// </summary>
        /// <param name="faultObj">The object returned from fault.</param>
        /// <param name="state">Internal interpreter state.</param>
        public ScriptFault(IScriptObject faultObj, InterpreterState state)
            : base(string.Format(ErrorMessages.ScriptFault, faultObj, state), InterpreterErrorCode.Internal, state)
        {
            m_fault = faultObj;
        }

        /// <summary>
        /// Initializes a new DynamicScript exception.
        /// </summary>
        /// <param name="message">The error message.</param>
        /// <param name="state">Internal interpreter state.</param>
        public ScriptFault(string message, InterpreterState state)
            : this(ScriptObject.Convert(message), state)
        {
        }

        /// <summary>
        /// Gets fault result.
        /// </summary>
        public IScriptObject Fault
        {
            get { return m_fault; }
        }

        internal static NewExpression New(Expression faultObj, ParameterExpression stateVar)
        {
            var ctor = LinqHelpers.BodyOf<IScriptObject, InterpreterState, ScriptFault, NewExpression>((f, s) => new ScriptFault(f, s));
            return ctor.Update(new Expression[] { faultObj, stateVar });
        }

        internal static UnaryExpression Throw(Expression faultObj, ParameterExpression stateVar)
        {
            //var @throw = LinqHelpers.BodyOf<IScriptObject, InterpreterState, IScriptObject, MethodCallExpression>((f, s) => Throw(f, s));
            //return @throw.Update(null, new[] { faultObj, stateVar });
            return Expression.Throw(New(faultObj, stateVar), typeof(ScriptFault));
        }

        private static IScriptObject Unwrap(object e)
        {
            if (e is ScriptFault)
                return ((ScriptFault)e).Fault;
            else if (e is RuntimeWrappedException)
                return Unwrap(((RuntimeWrappedException)e).WrappedException);
            else if (e is Exception)
                return new ScriptWrappedException((Exception)e);
            else
            {
                var scriptObject = default(IScriptObject);
                return ScriptObject.TryConvert(e, out scriptObject) ? scriptObject : ScriptObject.Void;
            }
        }

        /// <summary>
        /// Catches the exception and saves it to the slot variable.
        /// </summary>
        /// <param name="exception"></param>
        /// <param name="catchVar"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        public static bool Catch(object exception, IRuntimeSlot catchVar, InterpreterState state)
        {
            return RuntimeHelpers.TrySetValue(catchVar, Unwrap(exception), state);
        }

        internal static MethodCallExpression BindCatch(ParameterExpression exception, ParameterExpression catchVar, ParameterExpression stateVar)
        {
            var catchMethod = LinqHelpers.BodyOf<object, IRuntimeSlot, InterpreterState, bool, MethodCallExpression>((e, v, s) => Catch(e, v, s));
            return catchMethod.Update(null, new Expression[] { exception, catchVar, stateVar });
        }
    }
}
