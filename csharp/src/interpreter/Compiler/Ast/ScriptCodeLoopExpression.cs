using System;
using System.CodeDom;
using System.Linq.Expressions;

namespace DynamicScript.Compiler.Ast
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;

    /// <summary>
    /// Represents an abstract class for loop expressions.
    /// </summary>
    [ComVisible(false)]
    [Serializable]
    public abstract class ScriptCodeLoopExpression : ScriptCodeExpression
    {
        #region Nested Types

        /// <summary>
        /// Represents for-loop grouping method.
        /// </summary>
        [ComVisible(false)]
        [Serializable]
        public abstract class YieldGrouping: ISyntaxTreeNode
        {
            internal YieldGrouping()
            {
            }

            /// <summary>
            /// Converts binary operator to the grouping method.
            /// </summary>
            /// <param name="operator">The binary operator that supplies iteration result grouping.</param>
            /// <returns>for-loop grouping method.</returns>
            public static implicit operator YieldGrouping(ScriptCodeBinaryOperatorType @operator)
            {
                return new OperatorGrouping(@operator);
            }

            /// <summary>
            /// Converts expression that represents action object into the grouping method.
            /// </summary>
            /// <param name="expression">The expression that represents action object. Cannot be <see langword="null"/>.</param>
            /// <returns>for-loop grouping method.</returns>
            /// <exception cref="System.ArgumentNullException"><paramref name="expression"/> is <see langword="null"/>.</exception>
            public static implicit operator YieldGrouping(ScriptCodeExpression expression)
            {
                return new CustomGrouping(expression);
            }

            internal abstract Expression Restore();

            Expression IRestorable.Restore()
            {
                return Restore();
            }

            bool ISyntaxTreeNode.Completed
            {
                get { return true; }
            }

            void ISyntaxTreeNode.Verify()
            {
            }

            internal YieldGrouping Visit(ISyntaxTreeNode parent, Converter<ISyntaxTreeNode, ISyntaxTreeNode> visitor)
            {
                return visitor.Invoke(this) as YieldGrouping ?? this;
            }

            ISyntaxTreeNode ISyntaxTreeNode.Visit(ISyntaxTreeNode parent, Converter<ISyntaxTreeNode, ISyntaxTreeNode> visitor)
            {
                return Visit(parent, visitor);
            }

            /// <summary>
            /// 
            /// </summary>
            /// <returns></returns>
            protected abstract YieldGrouping Clone();

            object ICloneable.Clone()
            {
                return Clone();
            }
        }

        /// <summary>
        /// Represents grouping operator.
        /// This class cannot be inherited.
        /// </summary>
        [ComVisible(false)]
        [Serializable]
        public sealed class OperatorGrouping : YieldGrouping, IEquatable<OperatorGrouping>
        {
            /// <summary>
            /// Represents binary operator that is used to combine loop result.
            /// </summary>
            public readonly ScriptCodeBinaryOperatorType Operator;

            /// <summary>
            /// Initializes a new loop result combiner that is based on binary operator.
            /// </summary>
            /// <param name="operator"></param>
            public OperatorGrouping(ScriptCodeBinaryOperatorType @operator)
            {
                Operator = @operator;
            }

            /// <summary>
            /// Returns a string representation of the grouping method.
            /// </summary>
            /// <returns>The string representation of the grouping method.</returns>
            public override string ToString()
            {
                return ScriptCodeBinaryOperatorExpression.ToString(Operator);
            }

            /// <summary>
            /// Determines whether this object describes the same grouping mechanism as other.
            /// </summary>
            /// <param name="other">Other grouping mechanism to compare.</param>
            /// <returns></returns>
            public bool Equals(OperatorGrouping other)
            {
                return other != null && Operator == other.Operator;
            }

            /// <summary>
            /// Determines whether this object describes the same grouping mechanism as other.
            /// </summary>
            /// <param name="other">Other grouping mechanism to compare.</param>
            /// <returns></returns>
            public override bool Equals(object other)
            {
                return Equals(other as OperatorGrouping);
            }

            /// <summary>
            /// Computes a hash code for this grouping mechanism.
            /// </summary>
            /// <returns>A hash code of this grouping mechanism.</returns>
            public override int GetHashCode()
            {
                return Operator.GetHashCode();
            }

            internal override Expression Restore()
            {
                var ctor = LinqHelpers.BodyOf<ScriptCodeBinaryOperatorType, OperatorGrouping, NewExpression>(op => new OperatorGrouping(op));
                return ctor.Update(new[] { LinqHelpers.Constant(Operator) });
            }

            /// <summary>
            /// 
            /// </summary>
            /// <returns></returns>
            protected override YieldGrouping Clone()
            {
                return new OperatorGrouping(Operator);
            }
        }

        /// <summary>
        /// Represents grouping expression.
        /// This class cannot be inherited.
        /// </summary>
        [ComVisible(false)]
        [Serializable]
        public sealed class CustomGrouping : YieldGrouping, IEquatable<CustomGrouping>
        {
            /// <summary>
            /// Represents an expression that reference the action used to combine loop results.
            /// </summary>
            public readonly ScriptCodeExpression GroupingAction;

            /// <summary>
            /// 
            /// </summary>
            /// <param name="expression"></param>
            public CustomGrouping(ScriptCodeExpression expression)
            {
                if (expression == null) throw new ArgumentNullException("expression");
                GroupingAction = expression;
            }
            
            /// <summary>
            /// Returns a string representation of the grouping method.
            /// </summary>
            /// <returns>The string representation of the grouping method.</returns>
            public override string ToString()
            {
                return GroupingAction != null ? GroupingAction.ToString() : String.Empty;
            }

            /// <summary>
            /// Determines whether this object describes the same grouping mechanism as other.
            /// </summary>
            /// <param name="other">Other grouping mechanism to compare.</param>
            /// <returns></returns>
            public bool Equals(CustomGrouping other)
            {
                return other != null && Equals(GroupingAction, other.GroupingAction);
            }

            /// <summary>
            /// Determines whether this object describes the same grouping mechanism as other.
            /// </summary>
            /// <param name="other">Other grouping mechanism to compare.</param>
            /// <returns></returns>
            public override bool Equals(object other)
            {
                return Equals(other as CustomGrouping);
            }

            /// <summary>
            /// Computes a hash code for this grouping mechanism.
            /// </summary>
            /// <returns>A hash code of this grouping mechanism.</returns>
            public override int GetHashCode()
            {
                return GroupingAction.GetHashCode();
            }

            internal override Expression Restore()
            {
                var ctor = LinqHelpers.BodyOf<ScriptCodeExpression, CustomGrouping, NewExpression>(act => new CustomGrouping(act));
                return ctor.Update(new[] { LinqHelpers.Restore(GroupingAction) });
            }

            /// <summary>
            /// 
            /// </summary>
            /// <returns></returns>
            protected override YieldGrouping Clone()
            {
                return new CustomGrouping(Extensions.Clone(GroupingAction));
            }
        }
        #endregion

        /// <summary>
        /// Represents body of the loop.
        /// </summary>
        public readonly ScriptCodeExpressionStatement Body;
        private bool m_suppressResult;

        internal ScriptCodeLoopExpression(ScriptCodeExpressionStatement body = null)
        {
            Body = body ?? new ScriptCodeExpressionStatement(ScriptCodeVoidExpression.Instance);
            m_suppressResult = false;
        }

        /// <summary>
        /// Gets or sets iteration result grouping method.
        /// </summary>
        public YieldGrouping Grouping
        {
            get;
            set;
        }

        /// <summary>
        /// Gets a value indicating whether the loop expression is completed.
        /// </summary>
        internal override bool Completed
        {
            get { return Body.Completed; }
        }

        /// <summary>
        /// Gets or sets a value indicating that the 
        /// </summary>
        public bool SuppressResult
        {
            get { return Grouping == null && m_suppressResult; }
            set { m_suppressResult = value; }
        }

        /// <summary>
        /// Converts expression to the statement.
        /// </summary>
        /// <param name="expr">The expression to be converted.</param>
        /// <returns>The statement that encapsulates the expression.</returns>
        public static explicit operator ScriptCodeExpressionStatement(ScriptCodeLoopExpression expr)
        {
            return new ScriptCodeExpressionStatement<ScriptCodeLoopExpression>(expr);
        }
    }
}
