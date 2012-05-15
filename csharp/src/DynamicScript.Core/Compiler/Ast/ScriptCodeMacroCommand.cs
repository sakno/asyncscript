using System;
using System.Linq.Expressions;

namespace DynamicScript.Compiler.Ast
{
    using CodeStatement = System.CodeDom.CodeStatement;
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;

    /// <summary>
    /// Represents macro command.
    /// This class cannot be inherited.
    /// </summary>
    [Serializable]
    [ComVisible(false)]
    public sealed class ScriptCodeMacroCommand: ScriptCodeStatement, IEquatable<ScriptCodeMacroCommand>
    {
        /// <summary>
        /// Represents command content.
        /// </summary>
        public readonly string Command;

        /// <summary>
        /// Initializes a new macro command.
        /// </summary>
        /// <param name="cmd"></param>
        public ScriptCodeMacroCommand(string cmd)
        {
            Command = cmd ?? string.Empty;
        }

        internal ScriptCodeMacroCommand(Macro m)
            : this(m != null ? m.ToString() : null)
        {
        }

        /// <summary>
        /// Determines whether this macro command is equivalent to another.
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool Equals(ScriptCodeMacroCommand other)
        {
            return other != null && StringEqualityComparer.Equals(Command, other.Command);
        }

        /// <summary>
        /// Returns a string representation of macro command.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Concat(Punctuation.Diez, Command);
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
            return new ScriptCodeMacroCommand(Command);
        }

        /// <summary>
        /// Determines whether this macro command is equivalent to another.
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public override bool Equals(ScriptCodeStatement other)
        {
            return Equals(other as ScriptCodeMacroCommand);
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
