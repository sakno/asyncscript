using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace DynamicScript.Compiler.Ast
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;

    /// <summary>
    /// Represents an expression that references 'type' predefined type.
    /// This class cannot be inherited.
    /// </summary>
    [Serializable]
    [ComVisible(false)]
    public sealed class ScriptCodeMetaContractExpression : ScriptCodeBuiltInContractExpression, IStaticContractBinding<ScriptCodeMetaContractExpression>
    {
        private ScriptCodeMetaContractExpression()
            : base(Keyword.Type)
        {
        }

        /// <summary>
        /// Represents a singleton instance of the expression.
        /// </summary>
        public static readonly ScriptCodeMetaContractExpression Instance = new ScriptCodeMetaContractExpression();

        ScriptCodeMetaContractExpression IStaticContractBinding<ScriptCodeMetaContractExpression>.Contract
        {
            get { return this; }
        }

        /// <summary>
        /// Returns a LINQ expression that produces this object.
        /// </summary>
        /// <returns></returns>
        protected override Expression Restore()
        {
            return LinqHelpers.BodyOf<Func<ScriptCodeMetaContractExpression>, MemberExpression>(() => Instance);
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
