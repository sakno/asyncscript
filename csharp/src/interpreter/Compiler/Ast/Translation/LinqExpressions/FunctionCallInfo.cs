using System;

namespace DynamicScript.Compiler.Ast.Translation.LinqExpressions
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;
    using LambdaExpression = System.Linq.Expressions.LambdaExpression;

    /// <summary>
    /// Represents function call info that is used to inline its invocation.
    /// This class cannot be inherited.
    /// </summary>
    [ComVisible(false)]
    public sealed class FunctionCallInfo
    {
        /// <summary>
        /// Represents function implementation.
        /// </summary>
        public readonly LambdaExpression FunctionImpl;

        /// <summary>
        /// Represents signature of the function.
        /// </summary>
        public readonly ScriptCodeActionContractExpression Signature;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="implementation"></param>
        /// <param name="sig"></param>
        public FunctionCallInfo(LambdaExpression implementation, ScriptCodeActionContractExpression sig)
        {
            FunctionImpl = implementation;
            Signature = sig;
        }
    }
}
