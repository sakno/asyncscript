using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace DynamicScript.Compiler.Ast
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;

    /// <summary>
    /// Represents code comment.
    /// This class cannot be inherited.
    /// </summary>
    [Serializable]
    [ComVisible(false)]
    public sealed class ScriptCodeCommentStatement: ScriptCodeStatement, IEquatable<ScriptCodeCommentStatement>
    {
        private string m_comment;

        /// <summary>
        /// Initializes a new comment statement.
        /// </summary>
        public ScriptCodeCommentStatement()
        {
            m_comment = null;
        }

        internal ScriptCodeCommentStatement(Comment cmt)
            :this()
        {
            m_comment = cmt;
        }

        /// <summary>
        /// Gets comment value.
        /// </summary>
        public string Comment
        {
            get { return m_comment ?? string.Empty; }
            set { m_comment = value; }
        }

        /// <summary>
        /// Returns a source code that represents the comment.
        /// </summary>
        /// <returns>The source code that represents the comment.</returns>
        public override string ToString()
        {
            return String.Concat("/*", Comment, "*/");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="statement"></param>
        /// <returns></returns>
        public static implicit operator CodeCommentStatement(ScriptCodeCommentStatement statement)
        {
            return statement != null ? new CodeCommentStatement(statement.Comment) : null;
        }

        internal override bool Completed
        {
            get { return true; }
        }

        internal override ScriptCodeStatement Visit(ISyntaxTreeNode parent, Converter<ISyntaxTreeNode, ISyntaxTreeNode> visitor)
        {
            return visitor.Invoke(this) as ScriptCodeStatement ?? this;
        }

        /// <summary>
        /// Creates a new deep copy of this statement.
        /// </summary>
        /// <returns></returns>
        protected override ScriptCodeStatement Clone()
        {
            return new ScriptCodeCommentStatement { Comment = this.Comment };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool Equals(ScriptCodeCommentStatement other)
        {
            return other != null && StringEqualityComparer.Equals(Comment, other.Comment);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public override bool Equals(ScriptCodeStatement other)
        {
            return Equals(other as ScriptCodeCommentStatement);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected override Expression Restore()
        {
            return LinqHelpers.Restore(ScriptCodeEmptyStatement.Instance);
        }
    }
}
