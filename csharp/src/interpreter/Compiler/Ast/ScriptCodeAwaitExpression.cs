using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace DynamicScript.Compiler.Ast
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;

    /// <summary>
    /// Represents synchronization expression.
    /// This class cannot be inherited.
    /// </summary>
    [ComVisible(false)]
    [Serializable]
    public sealed class ScriptCodeAwaitExpression: ScriptCodeExpression, IEquatable<ScriptCodeAwaitExpression>
    {

        /// <summary>
        /// Initializes a new synchronization expression.
        /// </summary>
        /// <param name="asyncRes"></param>
        /// <param name="synchronizer"></param>
        public ScriptCodeAwaitExpression(ScriptCodeExpression asyncRes = null, ScriptCodeExpression synchronizer = null)
        {
            AsyncResult = asyncRes;
            Synchronizer = synchronizer;
        }

        /// <summary>
        /// Gets or sets the asynchronous expresssion to synchronize.
        /// </summary>
        public ScriptCodeExpression AsyncResult
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets synchronizer expression.
        /// </summary>
        /// <remarks>This is an optional part of this expression.</remarks>
        public ScriptCodeExpression Synchronizer
        {
            get;
            set;
        }

        internal override bool Completed
        {
            get { return AsyncResult != null; }
        }

        internal static ScriptCodeAwaitExpression Parse(IEnumerator<KeyValuePair<Lexeme.Position, Lexeme>> lexer, params Lexeme[] terminator)
        {
            if (terminator == null || terminator.LongLength == 0L) terminator = new[] { Punctuation.Semicolon };
            lexer.MoveNext(true);   //pass through await keyword
            var result = new ScriptCodeAwaitExpression();
            //parse asynchronous result
            result.AsyncResult = Parser.ParseExpression(lexer, terminator + Keyword.While);
            if (lexer.Current.Value == Keyword.While)
            {
                lexer.MoveNext(true);   //pass through while keyword
                //Parse synchronizer body.
                result.Synchronizer = Parser.ParseExpression(lexer, terminator);
            }
            return result;
        }

        /// <summary>
        /// Determines whether this expression represents the same tree as other expression.
        /// </summary>
        /// <param name="other">Other expression tree to compare.</param>
        /// <returns><see langword="true"/> if this expression represents the same tree as other expression; otherwise, <see langword="false"/>.</returns>
        public bool Equals(ScriptCodeAwaitExpression other)
        {
            return other != null &&
                Equals(AsyncResult, other.AsyncResult) &&
                Equals(Synchronizer, other.Synchronizer);
        }

        /// <summary>
        /// Determines whether this expression represents the same tree as other expression.
        /// </summary>
        /// <param name="other">Other expression tree to compare.</param>
        /// <returns><see langword="true"/> if this expression represents the same tree as other expression; otherwise, <see langword="false"/>.</returns>
        public override bool Equals(ScriptCodeExpression other)
        {
            return Equals(other as ScriptCodeAwaitExpression);
        }

        /// <summary>
        /// Returns a LINQ expression that produces this object.
        /// </summary>
        /// <returns></returns>
        protected override Expression Restore()
        {
            var ctor = LinqHelpers.BodyOf<ScriptCodeExpression, ScriptCodeExpression, ScriptCodeAwaitExpression, NewExpression>((ar, sync) => new ScriptCodeAwaitExpression(ar, sync));
            return ctor.Update(LinqHelpers.RestoreMany(AsyncResult, Synchronizer));
        }

        internal override void Verify()
        {
        }

        internal override ScriptCodeExpression Visit(ISyntaxTreeNode parent, Converter<ISyntaxTreeNode, ISyntaxTreeNode> visitor)
        {
            if (AsyncResult != null) AsyncResult = AsyncResult.Visit(this, visitor);
            if (Synchronizer != null) Synchronizer = Synchronizer.Visit(this, visitor);
            return visitor.Invoke(this) as ScriptCodeExpression ?? this;
        }

        /// <summary>
        /// Returns a new deep copy of this expression.
        /// </summary>
        /// <returns></returns>
        protected override ScriptCodeExpression Clone()
        {
            return new ScriptCodeAwaitExpression(Extensions.Clone(AsyncResult), Extensions.Clone(Synchronizer));
        }
    }
}
