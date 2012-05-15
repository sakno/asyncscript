using System;
using System.Linq.Expressions;

namespace DynamicScript.Compiler.Ast
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;

    /// <summary>
    /// Represents an expression that represents currently executed action.
    /// This class cannot be inherited.
    /// </summary>
    [Serializable]
    [ComVisible(false)]
    public sealed class ScriptCodeCurrentActionExpression: ScriptCodeExpression
    {
        private ScriptCodeCurrentActionExpression()
        {
        }

        internal override bool Completed
        {
            get { return true; }
        }

        /// <summary>
        /// Represents a singleton instance of the expression.
        /// </summary>
        public static readonly ScriptCodeCurrentActionExpression Instance = new ScriptCodeCurrentActionExpression();

        /// <summary>
        /// Returns a string representation of this expression.
        /// </summary>
        /// <returns>A string representation of this expression.</returns>
        public override string ToString()
        {
            return Punctuation.Dog;
        }

        /// <summary>
        /// Determines whether this expression represents the same tree as other expression.
        /// </summary>
        /// <param name="other">Other expression tree to compare.</param>
        /// <returns><see langword="true"/> if this expression represents the same tree as other expression; otherwise, <see langword="false"/>.</returns>
        public override bool Equals(ScriptCodeExpression other)
        {
            return other is ScriptCodeCurrentActionExpression;
        }

        /// <summary>
        /// Returns a LINQ expression that produces this object.
        /// </summary>
        /// <returns></returns>
        protected override Expression Restore()
        {
            return LinqHelpers.BodyOf<Func<ScriptCodeCurrentActionExpression>, MemberExpression>(() => Instance);
        }

        internal override void Verify()
        {
        }

        internal override ScriptCodeExpression Visit(ISyntaxTreeNode parent, Converter<ISyntaxTreeNode, ISyntaxTreeNode> visitor)
        {
            return visitor.Invoke(this) as ScriptCodeExpression ?? this;
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
