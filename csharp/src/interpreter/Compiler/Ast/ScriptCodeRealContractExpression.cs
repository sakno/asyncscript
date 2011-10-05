using System;
using System.Linq.Expressions;

namespace DynamicScript.Compiler.Ast
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;

    /// <summary>
    /// Represents floating-point contract expression.
    /// This class cannot be inherited.
    /// </summary>
    
    [ComVisible(false)]
    [Serializable]
    public sealed class ScriptCodeRealContractExpression: ScriptCodeBuiltInContractExpression, IStaticContractBinding<ScriptCodeMetaContractExpression>
    {
        private ScriptCodeRealContractExpression()
            : base(Keyword.Real)
        {
        }

        /// <summary>
        /// Represents a singleton instance of the floating-point contract expression.
        /// </summary>
        public static readonly ScriptCodeRealContractExpression Instance = new ScriptCodeRealContractExpression();

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
            return LinqHelpers.BodyOf<Func<ScriptCodeRealContractExpression>, MemberExpression>(() => Instance);
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
