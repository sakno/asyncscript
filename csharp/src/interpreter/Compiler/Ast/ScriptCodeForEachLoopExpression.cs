using System;
using System.CodeDom;
using System.Linq.Expressions;

namespace DynamicScript.Compiler.Ast
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;
    using StringBuilder = System.Text.StringBuilder;

    /// <summary>
    /// Represents for-each loop expression.
    /// This class cannot be inherited.
    /// </summary>
    [ComVisible(false)]
    [Serializable]
    public sealed class ScriptCodeForEachLoopExpression : ScriptCodeLoopWithVariableExpression, IEquatable<ScriptCodeForEachLoopExpression>
    {
        /// <summary>
        /// Initializes a new 'for-each' loop.
        /// </summary>
        /// <param name="body"></param>
        public ScriptCodeForEachLoopExpression(ScriptCodeExpressionStatement body = null)
            : base(body)
        {
        }

        /// <summary>
        /// Initializes a new 'for-each' loop expression.
        /// </summary>
        /// <param name="loopVar"></param>
        /// <param name="iterator"></param>
        /// <param name="grouping"></param>
        /// <param name="suppressResult"></param>
        /// <param name="body"></param>
        public ScriptCodeForEachLoopExpression(LoopVariable loopVar, ScriptCodeExpression iterator, YieldGrouping grouping, bool suppressResult, ScriptCodeExpressionStatement body)
            : this(body)
        {
            Variable = loopVar;
            Iterator = iterator;
            Grouping = grouping;
            SuppressResult = suppressResult;
        }

        /// <summary>
        /// Gets or sets iterator.
        /// </summary>
        /// <remarks>This is a required part of the expression.</remarks>
        public ScriptCodeExpression Iterator
        {
            get;
            set;
        }

        internal override bool Completed
        {
            get
            {
                return Iterator != null && base.Completed;
            }
        }

        /// <summary>
        /// Returns a string representation of the loop.
        /// </summary>
        /// <returns>The string representation of the loop.</returns>
        public override string ToString()
        {
            const char WhiteSpace = ' ';
            var result = new StringBuilder();
            result.Append((char)Punctuation.LeftBracket);
            result.Append(String.Concat(Keyword.For, WhiteSpace));
            if (Variable.Temporary) result.Append(String.Concat(Keyword.Var, WhiteSpace));
            result.Append(String.Concat(Variable.Name, WhiteSpace, Keyword.In, WhiteSpace, Iterator, WhiteSpace));
            if (Grouping != null) result.Append(String.Concat(Keyword.GroupBy, WhiteSpace, Grouping, WhiteSpace));
            result.Append(String.Concat(Keyword.Do, WhiteSpace));
            result.Append(Body.Expression);
            result.Append((char)Punctuation.RightBracket);
            return result.ToString();
        }

        /// <summary>
        /// Determines whether this expression represents the same tree as other expression.
        /// </summary>
        /// <param name="other">Other expression tree to compare.</param>
        /// <returns><see langword="true"/> if this expression represents the same tree as other expression; otherwise, <see langword="false"/>.</returns>
        public bool Equals(ScriptCodeForEachLoopExpression other)
        {
            return other != null &&
                Equals(Grouping, other.Grouping) &&
                Equals(Variable, other.Variable) &&
                Equals(Body, other.Body);
        }

        /// <summary>
        /// Determines whether this expression represents the same tree as other expression.
        /// </summary>
        /// <param name="other">Other expression tree to compare.</param>
        /// <returns><see langword="true"/> if this expression represents the same tree as other expression; otherwise, <see langword="false"/>.</returns>
        public override bool Equals(ScriptCodeExpression other)
        {
            return Equals(other as ScriptCodeForEachLoopExpression);
        }

        /// <summary>
        /// Returns a LINQ expression that produces this object.
        /// </summary>
        /// <returns></returns>
        protected override Expression Restore()
        {
            var ctor = LinqHelpers.BodyOf<LoopVariable, ScriptCodeExpression, YieldGrouping, bool, ScriptCodeExpression, ScriptCodeForEachLoopExpression, NewExpression>((loopvar, iter, gr, sup, body) => new ScriptCodeForEachLoopExpression(loopvar, iter, gr, sup, body));
            return ctor.Update(new[] { LinqHelpers.Restore(Variable), LinqHelpers.Restore(Iterator), LinqHelpers.Restore(Grouping), LinqHelpers.Constant(SuppressResult), LinqHelpers.Restore(Body) });
        }

        internal override void Verify()
        {
        }

        internal override ScriptCodeExpression Visit(ISyntaxTreeNode parent, Converter<ISyntaxTreeNode, ISyntaxTreeNode> visitor)
        {
            Body.Visit(this, visitor);
            if (Grouping != null) Grouping = Grouping.Visit(this, visitor);
            if (Iterator != null) Iterator = Iterator.Visit(this, visitor);
            if (Variable != null) Variable = Variable.Visit(this, visitor) as LoopVariable;
            return visitor.Invoke(this) as ScriptCodeExpression ?? this;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected override ScriptCodeExpression Clone()
        {
            return new ScriptCodeForEachLoopExpression(Extensions.Clone(Body))
            {
                Grouping = Extensions.Clone(Grouping),
                Iterator = Extensions.Clone(Iterator),
                SuppressResult = this.SuppressResult,
                Variable = Extensions.Clone(Variable)
            };
        }
    }
}
