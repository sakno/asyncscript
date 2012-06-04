using System;

namespace DynamicScript.Compiler.Ast
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;

    /// <summary>
    /// Represents type of the unary operator.
    /// </summary>
    [Serializable]
    [ComVisible(false)]
    public enum ScriptCodeUnaryOperatorType: byte
    {
        /// <summary>
        /// Represents unknown unary operator.
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// Represents unary plus in the prefix form.
        /// </summary>
        Plus,

        /// <summary>
        /// Represents increment operator in the prefix form.
        /// </summary>
        IncrementPrefix,

        /// <summary>
        /// Represents increment operator in the postfix form.
        /// </summary>
        IncrementPostfix,

        /// <summary>
        /// Represents unary minus in the prefix form.
        /// </summary>
        Minus,

        /// <summary>
        /// Represents decrement operator in the prefix form.
        /// </summary>
        DecrementPrefix,

        /// <summary>
        /// Represents decrement operator in the postfix form.
        /// </summary>
        DecrementPostfix,

        /// <summary>
        /// Represents square operator in the prefix form.
        /// </summary>
        SquarePrefix,

        /// <summary>
        /// Represents square operator in the postfix form.
        /// </summary>
        SquarePostfix,

        /// <summary>
        /// Represents typeof operator.
        /// </summary>
        TypeOf,

        /// <summary>
        /// Represents negotiation.
        /// </summary>
        Negate,

        /// <summary>
        /// Literal intern operator.
        /// </summary>
        Intern,
    }
}
