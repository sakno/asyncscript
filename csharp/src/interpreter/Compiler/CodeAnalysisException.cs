using System;
using System.Runtime.Serialization;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace DynamicScript.Compiler
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;

    /// <summary>
    /// Represents an exception occured when code parsing or interpretation fails.
    /// This class cannot be inherited.
    /// </summary>
    
    [ComVisible(false)]
    [Serializable]
    public class CodeAnalysisException : DynamicScriptException, IRestorable
    {
        private const string ErrorCodeSerializationEntry = "ErrorCode";
        private const string PositionSerializationEntry = "Position";
        private const string MessageSerializationEntry = "InterpreterMessage";

        private readonly InterpreterErrorCode m_errorCode;
        private readonly Lexeme.Position m_codePosition;
        private readonly string m_message;

        private CodeAnalysisException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            m_errorCode = (InterpreterErrorCode)info.GetValue(ErrorCodeSerializationEntry, typeof(InterpreterErrorCode));
            m_codePosition = (Lexeme.Position)info.GetValue(PositionSerializationEntry, typeof(Lexeme.Position));
            m_message = info.GetString(MessageSerializationEntry);
        }

        private CodeAnalysisException(string message, InterpreterErrorCode errorCode, Lexeme.Position codePosition)
            : base(String.Format(ErrorMessages.FmtErrorMessage, message, codePosition.Column, codePosition.Line))
        {
            m_errorCode = errorCode;
            m_codePosition = codePosition;
            m_message = message;
        }

        private CodeAnalysisException(string message, InterpreterErrorCode errorCode, int column, int line)
            : this(message, errorCode, new Lexeme.Position(line, column))
        {
            
        }

        internal CodeAnalysisException(string message, InterpreterErrorCode errorCode, ScriptDebugInfo info)
            : this(message, errorCode, info != null ? info.Start : new Lexeme.Position())
        {

        }

        /// <summary>
        /// Gets error message emitted during code analysis.
        /// </summary>
        public new string Message
        {
            get{return m_message;}
        }

        /// <summary>
        /// Gets error code.
        /// </summary>
        public sealed override InterpreterErrorCode ErrorCode
        {
            get { return m_errorCode; }
        }

        /// <summary>
        /// Gets position of the lexeme.
        /// </summary>
        internal Lexeme.Position Position
        {
            get { return m_codePosition; }
        }

        internal static Expression<Func<string, InterpreterErrorCode, int, int, CodeAnalysisException>> Constructor
        {
            get { return (msg, code, column, line) => new CodeAnalysisException(msg, code, column, line); }
        }

        /// <summary>
        /// Returns an expression that produces the current exception.
        /// </summary>
        /// <returns>An expression that produces the current exception.</returns>
        public Expression Restore()
        {
            return Expression.New(((NewExpression)Constructor.Body).Constructor,
                Expression.Constant(Message),
                Expression.Constant(ErrorCode),
                Expression.Constant(Position.Column),
                Expression.Constant(Position.Line));
        }

        #region Lexical analysis errors

        internal static CodeAnalysisException UnknownCharacter(char c, int column, int line)
        {
            return new CodeAnalysisException(String.Format(ErrorMessages.UnknownCharacter, c), InterpreterErrorCode.UnknownCharacter, column, line);
        }

        internal static CodeAnalysisException EndOfCommentExpected(int column, int line)
        {
            return new CodeAnalysisException(ErrorMessages.EndOfCommentExpected, InterpreterErrorCode.EndOfCommentExpected, column, line);
        }

        internal static CodeAnalysisException WrongEscapeSequence(int column, int line)
        {
            return new CodeAnalysisException(ErrorMessages.WrongEscapeSequence, InterpreterErrorCode.WrongEscapeSequence, column, line);
        }

        internal static CodeAnalysisException InvalidFloatingPointNumberFormat(int column, int line)
        {
            return new CodeAnalysisException(ErrorMessages.InvalidFloatingPointNumberFormat, InterpreterErrorCode.InvalidFloatingPointNumberFormat, column, line);
        }

        #endregion

        #region Syntax errors

        internal static CodeAnalysisException InvalidExpressionTerm(KeyValuePair<Lexeme.Position, Lexeme> current)
        {
            return new CodeAnalysisException(String.Format(ErrorMessages.InvalidExpressionTerm, current.Value), InterpreterErrorCode.InvalidExpressionTerm, current.Key);
        }

        internal static CodeAnalysisException IdentifierExpected(Lexeme.Position position)
        {
            return new CodeAnalysisException(ErrorMessages.IdentifierExpected, InterpreterErrorCode.IdentifierExpected, position);
        }

        internal static CodeAnalysisException InvalidPunctuation(Lexeme expected, KeyValuePair<Lexeme.Position, Lexeme> actual)
        {
            return new CodeAnalysisException(String.Format(ErrorMessages.InvalidPunctuation, expected, actual.Value), InterpreterErrorCode.InvalidPunctuation, actual.Key);
        }

        internal static CodeAnalysisException Expected<T>(KeyValuePair<Lexeme.Position, Lexeme> current)
            where T : Lexeme
        {
            if (Equals(typeof(T), typeof(NameToken)))
                return IdentifierExpected(current.Key);
            else return new CodeAnalysisException(ErrorMessages.FatalError, InterpreterErrorCode.Internal, current.Key);
        }

        internal static CodeAnalysisException ActionReturnTypeExpected(Lexeme.Position position)
        {
            return new CodeAnalysisException(ErrorMessages.ReturnTypeOrVoidExpected, InterpreterErrorCode.ReturnTypeOrVoidExpected, position);
        }

        internal static CodeAnalysisException IncompletedExpression(Lexeme.Position position)
        {
            return new CodeAnalysisException(ErrorMessages.IncompletedExpression, InterpreterErrorCode.IncompletedExpressionOrStatement, position);
        }

        internal static CodeAnalysisException InvalidLoopGrouping(Lexeme.Position position)
        {
            return new CodeAnalysisException(ErrorMessages.InvalidLoopGrouping, InterpreterErrorCode.InvalidLoopGrouping, position);
        }

        internal static CodeAnalysisException DuplicateIdentifier(string name, Lexeme.Position position)
        {
            return new CodeAnalysisException(String.Format(ErrorMessages.DuplicateIdentifier, name), InterpreterErrorCode.DuplicateVariableDeclaration, position);
        }

        internal static CodeAnalysisException UnitializedConstant(Lexeme.Position position)
        {
            return new CodeAnalysisException(ErrorMessages.UninitializedConstant, InterpreterErrorCode.UninitializedConstant, position);
        }

        internal static CodeAnalysisException UndeclaredIdentifier(string identifier, ScriptDebugInfo info)
        {
            return new CodeAnalysisException(String.Format(ErrorMessages.UndeclaredIdentifier, identifier), InterpreterErrorCode.UndeclaredIdentifier, info.StartColumn, info.StartLine);
        }

        internal static CodeAnalysisException ReturnFromFinally(ScriptDebugInfo info)
        {
            return new CodeAnalysisException(ErrorMessages.ReturnFormFinally, InterpreterErrorCode.ReturnFromFinally, info.StartColumn, info.StartLine);
        }
        #endregion

        #region Internal and Runtime errors

        internal static CodeAnalysisException IdentifierExpected(ScriptDebugInfo position)
        {
            return IdentifierExpected(position.Start);
        }

        internal static CodeAnalysisException IncompletedExpression(ScriptDebugInfo dinfo)
        {
            return IncompletedExpression(dinfo.Start);
        }

        internal static CodeAnalysisException DuplicateIdentifier(string name, ScriptDebugInfo info)
        {
            return new CodeAnalysisException(String.Format(ErrorMessages.DuplicateIdentifier, name), InterpreterErrorCode.DuplicateVariableDeclaration, new Lexeme.Position(info.StartLine, info.StartColumn));
        }
        #endregion
    }
}
