using System;

namespace DynamicScript.Runtime.Environment
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;
    using ScriptCodeBinaryOperatorType = Compiler.Ast.ScriptCodeBinaryOperatorType;

    /// <summary>
    /// Represents proxy script object that holds reference to another
    /// script object.
    /// </summary>
    [ComVisible(false)]
    public interface IScriptProxyObject: IScriptObject
    {
        /// <summary>
        /// Defines contract expectation.
        /// </summary>
        /// <param name="contract">The expected contract.</param>
        /// <param name="state">Internal interpreter state.</param>
        void RequiresContract(IScriptContract contract, InterpreterState state);

        /// <summary>
        /// Returns a wrapped script object.
        /// </summary>
        /// <param name="state"></param>
        /// <returns></returns>
        IScriptObject Unwrap(InterpreterState state);

        /// <summary>
        /// Schedules a new binary script operation.
        /// </summary>
        /// <param name="left">The left operand.</param>
        /// <param name="operator">The operator.</param>
        /// <param name="state">Internal interpreter state.</param>
        /// <returns>A new asynchronous object </returns>
        IScriptObject Enqueue(IScriptObject left, ScriptCodeBinaryOperatorType @operator, InterpreterState state);

        /// <summary>
        /// Gets a value indicating whether the underlying object is alread computed.
        /// </summary>
        bool IsCompleted { get; }
    }
}
