using System;
using System.Linq.Expressions;

namespace DynamicScript.Compiler.Ast
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;

    /// <summary>
    /// Represents 'this' reference.
    /// This class cannot be inherited.
    /// </summary>
    [ComVisible(false)]
    [Serializable]
    public sealed class ScriptCodeThisExpression: ScriptCodeExpression
    {
        private ScriptCodeThisExpression()
        {
        }

        /// <summary>
        /// Represents singleton instance of the expression.
        /// </summary>
        public static readonly ScriptCodeThisExpression Instance = new ScriptCodeThisExpression();

        internal override bool Completed
        {
            get { return true; }
        }

        /// <summary>
        /// Determines whether this expression represents the same tree as other expression.
        /// </summary>
        /// <param name="other">Other expression tree to compare.</param>
        /// <returns><see langword="true"/> if this expression represents the same tree as other expression; otherwise, <see langword="false"/>.</returns>
        public override bool Equals(ScriptCodeExpression other)
        {
            return other is ScriptCodeThisExpression;
        }

        /// <summary>
        /// Converts this expression to its string representation.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return Keyword.This;
        }

        /// <summary>
        /// Converts this expression to its string representation.
        /// </summary>
        /// <param name="style"></param>
        /// <returns></returns>
        public override string ToString(ScriptCodeExpression.FormattingStyle style)
        {
            switch (style)
            {
                case FormattingStyle.Parenthesize: return string.Concat(Punctuation.LeftBracket, Keyword.This, Punctuation.RightBracket);
                default: return ToString();
            }
        }

        /// <summary>
        /// Returns a LINQ expression that produces this object.
        /// </summary>
        /// <returns></returns>
        protected override Expression Restore()
        {
            return LinqHelpers.BodyOf<Func<ScriptCodeThisExpression>, MemberExpression>(() => Instance);
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
