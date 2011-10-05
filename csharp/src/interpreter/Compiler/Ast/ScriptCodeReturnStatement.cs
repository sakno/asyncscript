using System;
using System.Collections.Generic;
using System.CodeDom;
using System.ComponentModel;
using System.Linq.Expressions;

namespace DynamicScript.Compiler.Ast
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;

    /// <summary>
    /// Represents statement that returns flow control to the caller action.
    /// This class cannot be inherited.
    /// </summary>
    [Serializable]
    [ComVisible(false)]
    public sealed class ScriptCodeReturnStatement: ScriptCodeFlowControlStatement
    {
        /// <summary>
        /// Initializes a new statement that returns flow control to the caller action.
        /// </summary>
        public ScriptCodeReturnStatement()
            : base(Keyword.Return, null)
        {
        }

        /// <summary>
        /// Initializes a new 'return' statement.
        /// </summary>
        /// <param name="returnValue"></param>
        public ScriptCodeReturnStatement(ScriptCodeExpression returnValue)
            : this()
        {
            Value = returnValue;
        }

        /// <summary>
        /// Gets collection of the values to be returned from the action.
        /// You should not use this property.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override ScriptCodeExpressionCollection ArgList
        {
            get { return Value != null ? new ScriptCodeExpressionCollection(Value) : new ScriptCodeExpressionCollection(); }
        }

        /// <summary>
        /// Gets or sets the value returned from the action.
        /// </summary>
        public new ScriptCodeExpression Value
        {
            get;
            set;
        }

        internal static ScriptCodeReturnStatement Parse(IEnumerator<KeyValuePair<Lexeme.Position, Lexeme>> lexer)
        {
            return new ScriptCodeReturnStatement
            {
                Value = lexer.MoveNext(true) != Punctuation.Semicolon ? Parser.ParseExpression(lexer, Punctuation.Semicolon) : null
            };
        }

        /// <summary>
        /// Returns a LINQ expression that produces this object.
        /// </summary>
        /// <returns></returns>
        protected override Expression Restore()
        {
            var ctor = LinqHelpers.BodyOf<ScriptCodeExpression, ScriptCodeReturnStatement, NewExpression>(retval => new ScriptCodeReturnStatement(retval));
            return ctor.Update(new[] { LinqHelpers.Restore(Value) });
        }

        internal override ScriptCodeStatement Visit(ISyntaxTreeNode parent, Converter<ISyntaxTreeNode, ISyntaxTreeNode> visitor)
        {
            if (Value != null) Value = Value.Visit(this, visitor) as ScriptCodeExpression;
            return visitor.Invoke(this) as ScriptCodeFlowControlStatement;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected override ScriptCodeStatement Clone()
        {
            return new ScriptCodeReturnStatement(Extensions.Clone(Value)) { LinePragma = this.LinePragma };
        }

        /// <summary>
        /// 
        /// </summary>
        internal override bool Completed
        {
            get { return true; }
        }
    }
}
