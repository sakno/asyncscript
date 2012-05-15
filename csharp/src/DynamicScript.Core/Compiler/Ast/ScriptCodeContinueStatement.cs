using System;
using System.Collections.Generic;
using System.CodeDom;
using System.Linq.Expressions;

namespace DynamicScript.Compiler.Ast
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;

    /// <summary>
    /// Represents statement that initiates a new iteration of the loop or invokes the action
    /// in the current scope recursively.
    /// This class cannot be inherited.
    /// </summary>
    [ComVisible(false)]
    [Serializable]
    public sealed class ScriptCodeContinueStatement: ScriptCodeFlowControlStatement
    {
        private ScriptCodeContinueStatement(ScriptCodeExpressionCollection values)
            : base(Keyword.Continue, values)
        {
        }

        /// <summary>
        /// Initializes a new statement that initiates a new iteration of the loop or invokes the action
        /// in the current scope recursively.
        /// </summary>
        public ScriptCodeContinueStatement(params ScriptCodeExpression[] values)
            : this(new ScriptCodeExpressionCollection(values))
        {
        }

        internal static ScriptCodeContinueStatement Parse(IEnumerator<KeyValuePair<Lexeme.Position, Lexeme>> lexer)
        {
            var statement = new ScriptCodeContinueStatement();
            Parser.ParseExpressions(lexer, statement.ArgList, Punctuation.Semicolon);
            return statement;
        }

        /// <summary>
        /// Returns a LINQ expression that produces this object.
        /// </summary>
        /// <returns></returns>
        protected override Expression Restore()
        {
            var ctor = LinqHelpers.BodyOf<ScriptCodeExpression[], ScriptCodeContinueStatement, NewExpression>(args => new ScriptCodeContinueStatement(args));
            return ctor.Update(new[] { ArgList.NewArray() });
        }

        internal override ScriptCodeStatement Visit(ISyntaxTreeNode parent, Converter<ISyntaxTreeNode, ISyntaxTreeNode> visitor)
        {
            ArgList.Visit(this, visitor);
            return visitor.Invoke(this) as ScriptCodeFlowControlStatement ?? this;
        }

        internal override bool Completed
        {
            get { return true; }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected override ScriptCodeStatement Clone()
        {
            return new ScriptCodeContinueStatement(Extensions.Clone(ArgList)) { LinePragma = this.LinePragma };
        }
    }
}
