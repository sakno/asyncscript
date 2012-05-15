using System;
using System.Linq.Expressions;

namespace DynamicScript.Compiler.Ast
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;
    using CodeStatement = System.CodeDom.CodeStatement;

    /// <summary>
    /// Represents NOP(no operation) statement.
    /// This class cannot be inherited.
    /// </summary>
    [Serializable]
    [ComVisible(false)]
    public sealed class ScriptCodeEmptyStatement : ScriptCodeStatement
    {
        private ScriptCodeEmptyStatement()
        {
        }

        /// <summary>
        /// Represents singleton instance of the NOP statement.
        /// </summary>
        public static readonly ScriptCodeEmptyStatement Instance = new ScriptCodeEmptyStatement();

        /// <summary>
        /// Returns a string representation of the statement.
        /// </summary>
        /// <returns>The string representation of the statement.</returns>
        public override string ToString()
        {
            return String.Empty;
        }

        internal override ScriptCodeStatement Visit(ISyntaxTreeNode parent, Converter<ISyntaxTreeNode, ISyntaxTreeNode> visitor)
        {
            return visitor.Invoke(this) as ScriptCodeStatement ?? this;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected override ScriptCodeStatement Clone()
        {
            return Instance;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public override bool Equals(ScriptCodeStatement other)
        {
            return other is ScriptCodeEmptyStatement;
        }

        internal override bool Completed
        {
            get { return true; }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected override Expression Restore()
        {
            return LinqHelpers.BodyOf<Func<ScriptCodeEmptyStatement>, MemberExpression>(() => Instance);
        }
    }
}
