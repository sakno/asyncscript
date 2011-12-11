using System;
using System.Linq.Expressions;

namespace DynamicScript.Compiler.Ast
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;

    /// <summary>
    /// Represents reference to the global object.
    /// This class cannot be inherited.
    /// </summary>
    [ComVisible(false)]
    [Serializable]
    public sealed class ScriptCodeGlobalObjectExpression: ScriptCodeExpression
    {
        private ScriptCodeGlobalObjectExpression()
        {
        }

        /// <summary>
        /// Represents singleton instance of this expression.
        /// </summary>
        public static readonly ScriptCodeGlobalObjectExpression Instance = new ScriptCodeGlobalObjectExpression();

        internal override bool Completed
        {
            get { return true; }
        }

        /// <summary>
        /// Determines whether the specified AST tree is equal to this expression.
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public override bool Equals(ScriptCodeExpression other)
        {
            return other is ScriptCodeGlobalObjectExpression;
        }

        /// <summary>
        /// Returns an expression tha produces an instance of this AST node.
        /// </summary>
        /// <returns></returns>
        protected override Expression Restore()
        {
            return LinqHelpers.BodyOf<Func<ScriptCodeGlobalObjectExpression>, MemberExpression>(() => Instance);
        }

        internal override void Verify()
        {
        }

        /// <summary>
        /// Visits this AST node.
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="visitor"></param>
        /// <returns></returns>
        internal override ScriptCodeExpression Visit(ISyntaxTreeNode parent, Converter<ISyntaxTreeNode, ISyntaxTreeNode> visitor)
        {
            return visitor.Invoke(this) as ScriptCodeExpression ?? this;
        }

        /// <summary>
        /// Clones the current expression.
        /// </summary>
        /// <returns></returns>
        protected override ScriptCodeExpression Clone()
        {
            return Instance;
        }

        /// <summary>
        /// Returns string representation of this expression.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return Keyword.Global;
        }
    }
}
