using System;

namespace DynamicScript.Runtime
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;

    /// <summary>
    /// Represents delegate that contains script program.
    /// </summary>
    /// <param name="state">Internal interpreter state. Cannot be <see langword="null"/>.</param>
    /// <returns>Script invocation result.</returns>
    [ComVisible(false)]
    public delegate dynamic ScriptInvoker(InterpreterState state);
}
