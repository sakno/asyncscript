using System;
using System.CodeDom;
using System.Linq.Expressions;

namespace DynamicScript.Compiler.Ast
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;

    /// <summary>
    /// Represents action implementation expression. 
    /// This class cannot be inherited.
    /// </summary>
    [ComVisible(false)]
    [Serializable]
    public sealed class ScriptCodeActionImplementationExpression : ScriptCodeExpression, IStaticContractBinding<ScriptCodeActionContractExpression>, IEquatable<ScriptCodeActionImplementationExpression>
    {
        /// <summary>
        /// Represents body of the action.
        /// </summary>
        public readonly ScriptCodeStatementCollection Body;

        /// <summary>
        /// Represents signature of the action.
        /// </summary>
        public readonly ScriptCodeActionContractExpression Signature;

        /// <summary>
        /// Initializes a new action implementation.
        /// </summary>
        /// <param name="signature"></param>
        /// <param name="body"></param>
        public ScriptCodeActionImplementationExpression(ScriptCodeActionContractExpression signature = null, params ScriptCodeStatement[] body)
            : this(signature, new ScriptCodeStatementCollection(body))
        {
        }

        private ScriptCodeActionImplementationExpression(ScriptCodeActionContractExpression signature, ScriptCodeStatementCollection body)
        {
            Signature = signature ?? new ScriptCodeActionContractExpression();
            Body = body ?? new ScriptCodeStatementCollection();
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
            switch (Body.Count)
            {
                case 0:
                    return String.Concat(Punctuation.LeftBracket, Signature.ToString(false), Punctuation.LeftBrace, Punctuation.RightBracket);
                case 1:
                    var stmt = Body[0] as ScriptCodeExpressionStatement;
                    return String.Concat(Punctuation.LeftBracket, Signature.ToString(false), Punctuation.Colon, stmt != null ? stmt.Expression : null, Punctuation.RightBracket);
                default:
                    return String.Concat(Punctuation.LeftBracket, Signature.ToString(false), Punctuation.LeftBrace, Body, Punctuation.RightBrace, Punctuation.RightBracket);
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
            get { return Signature != null && Signature.Completed; }
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
                ScriptCodeStatementCollection.TheSame(Body, other.Body);
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

        internal static InvocationExpression Restore(ScriptCodeActionContractExpression signature, ScriptCodeStatementCollection body)
        {
            var ctor = LinqHelpers.BodyOf<ScriptCodeActionContractExpression, ScriptCodeStatement[], ScriptCodeActionImplementationExpression, NewExpression>((sig, b) => new ScriptCodeActionImplementationExpression(sig, b));
            return Expression.Invoke(Expression.Lambda(ctor.Update(new[] { LinqHelpers.Restore(signature), body.NewArray() })));
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
            get { return ScriptCodeExpressionStatement<ScriptCodePrimitiveExpression>.IsPrimitive(Body); }
        }

        internal override void Verify()
        {
        }

        internal override ScriptCodeExpression Visit(ISyntaxTreeNode parent, Converter<ISyntaxTreeNode, ISyntaxTreeNode> visitor)
        {
            Signature.Visit(this, visitor);
            Body.Visit(this, visitor);
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
    }
}
