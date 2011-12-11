using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace DynamicScript.Compiler.Ast
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;
    using Enumerable = System.Linq.Enumerable;
    using StringBuilder = System.Text.StringBuilder;

    /// <summary>
    /// Represents 'caseof' expression.
    /// This class cannot be inherited.
    /// </summary>
    [ComVisible(false)]
    [Serializable]
    public sealed class ScriptCodeSelectionExpression: ScriptCodeExpression, IEquatable<ScriptCodeSelectionExpression>
    {
        #region Nested Types

        /// <summary>
        /// Represents case handler.
        /// This class cannot be inherited.
        /// </summary>
        [ComVisible(false)]
        [Serializable]
        public sealed class SelectionCase : ScriptCodeStatement, IEquatable<SelectionCase>
        {
            /// <summary>
            /// Represents a collection of values.
            /// </summary>
            public readonly ScriptCodeExpressionCollection Values;

            /// <summary>
            /// Represents a body of the handler.
            /// </summary>
            public readonly ScriptCodeExpressionStatement Handler;

            private SelectionCase(ScriptCodeExpressionCollection values, ScriptCodeExpressionStatement handler)
            {
                Values = values ?? new ScriptCodeExpressionCollection();
                Handler = handler ?? new ScriptCodeExpressionStatement(ScriptCodeVoidExpression.Instance);
            }

            /// <summary>
            /// Initializes a new case handler.
            /// </summary>
            /// <param name="values"></param>
            /// <param name="body"></param>
            public SelectionCase(ScriptCodeExpression[] values, ScriptCodeExpressionStatement body)
                :this(new ScriptCodeExpressionCollection(values), body)
            {
            }

            /// <summary>
            /// Initializes a new case handler.
            /// </summary>
            public SelectionCase()
                : this(new ScriptCodeExpression[0], null)
            {
            }

            internal override bool Completed
            {
                get
                {
                    foreach(ScriptCodeExpression v in Values)
                        switch (v.Completed)
                        {
                            case true: continue;
                            default: return false;
                        }
                    return Handler.Completed;
                }
            }

            /// <summary>
            /// Determines whether this object describes the same selection case
            /// as other object.
            /// </summary>
            /// <param name="other">Other selection case to compare.</param>
            /// <returns></returns>
            public bool Equals(SelectionCase other)
            {
                return other != null &&
                    ScriptCodeExpressionCollection.TheSame(Values, other.Values) &&
                    Equals(Handler, other.Handler);
            }

            internal override ScriptCodeStatement Visit(ISyntaxTreeNode parent, Converter<ISyntaxTreeNode, ISyntaxTreeNode> visitor)
            {
                Values.Visit(this, visitor);
                Handler.Visit(this, visitor);
                return visitor.Invoke(this) as ScriptCodeStatement ?? this;
            }

            /// <summary>
            /// 
            /// </summary>
            /// <returns></returns>
            protected override ScriptCodeStatement Clone()
            {
                return new SelectionCase(Extensions.Clone(Values), Extensions.Clone(Handler));
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="other"></param>
            /// <returns></returns>
            public override bool Equals(ScriptCodeStatement other)
            {
                return Equals(other as SelectionCase);
            }

            /// <summary>
            /// 
            /// </summary>
            /// <returns></returns>
            protected override Expression Restore()
            {
                var ctor = LinqHelpers.BodyOf<ScriptCodeExpression[], ScriptCodeExpressionStatement, SelectionCase, NewExpression>((vals, body) => new SelectionCase(vals, body));
                return ctor.Update(new[] { Values.NewArray(), LinqHelpers.Restore(Handler) });
            }

            /// <summary>
            /// Returns a string representation of this expression.
            /// </summary>
            /// <returns></returns>
            public override string ToString()
            {
                return string.Concat(Keyword.If, Lexeme.WhiteSpace,
                    Values,
                    Lexeme.WhiteSpace,
                    Keyword.Then,
                    Handler.Expression);
            }
        }
        #endregion

        /// <summary>
        /// Represents a collection of selection cases.
        /// </summary>
        public readonly IList<SelectionCase> Cases;

        /// <summary>
        /// Represents default handler.
        /// </summary>
        public readonly ScriptCodeExpressionStatement DefaultHandler;

        private ScriptCodeSelectionExpression(IEnumerable<SelectionCase> cases, ScriptCodeExpressionStatement defaultHandler)
        {
            Cases = new List<SelectionCase>(cases ?? Enumerable.Empty<SelectionCase>());
            DefaultHandler = defaultHandler ?? new ScriptCodeExpressionStatement(ScriptCodeVoidExpression.Instance);
        }

        /// <summary>
        /// Initializes a new selection case expression.
        /// </summary>
        /// <param name="src">A value to compare with other cases.</param>
        /// <param name="comparer">Custom comparer. Can be omitted.</param>
        /// <param name="cases">A collection of cases.</param>
        /// <param name="defaultHandler">Default handler.</param>
        public ScriptCodeSelectionExpression(ScriptCodeExpression src, ScriptCodeExpression comparer, IEnumerable<SelectionCase> cases, ScriptCodeExpressionStatement defaultHandler)
            : this(cases, defaultHandler)
        {
            Source = src;
            Comparer = comparer;
        }

        /// <summary>
        /// Initializes a new selection expression.
        /// </summary>
        public ScriptCodeSelectionExpression()
            : this(null, null, Enumerable.Empty<SelectionCase>(), null)
        {
        }

        /// <summary>
        /// Gets or sets a value selection source.
        /// </summary>
        public ScriptCodeExpression Source
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets an expression that represents case value comparer.
        /// </summary>
        public ScriptCodeExpression Comparer
        {
            get;
            set;
        }

        internal override bool Completed
        {
            get 
            {
                switch (Source != null)
                {
                    case true:
                        foreach(var c in Cases)
                            switch (c.Completed)
                            {
                                case true: continue;
                                default: return false;
                            }
                        return true;
                    default: return false;
                }
            }
        }

        internal static ScriptCodeSelectionExpression Parse(IEnumerator<KeyValuePair<Lexeme.Position, Lexeme>> lexer, params Lexeme[] terminator)
        {
            if (terminator == null || terminator.Length == 0) terminator = new[] { Punctuation.Semicolon };
            lexer.MoveNext(true);   //pass through caseof keyword
            var result = new ScriptCodeSelectionExpression();
            result.Source = Parser.ParseExpression(lexer, terminator + Punctuation.Arrow + Keyword.If + Keyword.Else);
            //Parse comparer
            if (lexer.Current.Value == Punctuation.HashCodes.lxmArrow)
            {
                lexer.MoveNext(true);   //pass through -> token
                result.Comparer = Parser.ParseExpression(lexer, terminator + Keyword.If + Keyword.Else);
            }
            //Parse handlers
            while (lexer.Current.Value == Keyword.HashCodes.lxmIf)
            {
                //Parse case
                var @case = new SelectionCase();
                Parser.ParseExpressions(lexer, @case.Values, Keyword.Then);
                lexer.MoveNext(true); //pass through then keyword
                //Parse case handler.
                @case.Handler.SetExpression(Parser.ParseExpression, lexer, terminator + Keyword.Else + Keyword.If);
                result.Cases.Add(@case);
            }
            if (lexer.Current.Value == Keyword.HashCodes.lxmElse)
            {
                lexer.MoveNext(true);   //pass through else keyword
                //Parse default handler.
                result.DefaultHandler.SetExpression(Parser.ParseExpression, lexer, terminator);
            }
            return result;
        }

        /// <summary>
        /// Determines whether this expression represents the same tree as other expression.
        /// </summary>
        /// <param name="other">Other expression tree to compare.</param>
        /// <returns><see langword="true"/> if this expression represents the same tree as other expression; otherwise, <see langword="false"/>.</returns>
        public bool Equals(ScriptCodeSelectionExpression other)
        {
            switch (other != null &&
                Equals(Source, other.Source) &&
                Equals(Comparer, other.Comparer) &&
                Equals(DefaultHandler, other.DefaultHandler) &&
                Cases.Count == other.Cases.Count)
            {
                case true:
                    for (var i = 0; i < Cases.Count; i++)
                        if (Equals(Cases[i], other.Cases[i])) continue;
                        else return false;
                    return true;
                default: return false;
            }
        }

        /// <summary>
        /// Determines whether this expression represents the same tree as other expression.
        /// </summary>
        /// <param name="other">Other expression tree to compare.</param>
        /// <returns><see langword="true"/> if this expression represents the same tree as other expression; otherwise, <see langword="false"/>.</returns>
        public override bool Equals(ScriptCodeExpression other)
        {
            return Equals(other as ScriptCodeSelectionExpression);
        }

        /// <summary>
        /// Returns a LINQ expression that produces this object.
        /// </summary>
        /// <returns></returns>
        protected override Expression Restore()
        {
            var ctor = LinqHelpers.BodyOf<ScriptCodeExpression, ScriptCodeExpression, IEnumerable<SelectionCase>, ScriptCodeExpressionStatement, ScriptCodeSelectionExpression, NewExpression>((src, cmp, cases, def) => new ScriptCodeSelectionExpression(src, cmp, cases, def));
            return Expression.Invoke(Expression.Lambda(ctor.Update(new[] { LinqHelpers.Restore(Source), LinqHelpers.Restore(Comparer), LinqHelpers.NewArray(Cases), LinqHelpers.Restore(DefaultHandler) })));
        }

        internal override void Verify()
        {
        }

        internal override ScriptCodeExpression Visit(ISyntaxTreeNode parent, Converter<ISyntaxTreeNode, ISyntaxTreeNode> visitor)
        {
            if (Comparer != null) Comparer = Comparer.Visit(this, visitor);
            if (Source != null) Source = Source.Visit(this, visitor);
            DefaultHandler.Visit(this, visitor);
            for (var i = 0; i < Cases.Count; i++)
                Cases[i] = (SelectionCase)Cases[i].Visit(this, visitor);
            return visitor.Invoke(this) as ScriptCodeExpression ?? this;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected override ScriptCodeExpression Clone()
        {
            return new ScriptCodeSelectionExpression(Extensions.CloneCollection(Cases), Extensions.Clone(DefaultHandler))
            {
                Comparer = Extensions.Clone(Comparer),
                Source = Extensions.Clone(Source)
            };
        }

        /// <summary>
        /// Returns a string representation of this expression.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            var result = new StringBuilder();
            result.Append(string.Concat(Punctuation.LeftBracket, Keyword.Caseof, Lexeme.WhiteSpace, Source));
            if (Comparer != null)
                result.Append(string.Concat(Lexeme.WhiteSpace, Punctuation.Arrow, Lexeme.WhiteSpace, Comparer));
            result.Append(Lexeme.WhiteSpace);
            result.Append(string.Join(Environment.NewLine, Cases));
            if (!DefaultHandler.IsVoidExpression)
                result.Append(string.Concat(Environment.NewLine, Keyword.Else, Lexeme.WhiteSpace, DefaultHandler.Expression));
            result.Append((string)Punctuation.RightBracket);
            return result.ToString();
        }
    }
}
