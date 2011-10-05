using System;
using System.CodeDom;
using System.Linq.Expressions;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel;

namespace DynamicScript.Compiler.Ast
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;

    /// <summary>
    /// Represents an abstract class for DynamicScript expressions.
    /// </summary>
    [Serializable]
    [ComVisible(false)]
    public abstract class ScriptCodeExpression : CodeExpression, ISyntaxTreeNode, IEquatable<ScriptCodeExpression>, INotifyPropertyChanged
    {
        #region Nested Types
        /// <summary>
        /// Represents expression formatting style.
        /// </summary>
        [ComVisible(false)]
        [Serializable]
        public enum FormattingStyle: byte
        {
            /// <summary>
            /// Represents default formatting style.
            /// </summary>
            Default = 0,

            /// <summary>
            /// Parenthesize expression string representation.
            /// </summary>
            Parenthesize
        }
        #endregion

        private int? m_hash;
        private PropertyChangedEventHandler PropertyChanged;

        internal ScriptCodeExpression()
        {
            m_hash = null;
            PropertyChanged = null;
        }

        /// <summary>
        /// Gets a value indicating that the expression is completed.
        /// </summary>
        internal abstract bool Completed
        {
            get;
        }

        /// <summary>
        /// Reduces this expression.
        /// </summary>
        /// <param name="context">Interpretation context.</param>
        /// <returns>A new reduced expression.</returns>
        public virtual ScriptCodeExpression Reduce(InterpretationContext context)
        {
            return this;
        }

        /// <summary>
        /// Gets a value indicating that this expression can be reduced.
        /// </summary>
        public virtual bool CanReduce
        {
            get { return false; }
        }

        bool ISyntaxTreeNode.Completed
        {
            get { return Completed; }
        }

        /// <summary>
        /// Determines whether this expression represents the same tree as other expression.
        /// </summary>
        /// <param name="other">Other expression tree to compare.</param>
        /// <returns><see langword="true"/> if this expression represents the same tree as other expression; otherwise, <see langword="false"/>.</returns>
        public abstract bool Equals(ScriptCodeExpression other);

        /// <summary>
        /// Determines whether this expression represents the same tree as other expression.
        /// </summary>
        /// <param name="other">Other expression tree to compare.</param>
        /// <returns><see langword="true"/> if this expression represents the same tree as other expression; otherwise, <see langword="false"/>.</returns>
        public sealed override bool Equals(object other)
        {
            return Equals(other as ScriptCodeExpression);
        }

        /// <summary>
        /// Computes hash code for this expression.
        /// </summary>
        /// <returns>A hash code that uniquely identifies this expression.</returns>
        public sealed override int GetHashCode()
        {
            if(m_hash==null)m_hash=StringEqualityComparer.GetHashCode(ToString());
            return m_hash.Value;
        }

        /// <summary>
        /// Converts this expression to the DynamicScript source code.
        /// </summary>
        /// <param name="style">Expression formatting style.</param>
        /// <returns>A string representation of this expression.</returns>
        public virtual string ToString(FormattingStyle style)
        {
            switch (style)
            {
                case FormattingStyle.Parenthesize:
                    return string.Concat(Punctuation.LeftBracket, ToString(), Punctuation.RightBracket);
                default:
                    return ToString();
            }
        }

        /// <summary>
        /// Returns an expression that produces 
        /// </summary>
        /// <returns></returns>
        protected abstract Expression Restore();

        Expression IRestorable.Restore()
        {
            return Restore();
        }

        internal abstract void Verify();

        void ISyntaxTreeNode.Verify()
        {
            Verify();
        }

        /// <summary>
        /// Visits this expression node.
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="visitor"></param>
        internal abstract ScriptCodeExpression Visit(ISyntaxTreeNode parent, Converter<ISyntaxTreeNode, ISyntaxTreeNode> visitor);

        ISyntaxTreeNode ISyntaxTreeNode.Visit(ISyntaxTreeNode parent, Converter<ISyntaxTreeNode, ISyntaxTreeNode> visitor)
        {
            return Visit(parent, visitor);
        }

        /// <summary>
        /// Clones this expression tree in-to-deep.
        /// </summary>
        /// <returns></returns>
        protected abstract ScriptCodeExpression Clone();

        object ICloneable.Clone()
        {
            return Clone();
        }

        event PropertyChangedEventHandler INotifyPropertyChanged.PropertyChanged
        {
            add { PropertyChanged += value; }
            remove { PropertyChanged -= value; }
        }

        private void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, e);
        }

        /// <summary>
        /// Changes internal state of this expression.
        /// </summary>
        /// <param name="propertyName"></param>
        protected void OnPropertyChanged(string propertyName)
        {
            m_hash = null;
            OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
        }
    }
}
