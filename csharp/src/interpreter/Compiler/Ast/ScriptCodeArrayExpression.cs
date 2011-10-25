using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace DynamicScript.Compiler.Ast
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;
    using CodeExpressionCollection = System.CodeDom.CodeExpressionCollection;

    /// <summary>
    /// Represents array literal.
    /// This class cannot be inherited.
    /// </summary>
    [Serializable]
    [ComVisible(false)]
    public sealed class ScriptCodeArrayExpression: ScriptCodeExpression, IEquatable<ScriptCodeArrayExpression>
    {
        /// <summary>
        /// Represents array elements.
        /// </summary>
        public readonly ScriptCodeExpressionCollection Elements;

        private ScriptCodeArrayExpression(ScriptCodeExpressionCollection elements)
        {
            Elements = elements ?? new ScriptCodeExpressionCollection();
        }

        /// <summary>
        /// Initializes a new array expression.
        /// </summary>
        /// <param name="elements"></param>
        public ScriptCodeArrayExpression(params ScriptCodeExpression[] elements)
            :this(new ScriptCodeExpressionCollection(elements))
        {
        }

        internal override bool Completed
        {
            get { return true; }
        }

        /// <summary>
        /// Determines whether this expression represents the same tree as other expression.
        /// </summary>
        /// <param name="other">Other expression tree to compare.</param>
        /// <returns><see langword="true"/> if this expression represents the same tree as other expression; otherwise, <see langword="false"/>.</returns>
        public bool Equals(ScriptCodeArrayExpression other)
        {
            return other != null &&
                ScriptCodeExpressionCollection.TheSame(Elements, other.Elements);
        }

        /// <summary>
        /// Determines whether this expression represents the same tree as other expression.
        /// </summary>
        /// <param name="other">Other expression tree to compare.</param>
        /// <returns><see langword="true"/> if this expression represents the same tree as other expression; otherwise, <see langword="false"/>.</returns>
        public override bool Equals(ScriptCodeExpression other)
        {
            return Equals(other as ScriptCodeArrayExpression);
        }

        /// <summary>
        /// Returns a string representation of the array expression.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Concat(Punctuation.LeftSquareBracket, Elements, Punctuation.RightSquareBracket);
        }

        /// <summary>
        /// Returns a LINQ expression that produces this object.
        /// </summary>
        /// <returns></returns>
        protected override Expression Restore()
        {
            var ctor = LinqHelpers.BodyOf<ScriptCodeExpression[], ScriptCodeArrayExpression, NewExpression>(elems => new ScriptCodeArrayExpression(elems));
            return ctor.Update(new[] { Elements.NewArray() });
        }

        internal override void Verify()
        {
        }

        internal override ScriptCodeExpression Visit(ISyntaxTreeNode parent, Converter<ISyntaxTreeNode, ISyntaxTreeNode> visitor)
        {
            Elements.Visit(this, visitor);
            return visitor.Invoke(this) as ScriptCodeExpression;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected override ScriptCodeExpression Clone()
        {
            return new ScriptCodeArrayExpression(Extensions.Clone(Elements));
        }
    }
}
