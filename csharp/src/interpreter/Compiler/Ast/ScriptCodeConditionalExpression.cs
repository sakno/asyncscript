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
        public readonly ScriptCodeStatementCollection ThenBranch;

        /// <summary>
        /// Represents if-false branche
        /// </summary>
        public readonly ScriptCodeStatementCollection ElseBranch;

        private ScriptCodeConditionalExpression(ScriptCodeStatementCollection thenBranch, ScriptCodeStatementCollection elseBranch)
        {
            ThenBranch = thenBranch ?? new ScriptCodeStatementCollection();
            ElseBranch = elseBranch ?? new ScriptCodeStatementCollection();
        }

        /// <summary>
        /// Initializes a new conditional expression.
        /// </summary>
        public ScriptCodeConditionalExpression()
            : this(new ScriptCodeStatementCollection(), new ScriptCodeStatementCollection())
        {
        }

        /// <summary>
        /// Initializes a new conditional expression.
        /// </summary>
        /// <param name="condition"></param>
        /// <param name="thenBranch"></param>
        /// <param name="elseBranch"></param>
        public ScriptCodeConditionalExpression(ScriptCodeExpression condition, ScriptCodeStatement[] thenBranch, ScriptCodeStatement[] elseBranch)
            : this(new ScriptCodeStatementCollection(thenBranch), new ScriptCodeStatementCollection(elseBranch))
        {
            Condition = condition;
        }

        /// <summary>
        /// Gets or sets condition expression.
        /// </summary>
        public ScriptCodeExpression Condition
        {
            get;
            set;
        }

        internal override bool Completed
        {
            get { return Condition != null && (ThenBranch.Count + ElseBranch.Count > 0); }
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
                    if (ThenBranch.Count > 0) result.Append(string.Concat(Keyword.Then, Lexeme.WhiteSpace, ThenBranch));
                    //if-else branch
                    if (ElseBranch.Count > 0) result.Append(string.Concat(Punctuation.WhiteSpace, Keyword.Else, Lexeme.WhiteSpace, ElseBranch));
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
            switch (lexer.Current.Value == Punctuation.LeftBrace)
            {
                case true:
                    Parser.ParseStatements(lexer, conditional.ThenBranch, Punctuation.RightBrace);
                    break;
                default:
                    conditional.ThenBranch.Add(Parser.ParseExpression, lexer, terminator + Keyword.Else);
                    break;
            }
            if (lexer.Current.Value == Keyword.Else)
                switch (lexer.MoveNext(true) == Punctuation.LeftBrace) //pass through else keyword
                {
                    case true:
                        Parser.ParseStatements(lexer, conditional.ElseBranch, Punctuation.RightBrace);
                        break;
                    default:
                        conditional.ElseBranch.Add(Parser.ParseExpression, lexer, terminator);
                        break;
                }
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
            get
            {
                return Condition != null && Condition.CanReduce;
            }
        }

        /// <summary>
        /// Returns a new reduced conditional expression.
        /// </summary>
        /// <param name="context">Static interpreter context.</param>
        /// <returns>A new reduced conditional expression.</returns>
        public override ScriptCodeExpression Reduce(InterpretationContext context)
        {
            return new ScriptCodeConditionalExpression(ThenBranch, ElseBranch) { Condition = Condition != null ? Condition.Reduce(context) : null };
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
                ScriptCodeStatementCollection.TheSame(ThenBranch, other.ThenBranch) &&
                ScriptCodeStatementCollection.TheSame(ElseBranch, other.ElseBranch);
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
            var ctor = LinqHelpers.BodyOf<ScriptCodeExpression, ScriptCodeStatement[], ScriptCodeStatement[], ScriptCodeConditionalExpression, NewExpression>((cond, then, @else) => new ScriptCodeConditionalExpression(cond, then, @else));
            return ctor.Update(new[] { LinqHelpers.Restore(Condition), ThenBranch.NewArray(), ElseBranch.NewArray() });
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
