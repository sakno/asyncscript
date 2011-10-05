using System;
using System.Linq.Expressions;

namespace DynamicScript.Compiler.Ast
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;

    /// <summary>
    /// Represents a contract that is used to identify action objects.
    /// This class cannot be inherited.
    /// </summary>
    [Serializable]
    [ComVisible(false)]
    public sealed class ScriptCodeCallableContractExpression : ScriptCodeBuiltInContractExpression, IStaticContractBinding<ScriptCodeMetaContractExpression>
    {
        private ScriptCodeCallableContractExpression()
            : base(Keyword.Callable)
        {
        }

        /// <summary>
        /// Represents a singelton instance of the action contract expression.
        /// </summary>
        public static readonly ScriptCodeCallableContractExpression Instance = new ScriptCodeCallableContractExpression();

        /// <summary>
        /// Gets static contract binding.
        /// </summary>
        public ScriptCodeMetaContractExpression Contract
        {
            get { return ScriptCodeMetaContractExpression.Instance; }
        }

        /// <summary>
        /// Returns a LINQ expression that produces this object.
        /// </summary>
        /// <returns></returns>
        protected override Expression Restore()
        {
            return LinqHelpers.BodyOf<Func<ScriptCodeCallableContractExpression>, MemberExpression>(() => Instance);
        }

        /// <summary>
        /// Returns an object stored in <see cref="Instance"/> field.
        /// </summary>
        /// <returns></returns>
        protected override ScriptCodeExpression Clone()
        {
            return Instance;
        }
    }
}
