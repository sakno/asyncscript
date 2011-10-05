using System;

namespace DynamicScript.Runtime.Environment
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;

    /// <summary>
    /// Represents converter from .NET Framework object to DynamicScript-compliant object.
    /// </summary>
    [ComVisible(false)]
    
    public interface IRuntimeConverter : IEquatable<IRuntimeConverter>
    {
        /// <summary>
        /// Converts .NET Framework object to DynamicScript-compliant object.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <param name="result">Conversion result.</param>
        /// <returns><see langword="true"/> if conversion is supported; otherwise, <see langword="false"/>.</returns>
        bool Convert(object value, out IScriptObject result);
    }
}
