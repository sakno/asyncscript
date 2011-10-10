using System;
using System.CodeDom;
using System.Linq.Expressions;
using System.Collections.Generic;

namespace DynamicScript.Compiler.Ast
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;

    /// <summary>
    /// Represents expression statement.
    /// </summary>
    [Serializable]
    [ComVisible(false)]
    public class ScriptCodeExpressionStatement : ScriptCodeStatement, IScriptExpressionStatement, IEquatable<ScriptCodeExpressionStatement>
    {
        /// <summary>
        /// Represents expression wrapped to the statement.
        /// </summary>
        public ScriptCodeExpression Expression;

        /// <summary>
        /// Initializes a new statement that wraps a single expression.
        /// </summary>
        /// <param name="expression">An expression to be wrapped.</param>
        public ScriptCodeExpressionStatement(ScriptCodeExpression expression)
        {
            Expression = expression;
        }

        internal ScriptCodeExpressionStatement(Func<IEnumerator<KeyValuePair<Lexeme.Position, Lexeme>>, Lexeme[], ScriptCodeExpression> parser, IEnumerator<KeyValuePair<Lexeme.Position, Lexeme>> lexer, params Lexeme[] terminator)
        {
            SetExpression(parser, lexer, terminator);   
        }

        internal void SetExpression(Func<IEnumerator<KeyValuePair<Lexeme.Position, Lexeme>>, Lexeme[], ScriptCodeExpression> parser, IEnumerator<KeyValuePair<Lexeme.Position, Lexeme>> lexer, params Lexeme[] terminator)
        {
            LinePragma = new ScriptDebugInfo { Start = lexer.Current.Key };
            Expression = parser.Invoke(lexer, terminator);
            LinePragma.End = lexer.Current.Key;
        }

        ScriptCodeExpression IScriptExpressionStatement.Expression
        {
            get { return Expression; }
        }

        internal bool CanReduce
        {
            get { return Expression != null && Expression.CanReduce; }
        }

        internal bool IsComplexExpression
        {
            get { return Expression is IList<ScriptCodeExpression>; }
        }

        internal bool IsVoidExpression
        {
            get { return Expression == null || Expression is ScriptCodeVoidExpression; }
        }

        internal void Reduce(InterpretationContext context)
        {
            if (Expression != null) Expression = Expression.Reduce(context);
        }

        /// <summary>
        /// Returns a string representation of the statement.
        /// </summary>
        /// <returns>The string representation of the statement.</returns>
        public sealed override string ToString()
        {
            return string.Concat(Expression, Punctuation.Semicolon);
        }

        /// <summary>
        /// Extracts expression from the statement.
        /// </summary>
        /// <param name="stmt">The statement with expression.</param>
        /// <returns>The expression extracted from the statement.</returns>
        public static explicit operator ScriptCodeExpression(ScriptCodeExpressionStatement stmt)
        {
            return stmt != null ? stmt.Expression : null;
        }

        /// <summary>
        /// Returns a LINQ expression that produces this object.
        /// </summary>
        /// <returns></returns>
        protected override Expression Restore()
        {
            var ctor = LinqHelpers.BodyOf<ScriptCodeExpression, ScriptCodeExpressionStatement, NewExpression>(expr => new ScriptCodeExpressionStatement(expr));
            return ctor.Update(new[] { LinqHelpers.Restore(Expression) });
        }

        internal static bool OfType<TExpression>(ScriptCodeExpressionStatement stmt)
        {
            return stmt != null && stmt.Expression is TExpression;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected override ScriptCodeStatement Clone()
        {
            return new ScriptCodeExpressionStatement(Extensions.Clone(Expression));
        }

        internal sealed override bool Completed
        {
            get { return Expression != null && Expression.Completed; }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool Equals(ScriptCodeExpressionStatement other)
        {
            return other != null && Equals(Expression, other.Expression);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public sealed override bool Equals(ScriptCodeStatement other)
        {
            return Equals(other as ScriptCodeExpressionStatement);
        }

        internal override ScriptCodeStatement Visit(ISyntaxTreeNode parent, Converter<ISyntaxTreeNode, ISyntaxTreeNode> visitor)
        {
            if (Expression != null) Expression = Expression.Visit(this, visitor);
            return visitor.Invoke(this) as ScriptCodeStatement ?? this;
        }

        internal IList<ScriptCodeStatement> UnwrapStatements()
        {
            if (Expression == null) return new ScriptCodeStatement[0];
            else if (IsComplexExpression) return (IList<ScriptCodeStatement>)Expression;
            else return new[] { this };
        }
    }

    /// <summary>
    /// Represents a typed statement that wraps an expression.
    /// This class cannot be inherited.
    /// </summary>
    /// <typeparam name="TExpression">Type of the expression.</typeparam>
    [Serializable]
    [ComVisible(false)]
    public sealed class ScriptCodeExpressionStatement<TExpression> : ScriptCodeExpressionStatement
        where TExpression : ScriptCodeExpression
    {
        /// <summary>
        /// Initializes a new statement based on the expression.
        /// </summary>
        /// <param name="expr"></param>
        public ScriptCodeExpressionStatement(TExpression expr)
            : base(expr)
        {
        }

        /// <summary>
        /// Gets or sets expression stored in the statement.
        /// </summary>
        public new TExpression Expression
        {
            get { return base.Expression as TExpression; }
            set { base.Expression = value; }
        }

        /// <summary>
        /// Returns a LINQ expression that produces this object.
        /// </summary>
        /// <returns></returns>
        protected override Expression Restore()
        {
            var ctor = LinqHelpers.BodyOf<TExpression, ScriptCodeExpressionStatement, NewExpression>(expr => new ScriptCodeExpressionStatement<TExpression>(expr));
            return ctor.Update(new[] { LinqHelpers.Restore(Expression) });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected override ScriptCodeStatement Clone()
        {
            return new ScriptCodeExpressionStatement<TExpression>(Expression);
        }

        internal static bool IsPrimitive(CodeStatementCollection statements)
        {
            return statements.Count == 1 && OfType<TExpression>(statements[0] as ScriptCodeExpressionStatement);
        }
    }
}
