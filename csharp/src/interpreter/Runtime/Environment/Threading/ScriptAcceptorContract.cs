using System;
using System.Reflection;
using System.Linq.Expressions;
using System.Collections.Generic;

namespace DynamicScript.Runtime.Environment.Threading
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;
    using Enumerable = System.Linq.Enumerable;

    /// <summary>
    /// Represents contract of task asynchronous result.
    /// This class cannot be inherited.
    /// </summary>
    [ComVisible(false)]
    public sealed class ScriptAcceptorContract : ScriptFunctionContract
    {
        /// <summary>
        /// Initializes a new acceptor contract.
        /// </summary>
        /// <param name="resultType">Type of the accepted object.</param>
        public ScriptAcceptorContract(IScriptContract resultType = null)
            : base(CreateParameters(resultType), Void)
        {
        }

        private static IEnumerable<Parameter> CreateParameters(IScriptContract resultType)
        {
            yield return new Parameter("error", ScriptSuperContract.Instance);
            if (resultType != null) yield return new Parameter("result", resultType);
        }

        internal static NewExpression New(Expression resultType, ParameterExpression state)
        {
            resultType = RequiresContract(AsRightSide(resultType, state));
            var ctor = LinqHelpers.BodyOf<IScriptContract, ScriptAcceptorContract, NewExpression>(rt => new ScriptAcceptorContract(rt));
            return ctor.Update(new[] { resultType });
        }
    }
}
