using System;
using System.CodeDom;
using System.Linq.Expressions;

namespace DynamicScript.Compiler.Ast
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;

    /// <summary>
    /// Represents reference to the named variable.
    /// This class cannot be inherited.
    /// </summary>
    [Serializable]
    [ComVisible(false)]
    public sealed class ScriptCodeVariableReference : ScriptCodeExpression, IEquatable<ScriptCodeVariableReference>
    {
        private string m_name;

        /// <summary>
        /// Initializes a new reference to the named variable.
        /// </summary>
        public ScriptCodeVariableReference()
        {
        }

        /// <summary>
        /// Initializes a new reference to the named variable.
        /// </summary>
        /// <param name="variableName"></param>
        public ScriptCodeVariableReference(string variableName)
            : this()
        {
            VariableName = variableName;
        }

        internal ScriptCodeVariableReference(NameToken token)
            :this()
        {
            VariableName = token;
        }

        /// <summary>
        /// Gets or sets variable/constant/slot name.
        /// </summary>
        public string VariableName
        {
            get { return m_name ?? String.Empty; }
            set { m_name = value; }
        }

        /// <summary>
        /// Returns a string representation of the expression.
        /// </summary>
        /// <returns>A string representation of the expression.</returns>
        public override string ToString()
        {
            return NameToken.Normalize(VariableName);
        }

        internal override bool Completed
        {
            get { return !string.IsNullOrWhiteSpace(VariableName); }
        }

        /// <summary>
        /// Converts expression to the statement.
        /// </summary>
        /// <param name="expr">The expression to be converted.</param>
        /// <returns>The statement that encapsulates the expression.</returns>
        public static explicit operator ScriptCodeExpressionStatement(ScriptCodeVariableReference expr)
        {
            return new ScriptCodeExpressionStatement<ScriptCodeVariableReference>(expr);
        }

        /// <summary>
        /// Determines whether this expression represents the same tree as other expression.
        /// </summary>
        /// <param name="other">Other expression tree to compare.</param>
        /// <returns><see langword="true"/> if this expression represents the same tree as other expression; otherwise, <see langword="false"/>.</returns>
        public override bool Equals(ScriptCodeExpression other)
        {
            return Equals(other as ScriptCodeVariableReference);
        }

        /// <summary>
        /// Determines whether this expression represents the same tree as other expression.
        /// </summary>
        /// <param name="other">Other expression tree to compare.</param>
        /// <returns><see langword="true"/> if this expression represents the same tree as other expression; otherwise, <see langword="false"/>.</returns>
        public bool Equals(ScriptCodeVariableReference other)
        {
            return other != null && StringEqualityComparer.Equals(VariableName, other.VariableName);
        }

        /// <summary>
        /// Returns a LINQ expression that produces this object.
        /// </summary>
        /// <returns></returns>
        protected override Expression Restore()
        {
            var ctor = LinqHelpers.BodyOf<string, ScriptCodeVariableReference, NewExpression>(name => new ScriptCodeVariableReference(name));
            return ctor.Update(new[] { LinqHelpers.Constant(VariableName) });
        }

        internal override void Verify()
        {
        }

        internal override ScriptCodeExpression Visit(ISyntaxTreeNode parent, Converter<ISyntaxTreeNode, ISyntaxTreeNode> visitor)
        {
            return visitor.Invoke(this) as ScriptCodeExpression ?? this;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected override ScriptCodeExpression Clone()
        {
            return new ScriptCodeVariableReference { VariableName = this.VariableName };
        }
    }
}
