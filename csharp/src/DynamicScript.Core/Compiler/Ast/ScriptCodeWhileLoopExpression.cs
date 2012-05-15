using System;
using System.Collections.Generic;
using System.CodeDom;
using System.Linq.Expressions;

namespace DynamicScript.Compiler.Ast
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;
    using StringBuilder = System.Text.StringBuilder;

    /// <summary>
    /// Represens while-loop expression.
    /// This class cannot be inherited.
    /// </summary>
    [ComVisible(false)]
    [Serializable]
    public sealed class ScriptCodeWhileLoopExpression : ScriptCodeLoopExpression, IEquatable<ScriptCodeWhileLoopExpression>
    {
        #region Nested Types
        /// <summary>
        /// Represents while-loop style.
        /// </summary>
        [ComVisible(false)]
        [Serializable]
        public enum LoopStyle : byte
        {
            /// <summary>
            /// Condition expression evaluates before body execution.
            /// </summary>
            EvaluateConditionBeforeBody = 0,

            /// <summary>
            /// Condition expression evaluates after body execution.
            /// </summary>
            EvaluateConditionAfterBody,
        }
        #endregion

        /// <summary>
        /// Initializes a new 'while' loop expression.
        /// </summary>
        /// <param name="body"></param>
        public ScriptCodeWhileLoopExpression(ScriptCodeExpressionStatement body = null)
            :base(body)
        {
        }

        /// <summary>
        /// Initializes a new 'while-loop' expression.
        /// </summary>
        /// <param name="condition"></param>
        /// <param name="grouping"></param>
        /// <param name="style"></param>
        /// <param name="suppressResult"></param>
        /// <param name="body"></param>
        public ScriptCodeWhileLoopExpression(ScriptCodeExpression condition, YieldGrouping grouping, LoopStyle style, bool suppressResult, ScriptCodeExpressionStatement body)
            : base(body)
        {
            Condition = condition;
            Grouping = grouping;
            SuppressResult = suppressResult;
            Style = style;
        }

        /// <summary>
        /// Gets or sets condition expression.
        /// </summary>
        public ScriptCodeExpression Condition
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets while-loop style.
        /// </summary>
        public LoopStyle Style
        {
            get;
            set;
        }

        internal override bool Completed
        {
            get
            {
                return Condition != null;
            }
        }

        internal static ScriptCodeWhileLoopExpression ParseDoWhileLoop(IEnumerator<KeyValuePair<Lexeme.Position, Lexeme>> lexer, params Lexeme[] terminator)
        {
            if (lexer == null) throw new ArgumentNullException("lexer");
            var loop = new ScriptCodeWhileLoopExpression { Style = ScriptCodeWhileLoopExpression.LoopStyle.EvaluateConditionAfterBody };
            lexer.MoveNext(true);   //pass through do keyword.
            loop.Body.SetExpression(Parser.ParseExpression, lexer, Keyword.While);  //parse loop body
            if (lexer.Current.Value != Keyword.While) throw CodeAnalysisException.InvalidExpressionTerm(lexer.Current); //matches to the while keyword
            lexer.MoveNext(true);   //pass through while keyword
            loop.Condition = Parser.ParseExpression(lexer, terminator + Keyword.GroupBy); //parse conditional expression
            if (lexer.Current.Value == Keyword.HashCodes.lxmGroupBy)
            {
                var lexeme = lexer.MoveNext(true); //wait for binary operator or expression.
                switch (lexeme is Operator)
                {
                    case true:
                        var @operator = Parser.GetOperator(lexeme, true);
                        if (@operator is ScriptCodeBinaryOperatorType)
                            loop.Grouping = (ScriptCodeBinaryOperatorType)@operator;
                        else throw CodeAnalysisException.InvalidLoopGrouping(lexer.Current.Key);
                        lexer.MoveNext(true);   //pass through operator
                        break;
                    default:
                        loop.Grouping = Parser.ParseExpression(lexer, terminator);
                        break;
                }
            }
            return loop;
        }

        internal static ScriptCodeWhileLoopExpression ParseWhileLoop(IEnumerator<KeyValuePair<Lexeme.Position, Lexeme>> lexer, params Lexeme[] terminator)
        {
            if (lexer == null) throw new ArgumentNullException("lexer");
            lexer.MoveNext(true);   //pass through while keyword
            var loop = new ScriptCodeWhileLoopExpression { Style = ScriptCodeWhileLoopExpression.LoopStyle.EvaluateConditionBeforeBody, Condition = Parser.ParseExpression(lexer, Keyword.GroupBy, Keyword.Do) };
            if (lexer.Current.Value == Keyword.HashCodes.lxmGroupBy) //parse grouping expression.
            {
                var lexeme = lexer.MoveNext(true); //wait for binary operator or expression.
                switch (lexeme is Operator)
                {
                    case true:
                        var @operator = Parser.GetOperator(lexeme, true);
                        if (@operator is ScriptCodeBinaryOperatorType)
                            loop.Grouping = (ScriptCodeBinaryOperatorType)@operator;
                        else throw CodeAnalysisException.InvalidLoopGrouping(lexer.Current.Key);
                        lexer.MoveNext(true);   //pass through operator
                        break;
                    default:
                        loop.Grouping = Parser.ParseExpression(lexer, Keyword.Do);
                        break;
                }
            }
            lexer.MoveNext(true);   //pass through do keyword.
            loop.Body.SetExpression(Parser.ParseExpression, lexer, terminator);
            return loop;
        }

        /// <summary>
        /// Determines whether this expression represents the same tree as other expression.
        /// </summary>
        /// <param name="other">Other expression tree to compare.</param>
        /// <returns><see langword="true"/> if this expression represents the same tree as other expression; otherwise, <see langword="false"/>.</returns>
        public override bool Equals(ScriptCodeExpression other)
        {
            return Equals(other as ScriptCodeWhileLoopExpression);
        }

        /// <summary>
        /// Determines whether this expression represents the same tree as other expression.
        /// </summary>
        /// <param name="other">Other expression tree to compare.</param>
        /// <returns><see langword="true"/> if this expression represents the same tree as other expression; otherwise, <see langword="false"/>.</returns>
        public bool Equals(ScriptCodeWhileLoopExpression other)
        {
            return other != null &&
                Style == other.Style &&
                Equals(Body, other.Body) &&
                Equals(Grouping, other.Grouping);
        }

        /// <summary>
        /// Returns a LINQ expression that produces this object.
        /// </summary>
        /// <returns></returns>
        protected override Expression Restore()
        {
            var ctor = LinqHelpers.BodyOf<ScriptCodeExpression, YieldGrouping, LoopStyle, bool, ScriptCodeExpressionStatement, ScriptCodeWhileLoopExpression, NewExpression>((cond, grp, style, sup, body) => new ScriptCodeWhileLoopExpression(cond, grp, style, sup, body));
            return ctor.Update(new[] { LinqHelpers.Restore(Condition), LinqHelpers.Restore(Grouping), LinqHelpers.Constant(Style), LinqHelpers.Restore(Body) });
        }

        internal override void Verify()
        {
        }

        internal override ScriptCodeExpression Visit(ISyntaxTreeNode parent, Converter<ISyntaxTreeNode, ISyntaxTreeNode> visitor)
        {
            if (Condition != null) Condition = Condition.Visit(this, visitor) as ScriptCodeExpression ?? Condition;
            if (Grouping != null) Grouping = Grouping.Visit(this, visitor);
            Body.Visit(this, visitor);
            return visitor.Invoke(this) as ScriptCodeExpression;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected override ScriptCodeExpression Clone()
        {
            return new ScriptCodeWhileLoopExpression(Extensions.Clone(Body))
            {
                Condition = Extensions.Clone(Condition),
                Grouping = Extensions.Clone(Grouping),
                Style = this.Style,
                SuppressResult = this.SuppressResult
            };
        }

        /// <summary>
        /// Returns a string representation of this expression.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            var result = new StringBuilder();
            result.Append((string)Punctuation.LeftBracket);
            switch (Style)
            {
                case LoopStyle.EvaluateConditionBeforeBody:
                    result.Append(string.Concat(Keyword.While, Lexeme.WhiteSpace, Condition, Lexeme.WhiteSpace));
                    if (Grouping != null)
                        result.Append(string.Concat(Keyword.GroupBy, Lexeme.WhiteSpace, Grouping, Lexeme.WhiteSpace));
                    result.Append(string.Concat(Keyword.Do, Lexeme.WhiteSpace, Body.Expression));
                    break;
                case LoopStyle.EvaluateConditionAfterBody:
                    result.Append(string.Concat(Keyword.Do, Lexeme.WhiteSpace, Body.Expression, Lexeme.WhiteSpace, Keyword.While, Lexeme.WhiteSpace, Condition, Lexeme.WhiteSpace));
                    if (Grouping != null)
                        result.Append(string.Concat(Keyword.GroupBy, Lexeme.WhiteSpace, Grouping, Lexeme.WhiteSpace));
                    break;
            }
            result.Append((string)Punctuation.RightBracket);
            return result.ToString();
        }
    }
}
