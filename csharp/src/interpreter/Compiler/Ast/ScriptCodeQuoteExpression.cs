﻿using System;

namespace DynamicScript.Compiler.Ast
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;
    using CodeStatementCollection = System.CodeDom.CodeStatementCollection;
    using CodeStatement = System.CodeDom.CodeStatement;
    using Expression = System.Linq.Expressions.Expression;

    /// <summary>
    /// Represents quouted action.
    /// This class cannot be inherited.
    /// </summary>
    [ComVisible(false)]
    [Serializable]
    public sealed class ScriptCodeQuoteExpression : ScriptCodeExpression, IEquatable<ScriptCodeQuoteExpression>
    {
        /// <summary>
        /// Represents body of the quoted action.
        /// </summary>
        public readonly ScriptCodeStatementCollection Body;

        /// <summary>
        /// Represents signature of the quoted action.
        /// </summary>
        public readonly ScriptCodeActionContractExpression Signature;

        private ScriptCodeQuoteExpression(ScriptCodeActionContractExpression signature, ScriptCodeStatementCollection body)
        {
            Body = body ?? new ScriptCodeStatementCollection();
            Signature = signature ?? new ScriptCodeActionContractExpression();
        }

        /// <summary>
        /// Initializes a new expression.
        /// </summary>
        /// <param name="signature"></param>
        /// <param name="body"></param>
        public ScriptCodeQuoteExpression(ScriptCodeActionContractExpression signature = null, params ScriptCodeStatement[] body)
            : this(signature, new ScriptCodeStatementCollection(body))
        {
            Signature = signature ?? new ScriptCodeActionContractExpression();
        }

        internal ScriptCodeQuoteExpression(ScriptCodeActionImplementationExpression actionImplementation)
            :this(actionImplementation.Signature, actionImplementation.Body)
        {
        }

        internal override bool Completed
        {
            get { return Signature.Completed; }
        }

        /// <summary>
        /// Converts expression to the statement.
        /// </summary>
        /// <param name="expr">The expression to be converted.</param>
        /// <returns>The statement that encapsulates the expression.</returns>
        public static explicit operator ScriptCodeExpressionStatement(ScriptCodeQuoteExpression expr)
        {
            return new ScriptCodeExpressionStatement<ScriptCodeQuoteExpression>(expr);
        }

        /// <summary>
        /// Determines whether this expression represents the same tree as other expression.
        /// </summary>
        /// <param name="other">Other expression tree to compare.</param>
        /// <returns><see langword="true"/> if this expression represents the same tree as other expression; otherwise, <see langword="false"/>.</returns>
        public bool Equals(ScriptCodeQuoteExpression other)
        {
            return other != null &&
                Equals(Signature, other.Signature) &&
                ScriptCodeStatement.Equals(Body, other.Body);
        }

        /// <summary>
        /// Determines whether this expression represents the same tree as other expression.
        /// </summary>
        /// <param name="other">Other expression tree to compare.</param>
        /// <returns><see langword="true"/> if this expression represents the same tree as other expression; otherwise, <see langword="false"/>.</returns>
        public override bool Equals(ScriptCodeExpression other)
        {
            return Equals(other as ScriptCodeQuoteExpression);
        }

        /// <summary>
        /// Returns a LINQ expression that produces this object.
        /// </summary>
        /// <returns></returns>
        protected override Expression Restore()
        {
            return ScriptCodeActionImplementationExpression.Restore(Signature, Body);
        }

        internal override void Verify()
        {
        }

        internal override ScriptCodeExpression Visit(ISyntaxTreeNode parent, Converter<ISyntaxTreeNode, ISyntaxTreeNode> visitor)
        {
            Signature.Visit(this, visitor);
            Body.Visit(this, visitor);
            return visitor.Invoke(this) as ScriptCodeExpression ?? this;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected override ScriptCodeExpression Clone()
        {
            return new ScriptCodeQuoteExpression(Extensions.Clone(Signature), Extensions.Clone(Body));
        }
    }
}
