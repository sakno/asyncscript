using System;
using System.Linq.Expressions;

namespace DynamicScript.Compiler.Ast
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;

    /// <summary>
    /// Represents an expression that references 'finset' predefined type.
    /// This class cannot be inherited.
    /// </summary>
    [Serializable]
    [ComVisible(false)]
    public sealed class ScriptCodeDimensionalContractExpression : ScriptCodeBuiltInContractExpression, IStaticContractBinding<ScriptCodeMetaContractExpression>
    {
        private ScriptCodeDimensionalContractExpression()
            : base(Keyword.Dimensional)
        {
        }

        /// <summary>
        /// Represents a singleton instance of the expression.
        /// </summary>
        public static readonly ScriptCodeDimensionalContractExpression Instance = new ScriptCodeDimensionalContractExpression();

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
            return LinqHelpers.BodyOf<Func<ScriptCodeDimensionalContractExpression>, MemberExpression>(() => Instance);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected override ScriptCodeExpression Clone()
        {
            return Instance;
        }
    }
}
