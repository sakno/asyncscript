using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace DynamicScript.Compiler.Ast
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;

    /// <summary>
    /// Represents asynchronous contract.
    /// This class cannot be inherited.
    /// </summary>
    [ComVisible(false)]
    [Serializable]
    public sealed class ScriptCodeAsyncExpression: ScriptCodeExpression, IEquatable<ScriptCodeAsyncExpression>
    {
        /// <summary>
        /// Initializes a new asynchronous contract provider.
        /// </summary>
        /// <param name="contract"></param>
        public ScriptCodeAsyncExpression(ScriptCodeExpression contract = null)
        {
            Contract = contract;
        }

        /// <summary>
        /// Gets or sets underlying contract.
        /// </summary>
        public ScriptCodeExpression Contract
        {
            get;
            set;
        }

        internal override bool Completed
        {
            get { return Contract != null; }
        }

        internal static ScriptCodeAsyncExpression Parse(IEnumerator<KeyValuePair<Lexeme.Position, Lexeme>> lexer, params Lexeme[] terminator)
        {
            lexer.MoveNext(true);   //pass through keyword
            return new ScriptCodeAsyncExpression { Contract = Parser.ParseExpression(lexer, terminator) };
        }

        /// <summary>
        /// Determines whether this expression represents the same tree as other expression.
        /// </summary>
        /// <param name="other">Other expression tree to compare.</param>
        /// <returns><see langword="true"/> if this expression represents the same tree as other expression; otherwise, <see langword="false"/>.</returns>
        public override bool Equals(ScriptCodeExpression other)
        {
            return Equals(other as ScriptCodeAsyncExpression);
        }

        /// <summary>
        /// Determines whether this expression represents the same tree as other expression.
        /// </summary>
        /// <param name="other">Other expression tree to compare.</param>
        /// <returns><see langword="true"/> if this expression represents the same tree as other expression; otherwise, <see langword="false"/>.</returns>
        public bool Equals(ScriptCodeAsyncExpression other)
        {
            return other != null &&
                Completed &&
                other.Completed &&
                Equals(Contract, other.Contract);
        }

        /// <summary>
        /// Returns a LINQ expression that produces this object.
        /// </summary>
        /// <returns></returns>
        protected override Expression Restore()
        {
            var ctor = LinqHelpers.BodyOf<ScriptCodeExpression, ScriptCodeAsyncExpression, NewExpression>(c => new ScriptCodeAsyncExpression(c));
            return ctor.Update(new[] { LinqHelpers.Restore(Contract) });
        }

        internal override void Verify()
        {
        }

        internal override ScriptCodeExpression Visit(ISyntaxTreeNode parent, Converter<ISyntaxTreeNode, ISyntaxTreeNode> visitor)
        {
            if (Contract != null) Contract = Contract.Visit(this, visitor);
            return visitor.Invoke(this) as ScriptCodeExpression ?? this;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected override ScriptCodeExpression Clone()
        {
            return new ScriptCodeAsyncExpression(Extensions.Clone(Contract));
        }
    }
}
