using System;
using System.CodeDom;
using System.Linq.Expressions;

namespace DynamicScript.Compiler.Ast
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;
    using StringBuilder = System.Text.StringBuilder;

    /// <summary>
    /// Represents for-loop expression.
    /// This class cannot be inherited.
    /// </summary>
    [ComVisible(false)]
    [Serializable]
    public sealed class ScriptCodeForLoopExpression: ScriptCodeLoopWithVariableExpression, IEquatable<ScriptCodeForLoopExpression>
    {
        private ScriptCodeExpression m_condition;

        /// <summary>
        /// Initializes a new 'for' loop.
        /// </summary>
        /// <param name="body"></param>
        public ScriptCodeForLoopExpression(ScriptCodeExpressionStatement body = null)
            : base(body)
        {
        }

        /// <summary>
        /// Initializes a new 'for-loop' expression.
        /// </summary>
        /// <param name="loopVar"></param>
        /// <param name="condition"></param>
        /// <param name="grouping"></param>
        /// <param name="suppressResult"></param>
        /// <param name="body"></param>
        public ScriptCodeForLoopExpression(LoopVariable loopVar, ScriptCodeExpression condition, YieldGrouping grouping, bool suppressResult, ScriptCodeExpressionStatement body)
            : this(body)
        {
            Variable = loopVar;
            Condition = condition;
            SuppressResult = suppressResult;
            Grouping = grouping;
        }

        /// <summary>
        /// Gets or sets loop condition expression.
        /// </summary>
        /// <remarks>This is a required part of the expression.</remarks>
        public ScriptCodeExpression Condition
        {
            get { return m_condition ?? ScriptCodeVoidExpression.Instance; }
            set { m_condition = value; }
        }

        /// <summary>
        /// Determines whether this expression represents the same tree as other expression.
        /// </summary>
        /// <param name="other">Other expression tree to compare.</param>
        /// <returns><see langword="true"/> if this expression represents the same tree as other expression; otherwise, <see langword="false"/>.</returns>
        public bool Equals(ScriptCodeForLoopExpression other)
        {
            return other != null &&
                Equals(Grouping, other.Grouping) &&
                Equals(Condition, other.Condition) &&
                Equals(Body, other.Body);
        }

        /// <summary>
        /// Determines whether this expression represents the same tree as other expression.
        /// </summary>
        /// <param name="other">Other expression tree to compare.</param>
        /// <returns><see langword="true"/> if this expression represents the same tree as other expression; otherwise, <see langword="false"/>.</returns>
        public override bool Equals(ScriptCodeExpression other)
        {
            return Equals(other as ScriptCodeForLoopExpression);
        }

        /// <summary>
        /// Returns a LINQ expression that produces this object.
        /// </summary>
        /// <returns></returns>
        protected override Expression Restore()
        {
            var ctor = LinqHelpers.BodyOf<LoopVariable, ScriptCodeExpression, YieldGrouping, bool, ScriptCodeExpressionStatement, ScriptCodeForLoopExpression, NewExpression>((loopvar, cond, grp, sup, body) => new ScriptCodeForLoopExpression(loopvar, cond, grp, sup, body));
            return ctor.Update(new[] { LinqHelpers.Restore(Variable), LinqHelpers.Restore(Condition), LinqHelpers.Restore(Grouping), LinqHelpers.Constant(SuppressResult), LinqHelpers.Restore(Body) });
        }

        internal override void Verify()
        {
        }

        internal override ScriptCodeExpression Visit(ISyntaxTreeNode parent, Converter<ISyntaxTreeNode, ISyntaxTreeNode> visitor)
        {
            if (Grouping != null) Grouping = Grouping.Visit(this, visitor);
            if (Condition != null) Condition = Condition.Visit(this, visitor);
            if (Variable != null) Variable = Variable.Visit(this, visitor) as LoopVariable;
            Body.Visit(this, visitor);
            return visitor.Invoke(this) as ScriptCodeExpression;
        }

        /// <summary>
        /// Gets a value indicating whether the loop expression can be optimized.
        /// </summary>
        public override bool CanReduce
        {
            get { return Condition.CanReduce || Body.CanReduce; }
        }

        /// <summary>
        /// Simplifies the loop expression.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public override ScriptCodeExpression Reduce(InterpretationContext context)
        {
            switch (CanReduce)
            {
                case true:
                    var result = new ScriptCodeForLoopExpression(Variable, Condition.Reduce(context), Grouping, SuppressResult, Body);
                    result.Body.Reduce(context);
                    return result;
                default: return this;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected override ScriptCodeExpression Clone()
        {
            return new ScriptCodeForLoopExpression(Extensions.Clone(Body))
            {
                Grouping = Extensions.Clone(this.Grouping),
                Condition = Extensions.Clone(Condition),
                SuppressResult = this.SuppressResult,
                Variable = Extensions.Clone(Variable)
            };
        }

        /// <summary>
        /// Returns a string representation of this expression.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            const char WhiteSpace = Lexeme.WhiteSpace;
            var result = new StringBuilder();
            result.Append(String.Concat(Punctuation.LeftBracket, Keyword.For, WhiteSpace, Variable, WhiteSpace, Keyword.While, Condition, WhiteSpace));
            if (Grouping != null) result.Append(String.Concat(Keyword.GroupBy, WhiteSpace, Grouping, WhiteSpace));
            result.Append(String.Concat(Keyword.Do, WhiteSpace, Body.Expression, Punctuation.RightBracket));
            return result.ToString();
        }
    }
}
