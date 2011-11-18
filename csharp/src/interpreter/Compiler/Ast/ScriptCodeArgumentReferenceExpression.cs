using System;
using System.Linq.Expressions;

namespace DynamicScript.Compiler.Ast
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;

    /// <summary>
    /// Represents argument reference.
    /// This class cannot be inherited.
    /// </summary>
    [ComVisible(false)]
    [Serializable]
    public sealed class ScriptCodeArgumentReferenceExpression: ScriptCodeExpression, IEquatable<ScriptCodeArgumentReferenceExpression>
    {
        /// <summary>
        /// Represents argument index.
        /// </summary>
        public readonly long Index;

        /// <summary>
        /// Initializes a new argument reference.
        /// </summary>
        /// <param name="index"></param>
        public ScriptCodeArgumentReferenceExpression(long index)
        {
            Index = Math.Abs(index);
        }

        internal ScriptCodeArgumentReferenceExpression(ArgRef index)
            : this(index.Parse())
        {
        }

        internal override bool Completed
        {
            get { return true; }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public override bool Equals(ScriptCodeExpression other)
        {
            return Equals(other as ScriptCodeArgumentReferenceExpression);
        }

        /// <summary>
        /// Creates an expression that produces an instance of this object.
        /// </summary>
        /// <returns></returns>
        protected override Expression Restore()
        {
            var ctor = LinqHelpers.BodyOf<long, ScriptCodeArgumentReferenceExpression, NewExpression>(idx => new ScriptCodeArgumentReferenceExpression(idx));
            return ctor.Update(new[] { LinqHelpers.Constant(Index) });
        }

        internal override void Verify()
        {
        }

        internal override ScriptCodeExpression Visit(ISyntaxTreeNode parent, Converter<ISyntaxTreeNode, ISyntaxTreeNode> visitor)
        {
            return visitor.Invoke(this) as ScriptCodeExpression ?? this;
        }

        /// <summary>
        /// Creates clone of this expression.
        /// </summary>
        /// <returns></returns>
        protected override ScriptCodeExpression Clone()
        {
            return new ScriptCodeArgumentReferenceExpression(Index);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool Equals(ScriptCodeArgumentReferenceExpression other)
        {
            return other != null && Index == other.Index;
        }

        /// <summary>
        /// Returns a string representation of this expression.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Concat(Lexeme.Exclamation, Lexeme.Exclamation, Index);
        }
    }
}
