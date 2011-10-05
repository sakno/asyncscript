using System;
using System.CodeDom;
using System.Linq.Expressions;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel;

namespace DynamicScript.Compiler.Ast
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;
    using StringBuilder = System.Text.StringBuilder;

    /// <summary>
    /// Represents an abstract class for all script statements.
    /// </summary>
    [ComVisible(false)]
    [Serializable]
    public abstract class ScriptCodeStatement: CodeStatement, ISyntaxTreeNode, IEquatable<ScriptCodeStatement>, INotifyPropertyChanged
    {
        private int? m_hash;
        private PropertyChangedEventHandler PropertyChanged;

        internal ScriptCodeStatement()
        {
            m_hash = null;
            PropertyChanged = null;
        }

        internal abstract bool Completed
        {
            get;
        }

        bool ISyntaxTreeNode.Completed
        {
            get { return Completed; }
        }

        internal abstract ScriptCodeStatement Visit(ISyntaxTreeNode parent, Converter<ISyntaxTreeNode, ISyntaxTreeNode> visitor);

        ISyntaxTreeNode ISyntaxTreeNode.Visit(ISyntaxTreeNode parent, Converter<ISyntaxTreeNode, ISyntaxTreeNode> visitor)
        {
            return Visit(parent, visitor);
        }

        internal void Verify()
        {
            if (!Completed)
                throw CodeAnalysisException.IncompletedExpression(LinePragma);
        }

        void ISyntaxTreeNode.Verify()
        {
            Verify();
        }

        /// <summary>
        /// Creates a new deep clone of this statement.
        /// </summary>
        /// <returns></returns>
        protected abstract ScriptCodeStatement Clone();

        object ICloneable.Clone()
        {
            return Clone();
        }

        /// <summary>
        /// Gets or sets debug information associated with this statement.
        /// </summary>
        public new ScriptDebugInfo LinePragma
        {
            get { return base.LinePragma as ScriptDebugInfo; }
            set { base.LinePragma = value; }
        }

        /// <summary>
        /// Computes a hash code for this
        /// </summary>
        /// <returns></returns>
        public sealed override int GetHashCode()
        {
            if (m_hash == null) m_hash = StringEqualityComparer.GetHashCode(ToString());
            return m_hash.Value;
        }

        /// <summary>
        /// Determines whether this statement is equal to another.
        /// </summary>
        /// <param name="other">Another statement to compare.</param>
        /// <returns></returns>
        public abstract bool Equals(ScriptCodeStatement other);

        /// <summary>
        /// Determines whether this statement is equal to another.
        /// </summary>
        /// <param name="other">Another statement to compare.</param>
        /// <returns></returns>
        public sealed override bool Equals(object other)
        {
            return Equals(other as ScriptCodeStatement);
        }

        /// <summary>
        /// Returns an expression that produces this collection.
        /// </summary>
        /// <returns></returns>
        protected abstract Expression Restore();

        Expression IRestorable.Restore()
        {
            return Restore();
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
        /// Changes internal state of this statement.
        /// </summary>
        /// <param name="propertyName"></param>
        protected void OnPropertyChanged(string propertyName)
        {
            m_hash = null;
            OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
        }

        internal static TSlot Visit<TSlot>(ISyntaxTreeNode parent, TSlot slot, Converter<ISyntaxTreeNode, ISyntaxTreeNode> visitor, Func<ISlot, TSlot> ctor)
            where TSlot : ISlot
        {
            switch (slot != null)
            {
                case true:
                    var visited = slot.Visit(parent, visitor) as ISlot;
                    if (visited == null) return slot;
                    else if (visited is TSlot) return (TSlot)visited;
                    else return ctor.Invoke(visited);
                default: return default(TSlot);
            }
        }
    }
}
