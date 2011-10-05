using System;

namespace DynamicScript.Compiler
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;
    using IKeywordTable = System.Collections.Generic.IDictionary<string, Keyword>;
    using ParsedLexeme = System.Collections.Generic.KeyValuePair<Lexeme.Position, Lexeme>;
    using ILexemeAnalyzer =System.Collections.Generic.IEnumerator<System.Collections.Generic.KeyValuePair<Lexeme.Position, Lexeme>>;
    using CodeBinaryOperatorType = System.CodeDom.CodeBinaryOperatorType;
    using Ast;

    /// <summary>
    /// Represents additional interpreter services.
    /// </summary>
    [ComVisible(false)]
    static class InterpreterServices
    {
        /// <summary>
        /// Moves to the next lexeme.
        /// </summary>
        /// <param name="lexer">An enumerator through lexemes. Cannot be <see langword="null"/>.</param>
        /// <param name="throwError"><see langword="true"/> to throw an exception if iteration fails; <see langword="false"/> to return <see langword="null"/>.</param>
        /// <returns></returns>
        public static Lexeme MoveNext(this ILexemeAnalyzer lexer, bool throwError)
        {
            if (lexer == null) throw new ArgumentNullException("lexer");
            switch (lexer.MoveNext())
            {
                case true: return lexer.Current.Value;
                default:
                    if (throwError) throw CodeAnalysisException.IncompletedExpression(lexer.Current.Key);
                    else return null;
            }
        }

        /// <summary>
        /// Adds a new keyword to the symbol table.
        /// </summary>
        /// <param name="hashTable">The symbol table with keywords. Cannot be <see langword="null"/>.</param>
        /// <param name="keyword">The keyword to be added. Cannot be <see langword="null"/>.</param>
        /// <exception cref="System.ArgumentNullException"><paramref name="hashTable"/> or <paramref name="keyword"/> is <see langword="null"/>.</exception>
        public static void Add(this IKeywordTable hashTable, Keyword keyword)
        {
            if (hashTable == null) throw new ArgumentNullException("hashTable");
            if (keyword == null) throw new ArgumentNullException("keyword");
            hashTable.Add(keyword.ToString(), keyword);
        }

        public static bool MoveNext(this ILexemeAnalyzer lexer, Token p, bool throwError)
        {
            switch (MoveNext(lexer, throwError) == p)
            {
                case true:
                    return true;
                default:
                    if (throwError) throw CodeAnalysisException.InvalidPunctuation(p, lexer.Current);
                    else return false;
            }
        }

        public static T MoveNext<T>(this ILexemeAnalyzer lexer, bool throwError)
            where T: Lexeme
        {
            switch (MoveNext(lexer, throwError) is T)
            {
                case true:
                    return (T)lexer.Current.Value;
                default:
                    if (throwError) throw CodeAnalysisException.Expected<T>(lexer.Current);
                    else return null;
            }
        }

        public const int HighestPriority = 11;

        /// <summary>
        /// Returns priority of the binary operator.
        /// </summary>
        /// <param name="operator">DynamicScript operator type.</param>
        /// <returns>The integer that represents operator priority.</returns>
        public static int GetPriority(this ScriptCodeBinaryOperatorType @operator)
        {
            switch (@operator)
            {
                case ScriptCodeBinaryOperatorType.MemberAccess:
                case ScriptCodeBinaryOperatorType.MetadataDiscovery:
                    return HighestPriority;
                case ScriptCodeBinaryOperatorType.TypeCast:
                    return 10;
                case ScriptCodeBinaryOperatorType.Multiply:
                case ScriptCodeBinaryOperatorType.Modulo:
                case ScriptCodeBinaryOperatorType.Divide: return 9;
                case ScriptCodeBinaryOperatorType.Add:
                case ScriptCodeBinaryOperatorType.Subtract: return 8;
                case ScriptCodeBinaryOperatorType.LessThan:
                case ScriptCodeBinaryOperatorType.GreaterThan:
                case ScriptCodeBinaryOperatorType.GreaterThanOrEqual:
                case ScriptCodeBinaryOperatorType.LessThanOrEqual:
                case ScriptCodeBinaryOperatorType.InstanceOf:
                    return 7;
                case ScriptCodeBinaryOperatorType.Intersection:
                    return 6;
                case ScriptCodeBinaryOperatorType.Exclusion:
                    return 5;
                case ScriptCodeBinaryOperatorType.Union:
                    return 4;
                case ScriptCodeBinaryOperatorType.AndAlso:
                    return 3;
                case ScriptCodeBinaryOperatorType.OrElse:
                    return 2;
                case ScriptCodeBinaryOperatorType.ValueInequality:
                case ScriptCodeBinaryOperatorType.ReferenceInequality:
                case ScriptCodeBinaryOperatorType.ReferenceEquality:
                case ScriptCodeBinaryOperatorType.ValueEquality: return 1;
                case ScriptCodeBinaryOperatorType.AdditiveAssign:
                case ScriptCodeBinaryOperatorType.SubtractiveAssign:
                case ScriptCodeBinaryOperatorType.Expansion:
                case ScriptCodeBinaryOperatorType.Reduction:
                case ScriptCodeBinaryOperatorType.MultiplicativeAssign:
                case ScriptCodeBinaryOperatorType.DivideAssign:
                case ScriptCodeBinaryOperatorType.ExclusionAssign:
                case ScriptCodeBinaryOperatorType.Coalesce:
                case ScriptCodeBinaryOperatorType.ModuloAssign:
                case ScriptCodeBinaryOperatorType.Assign: return 0;
                default: return -1;
            }
        }

        public static int GetPriority(this ScriptCodeUnaryOperatorType @operator)
        {
            switch (@operator)
            {
                case ScriptCodeUnaryOperatorType.Intern:
                case ScriptCodeUnaryOperatorType.DecrementPostfix:
                case ScriptCodeUnaryOperatorType.IncrementPostfix:
                case ScriptCodeUnaryOperatorType.SquarePostfix:
                case ScriptCodeUnaryOperatorType.TypeOf:
                case ScriptCodeUnaryOperatorType.VoidCheck:
                    return HighestPriority;
                case ScriptCodeUnaryOperatorType.DecrementPrefix:
                case ScriptCodeUnaryOperatorType.IncrementPrefix:
                case ScriptCodeUnaryOperatorType.SquarePrefix:
                case ScriptCodeUnaryOperatorType.Minus:
                case ScriptCodeUnaryOperatorType.Plus:
                case ScriptCodeUnaryOperatorType.Negate:
                    return 10;
                default:
                case ScriptCodeUnaryOperatorType.Unknown:
                    return -1;
            }
        }
    }
}
