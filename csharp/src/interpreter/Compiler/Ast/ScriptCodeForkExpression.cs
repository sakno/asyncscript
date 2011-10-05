using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace DynamicScript.Compiler.Ast
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;

    /// <summary>
    /// Represents an expression that produces asynchronous task at runtime.
    /// This class cannot be inherited.
    /// </summary>
    [ComVisible(false)]
    [Serializable]
    public sealed class ScriptCodeForkExpression: ScriptCodeExpression, IEquatable<ScriptCodeForkExpression>
    {
        /// <summary>
        /// Represents body of the asynchronous task.
        /// </summary>
        public readonly ScriptCodeStatementCollection Body;

        private ScriptCodeForkExpression(ScriptCodeStatementCollection body)
        {
            Body = body ?? new ScriptCodeStatementCollection();
        }
        

        /// <summary>
        /// Initializes a new FORK expression.
        /// </summary>
        /// <param name="statements"></param>
        public ScriptCodeForkExpression(params ScriptCodeStatement[] statements)
            :this(new ScriptCodeStatementCollection(statements))
        {
        }

        internal override bool Completed
        {
            get { return true; }
        }

        internal static ScriptCodeForkExpression Parse(IEnumerator<KeyValuePair<Lexeme.Position, Lexeme>> lexer, params Lexeme[] terminator)
        {
            if (terminator == null || terminator.Length == 0) terminator = new[] { Punctuation.Semicolon };
            lexer.MoveNext(true);   //pass through fork keyword
            var fork = new ScriptCodeForkExpression();
            //Parse asynchronous task body.
            switch (lexer.Current.Value == Punctuation.LeftBrace)
            {
                case true:
                    Parser.ParseStatements(lexer, fork.Body, Punctuation.RightBrace);
                    break;
                default:
                    fork.Body.Add(Parser.ParseExpression, lexer, terminator);
                    break;
            }
            return fork;
        }

        /// <summary>
        /// Determines whether this expression represents the same tree as other expression.
        /// </summary>
        /// <param name="other">Other expression tree to compare.</param>
        /// <returns><see langword="true"/> if this expression represents the same tree as other expression; otherwise, <see langword="false"/>.</returns>
        public bool Equals(ScriptCodeForkExpression other)
        {
            return other != null &&
                ScriptCodeStatementCollection.TheSame(Body, other.Body);
        }

        /// <summary>
        /// Determines whether this expression represents the same tree as other expression.
        /// </summary>
        /// <param name="other">Other expression tree to compare.</param>
        /// <returns><see langword="true"/> if this expression represents the same tree as other expression; otherwise, <see langword="false"/>.</returns>
        public override bool Equals(ScriptCodeExpression other)
        {
            return Equals(other as ScriptCodeForkExpression);
        }

        /// <summary>
        /// Returns a LINQ expression that produces this object.
        /// </summary>
        /// <returns></returns>
        protected override Expression Restore()
        {
            var ctor = LinqHelpers.BodyOf<ScriptCodeStatement[], ScriptCodeForkExpression, NewExpression>(body => new ScriptCodeForkExpression(body));
            return ctor.Update(new[] { Body.NewArray() });
        }

        internal override void Verify()
        {
        }

        internal override ScriptCodeExpression Visit(ISyntaxTreeNode parent, Converter<ISyntaxTreeNode, ISyntaxTreeNode> visitor)
        {
            Body.Visit(this, visitor);
            return visitor.Invoke(this) as ScriptCodeExpression ?? this;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected override ScriptCodeExpression Clone()
        {
            return new ScriptCodeForkExpression(Extensions.Clone(Body));
        }
    }
}
