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
        /// Represents body of the context statement.
        /// </summary>
        public readonly ScriptCodeExpressionStatement Body;
        
        /// <summary>
        /// Represents interpretation context.
        /// </summary>
        public InterpretationContext Context;

        /// <summary>
        /// Initializes a new interpretation context block.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="body"></param>
        public ScriptCodeContextExpression(InterpretationContext context, ScriptCodeExpressionStatement body = null)
        {
            Context = context;
            Body = body ?? new ScriptCodeExpressionStatement(ScriptCodeVoidExpression.Instance);
        }

        private ScriptCodeContextExpression(Lexeme ctxToken)
        {
            if (ctxToken == Keyword.Checked) Context = InterpretationContext.Checked;
            else if (ctxToken == Keyword.Unchecked) Context = InterpretationContext.Unchecked;
            else Context = InterpretationContext.Default;
            Body = new ScriptCodeExpressionStatement(ScriptCodeVoidExpression.Instance);
        }

        internal override bool Completed
        {
            get { return Body.Completed; }
        }

        /// <summary>
        /// Returns a string representation of the context expression.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Concat(Context == InterpretationContext.Unchecked ? Keyword.Unchecked : Keyword.Checked, Punctuation.Colon, Body.Expression);
        }

        /// <summary>
        /// Gets a value indicating whether this expression can be simplified.
        /// </summary>
        public override bool CanReduce
        {
            get { return Body.CanReduce; }
        }

        /// <summary>
        /// Simplifies this expression.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public override ScriptCodeExpression Reduce(InterpretationContext context)
        {
            switch (CanReduce)
            {
                case true:
                    var result = new ScriptCodeContextExpression(Context, Body);
                    result.Body.Reduce(context);
                    return result;
                default: return this;
            }
        }

        internal static ScriptCodeContextExpression Parse(IEnumerator<KeyValuePair<Lexeme.Position, Lexeme>> lexer, params Lexeme[] terminator)
        {
            var expr = new ScriptCodeContextExpression(lexer.Current.Value);
            lexer.MoveNext(true);   //Pass through check or unchecked keyword
            expr.Body.SetExpression(Parser.ParseExpression, lexer, terminator);
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
                Equals(Body, other.Body);
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
            var ctor = LinqHelpers.BodyOf<InterpretationContext, ScriptCodeExpressionStatement, ScriptCodeContextExpression, NewExpression>((ctx, stmts) => new ScriptCodeContextExpression(ctx, stmts));
            return ctor.Update(new Expression[] { LinqHelpers.Constant(Context), LinqHelpers.Restore(Body) });
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
