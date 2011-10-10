using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace DynamicScript.Compiler.Ast
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;
    using StringBuilder = System.Text.StringBuilder;
    using Enumerable = System.Linq.Enumerable;

    /// <summary>
    /// Represents structured exception handling block.
    /// This class cannot be inherited.
    /// </summary>
    [ComVisible(false)]
    [Serializable]
    public sealed class ScriptCodeTryElseFinallyExpression: ScriptCodeExpression, IEquatable<ScriptCodeTryElseFinallyExpression>
    {
        #region Nested Types

        /// <summary>
        /// Represents exception trap. This class cannot be inherited.
        /// </summary>
        [ComVisible(false)]
        [Serializable]
        public sealed class FailureTrap: ScriptCodeStatement, IEquatable<FailureTrap>
        {
            /// <summary>
            /// Represents exception handler.
            /// </summary>
            public readonly ScriptCodeExpressionStatement Handler;

            /// <summary>
            /// Initializes a new exception handler.
            /// </summary>
            /// <param name="body"></param>
            public FailureTrap(ScriptCodeExpressionStatement body = null)
            {
                Handler = body ?? new ScriptCodeExpressionStatement(ScriptCodeVoidExpression.Instance);
            }
            
            /// <summary>
            /// Initializes a new exception handler.
            /// </summary>
            /// <param name="filter"></param>
            /// <param name="body"></param>
            public FailureTrap(ScriptCodeVariableDeclaration filter, ScriptCodeExpressionStatement body)
                : this(body)
            {
                Filter = filter;
            }

            /// <summary>
            /// Gets or sets variable that receives exception object.
            /// </summary>
            /// <remarks>This is an optional part of the trap.</remarks>
            public ScriptCodeVariableDeclaration Filter
            {
                get;
                set;
            }

            /// <summary>
            /// Determines whether this object describes the same trap as other object.
            /// </summary>
            /// <param name="other">Other trap to compare.</param>
            /// <returns></returns>
            public bool Equals(FailureTrap other)
            {
                return other != null &&
                    Equals(Filter, other.Filter) &&
                    Equals(Handler, other.Handler);
            }

            /// <summary>
            /// Converts exception trap to its string representation.
            /// </summary>
            /// <returns></returns>
            public override string ToString()
            {
                var builder = new StringBuilder();
                builder.Append(string.Concat(Keyword.Else, Lexeme.WhiteSpace));
                if (Filter != null)
                    builder.AppendLine(string.Concat(Punctuation.LeftBracket, Filter.ToString(true), Punctuation.RightBracket));
                builder.Append(Handler);
                return builder.ToString();
            }

            internal override bool Completed
            {
                get { return true; }
            }

            internal override ScriptCodeStatement Visit(ISyntaxTreeNode parent, Converter<ISyntaxTreeNode, ISyntaxTreeNode> visitor)
            {
                if (Filter != null) Filter = Filter.Visit(this, visitor) as ScriptCodeVariableDeclaration ?? Filter;
                Handler.Visit(this, visitor);
                return visitor.Invoke(this) as ScriptCodeStatement ?? this;
            }

            /// <summary>
            /// Returns a new deep copy of this trap.
            /// </summary>
            /// <returns></returns>
            protected override ScriptCodeStatement Clone()
            {
                return new FailureTrap(Extensions.Clone(Handler)) { Filter = Extensions.Clone(Filter) };
            }

            /// <summary>
            /// Determines whether this object describes the same trap as other object.
            /// </summary>
            /// <param name="other">Other trap to compare.</param>
            /// <returns></returns>
            public override bool Equals(ScriptCodeStatement other)
            {
                return Equals(other as FailureTrap);
            }

            /// <summary>
            /// 
            /// </summary>
            /// <returns></returns>
            protected override Expression Restore()
            {
                var ctor = LinqHelpers.BodyOf<ScriptCodeVariableDeclaration, ScriptCodeExpressionStatement, FailureTrap, NewExpression>((f, h) => new FailureTrap(f, h));
                return ctor.Update(new[] { LinqHelpers.Restore(Filter), LinqHelpers.Restore(Handler) });
            }
        }
        #endregion
        /// <summary>
        /// Represents a potentially dangerous code.
        /// </summary>
        public readonly ScriptCodeExpressionStatement DangerousCode;

        /// <summary>
        /// Represents a finalization code.
        /// </summary>
        public readonly ScriptCodeExpressionStatement Finally;

        /// <summary>
        /// Represents a collection of exception handlers.
        /// </summary>
        public readonly IList<FailureTrap> Traps;

        /// <summary>
        /// Initializes a new SEH block.
        /// </summary>
        /// <param name="dangerousCode"></param>
        /// <param name="traps"></param>
        /// <param name="finallyCode"></param>
        public ScriptCodeTryElseFinallyExpression(ScriptCodeExpressionStatement dangerousCode = null, IEnumerable<FailureTrap> traps = null, ScriptCodeExpressionStatement finallyCode = null)
        {
            DangerousCode = dangerousCode ?? new ScriptCodeExpressionStatement(ScriptCodeVoidExpression.Instance);
            Traps = new List<FailureTrap>(traps ?? Enumerable.Empty<FailureTrap>());
            Finally = finallyCode ?? new ScriptCodeExpressionStatement(ScriptCodeVoidExpression.Instance);
        }

        internal static ScriptCodeTryElseFinallyExpression Parse(IEnumerator<KeyValuePair<Lexeme.Position, Lexeme>> lexer, params Lexeme[] terminator)
        {
            lexer.MoveNext(true);   //pass through try keyword
            var expr = new ScriptCodeTryElseFinallyExpression();
            expr.DangerousCode.SetExpression(Parser.ParseExpression, lexer, terminator + Keyword.Else + Keyword.Finally);
            //Parse error traps
            while (lexer.Current.Value == Keyword.Else)
            {
                var trap = new FailureTrap();
                lexer.MoveNext(Punctuation.LeftBracket, true);   //pass through else keyword
                if (lexer.MoveNext(true) == Keyword.Var)   //parse exception receiver
                    trap.Filter = ScriptCodeVariableDeclaration.Parse(lexer, Punctuation.RightBracket);
                if (lexer.Current.Value != Punctuation.RightBracket)
                    throw CodeAnalysisException.InvalidPunctuation(Punctuation.RightBracket, lexer.Current);
                lexer.MoveNext(true);   //pass through right bracket
                //Parse trap body
                trap.Handler.SetExpression(Parser.ParseExpression, lexer, terminator + Keyword.Else + Keyword.Finally);
                expr.Traps.Add(trap);
            }
            if (lexer.Current.Value == Keyword.Finally)
            {
                lexer.MoveNext(true);   //pass through finally keyword
                //Parse finalization block.
                expr.Finally.SetExpression(Parser.ParseExpression, lexer, terminator);
            }
            return expr;
        }

        /// <summary>
        /// Returns a string representation of Structured Exception Handling expression.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            var builder = new StringBuilder();
            builder.Append(Keyword.Try);
            builder.Append(Lexeme.WhiteSpace);
            builder.Append(DangerousCode);
            foreach (var t in Traps)
                builder.AppendFormat(" {0}", t);
            builder.Append(Finally);
            return builder.ToString();
        }

        /// <summary>
        /// Gets a value indicating whether SEH expression can be reduced.
        /// </summary>
        public override bool CanReduce
        {
            get { return DangerousCode.Expression is ScriptCodePrimitiveExpression; }
        }

        /// <summary>
        /// Simplifies this expression.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public override ScriptCodeExpression Reduce(InterpretationContext context)
        {
            if (DangerousCode.Expression is ScriptCodePrimitiveExpression)
                return DangerousCode.Expression;
            else return this;
        }

        internal override bool Completed
        {
            get { return true; }
        }

        /// <summary>
        /// Converts expression to the statement.
        /// </summary>
        /// <param name="expr">The expression to be converted.</param>
        /// <returns>The statement that encapsulates the expression.</returns>
        public static explicit operator ScriptCodeExpressionStatement(ScriptCodeTryElseFinallyExpression expr)
        {
            return new ScriptCodeExpressionStatement<ScriptCodeTryElseFinallyExpression>(expr);
        }

        /// <summary>
        /// Determines whether this expression represents the same tree as other expression.
        /// </summary>
        /// <param name="other">Other expression tree to compare.</param>
        /// <returns><see langword="true"/> if this expression represents the same tree as other expression; otherwise, <see langword="false"/>.</returns>
        public bool Equals(ScriptCodeTryElseFinallyExpression other)
        {
            switch (other != null && Traps.Count == other.Traps.Count && Equals(DangerousCode, other.DangerousCode) && Equals(Finally, other.Finally))
            {
                case true:
                    for (var i = 0; i < Traps.Count; i++)
                        if (Equals(Traps[i], other.Traps[i])) continue;
                        else return false;
                    return true;
                default: return false;
            }
        }

        /// <summary>
        /// Determines whether this expression represents the same tree as other expression.
        /// </summary>
        /// <param name="other">Other expression tree to compare.</param>
        /// <returns><see langword="true"/> if this expression represents the same tree as other expression; otherwise, <see langword="false"/>.</returns>
        public override bool Equals(ScriptCodeExpression other)
        {
            return Equals(other as ScriptCodeTryElseFinallyExpression);
        }

        /// <summary>
        /// Returns a LINQ expression that produces this object.
        /// </summary>
        /// <returns></returns>
        protected override Expression Restore()
        {
            var ctor = LinqHelpers.BodyOf<ScriptCodeExpressionStatement, IEnumerable<FailureTrap>, ScriptCodeExpressionStatement, ScriptCodeTryElseFinallyExpression, NewExpression>((d, t, f) => new ScriptCodeTryElseFinallyExpression(d, t, f));
            return Expression.Invoke(Expression.Lambda(ctor.Update(new[] { LinqHelpers.Restore(DangerousCode), LinqHelpers.NewArray(Traps), LinqHelpers.Restore(Finally) }))); 
        }

        internal override void Verify()
        {
        }

        internal override ScriptCodeExpression Visit(ISyntaxTreeNode parent, Converter<ISyntaxTreeNode, ISyntaxTreeNode> visitor)
        {
            DangerousCode.Visit(this, visitor);
            for (var i = 0; i < Traps.Count; i++)
                Traps[i] = Traps[i].Visit(this, visitor) as FailureTrap;
            Finally.Visit(this, visitor);
            return visitor.Invoke(this) as ScriptCodeExpression ?? this;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected override ScriptCodeExpression Clone()
        {
            return new ScriptCodeTryElseFinallyExpression(Extensions.Clone(DangerousCode), Extensions.CloneCollection(Traps), Extensions.Clone(Finally));
        }
    }
}
