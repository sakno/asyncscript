﻿using System;
using System.CodeDom;
using System.Linq.Expressions;

namespace DynamicScript.Compiler.Ast
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;

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
        public ScriptCodeForLoopExpression(ScriptCodeExpression body)
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
        public ScriptCodeForLoopExpression(LoopVariable loopVar, ScriptCodeExpression condition, YieldGrouping grouping, bool suppressResult, ScriptCodeExpression body)
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
            set
            {
                m_condition = value;
                OnPropertyChanged("Condition");
            }
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
            var ctor = LinqHelpers.BodyOf<LoopVariable, ScriptCodeExpression, YieldGrouping, bool, ScriptCodeExpression, ScriptCodeForLoopExpression, NewExpression>((loopvar, cond, grp, sup, body) => new ScriptCodeForLoopExpression(loopvar, cond, grp, sup, body));
            return ctor.Update(new[] { LinqHelpers.Restore(Variable), LinqHelpers.Restore(Condition), LinqHelpers.Restore(Grouping), LinqHelpers.Constant(SuppressResult), LinqHelpers.Restore(Body) });
        }

        internal override void Verify()
        {
        }

        internal override ScriptCodeExpression Visit(ISyntaxTreeNode parent, Converter<ISyntaxTreeNode, ISyntaxTreeNode> visitor)
        {
            Body.Visit(this, visitor);
            if (Grouping != null) Grouping = Grouping.Visit(this, visitor);
            if (Condition != null) Condition = Condition.Visit(this, visitor);
            if (Variable != null) Variable = Variable.Visit(this, visitor) as LoopVariable;
            Body = Body.Visit(this, visitor) as ScriptCodeExpression;
            return visitor.Invoke(this) as ScriptCodeExpression;
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


    }
}
