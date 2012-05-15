using System;
using System.Linq.Expressions;

namespace DynamicScript.Compiler.Ast
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;

    /// <summary>
    /// Represents an expression that references 'object' predefined type.
    /// This class cannot be inherited.
    /// </summary>
    [Serializable]
    [ComVisible(false)]
    public sealed class ScriptCodeSuperContractExpression: ScriptCodeBuiltInContractExpression, IStaticContractBinding<ScriptCodeMetaContractExpression>
    {
        private ScriptCodeSuperContractExpression()
            : base(Keyword.Object)
        {
        }

        /// <summary>
        /// Represents a single instance of the 'object' predefined type.
        /// </summary>
        public static readonly ScriptCodeSuperContractExpression Instance = new ScriptCodeSuperContractExpression();

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
            return LinqHelpers.BodyOf<Func<ScriptCodeSuperContractExpression>, MemberExpression>(() => Instance);
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
