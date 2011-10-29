using System;
using System.Collections.Generic;

namespace DynamicScript.Runtime.Environment
{
    using Compiler.Ast;
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;

    /// <summary>
    /// Represents asynchronous object that implements Future pattern.
    /// </summary>
    [ComVisible(false)]
    interface IScriptAsyncObject: IScriptProxyObject, IAsyncResult
    {
        /// <summary>
        /// Schedules a new binary script operation.
        /// </summary>
        /// <param name="left">The left operand.</param>
        /// <param name="operator">The operator.</param>
        /// <param name="state">Internal interpreter state.</param>
        /// <returns>A new asynchronous object </returns>
        IScriptAsyncObject Enqueue(IScriptObject left, ScriptCodeBinaryOperatorType @operator, InterpreterState state);
    }
}
