using System;
using System.CodeDom;
using System.Linq.Expressions;

namespace DynamicScript.Compiler.Ast
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;
    using StringBuilder = System.Text.StringBuilder;

    /// <summary>
    /// Represents loop expression with variable.
    /// </summary>
    [ComVisible(false)]
    [Serializable]
    public abstract class ScriptCodeLoopWithVariableExpression : ScriptCodeLoopExpression
    {
        #region Nested Types

        /// <summary>
        /// Represents loop variable.
        /// This class cannot be inherited.
        /// </summary>
        [ComVisible(false)]
        [Serializable]
        public sealed class LoopVariable : ScriptCodeStatement, ISlot, IEquatable<LoopVariable>
        {
            private ScriptCodeExpression m_init;
            /// <summary>
            /// Represents name of the loop variable.
            /// </summary>
            public readonly string Name;
            private bool m_temporary;

            /// <summary>
            /// Initializes a new loop variable.
            /// </summary>
            public LoopVariable(string name, bool temporary = true, ScriptCodeExpression initExpr = null)
            {
                m_temporary = false;
                Name = name ?? string.Empty;
            }

            /// <summary>
            /// Gets or sets a value indicating that the temporary loop variable should be created.
            /// </summary>
            public bool Temporary
            {
                get { return m_temporary; }
                set { m_temporary = value; }
            }

            string ISlot.Name
            {
                get { return Name; }
            }

            /// <summary>
            /// Gets or sets init expression.
            /// </summary>
            public ScriptCodeExpression InitExpression
            {
                get { return m_init ?? ScriptCodeVoidExpression.Instance; }
                set { m_init = value; }
            }

            ScriptCodeExpression ISlot.ContractBinding
            {
                get { return ScriptCodeSuperContractExpression.Instance; }
            }

            SlotDeclarationStyle ISlot.Style
            {
                get { return SlotDeclarationStyle.InitExpressionOnly; }
            }

            internal override bool Completed
            {
                get { return true; }
            }

            /// <summary>
            /// Determines whether this object represents the same loop variable definition as other.
            /// </summary>
            /// <param name="other">Other loop variable definition to compare.</param>
            /// <returns><see langword="true"/> if this object represents the same loop variable definition as other; otherwise, <see langword="false"/>.</returns>
            public bool Equals(LoopVariable other)
            {
                return other != null &&
                    Completed &&
                    other.Completed &&
                    Temporary == other.Temporary &&
                    StringEqualityComparer.Equals(Name, other.Name) &&
                    Equals(InitExpression, other.InitExpression);
            }


            bool IEquatable<ISlot>.Equals(ISlot other)
            {
                return Equals(other as LoopVariable);
            }

            internal override ScriptCodeStatement Visit(ISyntaxTreeNode parent, Converter<ISyntaxTreeNode, ISyntaxTreeNode> visitor)
            {
                if (InitExpression != null) InitExpression = InitExpression.Visit(this, visitor) ?? InitExpression;
                return visitor.Invoke(this) as ScriptCodeStatement ?? this;
            }

            /// <summary>
            /// 
            /// </summary>
            /// <returns></returns>
            protected override ScriptCodeStatement Clone()
            {
                return new LoopVariable(Name, Temporary, Extensions.Clone(InitExpression));
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="other"></param>
            /// <returns></returns>
            public override bool Equals(ScriptCodeStatement other)
            {
                return Equals(other as LoopVariable);
            }

            /// <summary>
            /// 
            /// </summary>
            /// <returns></returns>
            protected override Expression Restore()
            {
                var ctor = LinqHelpers.BodyOf<string, bool, ScriptCodeExpression, LoopVariable, NewExpression>((name, temp, init) => new LoopVariable(name, temp, init));
                return ctor.Update(new[] { LinqHelpers.Constant(Name), LinqHelpers.Constant(Temporary), LinqHelpers.Restore(InitExpression) });
            }

            /// <summary>
            /// Returns a string representation of the loop variable.
            /// </summary>
            /// <returns></returns>
            public override string ToString()
            {
                var result = new StringBuilder();
                if (Temporary) result.Append(Keyword.Var + Lexeme.WhiteSpace);
                result.Append(Name);
                if (m_init != null)
                    result.AppendFormat("{0}{1}", Operator.Assignment, m_init);
                return result.ToString();
            }
        }

        #endregion

        internal ScriptCodeLoopWithVariableExpression(ScriptCodeExpressionStatement body = null)
            :base(body)
        {
        }

        /// <summary>
        /// Gets or sets loop variable.
        /// </summary>
        public LoopVariable Variable
        {
            get;
            set;
        }

        internal override bool Completed
        {
            get
            {
                return Variable != null && base.Completed;
            }
        }
    }
}
