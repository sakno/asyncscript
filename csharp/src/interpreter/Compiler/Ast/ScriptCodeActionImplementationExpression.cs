using System;
using System.CodeDom;
using System.Linq.Expressions;
using System.Collections.Generic;

namespace DynamicScript.Compiler.Ast
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;

    /// <summary>
    /// Represents action implementation expression. 
    /// This class cannot be inherited.
    /// </summary>
    [ComVisible(false)]
    [Serializable]
    public sealed class ScriptCodeActionImplementationExpression : ScriptCodeExpression, 
        IStaticContractBinding<ScriptCodeActionContractExpression>, 
        IEquatable<ScriptCodeActionImplementationExpression>
    {
        private ScriptCodeExpression m_body;

        /// <summary>
        /// Represents signature of the action.
        /// </summary>
        public readonly ScriptCodeActionContractExpression Signature;

        /// <summary>
        /// Initializes a new action implementation.
        /// </summary>
        /// <param name="signature"></param>
        /// <param name="body"></param>
        public ScriptCodeActionImplementationExpression(ScriptCodeActionContractExpression signature = null, ScriptCodeExpression body = null)
        {
            Signature = signature ?? new ScriptCodeActionContractExpression();
            Body = body;
        }

        /// <summary>
        /// Gets or sets body of the action.
        /// </summary>
        public ScriptCodeExpression Body
        {
            get { return m_body ?? ScriptCodeVoidExpression.Instance; }
            set { m_body = value; }
        }

        /// <summary>
        /// Gets a value indicating that the action implements asynchronous pattern.
        /// </summary>
        public bool IsAsynchronous
        {
            get { return Signature.IsAsynchronous; }
        }

        /// <summary>
        /// Returns a string representation of the action implementation.
        /// </summary>
        /// <returns>The string representation of the action implementation.</returns>
        public override string ToString()
        {
            return string.Concat(Signature, Punctuation.Colon, Body); 
        }

        /// <summary>
        /// Returns a string representation of the action implementation.
        /// </summary>
        /// <param name="style"></param>
        /// <returns></returns>
        public override string ToString(FormattingStyle style)
        {
            switch (style)
            {
                case FormattingStyle.Parenthesize: return string.Concat(Punctuation.LeftBracket, ToString(), Punctuation.RightBracket);
                default: return ToString();
            }
        }

        /// <summary>
        /// Gets static contract binding.
        /// </summary>
        ScriptCodeActionContractExpression IStaticContractBinding<ScriptCodeActionContractExpression>.Contract
        {
            get { return Signature; }
        }

        internal override bool Completed
        {
            get { return Signature.Completed && Body.Completed; }
        }

        /// <summary>
        /// Converts expression to the statement.
        /// </summary>
        /// <param name="expr">The expression to be converted.</param>
        /// <returns>The statement that encapsulates the expression.</returns>
        public static explicit operator ScriptCodeExpressionStatement(ScriptCodeActionImplementationExpression expr)
        {
            return new ScriptCodeExpressionStatement<ScriptCodeActionImplementationExpression>(expr);
        }

        /// <summary>
        /// Determines whether this expression represents the same tree as other expression.
        /// </summary>
        /// <param name="other">Other expression tree to compare.</param>
        /// <returns><see langword="true"/> if this expression represents the same tree as other expression; otherwise, <see langword="false"/>.</returns>
        public bool Equals(ScriptCodeActionImplementationExpression other)
        {
            return other != null &&
                Completed &&
                other.Completed &&
                Equals(Signature, other.Signature) &&
                Equals(Body, Body);
        }

        /// <summary>
        /// Determines whether this expression represents the same tree as other expression.
        /// </summary>
        /// <param name="other">Other expression tree to compare.</param>
        /// <returns><see langword="true"/> if this expression represents the same tree as other expression; otherwise, <see langword="false"/>.</returns>
        public override bool Equals(ScriptCodeExpression other)
        {
            return Equals(other as ScriptCodeActionImplementationExpression);
        }

        internal static InvocationExpression Restore(ScriptCodeActionContractExpression signature, ScriptCodeExpression body)
        {
            var ctor = LinqHelpers.BodyOf<ScriptCodeActionContractExpression, ScriptCodeExpression, ScriptCodeActionImplementationExpression, NewExpression>((sig, b) => new ScriptCodeActionImplementationExpression(sig, b));
            return Expression.Invoke(Expression.Lambda(ctor.Update(new[] { LinqHelpers.Restore(signature), LinqHelpers.Restore(body) })));
        }

        /// <summary>
        /// Returns a LINQ expression that produces this object.
        /// </summary>
        /// <returns></returns>
        protected override Expression Restore()
        {
            return Restore(Signature, Body);
        }

        internal bool IsPrimitive
        {
            get { return Body is ScriptCodePrimitiveExpression; }
        }

        internal override void Verify()
        {
        }

        internal override ScriptCodeExpression Visit(ISyntaxTreeNode parent, Converter<ISyntaxTreeNode, ISyntaxTreeNode> visitor)
        {
            Signature.Visit(this, visitor);
            if (Body != null) Body = Body.Visit(this, visitor) as ScriptCodeExpression;
            return visitor.Invoke(this) as ScriptCodeExpression;
        }

        /// <summary>
        /// Creates a new deep copy of this expression.
        /// </summary>
        /// <returns></returns>
        protected override ScriptCodeExpression Clone()
        {
            return new ScriptCodeActionImplementationExpression(Extensions.Clone(Signature), Extensions.Clone(Body));
        }

        internal IList<ScriptCodeStatement> GetBody()
        {
            return Body is IList<ScriptCodeStatement> ? (IList<ScriptCodeStatement>)Body : new[] { new ScriptCodeExpressionStatement(Body) };
        }

        /// <summary>
        /// Gets a value indicating whether this expression can be reduced.
        /// </summary>
        public override bool CanReduce
        {
            get { return Body.CanReduce; }
        }

        /// <summary>
        /// Simplifies the current expression.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public override ScriptCodeExpression Reduce(InterpretationContext context)
        {
            return CanReduce ? new ScriptCodeActionImplementationExpression(Signature, Body.Reduce(context)) : this;
        }
    }
}
