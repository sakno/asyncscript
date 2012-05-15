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

        private static int GetPriority(Enum @operator)
        {
            if (@operator is ScriptCodeUnaryOperatorType)
                return InterpreterServices.GetPriority((ScriptCodeUnaryOperatorType)@operator);
            else if (@operator is ScriptCodeBinaryOperatorType)
                return InterpreterServices.GetPriority((ScriptCodeBinaryOperatorType)@operator);
            return -1;
        }

        private static Associativity GetAssociativity(ScriptCodeBinaryOperatorType @operator)
        {
            switch (@operator)
            {
                case ScriptCodeBinaryOperatorType.Assign:
                case ScriptCodeBinaryOperatorType.AdditiveAssign:
                case ScriptCodeBinaryOperatorType.SubtractiveAssign:
                case ScriptCodeBinaryOperatorType.MultiplicativeAssign:
                case ScriptCodeBinaryOperatorType.Initializer:
                case ScriptCodeBinaryOperatorType.DivideAssign:
                case ScriptCodeBinaryOperatorType.ModuloAssign:
                case ScriptCodeBinaryOperatorType.Expansion:
                case ScriptCodeBinaryOperatorType.Reduction:
                case ScriptCodeBinaryOperatorType.ExclusionAssign:
                    return Associativity.Right;
                default: return Associativity.Left;
            }
        }

        private static Associativity GetAssociativity(Enum @operator)
        {
            return @operator is ScriptCodeBinaryOperatorType ? GetAssociativity((ScriptCodeBinaryOperatorType)@operator) : Associativity.Left;
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
            switch (@operator.GetHashCode())
            {
                case Operator.HashCodes.lxmPlus: return lastIsExpression ? (Enum)ScriptCodeBinaryOperatorType.Add : ScriptCodeUnaryOperatorType.Plus;
                case Operator.HashCodes.lxmDoublePlus: return lastIsExpression ? ScriptCodeUnaryOperatorType.IncrementPostfix : ScriptCodeUnaryOperatorType.IncrementPrefix;
                case Operator.HashCodes.lxmDoubleAsterisk: return lastIsExpression ? ScriptCodeUnaryOperatorType.SquarePostfix : ScriptCodeUnaryOperatorType.SquarePrefix;
                case Operator.HashCodes.lxmMinus: return lastIsExpression ? (Enum)ScriptCodeBinaryOperatorType.Subtract : ScriptCodeUnaryOperatorType.Minus;
                case Operator.HashCodes.lxmDoubleMinus: return lastIsExpression ? ScriptCodeUnaryOperatorType.DecrementPostfix : ScriptCodeUnaryOperatorType.DecrementPrefix;
                case Operator.HashCodes.lxmAsterisk: return ScriptCodeBinaryOperatorType.Multiply;
                case Operator.HashCodes.lxmSlash: return ScriptCodeBinaryOperatorType.Divide;
                case Operator.HashCodes.lxmMemberAccess: return ScriptCodeBinaryOperatorType.MemberAccess;
                case Operator.HashCodes.lxmAssignment: return ScriptCodeBinaryOperatorType.Assign;
                case Operator.HashCodes.lxmExclusionAssignment: return ScriptCodeBinaryOperatorType.ExclusionAssign;
                case Operator.HashCodes.lxmSlashAssignment: return ScriptCodeBinaryOperatorType.DivideAssign;
                case Operator.HashCodes.lxmMinusAssignment: return ScriptCodeBinaryOperatorType.SubtractiveAssign;
                case Operator.HashCodes.lxmPlusAssignment: return ScriptCodeBinaryOperatorType.AdditiveAssign;
                case Operator.HashCodes.lxmAsteriskAssignment: return ScriptCodeBinaryOperatorType.MultiplicativeAssign;
                case Operator.HashCodes.lxmUnionAssignment: return ScriptCodeBinaryOperatorType.Expansion;
                case Operator.HashCodes.lxmIntersectionAssignment: return ScriptCodeBinaryOperatorType.Reduction;
                case Operator.HashCodes.lxmValueEquality: return ScriptCodeBinaryOperatorType.ValueEquality;
                case Operator.HashCodes.lxmReferenceEquality: return ScriptCodeBinaryOperatorType.ReferenceEquality;
                case Operator.HashCodes.lxmModulo: return ScriptCodeBinaryOperatorType.Modulo;
                case Keyword.HashCodes.lxmIs: return ScriptCodeBinaryOperatorType.InstanceOf;
                case Keyword.HashCodes.lxmIn: return ScriptCodeBinaryOperatorType.PartOf;
                case Keyword.HashCodes.lxmTo: return ScriptCodeBinaryOperatorType.TypeCast;
                case Operator.HashCodes.lxmLessThan: return ScriptCodeBinaryOperatorType.LessThan;
                case Operator.HashCodes.lxmLessThanOrEqual: return ScriptCodeBinaryOperatorType.LessThanOrEqual;
                case Operator.HashCodes.lxmGreaterThan: return ScriptCodeBinaryOperatorType.GreaterThan;
                case Operator.HashCodes.lxmGreaterThanOrEqual: return ScriptCodeBinaryOperatorType.GreaterThanOrEqual;
                case Operator.HashCodes.lxmIntersection: return ScriptCodeBinaryOperatorType.Intersection;
                case Operator.HashCodes.lxmUnion: return ScriptCodeBinaryOperatorType.Union;
                case Operator.HashCodes.lxmValueInequality: return ScriptCodeBinaryOperatorType.ValueInequality;
                case Operator.HashCodes.lxmReferenceInequality: return ScriptCodeBinaryOperatorType.ReferenceInequality;
                case Operator.HashCodes.lxmNegotiation: return ScriptCodeUnaryOperatorType.Negate;
                case Operator.HashCodes.lxmExclusion: return lastIsExpression ? (Enum)ScriptCodeBinaryOperatorType.Exclusion : ScriptCodeUnaryOperatorType.Intern;
                case Operator.HashCodes.lxmAndAlso: return ScriptCodeBinaryOperatorType.AndAlso;
                case Operator.HashCodes.lxmOrElse: return ScriptCodeBinaryOperatorType.OrElse;
                case Operator.HashCodes.lxmTypeOf: return lastIsExpression ? null : (Enum)ScriptCodeUnaryOperatorType.TypeOf;
                case Operator.HashCodes.lxmMetadataDiscovery: return ScriptCodeBinaryOperatorType.MetadataDiscovery;
                case Operator.HashCodes.lxmVoidCheck: return lastIsExpression ? (Enum)ScriptCodeUnaryOperatorType.VoidCheck : null;
                case Operator.HashCodes.lxmCoalesce: return ScriptCodeBinaryOperatorType.Coalesce;
                case Operator.HashCodes.lxmInitializer: return ScriptCodeBinaryOperatorType.Initializer;
                default: return null;
            }
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
                switch (lexer.Current.Value.GetHashCode())
                {
                    case Keyword.HashCodes.lxmObject:
                        if (expression == null) expression = ScriptCodeSuperContractExpression.Instance;
                        break;
                    case Keyword.HashCodes.lxmString:
                        if (expression == null) expression = ScriptCodeStringContractExpression.Instance;
                        break;
                    case Keyword.HashCodes.lxmDimensional:
                        if (expression == null) expression = ScriptCodeDimensionalContractExpression.Instance;
                        break;
                    case Keyword.HashCodes.lxmType:
                        if (expression == null) expression = ScriptCodeMetaContractExpression.Instance;
                        break;
                    case Keyword.HashCodes.lxmAsync:
                        if (expression == null)
                        { expression = ScriptCodeAsyncExpression.Parse(lexer, terminator); continue; }
                        break;
                    case Keyword.HashCodes.lxmTrue:
                        if (expression == null) expression = new ScriptCodeBooleanExpression(true);
                        break;
                    case Keyword.HashCodes.lxmFalse:
                        if (expression == null) expression = new ScriptCodeBooleanExpression(false);
                        break;
                    case Keyword.HashCodes.lxmBoolean:
                        if (expression == null) expression = ScriptCodeBooleanContractExpression.Instance;
                        break;
                    case Keyword.HashCodes.lxmReal:
                        if (expression == null) expression = ScriptCodeRealContractExpression.Instance;
                        break;
                    case Keyword.HashCodes.lxmThis:
                        if (expression == null) expression = ScriptCodeThisExpression.Instance;
                        break;
                    case Keyword.HashCodes.lxmInteger:
                        if (expression == null) expression = ScriptCodeIntegerContractExpression.Instance;
                        break;
                    case Keyword.HashCodes.lxmVoid:
                        if (expression == null) expression = ScriptCodeVoidExpression.Instance;
                        break;
                    case Keyword.HashCodes.lxmFinSet:
                        if (expression == null) expression = ScriptCodeFinSetContractExpression.Instance;
                        break;
                    case Keyword.HashCodes.lxmExpr:
                        if (expression == null) expression = ScriptCodeExpressionContractExpression.Instance;
                        break;
                    case Keyword.HashCodes.lxmStmt:
                        if (expression == null) expression = ScriptCodeStatementContractExpression.Instance;
                        break;
                    case Punctuation.HashCodes.lxmLeftBrace:
                        if (expression == null)
                        { expression = ScriptCodeComplexExpression.Parse(lexer); continue; }
                        break;
                    case Keyword.HashCodes.lxmCaseof:
                        if (expression == null)
                        { expression = ScriptCodeSelectionExpression.Parse(lexer, terminator); continue; }
                        break;
                    case Keyword.HashCodes.lxmFork:
                        if (expression == null)
                        { expression = ScriptCodeForkExpression.Parse(lexer, terminator); continue; }
                        break;
                    case Keyword.HashCodes.lxmCallable:
                        if (expression == null) expression = ScriptCodeCallableContractExpression.Instance;
                        break;
                    case Punctuation.HashCodes.lxmBackquote:
                        if (expression == null) expression = ScriptCodeObjectExpression.Parse(lexer, terminator);
                        break;
                    case Keyword.HashCodes.lxmGlobal:
                        if (expression == null) expression = ScriptCodeGlobalObjectExpression.Instance;
                        break;
                    case Keyword.HashCodes.lxmBase:
                        if (expression == null) expression = ScriptCodeBaseObjectExpression.Instance;
                        break;
                    case Keyword.HashCodes.lxmChecked:
                    case Keyword.HashCodes.lxmUnchecked:
                        if (expression == null)
                        { expression = ScriptCodeContextExpression.Parse(lexer, terminator); continue; }
                        break;
                    case Keyword.HashCodes.lxmTry://parse SEH block
                        if (expression == null)
                        { expression = ScriptCodeTryElseFinallyExpression.Parse(lexer, terminator); continue; }
                        break;
                    case Keyword.HashCodes.lxmDo://parse do-while-loop expression
                        if (expression == null)
                        { expression = ScriptCodeWhileLoopExpression.ParseDoWhileLoop(lexer, terminator); continue; }
                        break;
                    case Keyword.HashCodes.lxmWhile: //parse while-loop expresion
                        if (expression == null)
                        { expression = ScriptCodeWhileLoopExpression.ParseWhileLoop(lexer, terminator); continue; }
                        break;
                    case Keyword.HashCodes.lxmIf://parse conditional expression
                        if (expression == null)
                        { expression = ScriptCodeConditionalExpression.Parse(lexer, terminator); continue; }
                        break;
                    case Keyword.HashCodes.lxmFor://parse 'for' loop
                        if (expression == null)
                        { expression = ParseForLoop(lexer, terminator); continue; }
                        break;
                    case Keyword.HashCodes.lxmExpandq:
                        if (expression == null)
                            expression = ScriptCodeExpandExpression.Parse(lexer);
                        break;
                    case Punctuation.HashCodes.lxmDog://parse function
                        if (expression == null)
                        { expression = lexer.MoveNext() ? ParseAction(lexer, terminator) : ScriptCodeCurrentActionExpression.Instance; continue; }
                        break;
                    case Punctuation.HashCodes.lxmDoubleDog://parse quoted expression list
                        if (expression == null)
                        { expression = lexer.MoveNext() ? ParseQuoteExpression(lexer, terminator) : ScriptCodeCurrentQuoteExpression.Instance; continue; }
                        break;
                    case Punctuation.HashCodes.lxmLeftSquareBracket://parse indexer or array type or array
                        if (expression == null || InterpreterServices.HighestPriority > priority)
                            ParseIndexer(lexer, ref expression);
                        else return expression;
                        break;
                    case Punctuation.HashCodes.lxmLeftBracket:
                        if (expression == null)
                        {
                            //parse subexpression
                            lexer.MoveNext(true); //Select next token after bracket
                            expression = Parse(lexer, int.MinValue, Punctuation.RightBracket);
                        }
                        else
                        {
                            //parse function invocation
                            if (InterpreterServices.HighestPriority <= priority) return expression;
                            var invocation = new ScriptCodeInvocationExpression { Target = expression };
                            ParseExpressions(lexer, invocation.ArgList, Punctuation.RightBracket);
                            expression = invocation;
                        }
                        break;
                    case Keyword.HashCodes.lxmIs:
                    case Keyword.HashCodes.lxmIn:
                    case Keyword.HashCodes.lxmTo:
                        switch (ParseOperator(lexer, priority, ref expression, terminator))
                        {
                            case 0: break;
                            case 1: continue;
                            default: return expression;
                        }
                        break;
                    default:
                        if (lexer.Current.Value is NameToken && expression == null)
                            expression = new ScriptCodeVariableReference((NameToken)lexer.Current.Value);
                        else if (lexer.Current.Value is ArgRef && expression == null)
                            expression = new ScriptCodeArgumentReferenceExpression((ArgRef)lexer.Current.Value);
                        else if (lexer.Current.Value is IntegerLiteral && expression == null)
                            expression = new ScriptCodeIntegerExpression((IntegerLiteral)lexer.Current.Value);
                        else if (lexer.Current.Value is StringLiteral && expression == null)
                            expression = new ScriptCodeStringExpression((StringLiteral)lexer.Current.Value);
                        else if (lexer.Current.Value is RealLiteral && expression == null)
                            expression = new ScriptCodeRealExpression((RealLiteral)lexer.Current.Value);
                        else if (lexer.Current.Value is PlaceholderID && expression == null)
                            expression = new ScriptCodePlaceholderExpression((PlaceholderID)lexer.Current.Value);
                        else if (lexer.Current.Value is Operator)//Parse operator
                            switch (ParseOperator(lexer, priority, ref expression, terminator))
                            {
                                case 0: break;
                                case 1: continue;
                                default: return expression;
                            }
                        else throw CodeAnalysisException.InvalidExpressionTerm(lexer.Current); //Invalid expression term
                        break;
                }
                if (!lexer.MoveNext())
                    throw CodeAnalysisException.InvalidPunctuation(terminator[0], lexer.Current); //end of expression expected
            } while (!lexer.Current.Value.OneOf(terminator));
            return expression;
        }

        private static byte ParseOperator(IEnumerator<KeyValuePair<Lexeme.Position, Lexeme>> lexer, int priority, ref ScriptCodeExpression expression, Lexeme[] terminator)
        {
            var @operator = GetOperator(lexer.Current.Value, expression != null);
            if (@operator != null)
                if (GetAssociativity(@operator) == Associativity.Left ? GetPriority(@operator) > priority : GetPriority(@operator) >= priority)
                    if (@operator is ScriptCodeUnaryOperatorType)//Handles unary operator
                        switch (expression != null)
                        {
                            case true:
                                expression = new ScriptCodeUnaryOperatorExpression((ScriptCodeUnaryOperatorType)@operator, expression);
                                return 0;
                            default:
                                lexer.MoveNext(true);   //expression expected
                                expression = new ScriptCodeUnaryOperatorExpression((ScriptCodeUnaryOperatorType)@operator, Parse(lexer, GetPriority(@operator), terminator));
                                return 1;
                        }
                    else if (@operator is ScriptCodeBinaryOperatorType)//Handles binary operator
                    {
                        lexer.MoveNext(true); //Pass through operator token
                        var leftOperand = expression;
                        expression = new ScriptCodeBinaryOperatorExpression(leftOperand, (ScriptCodeBinaryOperatorType)@operator, Parse(lexer, GetPriority(@operator), terminator));
                        return 1;
                    }
            return 2;
        }

        public static ScriptCodeLoopWithVariableExpression ParseForLoop(IEnumerator<KeyValuePair<Lexeme.Position, Lexeme>> lexer, params Lexeme[] terminator)
        {
            if (lexer == null) throw new ArgumentNullException("lexer");
            var loopVariable = default(ScriptCodeLoopWithVariableExpression.LoopVariable);
            var temporary = default(bool);
            var loop = default(ScriptCodeLoopWithVariableExpression);
            var lexeme = lexer.MoveNext(true);    //pass through for keyword
            if (lexer.Current.Value.GetHashCode() == Keyword.HashCodes.lxmVar) //if var keyword is detected then temp var should be created
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
            switch (lexeme.GetHashCode())
            {
                case Keyword.HashCodes.lxmIn://parse for-each loop.
                    lexer.MoveNext(true); //pass through in keyword
                    loop = new ScriptCodeForEachLoopExpression { Variable = loopVariable, Iterator = ParseExpression(lexer, Keyword.GroupBy, Keyword.Do) };
                    break;
                case Operator.HashCodes.lxmAssignment://parse classic for loop
                    lexer.MoveNext(true); //pass through = operator
                    loopVariable.InitExpression = ParseExpression(lexer, Keyword.While);
                    lexer.MoveNext(true); //pass through while keyword
                    loop = new ScriptCodeForLoopExpression
                    {
                        Variable = loopVariable,
                        Condition = ParseExpression(lexer, Keyword.Do, Keyword.GroupBy) //parse condition
                    };
                    break;
                default:
                    throw CodeAnalysisException.InvalidExpressionTerm(lexer.Current);
            }
            if (lexer.Current.Value.GetHashCode() == Keyword.HashCodes.lxmGroupBy) //parse grouping expression.
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
            lexer.MoveNext(true);   //pass 'do' keyword
            loop.Body.SetExpression(ParseExpression, lexer, terminator);
            return loop;
        }
        

        private static void ParseIndexer(IEnumerator<KeyValuePair<Lexeme.Position, Lexeme>> lexer, ref ScriptCodeExpression expression)
        {
            lexer.MoveNext(true);   //pass through [ token
            //if the next token is ] then it is a single-dimensional array
            if (lexer.Current.Value == Punctuation.HashCodes.lxmRightSquareBracket)
                expression = expression != null ? (ScriptCodeExpression)new ScriptCodeArrayContractExpression { ElementContract = expression } : new ScriptCodeArrayExpression();
            else if (lexer.Current.Value == Punctuation.HashCodes.lxmComma && expression != null)   //if `,` then it is a multi-dimensional array
            {
                var arrayContract = new ScriptCodeArrayContractExpression { ElementContract = expression };
                do
                {
                    //Comma expected
                    if (lexer.Current.Value != Punctuation.HashCodes.lxmComma) throw CodeAnalysisException.InvalidPunctuation(Punctuation.Comma, lexer.Current);
                    arrayContract.Rank += 1;
                }
                while (lexer.MoveNext() && lexer.Current.Value.GetHashCode() != Punctuation.HashCodes.lxmRightSquareBracket);
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
            var signature = new ScriptCodeActionContractExpression();
            if (lexer.Current.Value == Keyword.HashCodes.lxmVoid)    //handles empty parameter list
            {
                signature.ParamList.Clear();
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
                    if (signature.ParamList.Contains(paramName))
                        throw CodeAnalysisException.DuplicateIdentifier(paramName, lexer.Current.Key);
                    else signature.ParamList.Add(paramName, paramDefVal, paramBinding);
                    if (lexer.Current.Value == Punctuation.HashCodes.lxmComma && !lexer.MoveNext()) throw CodeAnalysisException.InvalidPunctuation(Punctuation.Arrow, lexer.Current);
                }
            else return ScriptCodeCurrentQuoteExpression.Instance;
            //Handles -> punctuation token because it separates parameter list and return type.
            if (lexer.Current.Value != Punctuation.HashCodes.lxmArrow || !lexer.MoveNext()) throw CodeAnalysisException.InvalidPunctuation(Punctuation.Arrow, lexer.Current);
            //Parse return type expression
            signature.ReturnType = Parse(lexer, int.MinValue, terminator + Punctuation.Colon + Punctuation.LeftBrace);
            switch (lexer.Current.Value == Punctuation.HashCodes.lxmColon)
            {
                case true:
                    lexer.MoveNext(true);   //pass through colon
                    return new ScriptCodeQuoteExpression(signature, new ScriptCodeExpressionStatement(ParseExpression, lexer, terminator));
                default: throw CodeAnalysisException.InvalidPunctuation(Punctuation.Colon, lexer.Current);
            }
        }

        private static ScriptCodeExpression ParseAction(IEnumerator<KeyValuePair<Lexeme.Position, Lexeme>> lexer, Lexeme[] terminator)
        {
            var actionContract = new ScriptCodeActionContractExpression();
            if (lexer.Current.Value == Keyword.HashCodes.lxmVoid)    //handles empty parameter list
            {
                actionContract.ParamList.Clear();
                lexer.MoveNext();
            }
            else if (lexer.Current.Value is IntegerLiteral) //fast function, generates parameters
            {
                var count = ((IntegerLiteral)lexer.Current.Value).Value;
                for (var i = 0L; i < count; i++)
                    actionContract.ParamList.Add(ArgRef.MakeName(i), contractBinding: ScriptCodeSuperContractExpression.Instance);
                lexer.MoveNext(true);   //pass through argument count
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
                    if (lexer.Current.Value == Punctuation.HashCodes.lxmComma && !lexer.MoveNext()) throw CodeAnalysisException.InvalidPunctuation(Punctuation.Arrow, lexer.Current);
                }
            else return ScriptCodeCurrentActionExpression.Instance; //if token after @ is not VOID or name token then this is an action root contract.
            //Handles -> punctuation token because it separates parameter list and return type.
            if (lexer.Current.Value != Punctuation.HashCodes.lxmArrow || !lexer.MoveNext()) throw CodeAnalysisException.InvalidPunctuation(Punctuation.Arrow, lexer.Current);
            //Parse return type expression
            actionContract.ReturnType = Parse(lexer, int.MinValue, terminator + Punctuation.Colon + Punctuation.LeftBrace);
            //The expression represents action implementation, not contract
            switch (lexer.Current.Value == Punctuation.HashCodes.lxmColon)
            {
                case true:
                    lexer.MoveNext(true);   //pass through colon
                    return new ScriptCodeActionImplementationExpression(actionContract, new ScriptCodeExpressionStatement(ParseExpression, lexer, terminator));
                default: return actionContract;
            }
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
            if (terminator.LongLength > 0L && !lexer.MoveNext())
                throw CodeAnalysisException.EndOfStatementExpected(lexer.Current.Key);
        }

        /// <summary>
        /// Parse single statement.
        /// </summary>
        /// <param name="lexer"></param>
        /// <param name="terminator">An array of the statement terminators.</param>
        /// <returns></returns>
        public static ScriptCodeStatement ParseStatement(IEnumerator<KeyValuePair<Lexeme.Position, Lexeme>> lexer, params Punctuation[] terminator)
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
            switch (lexer.Current.Value.GetHashCode())
            {
                case Keyword.HashCodes.lxmVar:
                case Keyword.HashCodes.lxmConst:
                    result = ScriptCodeVariableDeclaration.Parse(lexer);
                    break;
                case Punctuation.HashCodes.lxmDog:
                case Keyword.HashCodes.lxmThis:
                case Keyword.HashCodes.lxmIf:
                case Keyword.HashCodes.lxmFor:
                case Keyword.HashCodes.lxmWhile:
                case Keyword.HashCodes.lxmDo:
                case Keyword.HashCodes.lxmTry:
                case Keyword.HashCodes.lxmChecked:
                case Keyword.HashCodes.lxmUnchecked:
                case Keyword.HashCodes.lxmCaseof:
                case Keyword.HashCodes.lxmFork:
                case Keyword.HashCodes.lxmInteger:
                case Keyword.HashCodes.lxmBoolean:
                case Keyword.HashCodes.lxmDimensional:
                case Keyword.HashCodes.lxmReal:
                case Keyword.HashCodes.lxmFinSet:
                case Keyword.HashCodes.lxmObject:
                case Keyword.HashCodes.lxmType:
                case Keyword.HashCodes.lxmExpr:
                case Keyword.HashCodes.lxmStmt:
                case Keyword.HashCodes.lxmString:
                case Keyword.HashCodes.lxmGlobal:
                case Keyword.HashCodes.lxmBase:
                case Keyword.HashCodes.lxmTrue:
                case Keyword.HashCodes.lxmFalse:
                case Keyword.HashCodes.lxmVoid:
                case Keyword.HashCodes.lxmExpandq:
                    result = new ScriptCodeExpressionStatement(ParseExpression(lexer, Punctuation.Semicolon));
                    break;
                case Punctuation.HashCodes.lxmSemicolon:
                    result = ScriptCodeEmptyStatement.Instance;
                    break;
                case Keyword.HashCodes.lxmReturn:
                    result = ScriptCodeReturnStatement.Parse(lexer);
                    break;
                case Keyword.HashCodes.lxmContinue:
                    result = ScriptCodeContinueStatement.Parse(lexer);
                    break;
                case Keyword.HashCodes.lxmLeave:
                    result = ScriptCodeBreakLexicalScopeStatement.Parse(lexer);
                    break;
                case Keyword.HashCodes.lxmFault:
                    result = ScriptCodeFaultStatement.Parse(lexer);
                    break;
                default:
                    if (lexer.Current.Value.OneOf<IntegerLiteral, NameToken, Operator, StringLiteral, RealLiteral>())
                        result = new ScriptCodeExpressionStatement(ParseExpression(lexer, Punctuation.Semicolon));
                    else if (lexer.Current.Value is Comment)
                        result = new ScriptCodeCommentStatement((Comment)lexer.Current.Value);
                    else throw CodeAnalysisException.InvalidExpressionTerm(lexer.Current);
                    break;
            }
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
            if (lexer.Current.Value == Operator.HashCodes.lxmAssignment)
                switch (lexer.MoveNext())
                {
                    case true:
                        initExpression = Parser.ParseExpression(lexer, terminator + Punctuation.Colon);
                        break;
                    default:
                        throw CodeAnalysisException.InvalidPunctuation(Punctuation.Colon, lexer.Current);
                }
            //Parse variable type.
            if (lexer.Current.Value == Punctuation.HashCodes.lxmColon)
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
        public static void ParseExpressions(IEnumerator<KeyValuePair<Lexeme.Position, Lexeme>> lexer, ScriptCodeExpressionCollection expressions, Lexeme terminator)
        {
            ParseExpressions(lexer, true, expressions, terminator);
        }

        private static void ParseExpressions(IEnumerator<KeyValuePair<Lexeme.Position, Lexeme>> lexer, bool moveLexer, ScriptCodeExpressionCollection expressions, Lexeme terminator)
        {
            if (lexer == null) throw new ArgumentNullException("lexer");
            if (expressions == null) expressions = new ScriptCodeExpressionCollection();
            if (moveLexer && !lexer.MoveNext()) throw CodeAnalysisException.InvalidExpressionTerm(lexer.Current); //Select next token after bracket
            if (lexer.Current.Value != terminator)
            {
                var expr = ParseExpression(lexer, Punctuation.Comma, terminator);
                if (expr != null) expressions.Add(expr);
                if (lexer.Current.Value == Punctuation.HashCodes.lxmComma) ParseExpressions(lexer, true, expressions, terminator);
            }
        }
    }
}
