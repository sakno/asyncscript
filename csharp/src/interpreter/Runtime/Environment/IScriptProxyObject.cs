using System;

namespace DynamicScript.Runtime.Environment
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;

    /// <summary>
    /// Represents proxy script object that holds reference to another
    /// script object.
    /// </summary>
    [ComVisible(false)]
    interface IScriptProxyObject: IScriptObject
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
    }
}
