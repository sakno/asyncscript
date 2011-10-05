using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace DynamicScript.Compiler.Ast
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;

    /// <summary>
    /// Represents statement that breaks the current lexical scope.
    /// This class cannot be inherited.
    /// </summary>
    [Serializable]
    [ComVisible(false)]
    public sealed class ScriptCodeBreakLexicalScopeStatement : ScriptCodeFlowControlStatement
    {
        /// <summary>
        /// Initializes a new statement that breaks the current lexical scope.
        /// </summary>
        /// <param name="arguments"></param>
        public ScriptCodeBreakLexicalScopeStatement(params ScriptCodeExpression[] arguments)
            : this(new ScriptCodeExpressionCollection(arguments))
        {
        }

        private ScriptCodeBreakLexicalScopeStatement(ScriptCodeExpressionCollection arguments)
            : base(Keyword.Leave, arguments)
        {
        }

        internal override bool Completed
        {
            get
            {
                return true;
            }
        }

        internal static ScriptCodeBreakLexicalScopeStatement Parse(IEnumerator<KeyValuePair<Lexeme.Position, Lexeme>> lexer)
        {
            var statement = new ScriptCodeBreakLexicalScopeStatement();
            Parser.ParseExpressions(lexer, statement.ArgList, Punctuation.Semicolon);
            return statement;
        }

        /// <summary>
        /// Returns a LINQ expression that produces this object.
        /// </summary>
        /// <returns></returns>
        protected override Expression Restore()
        {
            var ctor = LinqHelpers.BodyOf<ScriptCodeExpression[], ScriptCodeBreakLexicalScopeStatement, NewExpression>(args => new ScriptCodeBreakLexicalScopeStatement(args));
            return ctor.Update(new[] { ArgList.NewArray() });
        }

        internal override ScriptCodeStatement Visit(ISyntaxTreeNode parent, Converter<ISyntaxTreeNode, ISyntaxTreeNode> visitor)
        {
            ArgList.Visit(this, visitor);
            return visitor.Invoke(this) as ScriptCodeFlowControlStatement ?? this;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected override ScriptCodeStatement Clone()
        {
            return new ScriptCodeBreakLexicalScopeStatement(Extensions.Clone(ArgList)) { LinePragma = this.LinePragma };
        }
    }
}
