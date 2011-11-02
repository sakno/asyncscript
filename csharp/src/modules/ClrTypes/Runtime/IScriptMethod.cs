using System;

namespace DynamicScript.Runtime
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;
    using MethodInfo = System.Reflection.MethodInfo;

    /// <summary>
    /// Represents a script wrapper for the managed method.
    /// </summary>
    [ComVisible(false)]
    public interface IScriptMethod : IScriptAction, IScriptConvertible
    {
        /// <summary>
        /// Gets an object for the instance method.
        /// </summary>
        new object This { get; }

        /// <summary>
        /// Gets method metadata.
        /// </summary>
        MethodInfo Method { get; }
    }
}
