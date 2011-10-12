using System;
using System.CodeDom;
using System.Linq.Expressions;

namespace DynamicScript.Compiler.Ast
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;

    /// <summary>
    /// Represents invocation expression.
    /// This class cannot be inherited.
    /// </summary>
    [ComVisible(false)]
    [Serializable]
    public sealed class ScriptCodeInvocationExpression : ScriptCodeExpression, ICodeFlowControlInstruction, IEquatable<ScriptCodeInvocationExpression>
    {
        private ScriptCodeExpression m_target;

        /// <summary>
        /// Represents invocation arguments.
        /// </summary>
        public readonly ScriptCodeExpressionCollection ArgList;

        private ScriptCodeInvocationExpression(ScriptCodeExpressionCollection arguments)
        {
            ArgList = arguments ?? new ScriptCodeExpressionCollection();
        }

        /// <summary>
        /// Initializes a new invocation expression.
        /// </summary>
        /// <param name="target"></param>
        /// <param name="arguments"></param>
        public ScriptCodeInvocationExpression(ScriptCodeExpression target = null, params ScriptCodeExpression[] arguments)
            :this(new ScriptCodeExpressionCollection(arguments))
        {
            m_target = target;
        }

        ScriptCodeExpressionCollection ICodeFlowControlInstruction.ArgList
        {
            get { return ArgList; }
        }

        /// <summary>
        /// Gets or sets invocation target.
        /// </summary>
        public ScriptCodeExpression Target
        {
            get { return m_target ?? ScriptCodeVoidExpression.Instance; }
            set{  m_target = value;}
        }

        /// <summary>
        /// Returns a string representation of the expression.
        /// </summary>
        /// <returns>The string representation of the expression.</returns>
        public override string ToString()
        {
            return string.Concat(Target, Punctuation.LeftBracket, ArgList, Punctuation.RightBracket);
        }

        internal override bool Completed
        {
            get { return Target != null; }
        }

        /// <summary>
        /// Converts expression to the statement.
        /// </summary>
        /// <param name="expr">The expression to be converted.</param>
        /// <returns>The statement that encapsulates the expression.</returns>
        public static explicit operator ScriptCodeExpressionStatement(ScriptCodeInvocationExpression expr)
        {
            return new ScriptCodeExpressionStatement<ScriptCodeInvocationExpression>(expr);
        }

        /// <summary>
        /// Determines whether this expression represents the same tree as other expression.
        /// </summary>
        /// <param name="other">Other expression tree to compare.</param>
        /// <returns><see langword="true"/> if this expression represents the same tree as other expression; otherwise, <see langword="false"/>.</returns>
        public bool Equals(ScriptCodeInvocationExpression other)
        {
            return other != null &&
                Completed &&
                other.Completed &&
                Equals(Target, other.Target) &&
                ScriptCodeExpressionCollection.TheSame(ArgList, other.ArgList);
        }

        /// <summary>
        /// Determines whether this expression represents the same tree as other expression.
        /// </summary>
        /// <param name="other">Other expression tree to compare.</param>
        /// <returns><see langword="true"/> if this expression represents the same tree as other expression; otherwise, <see langword="false"/>.</returns>
        public override bool Equals(ScriptCodeExpression other)
        {
            return Equals(other as ScriptCodeInvocationExpression);
        }

        /// <summary>
        /// Returns a LINQ expression that produces this object.
        /// </summary>
        /// <returns></returns>
        protected override Expression Restore()
        {
            var ctor = LinqHelpers.BodyOf<ScriptCodeExpression, ScriptCodeExpression[], ScriptCodeInvocationExpression, NewExpression>((tgt, args) => new ScriptCodeInvocationExpression(tgt, args));
            return ctor.Update(new[] { LinqHelpers.Restore(Target), ArgList.NewArray() });
        }

        internal override void Verify()
        {
        }

        internal override ScriptCodeExpression Visit(ISyntaxTreeNode parent, Converter<ISyntaxTreeNode, ISyntaxTreeNode> visitor)
        {
            ArgList.Visit(this, visitor);
            return visitor.Invoke(this) as ScriptCodeExpression ?? this;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected override ScriptCodeExpression Clone()
        {
            return new ScriptCodeInvocationExpression(Extensions.Clone(ArgList)) { Target = Extensions.Clone(Target) };
        }
    }
}
