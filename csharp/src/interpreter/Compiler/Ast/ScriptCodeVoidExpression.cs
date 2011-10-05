using System;
using System.Linq.Expressions;

namespace DynamicScript.Compiler.Ast
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;
    using RuntimeHelpers = Runtime.Environment.RuntimeHelpers;

    /// <summary>
    /// Represents expression that is used to indicate missing value or empty set.
    /// This class cannot be inherited.
    /// </summary>
    [ComVisible(false)]
    [Serializable]
    public sealed class ScriptCodeVoidExpression : ScriptCodePrimitiveExpression
    {
        private ScriptCodeVoidExpression()
        {
            Value = Keyword.Void;
        }

        /// <summary>
        /// Represents singleton value of the expression.
        /// </summary>
        public static readonly ScriptCodeVoidExpression Instance = new ScriptCodeVoidExpression();

        /// <summary>
        /// Returns a LINQ expression that produces this object.
        /// </summary>
        /// <returns></returns>
        protected override Expression Restore()
        {
            return LinqHelpers.BodyOf<Func<ScriptCodeVoidExpression>, MemberExpression>(() => Instance);
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
