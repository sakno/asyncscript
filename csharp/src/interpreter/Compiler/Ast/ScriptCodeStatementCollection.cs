using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace DynamicScript.Compiler.Ast
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;
    using CodeStatementCollection = System.CodeDom.CodeStatementCollection;
    using StringBuilder = System.Text.StringBuilder;

    /// <summary>
    /// Represents a collection of script statements.
    /// This class cannot be inherited.
    /// </summary>
    [ComVisible(false)]
    [Serializable]
    public sealed class ScriptCodeStatementCollection: CodeStatementCollection, ICollection<ScriptCodeStatement>, ISyntaxTreeNode
    {
        /// <summary>
        /// Initializes a new collection of script statements.
        /// </summary>
        /// <param name="statements">An array of script statements.</param>
        public ScriptCodeStatementCollection(params ScriptCodeStatement[] statements)
            : base(statements ?? new ScriptCodeStatement[0])
        {
        }

        private ScriptCodeStatementCollection(ScriptCodeStatementCollection statements)
            : base(statements)
        {
        }

        /// <summary>
        /// Adds a new statement to this collection.
        /// </summary>
        /// <param name="stmt"></param>
        /// <returns></returns>
        public int Add(ScriptCodeStatement stmt)
        {
            return base.Add(stmt);
        }

        internal int Add(ScriptCodeExpression expr, Lexeme.Position beginning, Lexeme.Position ending)
        {
            return Add(new ScriptCodeExpressionStatement(expr) { LinePragma = new ScriptDebugInfo { Start = beginning, End = ending } });
        }

        internal int Add(Func<IEnumerator<KeyValuePair<Lexeme.Position, Lexeme>>, Lexeme[], ScriptCodeExpression> parser, IEnumerator<KeyValuePair<Lexeme.Position, Lexeme>> lexer, params Lexeme[] terminator)
        {
            var beginning = lexer.Current.Key;
            return Add(parser.Invoke(lexer, terminator), beginning, lexer.Current.Key);
        }

        /// <summary>
        /// Gets or sets script statement at the specified position.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public new ScriptCodeStatement this[int index]
        {
            get { return base[index] as ScriptCodeStatement; }
            set { base[index] = value; }
        }

        void ICollection<ScriptCodeStatement>.Add(ScriptCodeStatement stmt)
        {
            Add(stmt);
        }

        bool ICollection<ScriptCodeStatement>.Contains(ScriptCodeStatement stmt)
        {
            return base.Contains(stmt);
        }

        void ICollection<ScriptCodeStatement>.CopyTo(ScriptCodeStatement[] array, int arrayIndex)
        {
            base.CopyTo(array, arrayIndex);
        }

        bool ICollection<ScriptCodeStatement>.IsReadOnly
        {
            get { return false; }
        }

        bool ICollection<ScriptCodeStatement>.Remove(ScriptCodeStatement stmt)
        {
            base.Remove(stmt);
            return true;
        }

        /// <summary>
        /// Returns an enumerator through all statements.
        /// </summary>
        /// <returns></returns>
        public new IEnumerator<ScriptCodeStatement> GetEnumerator()
        {
            var enumerator = base.GetEnumerator();
            while (enumerator.MoveNext() && enumerator.Current is ScriptCodeStatement) yield return (ScriptCodeStatement)enumerator.Current;
        }

        bool ISyntaxTreeNode.Completed
        {
            get { return LinqHelpers.IsTrue(this, stmt => stmt.Completed); }
        }

        void ISyntaxTreeNode.Verify()
        {
            LinqHelpers.ForEach(this, stmt => stmt.Verify());
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
            return new ScriptCodeStatementCollection(Extensions.CloneCollection(this));
        }

        Expression IRestorable.Restore()
        {
            var ctor = LinqHelpers.BodyOf<ScriptCodeStatement[], ScriptCodeStatementCollection, NewExpression>(elems => new ScriptCodeStatementCollection(elems));
            return ctor.Update(new[] { LinqHelpers.NewArray(this) });
        }

        internal static string ToString(CodeStatementCollection statements, bool emitVoidIfEmpty)
        {
            if (statements == null) statements = new CodeStatementCollection();
            var result = new StringBuilder();
            switch (statements.Count)
            {
                case 0: result.Append(emitVoidIfEmpty ? Keyword.Void : string.Empty); break;
                case 1:
                    var stmt = statements[0];
                    switch (stmt is IScriptExpressionStatement)
                    {
                        case true:
                            result.Append(((IScriptExpressionStatement)stmt).Expression.ToString());
                            break;
                        default:
                            result.AppendFormat("{0}{1}{2}", Punctuation.LeftBrace, stmt, Punctuation.RightBrace);
                            break;
                    }
                    break;
                default:
                    result.AppendLine(Punctuation.LeftBrace);
                    foreach (var s in statements)
                        result.AppendLine(Convert.ToString(s));
                    result.AppendLine(Punctuation.RightBrace);
                    break;
            }
            return result.ToString();
        }

        /// <summary>
        /// Returns a string representation of this collection.
        /// </summary>
        /// <param name="emitVoidIfEmpty"></param>
        /// <returns></returns>
        public string ToString(bool emitVoidIfEmpty)
        {
            return ToString(this, emitVoidIfEmpty);
        }

        /// <summary>
        /// Returns a string representation of this collection.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return ToString(false);
        }

        internal static bool TheSame(CodeStatementCollection col1, CodeStatementCollection col2)
        {
            if (col1 == null) col1 = new CodeStatementCollection();
            if (col2 == null) col2 = new CodeStatementCollection();
            switch (col1.Count == col2.Count)
            {
                case true:
                    for (var i = 0; i < col1.Count; i++)
                        if (Equals(col1[i], col2[i])) continue;
                        else return false;
                    return true;
                default: return false;
            }
        }
    }
}
