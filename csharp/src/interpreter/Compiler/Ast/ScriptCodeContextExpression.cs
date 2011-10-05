using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace DynamicScript.Compiler.Ast
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;

    /// <summary>
    /// Represents context-specific expression.
    /// This class cannot be inherited.
    /// </summary>
    [ComVisible(false)]
    [Serializable]
    public sealed class ScriptCodeContextExpression: ScriptCodeExpression, IEquatable<ScriptCodeContextExpression>
    {
        /// <summary>
        /// Represents body of this complex expression.
        /// </summary>
        public readonly ScriptCodeStatementCollection Body;
        
        /// <summary>
        /// Represents interpretation context.
        /// </summary>
        public InterpretationContext Context;

        private ScriptCodeContextExpression(InterpretationContext context, ScriptCodeStatementCollection statements)
        {
            Context = context;
            Body = statements ?? new ScriptCodeStatementCollection();
        }

        /// <summary>
        /// Initializes a new instance of the expression.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="statements"></param>
        public ScriptCodeContextExpression(InterpretationContext context = InterpretationContext.Default, params ScriptCodeStatement[] statements)
            : this(context, new ScriptCodeStatementCollection(statements))
        {
        }

        private ScriptCodeContextExpression(Lexeme ctxToken)
            : this()
        {
            if (ctxToken == Keyword.Checked) Context = InterpretationContext.Checked;
            else if (ctxToken == Keyword.Unchecked) Context = InterpretationContext.Unchecked;
            else Context = InterpretationContext.Default;
        }

        internal override bool Completed
        {
            get { return true; }
        }

        internal static ScriptCodeContextExpression Parse(IEnumerator<KeyValuePair<Lexeme.Position, Lexeme>> lexer, params Lexeme[] terminator)
        {
            var expr = new ScriptCodeContextExpression(lexer.Current.Value);
            lexer.MoveNext(true);   //pass through checked or unchecked keyword
            switch (lexer.Current.Value == Punctuation.LeftBrace)
            {
                case true:
                    Parser.ParseStatements(lexer, expr.Body, null, Punctuation.RightBrace);
                    break;
                default:
                    expr.Body.Add(Parser.ParseExpression, lexer, terminator);
                    break;
            }
            return expr;
        }

        /// <summary>
        /// Converts expression to the statement.
        /// </summary>
        /// <param name="expr">The expression to be converted.</param>
        /// <returns>The statement that encapsulates the expression.</returns>
        public static explicit operator ScriptCodeExpressionStatement(ScriptCodeContextExpression expr)
        {
            return new ScriptCodeExpressionStatement<ScriptCodeContextExpression>(expr);
        }

        /// <summary>
        /// Determines whether this expression represents the same tree as other expression.
        /// </summary>
        /// <param name="other">Other expression tree to compare.</param>
        /// <returns><see langword="true"/> if this expression represents the same tree as other expression; otherwise, <see langword="false"/>.</returns>
        public bool Equals(ScriptCodeContextExpression other)
        {
            return other != null &&
                Context == other.Context &&
                ScriptCodeStatementCollection.TheSame(Body, other.Body);
        }

        /// <summary>
        /// Determines whether this expression represents the same tree as other expression.
        /// </summary>
        /// <param name="other">Other expression tree to compare.</param>
        /// <returns><see langword="true"/> if this expression represents the same tree as other expression; otherwise, <see langword="false"/>.</returns>
        public override bool Equals(ScriptCodeExpression other)
        {
            return Equals(other as ScriptCodeContextExpression);
        }

        /// <summary>
        /// Returns a LINQ expression that produces this object.
        /// </summary>
        /// <returns></returns>
        protected override Expression Restore()
        {
            var ctor = LinqHelpers.BodyOf<InterpretationContext, ScriptCodeStatement[], ScriptCodeContextExpression, NewExpression>((ctx, stmts) => new ScriptCodeContextExpression(ctx, stmts));
            return ctor.Update(new Expression[] { LinqHelpers.Constant(Context), Body.NewArray() });
        }

        internal override void Verify()
        {
        }

        internal override ScriptCodeExpression Visit(ISyntaxTreeNode parent, Converter<ISyntaxTreeNode, ISyntaxTreeNode> visitor)
        {
            Body.Visit(this, visitor);
            return visitor.Invoke(this) as ScriptCodeExpression;
        }

        /// <summary>
        /// Returns a new deep copy of this expression.
        /// </summary>
        /// <returns></returns>
        protected override ScriptCodeExpression Clone()
        {
            return new ScriptCodeContextExpression(Context, Extensions.Clone(Body));
        }
    }
}
