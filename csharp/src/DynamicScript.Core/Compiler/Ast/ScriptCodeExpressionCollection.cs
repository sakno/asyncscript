using System;
using System.Collections.Generic;
using System.CodeDom;
using System.Linq.Expressions;

namespace DynamicScript.Compiler.Ast
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;

    /// <summary>
    /// Represents collection of sript expressions.
    /// This class cannot be inherited.
    /// </summary>
    [ComVisible(false)]
    public sealed class ScriptCodeExpressionCollection: CodeExpressionCollection, ICollection<ScriptCodeExpression>, ISyntaxTreeNode
    {
        /// <summary>
        /// Initializes a new collection of expressions.
        /// </summary>
        /// <param name="expressions">An array of expressions to be added into the collection.</param>
        public ScriptCodeExpressionCollection(params ScriptCodeExpression[] expressions)
            : base(expressions??new ScriptCodeExpression[0])
        {
        }

        private ScriptCodeExpressionCollection(ScriptCodeExpressionCollection other)
            : base(other)
        {
        }

        /// <summary>
        /// Gets or sets expression at the specified inde in this collection.
        /// </summary>
        /// <param name="index">An index of the expression of the collection.</param>
        /// <returns></returns>
        public new ScriptCodeExpression this[int index]
        {
            get { return base[index] as ScriptCodeExpression; }
            set { base[index] = value; }
        }

        /// <summary>
        /// Adds a new expression to the collection.
        /// </summary>
        /// <param name="expr">An expression to add.</param>
        /// <returns>The index at which the new element was inserted.</returns>
        public int Add(ScriptCodeExpression expr)
        {
            return base.Add(expr);
        }

        void ICollection<ScriptCodeExpression>.Add(ScriptCodeExpression expr)
        {
            Add(expr);
        }

        /// <summary>
        /// Returns a value that indicates whether the collection contains the specified expression.
        /// </summary>
        /// <param name="expr"></param>
        /// <returns><see langword="true"/> if the collection contains the specified object; otherwise, <see langword="false"/>.</returns>
        bool ICollection<ScriptCodeExpression>.Contains(ScriptCodeExpression expr)
        {
            return base.Contains(expr);
        }

        /// <summary>
        /// Copies an expressions contain in this collection to the array.
        /// </summary>
        /// <param name="array">An output array.</param>
        /// <param name="arrayIndex"></param>
        void ICollection<ScriptCodeExpression>.CopyTo(ScriptCodeExpression[] array, int arrayIndex)
        {
            base.CopyTo(array, arrayIndex);
        }

        bool ICollection<ScriptCodeExpression>.IsReadOnly
        {
            get { return false; }
        }

        bool ICollection<ScriptCodeExpression>.Remove(ScriptCodeExpression item)
        {
             base.Remove(item);
             return true;
        }

        /// <summary>
        /// Returns an enumerator through all script expressions.
        /// </summary>
        /// <returns></returns>
        public new IEnumerator<ScriptCodeExpression> GetEnumerator()
        {
            var enumerator = base.GetEnumerator();
            while (enumerator.MoveNext() && enumerator.Current is ScriptCodeExpression) yield return (ScriptCodeExpression)enumerator.Current;

        }

        bool ISyntaxTreeNode.Completed
        {
            get { return LinqHelpers.IsTrue(this, expr => expr.Completed); }
        }

        void ISyntaxTreeNode.Verify()
        {
            LinqHelpers.ForEach(this, expr => expr.Verify());
        }

        internal ISyntaxTreeNode Visit(ISyntaxTreeNode parent, Converter<ISyntaxTreeNode, ISyntaxTreeNode> visitor)
        {
            for (var i = 0; i < Count; i++)
                this[i] = this[i].Visit(this, visitor);
            return visitor.Invoke(this);
        } 

        ISyntaxTreeNode ISyntaxTreeNode.Visit(ISyntaxTreeNode parent, Converter<ISyntaxTreeNode, ISyntaxTreeNode> visitor)
        {
            return Visit(parent, visitor);
        }

        object ICloneable.Clone()
        {
            return new ScriptCodeExpressionCollection(Extensions.CloneCollection(this));
        }

        Expression IRestorable.Restore()
        {
            var ctor = LinqHelpers.BodyOf<ScriptCodeExpression[], ScriptCodeExpressionCollection, NewExpression>(elems => new ScriptCodeExpressionCollection(elems));
            return ctor.Update(new[] { LinqHelpers.NewArray(this) });
        }

        /// <summary>
        /// Returns a string representation of this collection.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Join(Punctuation.Comma, this);
        }

        internal static bool TheSame(CodeExpressionCollection collection1, CodeExpressionCollection collection2)
        {
            if (collection1 == null) collection1 = new CodeExpressionCollection();
            if (collection2 == null) collection2 = new CodeExpressionCollection();
            switch (collection1.Count == collection2.Count)
            {
                case true:
                    for (var i = 0; i < collection1.Count; i++)
                        if (Equals(collection1[i], collection2[i])) continue;
                        else return false;
                    return true;
                default: return false;
            }
        }
    }
}
