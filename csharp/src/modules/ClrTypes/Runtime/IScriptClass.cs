using System;

namespace DynamicScript.Runtime
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;

    /// <summary>
    /// Represents the script wrapper of the native .NET type.
    /// </summary>
    [ComVisible(false)]
    public interface IScriptClass: IScriptContract
    {
        /// <summary>
        /// Gets underlying type.
        /// </summary>
        Type NativeType { get; }
    }
}
