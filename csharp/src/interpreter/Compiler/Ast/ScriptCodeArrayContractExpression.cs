using System;
using System.Linq.Expressions;

namespace DynamicScript.Compiler.Ast
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;
    using CodeExpression = System.CodeDom.CodeExpression;

    /// <summary>
    /// Represents array contract.
    /// This class cannot be inherited.
    /// </summary>
    [ComVisible(false)]
    [Serializable]
    public sealed class ScriptCodeArrayContractExpression : ScriptCodeExpression, IEquatable<ScriptCodeArrayContractExpression>
    {
        private int m_rank;

        /// <summary>
        /// Initializes a new array contract definition.
        /// </summary>
        /// <param name="rank"></param>
        /// <param name="elementContract"></param>
        public ScriptCodeArrayContractExpression(int rank = 1, ScriptCodeExpression elementContract = null)
        {
            m_rank = rank;
            ElementContract = elementContract;
        }

        /// <summary>
        /// Gets or sets rank of the array.
        /// </summary>
        public int Rank
        {
            get { return m_rank > 0 ? m_rank : 1; }
            set { m_rank = value; }
        }

        /// <summary>
        /// Gets or sets array element contract.
        /// </summary>
        public ScriptCodeExpression ElementContract
        {
            get;
            set;
        }

        internal override bool Completed
        {
            get { return ElementContract != null; }
        }

        /// <summary>
        /// Converts expression to the statement.
        /// </summary>
        /// <param name="expr">The expression to be converted.</param>
        /// <returns>The statement that encapsulates the expression.</returns>
        public static explicit operator ScriptCodeExpressionStatement(ScriptCodeArrayContractExpression expr)
        {
            return new ScriptCodeExpressionStatement<ScriptCodeArrayContractExpression>(expr);
        }

        /// <summary>
        /// Determines whether this expression represents the same tree as other expression.
        /// </summary>
        /// <param name="other">Other expression tree to compare.</param>
        /// <returns><see langword="true"/> if this expression represents the same tree as other expression; otherwise, <see langword="false"/>.</returns>
        public bool Equals(ScriptCodeArrayContractExpression other)
        {
            return other != null &&
                Completed &&
                other.Completed &&
                Rank == other.Rank &&
                Equals(ElementContract, other.ElementContract);
        }

        /// <summary>
        /// Determines whether this expression represents the same tree as other expression.
        /// </summary>
        /// <param name="other">Other expression tree to compare.</param>
        /// <returns><see langword="true"/> if this expression represents the same tree as other expression; otherwise, <see langword="false"/>.</returns>
        public override bool Equals(ScriptCodeExpression other)
        {
            return Equals(other as ScriptCodeArrayContractExpression);
        }

        /// <summary>
        /// Returns a string representation of this expression.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Concat(ElementContract, Punctuation.LeftSquareBracket, new string(Lexeme.Comma, Rank - 1), Punctuation.RightSquareBracket);
        }

        /// <summary>
        /// Returns a LINQ expression that produces this object.
        /// </summary>
        /// <returns></returns>
        protected override Expression Restore()
        {
            var ctor = LinqHelpers.BodyOf<int, ScriptCodeExpression, ScriptCodeArrayContractExpression, NewExpression>((rank, elem) => new ScriptCodeArrayContractExpression(rank, elem));
            return ctor.Update(new[] { LinqHelpers.Constant(Rank), LinqHelpers.Restore(ElementContract) });
        }

        internal override void Verify()
        {
        }

        internal override ScriptCodeExpression Visit(ISyntaxTreeNode parent, Converter<ISyntaxTreeNode, ISyntaxTreeNode> visitor)
        {
            if (ElementContract != null) ElementContract = visitor.Invoke(ElementContract) as ScriptCodeExpression;
            return visitor.Invoke(this) as ScriptCodeExpression;
        }

        /// <summary>
        /// Creates a new deep copy of this expression.
        /// </summary>
        /// <returns></returns>
        protected override ScriptCodeExpression Clone()
        {
            return new ScriptCodeArrayContractExpression(Rank, ElementContract);
        }
    }
}
