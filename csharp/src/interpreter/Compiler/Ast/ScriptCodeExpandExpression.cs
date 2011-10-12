using System;
using System.Linq.Expressions;
using System.Collections.Generic;

namespace DynamicScript.Compiler.Ast
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;

    /// <summary>
    /// Represents an expression that expands quoted expression at runtime.
    /// This class cannot be inherited.
    /// </summary>
    [ComVisible(false)]
    [Serializable]
    public sealed class ScriptCodeExpandExpression: ScriptCodeExpression, IEquatable<ScriptCodeExpandExpression>
    {
        /// <summary>
        /// Represents an collection of placeholder substitutes.
        /// </summary>
        public readonly ScriptCodeExpressionCollection Substitutes;
        private ScriptCodeExpression m_target;

        /// <summary>
        /// Initializes a new expand expression.
        /// </summary>
        /// <param name="target"></param>
        /// <param name="subst"></param>
        public ScriptCodeExpandExpression(ScriptCodeExpression target, ScriptCodeExpressionCollection subst)
        {
            Substitutes = subst ?? new ScriptCodeExpressionCollection();
            m_target = target;
        }

        /// <summary>
        /// Initializes a new expand expression.
        /// </summary>
        public ScriptCodeExpandExpression()
            : this(null, new ScriptCodeExpressionCollection())
        {
        }

        /// <summary>
        /// Gets or sets an expression that produces a quoted expression.
        /// </summary>
        public ScriptCodeExpression Target
        {
            get { return m_target ?? ScriptCodeVoidExpression.Instance; }
            set { m_target = value; }
        }

        internal override bool Completed
        {
            get { return true; }
        }

        /// <summary>
        /// Determines whether the current expression is equal to another.
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool Equals(ScriptCodeExpandExpression other)
        {
            return other != null && Equals(Target, other.Target) && ScriptCodeExpressionCollection.TheSame(Substitutes, other.Substitutes);
        }

        /// <summary>
        /// Determines whether the current expression is equal to another.
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public override bool Equals(ScriptCodeExpression other)
        {
            return Equals(other as ScriptCodeExpandExpression);
        }

        /// <summary>
        /// Returns an expression that produces the current expression.
        /// </summary>
        /// <returns></returns>
        protected override Expression Restore()
        {
            var ctor = LinqHelpers.BodyOf<ScriptCodeExpression, ScriptCodeExpressionCollection, ScriptCodeExpandExpression, NewExpression>((t, s) => new ScriptCodeExpandExpression(t, s));
            return ctor.Update(new[] { LinqHelpers.Restore(Target), LinqHelpers.Restore(Substitutes) });
        }

        internal override void Verify()
        {
        }

        internal override ScriptCodeExpression Visit(ISyntaxTreeNode parent, Converter<ISyntaxTreeNode, ISyntaxTreeNode> visitor)
        {
            Substitutes.Visit(this, visitor);
            Target = Target.Visit(this, visitor) as ScriptCodeExpression;
            return visitor.Invoke(this) as ScriptCodeExpression ?? this;
        }


        /// <summary>
        /// Returns a new deep clone of this expression.
        /// </summary>
        /// <returns></returns>
        protected override ScriptCodeExpression Clone()
        {
            return new ScriptCodeExpandExpression(Extensions.Clone(Target), Extensions.Clone(Substitutes));
        }

        /// <summary>
        /// Returns a string representation of this expression.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Concat(Keyword.Expandq, Lexeme.WhiteSpace, Target, Punctuation.LeftBracket, Substitutes, Punctuation.RightBracket);
        }

        internal static ScriptCodeExpandExpression Parse(IEnumerator<KeyValuePair<Lexeme.Position, Lexeme>> lexer)
        {
            lexer.MoveNext(true);   //iterates through expandq keyword
            var result = new ScriptCodeExpandExpression();
            //parse target
            result.Target = Parser.ParseExpression(lexer, Punctuation.LeftBracket);
            Parser.ParseExpressions(lexer, result.Substitutes, Punctuation.RightBracket);
            return result;
        }
    }
}
