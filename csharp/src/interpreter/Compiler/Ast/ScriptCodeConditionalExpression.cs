using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace DynamicScript.Compiler.Ast
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;
    using StringBuilder = System.Text.StringBuilder;

    /// <summary>
    /// Represents conditional expression(if-then or if-then-else).
    /// This class cannot be inherited.
    /// </summary>
    [Serializable]
    [ComVisible(false)]
    public sealed class ScriptCodeConditionalExpression : ScriptCodeExpression, IEquatable<ScriptCodeConditionalExpression>
    {
        /// <summary>
        /// Represents if-true branch.
        /// </summary>
        private ScriptCodeExpression m_then;

        /// <summary>
        /// Represents if-false branche
        /// </summary>
        private ScriptCodeExpression m_else;

        private ScriptCodeExpression m_condition;

        /// <summary>
        /// Initializes a new conditional expression.
        /// </summary>
        /// <param name="condition"></param>
        /// <param name="thenValue"></param>
        /// <param name="elseValue"></param>
        public ScriptCodeConditionalExpression(ScriptCodeExpression condition = null, ScriptCodeExpression thenValue = null, ScriptCodeExpression elseValue = null)
        {
            m_condition = condition;
            m_then = thenValue;
            m_else = elseValue;
        }

        /// <summary>
        /// Gets or sets condition expression.
        /// </summary>
        public ScriptCodeExpression Condition
        {
            get { return m_condition ?? ScriptCodeVoidExpression.Instance; }
            set { m_condition = value; }
        }

        /// <summary>
        /// Gets or sets a value returned from the conditional expression if it is True.
        /// </summary>
        public ScriptCodeExpression ThenBranch
        {
            get { return m_then ?? ScriptCodeVoidExpression.Instance; }
            set { m_then = value; }
        }

        /// <summary>
        /// Gets or sets a value returned from the conditional expression if it is False.
        /// </summary>
        public ScriptCodeExpression ElseBranch
        {
            get { return m_else ?? ScriptCodeVoidExpression.Instance; }
            set { m_else = value; }
        }

        internal override bool Completed
        {
            get { return true; }
        }

        /// <summary>
        /// Returns a string representation of conditional expression.
        /// </summary>
        /// <returns>The string representation of conditional expression.</returns>
        public override string ToString()
        {
            switch (Completed)
            {
                case true:
                    var result = new StringBuilder();
                    result.Append(string.Concat(Keyword.If, Lexeme.WhiteSpace, Condition, Lexeme.WhiteSpace));
                    //if-then branch output
                    result.Append(string.Concat(Keyword.Then, Lexeme.WhiteSpace, ThenBranch));
                    //if-else branch
                    if (!(ElseBranch is ScriptCodeVoidExpression))
                        result.Append(string.Concat(Punctuation.WhiteSpace, Keyword.Else, Lexeme.WhiteSpace, ElseBranch));
                    return result.ToString();
                default:
                    return ErrorMessages.IncompletedExpression;
            }
        }

        internal static ScriptCodeConditionalExpression Parse(IEnumerator<KeyValuePair<Lexeme.Position, Lexeme>> lexer, params Lexeme[] terminator)
        {
            if (lexer == null) throw new ArgumentNullException("lexer");
            lexer.MoveNext(true);    //pass through if keyword
            var conditional = new ScriptCodeConditionalExpression
            {
                Condition = Parser.ParseExpression(lexer, Keyword.Then)  //parse condition expression
            };
            lexer.MoveNext(true);    //pass through then keyword
            conditional.ThenBranch = Parser.ParseExpression(lexer, terminator + Keyword.Else);
            if (lexer.Current.Value == Keyword.Else)
                conditional.ElseBranch = Parser.ParseExpression(lexer, terminator);
            return conditional;
        }

        /// <summary>
        /// Converts expression to the statement.
        /// </summary>
        /// <param name="expr">The expression to be converted.</param>
        /// <returns>The statement that encapsulates the expression.</returns>
        public static explicit operator ScriptCodeExpressionStatement(ScriptCodeConditionalExpression expr)
        {
            return new ScriptCodeExpressionStatement<ScriptCodeConditionalExpression>(expr);
        }

        /// <summary>
        /// Gets a value indicating that this expression can be reduced.
        /// </summary>
        public override bool CanReduce
        {
            get { return Condition.CanReduce || ThenBranch.CanReduce || ElseBranch.CanReduce; }
        }

        /// <summary>
        /// Returns a new reduced conditional expression.
        /// </summary>
        /// <param name="context">Static interpreter context.</param>
        /// <returns>A new reduced conditional expression.</returns>
        public override ScriptCodeExpression Reduce(InterpretationContext context)
        {
            return new ScriptCodeConditionalExpression(Condition.Reduce(context), ThenBranch.Reduce(context), ElseBranch.Reduce(context));
        }

        /// <summary>
        /// Determines whether this expression represents the same tree as other expression.
        /// </summary>
        /// <param name="other">Other expression tree to compare.</param>
        /// <returns><see langword="true"/> if this expression represents the same tree as other expression; otherwise, <see langword="false"/>.</returns>
        public bool Equals(ScriptCodeConditionalExpression other)
        {
            return other != null &&
                Equals(Condition, other.Condition) &&
                Equals(ThenBranch, other.ThenBranch) &&
                Equals(ElseBranch, other.ElseBranch);
        }

        /// <summary>
        /// Determines whether this expression represents the same tree as other expression.
        /// </summary>
        /// <param name="other">Other expression tree to compare.</param>
        /// <returns><see langword="true"/> if this expression represents the same tree as other expression; otherwise, <see langword="false"/>.</returns>
        public override bool Equals(ScriptCodeExpression other)
        {
            return Equals(other as ScriptCodeConditionalExpression);
        }

        /// <summary>
        /// Returns a LINQ expression that produces this object.
        /// </summary>
        /// <returns></returns>
        protected override Expression Restore()
        {
            var ctor = LinqHelpers.BodyOf<ScriptCodeExpression, ScriptCodeExpression, ScriptCodeExpression, ScriptCodeConditionalExpression, NewExpression>((cond, then, @else) => new ScriptCodeConditionalExpression(cond, then, @else));
            return ctor.Update(new[] { LinqHelpers.Restore(Condition), LinqHelpers.Restore(ThenBranch), LinqHelpers.Restore(ElseBranch) });
        }

        internal override void Verify()
        {
        }

        internal override ScriptCodeExpression Visit(ISyntaxTreeNode parent, Converter<ISyntaxTreeNode, ISyntaxTreeNode> visitor)
        {
            ThenBranch.Visit(this, visitor);
            ElseBranch.Visit(this, visitor);
            return visitor.Invoke(this) as ScriptCodeExpression ?? this;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected override ScriptCodeExpression Clone()
        {
            return new ScriptCodeConditionalExpression(Extensions.Clone(ThenBranch), Extensions.Clone(ElseBranch));
        }
    }
}
