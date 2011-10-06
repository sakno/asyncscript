using System;
using System.CodeDom;
using System.Collections.Generic;

namespace DynamicScript.Compiler.Ast
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;

    /// <summary>
    /// Represents expression parser.
    /// This class cannot be inherited.
    /// </summary>
    /// <remarks>
    /// For more information about expression parsing see Red Dragon Book.
    /// </remarks>
    [ComVisible(false)]
    static class Parser
    {
        #region Nested Types
        [Serializable]
        [ComVisible(false)]
        private enum Associativity
        {
            Left=0,
            Right
        }
        #endregion

        private static readonly Lexeme[] StatementStartLexemes = new Lexeme[]
        {
            Punctuation.Dog, 
            Keyword.This, 
            Keyword.If, 
            Keyword.For, 
            Keyword.While, 
            Keyword.Do, 
            Keyword.Try, 
            Keyword.Checked, 
            Keyword.Unchecked, 
            Keyword.Caseof, 
            Keyword.Fork, 
            Keyword.Await,
            Keyword.Integer,
            Keyword.Boolean,
            Keyword.Dimensional,
            Keyword.Real,
            Keyword.FinSet,
            Keyword.Object,
            Keyword.Type,
            Keyword.Expr,
            Keyword.Stmt,
            Keyword.String,
            Keyword.True,
            Keyword.False,
            Keyword.Void
        };

        private static int GetPriority(Enum @operator)
        {
            if (@operator is ScriptCodeUnaryOperatorType)
                return InterpreterServices.GetPriority((ScriptCodeUnaryOperatorType)@operator);
            else if (@operator is ScriptCodeBinaryOperatorType)
                return InterpreterServices.GetPriority((ScriptCodeBinaryOperatorType)@operator);
            return -1;
        }

        private static Associativity GetAssociativity(Enum @operator)
        {
            return Equals(@operator, ScriptCodeBinaryOperatorType.Assign) ||
                Equals(@operator, ScriptCodeBinaryOperatorType.AdditiveAssign) ||
                Equals(@operator, ScriptCodeBinaryOperatorType.SubtractiveAssign) ||
            Equals(@operator, ScriptCodeBinaryOperatorType.MultiplicativeAssign) ||
            Equals(@operator, ScriptCodeBinaryOperatorType.DivideAssign) ||
            Equals(@operator, ScriptCodeBinaryOperatorType.ModuloAssign) ||
            Equals(@operator, ScriptCodeBinaryOperatorType.Expansion) ||
            Equals(@operator, ScriptCodeBinaryOperatorType.Reduction) ||
            Equals(@operator, ScriptCodeBinaryOperatorType.ExclusionAssign) ? Associativity.Right : Associativity.Left;
        }

        public static Enum ParseOperator(string literal, bool lastIsExpression)
        {
            using (var lexer = new LexemeAnalyzer(literal))
                return lexer.MoveNext() ? GetOperator(lexer.Current.Value, lastIsExpression) : null;
        }

        public static ScriptCodeBinaryOperatorType? ParseBinaryOperator(string literal)
        {
            var @operator = ParseOperator(literal, true);
            return @operator is ScriptCodeBinaryOperatorType ? new ScriptCodeBinaryOperatorType?((ScriptCodeBinaryOperatorType)@operator) : null;
        }

        public static ScriptCodeUnaryOperatorType? ParseUnaryOperator(string literal)
        {
            var @operator = ParseOperator(literal, false);
            return @operator is ScriptCodeUnaryOperatorType ? new ScriptCodeUnaryOperatorType?((ScriptCodeUnaryOperatorType)@operator) : null;
        }

        public static Enum GetOperator(Lexeme @operator, bool lastIsExpression)
        {
            if (@operator == Operator.Plus) return lastIsExpression ? (Enum)ScriptCodeBinaryOperatorType.Add : ScriptCodeUnaryOperatorType.Plus;
            else if (@operator == Operator.DoublePlus) return lastIsExpression ? ScriptCodeUnaryOperatorType.IncrementPostfix : ScriptCodeUnaryOperatorType.IncrementPrefix;
            else if (@operator == Operator.DoubleAsterisk) return lastIsExpression ? ScriptCodeUnaryOperatorType.SquarePostfix : ScriptCodeUnaryOperatorType.SquarePrefix;
            else if (@operator == Operator.Minus) return lastIsExpression ? (Enum)ScriptCodeBinaryOperatorType.Subtract : ScriptCodeUnaryOperatorType.Minus;
            else if (@operator == Operator.DoubleMinus) return lastIsExpression ? ScriptCodeUnaryOperatorType.DecrementPostfix : ScriptCodeUnaryOperatorType.DecrementPrefix;
            else if (@operator == Operator.Asterisk) return ScriptCodeBinaryOperatorType.Multiply;
            else if (@operator == Operator.Slash) return ScriptCodeBinaryOperatorType.Divide;
            else if (@operator == Operator.MemberAccess) return ScriptCodeBinaryOperatorType.MemberAccess;
            else if (@operator == Operator.Assignment) return ScriptCodeBinaryOperatorType.Assign;
            else if (@operator == Operator.ExclusionAssignment) return ScriptCodeBinaryOperatorType.ExclusionAssign;
            else if (@operator == Operator.SlashAssignment) return ScriptCodeBinaryOperatorType.DivideAssign;
            else if (@operator == Operator.MinusAssignment) return ScriptCodeBinaryOperatorType.SubtractiveAssign;
            else if (@operator == Operator.PlusAssignment) return ScriptCodeBinaryOperatorType.AdditiveAssign;
            else if (@operator == Operator.AsteriskAssignment) return ScriptCodeBinaryOperatorType.MultiplicativeAssign;
            else if (@operator == Operator.UnionAssignment) return ScriptCodeBinaryOperatorType.Expansion;
            else if (@operator == Operator.IntersectionAssignment) return ScriptCodeBinaryOperatorType.Reduction;
            else if (@operator == Operator.ValueEquality) return ScriptCodeBinaryOperatorType.ValueEquality;
            else if (@operator == Operator.ReferenceEquality) return ScriptCodeBinaryOperatorType.ReferenceEquality;
            else if (@operator == Operator.Modulo) return ScriptCodeBinaryOperatorType.Modulo;
            else if (@operator == Keyword.Is) return ScriptCodeBinaryOperatorType.InstanceOf;
            else if (@operator == Keyword.In) return ScriptCodeBinaryOperatorType.PartOf;
            else if (@operator == Keyword.To) return ScriptCodeBinaryOperatorType.TypeCast;
            else if (@operator == Operator.LessThan) return ScriptCodeBinaryOperatorType.LessThan;
            else if (@operator == Operator.LessThanOrEqual) return ScriptCodeBinaryOperatorType.LessThanOrEqual;
            else if (@operator == Operator.GreaterThan) return ScriptCodeBinaryOperatorType.GreaterThan;
            else if (@operator == Operator.GreaterThanOrEqual) return ScriptCodeBinaryOperatorType.GreaterThanOrEqual;
            else if (@operator == Operator.Intersection) return ScriptCodeBinaryOperatorType.Intersection;
            else if (@operator == Operator.Union) return ScriptCodeBinaryOperatorType.Union;
            else if (@operator == Operator.ValueInequality) return ScriptCodeBinaryOperatorType.ValueInequality;
            else if (@operator == Operator.ReferenceInequality) return ScriptCodeBinaryOperatorType.ReferenceInequality;
            else if (@operator == Operator.Negotiation) return ScriptCodeUnaryOperatorType.Negate;
            else if (@operator == Operator.Exclusion) return lastIsExpression ? (Enum)ScriptCodeBinaryOperatorType.Exclusion : ScriptCodeUnaryOperatorType.Intern;
            else if (@operator == Operator.AndAlso) return ScriptCodeBinaryOperatorType.AndAlso;
            else if (@operator == Operator.OrElse) return ScriptCodeBinaryOperatorType.OrElse;
            else if (@operator == Operator.TypeOf) return lastIsExpression ? null : (Enum)ScriptCodeUnaryOperatorType.TypeOf;
            else if (@operator == Operator.MetadataDiscovery) return ScriptCodeBinaryOperatorType.MetadataDiscovery;
            else if (@operator == Operator.VoidCheck) return lastIsExpression ? (Enum)ScriptCodeUnaryOperatorType.VoidCheck : null;
            else if (@operator == Operator.Coalesce) return ScriptCodeBinaryOperatorType.Coalesce;
            throw new InvalidOperationException();
        }

        /// <summary>
        /// Parses expression tree.
        /// </summary>
        /// <param name="lexer">The enumerator through lexemes in the source code. Cannot be <see langword="null"/>.</param>
        /// <param name="terminator">An array of the expression terminators. Cannot be <see langword="null"/> or empty.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException"><paramref name="lexer"/> or <paramref name="terminator"/> is <see langword="null"/>.</exception>
        public static ScriptCodeExpression ParseExpression(IEnumerator<KeyValuePair<Lexeme.Position, Lexeme>> lexer, params Lexeme[] terminator)
        {
            if (lexer == null) throw new ArgumentNullException("lexer");
            if (terminator == null || terminator.Length == 0) throw new ArgumentNullException("terminator");
            return Parse(lexer, int.MinValue, terminator);
        }

        private static ScriptCodeExpression Parse(IEnumerator<KeyValuePair<Lexeme.Position, Lexeme>> lexer, int priority, params Lexeme[] terminator)
        {
            ScriptCodeExpression expression = null;
            do
            {
                if (lexer.Current.Value is NameToken)   //reference to the name slot
                    expression = new ScriptCodeVariableReference((NameToken)lexer.Current.Value);
                else if (lexer.Current.Value == Keyword.Object)
                    expression = ScriptCodeSuperContractExpression.Instance;
                else if (lexer.Current.Value == Keyword.String)
                    expression = ScriptCodeStringContractExpression.Instance;
                else if (lexer.Current.Value == Keyword.Dimensional)
                    expression = ScriptCodeDimensionalContractExpression.Instance;
                else if (lexer.Current.Value == Keyword.Type)
                    expression = ScriptCodeMetaContractExpression.Instance;
                else if (lexer.Current.Value == Keyword.Async)
                { expression = ScriptCodeAsyncExpression.Parse(lexer, terminator); continue; }
                else if (lexer.Current.Value == Keyword.True)
                    expression = new ScriptCodeBooleanExpression(true);
                else if (lexer.Current.Value == Keyword.False)
                    expression = new ScriptCodeBooleanExpression(false);
                else if (lexer.Current.Value == Keyword.Boolean)
                    expression = ScriptCodeBooleanContractExpression.Instance;
                else if (lexer.Current.Value == Keyword.Real)
                    expression = ScriptCodeRealContractExpression.Instance;
                else if (lexer.Current.Value == Keyword.This)
                    expression = ScriptCodeThisExpression.Instance;
                else if (lexer.Current.Value == Keyword.Integer)
                    expression = ScriptCodeIntegerContractExpression.Instance;
                else if (lexer.Current.Value == Keyword.Void)
                    expression = ScriptCodeVoidExpression.Instance;
                else if (lexer.Current.Value == Keyword.FinSet)
                    expression = ScriptCodeFinSetContractExpression.Instance;
                else if (lexer.Current.Value == Keyword.Expr)
                    expression = ScriptCodeExpressionContractExpression.Instance;
                else if (lexer.Current.Value == Keyword.Stmt)
                    expression = ScriptCodeStatementContractExpression.Instance;
                else if (lexer.Current.Value == Keyword.Await)
                { expression = ScriptCodeAwaitExpression.Parse(lexer, terminator); continue; }
                else if (lexer.Current.Value is IntegerLiteral)
                    expression = new ScriptCodeIntegerExpression((IntegerLiteral)lexer.Current.Value);
                else if (lexer.Current.Value is StringLiteral)
                    expression = new ScriptCodeStringExpression((StringLiteral)lexer.Current.Value);
                else if (lexer.Current.Value == Keyword.Caseof)
                { expression = ScriptCodeSelectionExpression.Parse(lexer, terminator); continue; }
                else if (lexer.Current.Value == Keyword.Fork)
                { expression = ScriptCodeForkExpression.Parse(lexer, terminator); continue; }
                else if (lexer.Current.Value is RealLiteral)
                    expression = new ScriptCodeRealExpression((RealLiteral)lexer.Current.Value);
                else if (lexer.Current.Value == Keyword.Callable)
                    expression = ScriptCodeCallableContractExpression.Instance;
                else if (lexer.Current.Value is PlaceholderID)
                    expression = new ScriptCodePlaceholderExpression((PlaceholderID)lexer.Current.Value);
                else if (lexer.Current.Value == Punctuation.LeftBrace)      //parse object
                    expression = ScriptCodeObjectExpression.Parse(lexer, terminator);
                else if (lexer.Current.Value.OneOf(Keyword.Checked, Keyword.Unchecked)) //parse context
                { expression = ScriptCodeContextExpression.Parse(lexer, terminator); continue; }
                else if (lexer.Current.Value == Keyword.Try)        //parse SEH
                { expression = ScriptCodeTryElseFinallyExpression.Parse(lexer, terminator); continue; }
                else if (lexer.Current.Value == Keyword.Do)         //parse do-while-loop expression
                { expression = ScriptCodeWhileLoopExpression.ParseDoWhileLoop(lexer, terminator); continue; }
                else if (lexer.Current.Value == Keyword.While)      //parse while-loop expresion
                { expression = ScriptCodeWhileLoopExpression.ParseWhileLoop(lexer, terminator); continue; }
                else if (lexer.Current.Value == Keyword.If)         //parse conditional expression
                { expression = ScriptCodeConditionalExpression.Parse(lexer, terminator); continue; }
                else if (lexer.Current.Value == Keyword.For)         //parse for loop
                { expression = ParseForLoop(lexer, terminator); continue; }
                else if (lexer.Current.Value == Punctuation.Dog)    //parse action
                { expression = lexer.MoveNext() ? ParseAction(lexer, terminator) : ScriptCodeCurrentActionExpression.Instance; continue; }
                else if (lexer.Current.Value == Punctuation.DoubleDog) //parse quoted expression list
                { expression = lexer.MoveNext() ? ParseQuoteExpression(lexer, terminator) : ScriptCodeCurrentQuoteExpression.Instance; continue; }
                else if (lexer.Current.Value == Punctuation.LeftSquareBracket)//parse indexer or array type or array
                    switch (expression == null || InterpreterServices.HighestPriority > priority)
                    {
                        case true:
                            ParseIndexer(lexer, ref expression);
                            break;
                        default:
                            return expression;
                    }
                else if (lexer.Current.Value == Punctuation.LeftBracket)    //parse subexpression or invocation
                    switch (expression == null)
                    {
                        case true: //parse subexpression
                            lexer.MoveNext(true); //Select next token after bracket
                            expression = Parse(lexer, int.MinValue, Punctuation.RightBracket);
                            break;
                        default:    //parse action invocation
                            if (InterpreterServices.HighestPriority <= priority) return expression;
                            var invocation = new ScriptCodeInvocationExpression { Target = expression };
                            ParseExpressions(lexer, invocation.ArgList, Punctuation.RightBracket);
                            expression = invocation;
                            break;
                    }
                else if (lexer.Current.Value is Operator || lexer.Current.Value.OneOf(Keyword.Is, Keyword.In, Keyword.To))//Parse operator
                {
                    var @operator = GetOperator(lexer.Current.Value, expression != null);
                    if (@operator != null)
                        switch (GetAssociativity(@operator) == Associativity.Left ? GetPriority(@operator) > priority : GetPriority(@operator) >= priority)
                        {
                            case true:
                                if (@operator is ScriptCodeUnaryOperatorType)//Handles unary operator
                                    switch (expression != null)
                                    {
                                        case true:
                                            expression = new ScriptCodeUnaryOperatorExpression((ScriptCodeUnaryOperatorType)@operator, expression);
                                            break;
                                        default:
                                            lexer.MoveNext(true);   //expression expected
                                            expression = new ScriptCodeUnaryOperatorExpression((ScriptCodeUnaryOperatorType)@operator, Parse(lexer, GetPriority(@operator), terminator));
                                            continue;
                                    }
                                else if (@operator is ScriptCodeBinaryOperatorType)//Handles binary operator
                                {
                                    lexer.MoveNext(true); //Pass through operator token
                                    var leftOperand = expression;
                                    expression = new ScriptCodeBinaryOperatorExpression(leftOperand, (ScriptCodeBinaryOperatorType)@operator, Parse(lexer, GetPriority(@operator), terminator));
                                    continue;
                                }
                                break;
                            default:
                                return expression;
                        }
                }
                else throw CodeAnalysisException.InvalidExpressionTerm(lexer.Current); //Invalid expression term
                if (!lexer.MoveNext())
                    throw CodeAnalysisException.InvalidPunctuation(terminator[0], lexer.Current); //end of expression expected
            } while (!lexer.Current.Value.OneOf(terminator));
            return expression;
        }

        public static ScriptCodeLoopWithVariableExpression ParseForLoop(IEnumerator<KeyValuePair<Lexeme.Position, Lexeme>> lexer, params Lexeme[] terminator)
        {
            if (lexer == null) throw new ArgumentNullException("lexer");
            var loopVariable = default(ScriptCodeLoopWithVariableExpression.LoopVariable);
            var temporary = default(bool);
            var loop = default(ScriptCodeLoopWithVariableExpression);
            var lexeme = lexer.MoveNext(true);    //pass through for keyword
            if (lexer.Current.Value == Keyword.Var) //if var keyword is detected then temp var should be created
            {
                temporary = true;
                lexeme = lexer.MoveNext(true);
            }
            switch (lexeme is NameToken)
            {
                case true: loopVariable = new ScriptCodeLoopWithVariableExpression.LoopVariable((NameToken)lexeme) { Temporary = temporary }; break;
                default: throw CodeAnalysisException.IdentifierExpected(lexer.Current.Key);
            }
            lexeme = lexer.MoveNext(true);  //= or in expected
            if (lexeme == Keyword.In)           //parse for-each loop.
            {
                lexer.MoveNext(true); //pass through in keyword
                loop = new ScriptCodeForEachLoopExpression { Variable = loopVariable, Iterator = ParseExpression(lexer, Keyword.GroupBy, Keyword.Do) };
            }
            else if (lexeme == Operator.Assignment)     //parse classic for loop
            {
                lexer.MoveNext(true); //pass through = operator
                loopVariable.InitExpression = ParseExpression(lexer, Keyword.While);
                lexer.MoveNext(true); //pass through while keyword
                loop = new ScriptCodeForLoopExpression 
                { 
                    Variable = loopVariable,
                    Condition = ParseExpression(lexer, Keyword.Do, Keyword.GroupBy) //parse condition
                };
            }
            else throw CodeAnalysisException.InvalidExpressionTerm(lexer.Current);
            if (lexer.Current.Value == Keyword.GroupBy) //parse grouping expression.
            {
                lexeme = lexer.MoveNext(true); //wait for binary operator.
                switch (lexeme is Operator)
                {
                    case true:
                        var @operator = GetOperator(lexeme, true);
                        if (@operator is ScriptCodeBinaryOperatorType)
                            loop.Grouping = (ScriptCodeBinaryOperatorType)@operator;
                        else throw CodeAnalysisException.InvalidLoopGrouping(lexer.Current.Key);
                        lexer.MoveNext(true);   //pass through operator
                        break;
                    default:
                        loop.Grouping = ParseExpression(lexer, Keyword.Do);
                        break;
                }
            }
            lexer.MoveNext(true);   //pass through do keyword.
            switch (lexer.Current.Value == Punctuation.LeftBrace)   //Parse loop body.
            {
                case true:
                    ParseStatements(lexer, loop.Body, null, Punctuation.RightBrace);
                    break;
                default:
                    loop.Body.Add(ParseExpression, lexer, terminator);
                    break;
            }
            return loop;
        }
        

        private static void ParseIndexer(IEnumerator<KeyValuePair<Lexeme.Position, Lexeme>> lexer, ref ScriptCodeExpression expression)
        {
            lexer.MoveNext(true);   //pass through [ token
            //if the next token is ] then it is a single-dimensional array
            if (lexer.Current.Value == Punctuation.RightSquareBracket)
                expression = expression != null ? (ScriptCodeExpression)new ScriptCodeArrayContractExpression { ElementContract = expression } : new ScriptCodeArrayExpression();
            else if (lexer.Current.Value == Punctuation.Comma && expression != null)   //if `,` then it is a multi-dimensional array
            {
                var arrayContract = new ScriptCodeArrayContractExpression { ElementContract = expression };
                do
                {
                    //Comma expected
                    if (lexer.Current.Value != Punctuation.Comma) throw CodeAnalysisException.InvalidPunctuation(Punctuation.Comma, lexer.Current);
                    arrayContract.Rank += 1;
                }
                while (lexer.MoveNext() && lexer.Current.Value != Punctuation.RightSquareBracket);
                expression = arrayContract;
            }
            else switch (expression != null) //it is an indexer expression or array
                {
                    case true:
                        var indexer = new ScriptCodeIndexerExpression { Target = expression };
                        ParseExpressions(lexer, false, indexer.ArgList, Punctuation.RightSquareBracket);
                        expression = indexer;
                        return;
                default:
                        var array = new ScriptCodeArrayExpression();
                        ParseExpressions(lexer, false, array.Elements, Punctuation.RightSquareBracket);
                        expression = array;
                        return;
                }
        }

        private static ScriptCodeExpression ParseQuoteExpression(IEnumerator<KeyValuePair<Lexeme.Position, Lexeme>> lexer, Lexeme[] terminator)
        {
            var quoted = new ScriptCodeQuoteExpression();
            if (lexer.Current.Value == Keyword.Void)    //handles empty parameter list
            {
                quoted.Signature.ParamList.Clear();
                lexer.MoveNext();
            }
            else if (lexer.Current.Value is NameToken)  //handles non-empty parameter list
                while (lexer.Current.Value is NameToken)    //parameter should begins with name token
                {
                    var paramName = default(string);
                    var paramDefVal = default(ScriptCodeExpression);
                    var paramBinding = default(ScriptCodeExpression);
                    //Parameter is a syntax slot
                    ParseSlot(lexer, false, out paramName, out paramDefVal, out paramBinding, Punctuation.Arrow, Punctuation.Comma);
                    if (quoted.Signature.ParamList.Contains(paramName))
                        throw CodeAnalysisException.DuplicateIdentifier(paramName, lexer.Current.Key);
                    else quoted.Signature.ParamList.Add(paramName, paramDefVal, paramBinding);
                    if (lexer.Current.Value == Punctuation.Comma && !lexer.MoveNext()) throw CodeAnalysisException.InvalidPunctuation(Punctuation.Arrow, lexer.Current);
                }
            else return ScriptCodeCurrentQuoteExpression.Instance;
            //Handles -> punctuation token because it separates parameter list and return type.
            if (lexer.Current.Value != Punctuation.Arrow || !lexer.MoveNext()) throw CodeAnalysisException.InvalidPunctuation(Punctuation.Arrow, lexer.Current);
            //Parse return type expression
            quoted.Signature.ReturnType = Parse(lexer, int.MinValue, terminator + Punctuation.Colon + Punctuation.LeftBrace);
            if (lexer.Current.Value == Punctuation.Colon && lexer.MoveNext(true) != null)
                quoted.Body.Add(ParseExpression, lexer, terminator);
            else if (lexer.Current.Value == Punctuation.LeftBrace)
                ParseStatements(lexer, quoted.Body, Punctuation.RightBrace);
            else throw CodeAnalysisException.InvalidPunctuation(Punctuation.Colon, lexer.Current);
            return quoted;
        }

        private static ScriptCodeExpression ParseAction(IEnumerator<KeyValuePair<Lexeme.Position, Lexeme>> lexer, Lexeme[] terminator)
        {
            var actionContract = new ScriptCodeActionContractExpression();
            if (lexer.Current.Value == Keyword.Void)    //handles empty parameter list
            {
                actionContract.ParamList.Clear();
                lexer.MoveNext();
            }
            else if (lexer.Current.Value is NameToken)  //handles non-empty parameter list
                while (lexer.Current.Value is NameToken)    //parameter should begins with name token
                {
                    var paramName = default(string);
                    var paramDefVal = default(ScriptCodeExpression);
                    var paramBinding = default(ScriptCodeExpression);
                    //Parameter is a syntax slot
                    ParseSlot(lexer, false, out paramName, out paramDefVal, out paramBinding, Punctuation.Arrow, Punctuation.Comma);
                    if (actionContract.ParamList.Contains(paramName))
                        throw CodeAnalysisException.DuplicateIdentifier(paramName, lexer.Current.Key);
                    else actionContract.ParamList.Add(paramName, paramDefVal, paramBinding);
                    if (lexer.Current.Value == Punctuation.Comma && !lexer.MoveNext()) throw CodeAnalysisException.InvalidPunctuation(Punctuation.Arrow, lexer.Current);
                }
            else return ScriptCodeCurrentActionExpression.Instance; //if token after @ is not VOID or name token then this is an action root contract.
            //Handles -> punctuation token because it separates parameter list and return type.
            if (lexer.Current.Value != Punctuation.Arrow || !lexer.MoveNext()) throw CodeAnalysisException.InvalidPunctuation(Punctuation.Arrow, lexer.Current);
            //Parse return type expression
            actionContract.ReturnType = Parse(lexer, int.MinValue, terminator + Punctuation.Colon + Punctuation.LeftBrace);
            //The expression represents action implementation, not contract
            if (lexer.Current.Value == Punctuation.Colon && lexer.MoveNext(true) != null)
            {
                //Replaces action contract with action expression
                var actionExpression = new ScriptCodeActionImplementationExpression(actionContract);
                actionExpression.Body.Add(ParseExpression, lexer, terminator);
                return actionExpression;
            }
            else if (lexer.Current.Value == Punctuation.LeftBrace)
            {
                //Replaces action contract with action expression
                var actionExpression = new ScriptCodeActionImplementationExpression(actionContract);
                ParseStatements(lexer, actionExpression.Body, Punctuation.RightBrace);
                return actionExpression;
            }
            return actionContract;
        }

        public static void ParseStatements(IEnumerator<KeyValuePair<Lexeme.Position, Lexeme>> lexer, ScriptCodeStatementCollection statements, params Punctuation[] terminator)
        {
            if (statements == null) statements = new ScriptCodeStatementCollection();
            if (terminator == null) terminator = new Punctuation[0];
            while (!lexer.Current.Value.OneOf(terminator))
            {
                var stmt = ParseStatement(lexer, terminator);
                if (stmt != null) statements.Add(stmt); else break;
            }
            if (terminator.LongLength > 0L) lexer.MoveNext();   //pass through terminator
        }

        /// <summary>
        /// Parse single statement.
        /// </summary>
        /// <param name="lexer"></param>
        /// <param name="terminator">An array of the statement terminators.</param>
        /// <returns></returns>
        public static CodeStatement ParseStatement(IEnumerator<KeyValuePair<Lexeme.Position, Lexeme>> lexer, params Punctuation[] terminator)
        {
            if (terminator == null) terminator = new Punctuation[0];
            switch (lexer.MoveNext() && (terminator.LongLength > 0L ? !lexer.Current.Value.OneOf(terminator) : true))
            {
                case true:
                    var stmt = ParseStatement(lexer);
                    return stmt != null ? stmt : ParseStatement(lexer, terminator);
                default:
                    return null;
            }
        }

        private static ScriptCodeStatement ParseStatement(IEnumerator<KeyValuePair<Lexeme.Position, Lexeme>> lexer)
        {
            var beginning = lexer.Current.Key;
            var result = default(ScriptCodeStatement);
            if (lexer.Current.Value.OneOf(Keyword.Var, Keyword.Const))
                result = ScriptCodeVariableDeclaration.Parse(lexer);
            else if (lexer.Current.Value is Comment)
                result = new ScriptCodeCommentStatement((Comment)lexer.Current.Value);
            else if (lexer.Current.Value.OneOf<IntegerLiteral, NameToken, Operator, StringLiteral, RealLiteral>() || lexer.Current.Value.OneOf(StatementStartLexemes))
                result = new ScriptCodeExpressionStatement(ParseExpression(lexer, Punctuation.Semicolon));
            else if (lexer.Current.Value == Punctuation.Semicolon)
                result = ScriptCodeEmptyStatement.Instance;
            else if (lexer.Current.Value == Keyword.Return)
                result = ScriptCodeReturnStatement.Parse(lexer);
            else if (lexer.Current.Value == Keyword.Continue)
                result = ScriptCodeContinueStatement.Parse(lexer);
            else if (lexer.Current.Value == Keyword.Leave)
                return ScriptCodeBreakLexicalScopeStatement.Parse(lexer);
            else if (lexer.Current.Value == Keyword.Fault)
                result = ScriptCodeFaultStatement.Parse(lexer);
            else if (lexer.Current.Value is Macro)
                result = new ScriptCodeMacroCommand((Macro)lexer.Current.Value);
            else throw CodeAnalysisException.InvalidExpressionTerm(lexer.Current);
            result.LinePragma = new ScriptDebugInfo { Start = beginning, End = lexer.Current.Key };
            return result;
        }

        private static bool ParseSlot(IEnumerator<KeyValuePair<Lexeme.Position, Lexeme>> lexer, bool parseName, out string slotName, out ScriptCodeExpression initExpression, out ScriptCodeExpression typeExpression, params Punctuation[] terminator)
        {
            slotName = null;
            typeExpression = initExpression = null;
            //Matches to the slot name.
            switch (parseName)
            {
                case true:
                    var token = lexer.MoveNext(true);
                    if (token.OneOf(terminator)) return false;
                    else if (token is NameToken)
                        slotName = token;
                    else throw CodeAnalysisException.IdentifierExpected(lexer.Current.Key);
                    break;
                default:
                    slotName = lexer.Current.Value;
                    break;
            }
            if (!lexer.MoveNext()) throw CodeAnalysisException.InvalidPunctuation(Punctuation.Semicolon, lexer.Current);
            //Parse initialization expression
            if (lexer.Current.Value.OneOf(Operator.Assignment))
                switch (lexer.MoveNext())
                {
                    case true:
                        initExpression = Parser.ParseExpression(lexer, terminator + Punctuation.Colon);
                        break;
                    default:
                        throw CodeAnalysisException.InvalidPunctuation(Punctuation.Colon, lexer.Current);
                }
            //Parse variable type.
            if (lexer.Current.Value.OneOf(Punctuation.Colon))
                switch (lexer.MoveNext())
                {
                    case true:
                        typeExpression = Parser.ParseExpression(lexer, terminator);
                        break;
                    default:
                        throw CodeAnalysisException.InvalidPunctuation(terminator[0], lexer.Current);
                }
            switch (lexer.Current.Value.OneOf(terminator))
            {
                case true: return true;
                default:
                    throw CodeAnalysisException.InvalidPunctuation(terminator[0], lexer.Current);
            }
        } 

        /// <summary>
        /// Parses object or type slot.
        /// </summary>
        /// <param name="lexer">An enumerator through lexemes. Cannot be <see langword="null"/>.</param>
        /// <param name="slotName">The name of the slot.</param>
        /// <param name="initExpression">Initialization expression.</param>
        /// <param name="typeExpression">Type binding expression.</param>
        /// <param name="terminator">An array of the slot termination lexemes.</param>
        /// <returns><see langword="true"/> if slot is parsed successfully; <see langword="false"/> if slot declaration is empty.</returns>
        /// <exception cref="System.ArgumentNullException"><paramref name="lexer"/> is <see langword="null"/>.</exception>
        /// <remarks>The slot represents the following construction: slot-name[`=`init-expression][`:`type-expression]</remarks>
        public static bool ParseSlot(IEnumerator<KeyValuePair<Lexeme.Position, Lexeme>> lexer, out string slotName, out ScriptCodeExpression initExpression, out ScriptCodeExpression typeExpression, params Punctuation[] terminator)
        {
            if (lexer == null) throw new ArgumentNullException("lexer");
            return ParseSlot(lexer, true, out slotName, out initExpression, out typeExpression, terminator);
        }

        /// <summary>
        /// Parses expression list.
        /// </summary>
        /// <param name="lexer">An enumerator through lexemes. Cannot be <see langword="null"/>.</param>
        /// <param name="expressions"></param>
        /// <param name="terminator">An array of the expression list terminators.</param>
        /// <remarks>The expression list has the following syntax: [expression`,`]*</remarks>
        public static void ParseExpressions(IEnumerator<KeyValuePair<Lexeme.Position, Lexeme>> lexer, CodeExpressionCollection expressions, Lexeme terminator)
        {
            ParseExpressions(lexer, true, expressions, terminator);
        }

        private static void ParseExpressions(IEnumerator<KeyValuePair<Lexeme.Position, Lexeme>> lexer, bool moveLexer, CodeExpressionCollection expressions, Lexeme terminator)
        {
            if (lexer == null) throw new ArgumentNullException("lexer");
            if (expressions == null) expressions = new CodeExpressionCollection();
            if (moveLexer && !lexer.MoveNext()) throw CodeAnalysisException.InvalidExpressionTerm(lexer.Current); //Select next token after bracket
            if (lexer.Current.Value != terminator)
            {
                var expr = ParseExpression(lexer, Punctuation.Comma, terminator);
                if (expr != null) expressions.Add(expr);
                if (lexer.Current.Value == Punctuation.Comma) ParseExpressions(lexer, true, expressions, terminator);
            }
        }
    }
}
