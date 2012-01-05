using System;
using System.Linq.Expressions;

namespace DynamicScript.Compiler.Ast
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;

    /// <summary>
    /// Represents boolean contract.
    /// This type cannot be inherited.
    /// </summary>
    [ComVisible(false)]
    [Serializable]
    public sealed class ScriptCodeBooleanContractExpression : ScriptCodeBuiltInContractExpression, 
        IStaticContractBinding<ScriptCodeMetaContractExpression>,
        IWellKnownContractInfo
    {
        /// <summary>
        /// Represents compile-time type code.
        /// </summary>
        public const ScriptTypeCode TypeCode = ScriptTypeCode.Boolean;

        private ScriptCodeBooleanContractExpression()
            : base(Keyword.Boolean)
        {
        }

        /// <summary>
        /// Represents a singleton instance of the boolean contract expression.
        /// </summary>
        public static readonly ScriptCodeBooleanContractExpression Instance = new ScriptCodeBooleanContractExpression();

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
            return LinqHelpers.BodyOf<Func<ScriptCodeBooleanContractExpression>, MemberExpression>(() => Instance);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected override ScriptCodeExpression Clone()
        {
            return Instance;
        }

        ScriptTypeCode IWellKnownContractInfo.GetTypeCode()
        {
            return ScriptTypeCode.Boolean;
        }
    }
}
