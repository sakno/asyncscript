using System;

namespace DynamicScript
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;

    /// <summary>
    /// Represents error code that can be occured during DynamicScript interpretation.
    /// </summary>
    [ComVisible(false)]
    [Serializable]
    public enum InterpreterErrorCode: int
    {
        /// <summary>
        /// Represents undefined fatal interpreter error.
        /// </summary>
        Internal = 0,

        /// <summary>
        /// The input character cannot be recognized by lexical analyzer.
        /// </summary>
        UnknownCharacter = 1,

        /// <summary>
        /// End of the multiline comment expected.
        /// </summary>
        EndOfCommentExpected = 2,

        /// <summary>
        /// Identifier name expected.
        /// </summary>
        IdentifierExpected = 3,

        /// <summary>
        /// Special punctuation token expected.
        /// </summary>
        InvalidPunctuation = 4,

        /// <summary>
        /// Expression has invalid format.
        /// </summary>
        InvalidExpressionTerm = 5,
        
        /// <summary>
        /// Wrong escape sequence in string.
        /// </summary>
        WrongEscapeSequence=6,

        /// <summary>
        /// Floating-point literal has invalid format.
        /// </summary>
        InvalidFloatingPointNumberFormat = 7,

        /// <summary>
        /// The variable with the specified name is already declared in the lexical scope.
        /// </summary>
        DuplicateVariableDeclaration = 8,

        /// <summary>
        /// Action expression doesn't contain return type.
        /// </summary>
        ReturnTypeOrVoidExpected = 9,

        /// <summary>
        /// Expression or statement is incompleted.
        /// </summary>
        IncompletedExpressionOrStatement = 10,

        /// <summary>
        /// Invalid for-loop grouping expression or operator.
        /// </summary>
        InvalidLoopGrouping = 11,

        /// <summary>
        /// Constant statement doesn't have initialization expression.
        /// </summary>
        UninitializedConstant = 12,

        /// <summary>
        /// Identifier is undeclared.
        /// </summary>
        UndeclaredIdentifier = 13,

        /// <summary>
        /// DynamicScript object doesn't satisfy to the contract.
        /// </summary>
        FailedContractBinding = 14,

        /// <summary>
        /// Attempts to set value to the constant after its initialization.
        /// </summary>
        ConstantValueCannotBeChanged = 15,

        /// <summary>
        /// Attempts to read value from unassigned variable in the checked context.
        /// </summary>
        UnassignedSlotReading = 16,

        /// <summary>
        /// Attempts to use binary, unary or application operator with unsupported operand types.
        /// </summary>
        UnsupportedOperation = 17,

        /// <summary>
        /// Operation with void object is performed.
        /// </summary>
        OperationOnVoidPerformed = 18,

        /// <summary>
        /// The actual arguments are not satisfied to the action parameters.
        /// </summary>
        ArgumentMistmatch = 19,

        /// <summary>
        /// The object is not a contract.
        /// </summary>
        ContractExpected = 20,

        /// <summary>
        /// It is not possible to return from FINALLY block.
        /// </summary>
        ReturnFromFinally = 21
    }
}
