using System;
using System.Linq.Expressions;

namespace DynamicScript.Compiler.Ast
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;

    /// <summary>
    /// Represents placeholder expression that is used to generalize some parts
    /// of quoted expression. This class cannot be inherited.
    /// </summary>
    [ComVisible(false)]
    [Serializable]
    public sealed class ScriptCodePlaceholderExpression: ScriptCodeExpression, IEquatable<ScriptCodePlaceholderExpression>
    {
        /// <summary>
        /// Represents placeholder identifier.
        /// </summary>
        public readonly long PlaceholderID;

        /// <summary>
        /// Initializes a new placeholder.
        /// </summary>
        /// <param name="id"></param>
        public ScriptCodePlaceholderExpression(long id)
        {
            PlaceholderID = id;
        }

        internal ScriptCodePlaceholderExpression(PlaceholderID id)
            : this(id.Parse())
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
        public bool Equals(ScriptCodePlaceholderExpression other)
        {
            return other != null && PlaceholderID == other.PlaceholderID;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public override bool Equals(ScriptCodeExpression other)
        {
            return Equals(other as ScriptCodePlaceholderExpression);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected override Expression Restore()
        {
            var ctor = LinqHelpers.BodyOf<long, ScriptCodePlaceholderExpression, NewExpression>(id => new ScriptCodePlaceholderExpression(id));
            return ctor.Update(new[] { LinqHelpers.Constant(PlaceholderID) });
        }

        internal override void Verify()
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected override ScriptCodeExpression Clone()
        {
            return new ScriptCodePlaceholderExpression(PlaceholderID);
        }

        internal override ScriptCodeExpression Visit(ISyntaxTreeNode parent, Converter<ISyntaxTreeNode, ISyntaxTreeNode> visitor)
        {
            return visitor.Invoke(this) as ScriptCodeExpression ?? this;
        }

        /// <summary>
        /// Returns a string representation of this expression.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Concat(Lexeme.Percent, Lexeme.Percent, PlaceholderID);
        }
    }
}
