using System;
using System.CodeDom;

namespace DynamicScript.Compiler.Ast
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;

    /// <summary>
    /// Represents a base class for predefined type expressions.
    /// </summary>
    [Serializable]
    [ComVisible(false)]
    public abstract class ScriptCodePrimitiveExpression : ScriptCodeExpression, IEquatable<ScriptCodePrimitiveExpression>
    {
        internal ScriptCodePrimitiveExpression()
        {
        }

        /// <summary>
        /// Gets or sets a value of the primitive expression.
        /// </summary>
        internal Lexeme Value
        {
            get;
            set;
        }

        /// <summary>
        /// Gets a value indicating that this literal should be interned.
        /// </summary>
        public virtual bool IsInterned
        {
            get { return false; }
            set { }
        }

        /// <summary>
        /// Returns a string representation of the predefined expression.
        /// </summary>
        /// <returns>The string representation of the predefined expression.</returns>
        public sealed override string ToString()
        {
            return Value;
        }

        internal sealed override bool Completed
        {
            get { return true; }
        }

        /// <summary>
        /// Converts expression to the statement.
        /// </summary>
        /// <param name="expr">The expression to be converted.</param>
        /// <returns>The statement that encapsulates the expression.</returns>
        public static explicit operator ScriptCodeExpressionStatement(ScriptCodePrimitiveExpression expr)
        {
            return new ScriptCodeExpressionStatement<ScriptCodePrimitiveExpression>(expr);
        }

        /// <summary>
        /// Determines whether this expression represents the same tree as other expression.
        /// </summary>
        /// <param name="other">Other expression tree to compare.</param>
        /// <returns><see langword="true"/> if this expression represents the same tree as other expression; otherwise, <see langword="false"/>.</returns>
        public sealed override bool Equals(ScriptCodeExpression other)
        {
            return Equals(other as ScriptCodePrimitiveExpression);
        }

        /// <summary>
        /// Determines whether this expression represents the same tree as other expression.
        /// </summary>
        /// <param name="other">Other expression tree to compare.</param>
        /// <returns><see langword="true"/> if this expression represents the same tree as other expression; otherwise, <see langword="false"/>.</returns>
        public bool Equals(ScriptCodePrimitiveExpression other)
        {
            return other != null && Value.Equals(other.Value);
        }

        internal override void Verify()
        {
        }

        internal sealed override ScriptCodeExpression Visit(ISyntaxTreeNode parent, Converter<ISyntaxTreeNode, ISyntaxTreeNode> visitor)
        {
            return visitor.Invoke(this) as ScriptCodeExpression ?? this;
        }
    }
}
