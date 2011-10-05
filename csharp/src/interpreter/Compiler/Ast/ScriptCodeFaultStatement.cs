using System;
using System.CodeDom;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq.Expressions;

namespace DynamicScript.Compiler.Ast
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;

    /// <summary>
    /// Represents exception throwing statement.
    /// This class cannot be inherited.
    /// </summary>
    [ComVisible(false)]
    [Serializable]
    public sealed class ScriptCodeFaultStatement: ScriptCodeFlowControlStatement
    {
        /// <summary>
        /// Initializes a new instance of the statement.
        /// </summary>
        public ScriptCodeFaultStatement()
            : base(Keyword.Fault, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the FAULT statement.
        /// </summary>
        /// <param name="fault"></param>
        public ScriptCodeFaultStatement(ScriptCodeExpression fault)
            : this()
        {
            Error = fault;
        }

        /// <summary>
        /// Gets or sets an exception to be thrown.
        /// </summary>
        public ScriptCodeExpression Error
        {
            get;
            set;
        }

        /// <summary>
        /// Gets argument list for the control flow.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override ScriptCodeExpressionCollection ArgList
        {
            get
            {
                return new ScriptCodeExpressionCollection(Error);
            }
        }

        internal override bool Completed
        {
            get
            {
                return Error != null;
            }
        }

        internal static ScriptCodeFaultStatement Parse(IEnumerator<KeyValuePair<Lexeme.Position, Lexeme>> lexer)
        {
            if (!lexer.MoveNext()) throw CodeAnalysisException.IncompletedExpression(lexer.Current.Key); //pass through fault keyword
            return new ScriptCodeFaultStatement
            {
                Error = Parser.ParseExpression(lexer, Punctuation.Semicolon)
            };
        }

        /// <summary>
        /// Returns a LINQ expression that produces this object.
        /// </summary>
        /// <returns></returns>
        protected override Expression Restore()
        {
            var ctor = LinqHelpers.BodyOf<ScriptCodeExpression, ScriptCodeFaultStatement, NewExpression>(fault => new ScriptCodeFaultStatement(fault));
            return ctor.Update(new[] { LinqHelpers.Restore(Error) });
        }

        internal override ScriptCodeStatement Visit(ISyntaxTreeNode parent, Converter<ISyntaxTreeNode, ISyntaxTreeNode> visitor)
        {
            if (Error != null) Error= Error.Visit(this, visitor) as ScriptCodeExpression ?? Error;
            return visitor.Invoke(this) as ScriptCodeFlowControlStatement ?? this;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected override ScriptCodeStatement Clone()
        {
            return new ScriptCodeFaultStatement(Extensions.Clone(Error)) { LinePragma = this.LinePragma };
        }
    }
}
