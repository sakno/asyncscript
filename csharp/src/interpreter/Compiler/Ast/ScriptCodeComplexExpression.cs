using System;
using System.Linq.Expressions;
using System.Collections.Generic;

namespace DynamicScript.Compiler.Ast
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;

    /// <summary>
    /// Represents a complex expression that consists of one or more statements.
    /// This class cannot be inherited.
    /// </summary>
    /// <remarks>Use 'leave' keyword to return value from the complex expression scope.</remarks>
    [ComVisible(false)]
    [Serializable]
    public sealed class ScriptCodeComplexExpression: ScriptCodeExpression, IEquatable<ScriptCodeComplexExpression>, IList<ScriptCodeStatement>
    {
        /// <summary>
        /// Represents body of the complex expression.
        /// </summary>
        public readonly ScriptCodeStatementCollection Body;

        private ScriptCodeComplexExpression(ScriptCodeStatementCollection body)
        {
            Body = body ?? new ScriptCodeStatementCollection();
        }

        /// <summary>
        /// Initializes a new complex expression.
        /// </summary>
        /// <param name="statements"></param>
        public ScriptCodeComplexExpression(params ScriptCodeStatement[] statements)
            : this(new ScriptCodeStatementCollection(statements))
        {
        }

        /// <summary>
        /// Gets a value indicating whether the complex expression contains a single expression.
        /// </summary>
        public bool IsExpressionBody
        {
            get { return Body.Count == 1 && Body[0] is IScriptExpressionStatement; }
        }

        internal override bool Completed
        {
            get { return true; }
        }

        /// <summary>
        /// Simplifies this expression.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public override ScriptCodeExpression Reduce(InterpretationContext context)
        {
            return CanReduce ? ((IScriptExpressionStatement)Body[0]).Expression : this;
        }

        /// <summary>
        /// Gets a value indicating whether this expression can be simplified.
        /// </summary>
        public override bool CanReduce
        {
            get
            {
                return Body.Count == 1 && Body[0] is IScriptExpressionStatement;
            }
        }

        /// <summary>
        /// Determines whether this complex expression contains the same collection
        /// of statements as other.
        /// </summary>
        /// <param name="other">Other expression to compare.</param>
        /// <returns> </returns>
        public bool Equals(ScriptCodeComplexExpression other)
        {
            return other != null && ScriptCodeStatementCollection.TheSame(Body, other.Body);
        }

        /// <summary>
        /// Determines whether this complex expression contains the same collection
        /// of statements as other.
        /// </summary>
        /// <param name="other">Other expression to compare.</param>
        /// <returns> </returns>
        public override bool Equals(ScriptCodeExpression other)
        {
            return Equals(other as ScriptCodeComplexExpression);
        }

        /// <summary>
        /// Creates an expression that produces this instance.
        /// </summary>
        /// <returns></returns>
        protected override Expression Restore()
        {
            var ctor = LinqHelpers.BodyOf<ScriptCodeStatement[], ScriptCodeComplexExpression, NewExpression>(body => new ScriptCodeComplexExpression(body));
            return ctor.Update(new[]{Body.NewArray()});
        }

        internal override void Verify()
        {
            Body.Verify();
        }

        internal override ScriptCodeExpression Visit(ISyntaxTreeNode parent, Converter<ISyntaxTreeNode, ISyntaxTreeNode> visitor)
        {
            Body.Visit(this, visitor);
            return visitor.Invoke(this) as ScriptCodeExpression ?? this;
        }

        /// <summary>
        /// Creates a new deep clone of this instance.
        /// </summary>
        /// <returns></returns>
        protected override ScriptCodeExpression Clone()
        {
            return new ScriptCodeComplexExpression(Extensions.Clone(Body));
        }

        internal static ScriptCodeComplexExpression Parse(IEnumerator<KeyValuePair<Lexeme.Position, Lexeme>> lexer)
        {
            var result = new ScriptCodeStatementCollection();
            Parser.ParseStatements(lexer, result, Punctuation.RightBrace);
            return new ScriptCodeComplexExpression(result);
        }

        /// <summary>
        /// Returns a string representation of this expression.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return Body.ToString();
        }

        /// <summary>
        /// Returns an enumerator through all statements.
        /// </summary>
        /// <returns></returns>
        public IEnumerator<ScriptCodeStatement> GetEnumerator()
        {
            return Body.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        void ICollection<ScriptCodeStatement>.Add(ScriptCodeStatement stmt)
        {
            Body.Add(stmt);
        }

        void ICollection<ScriptCodeStatement>.Clear()
        {
            Body.Clear();
        }

        bool ICollection<ScriptCodeStatement>.Contains(ScriptCodeStatement stmt)
        {
            return Body.Contains(stmt);
        }

        void ICollection<ScriptCodeStatement>.CopyTo(ScriptCodeStatement[] array, int arrayIndex)
        {
            Body.CopyTo(array, arrayIndex);
        }

        int ICollection<ScriptCodeStatement>.Count
        {
            get { return Body.Count; }
        }

        bool ICollection<ScriptCodeStatement>.IsReadOnly
        {
            get { return false; }
        }

        bool ICollection<ScriptCodeStatement>.Remove(ScriptCodeStatement stmt)
        {
            Body.Remove(stmt);
            return true;
        }

        int IList<ScriptCodeStatement>.IndexOf(ScriptCodeStatement stmt)
        {
            return Body.IndexOf(stmt);
        }

        void IList<ScriptCodeStatement>.Insert(int index, ScriptCodeStatement stmt)
        {
            Body.Insert(index, stmt);
        }

        void IList<ScriptCodeStatement>.RemoveAt(int index)
        {
            Body.RemoveAt(index);
        }

        ScriptCodeStatement IList<ScriptCodeStatement>.this[int index]
        {
            get { return Body[index]; }
            set { Body[index] = value; }
        }
    }
}
