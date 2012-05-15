using System;

namespace DynamicScript.Runtime
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;

    /// <summary>
    /// Represents script object that can be converted into the native .NET object.
    /// </summary>
    [ComVisible(false)]
    public interface IScriptConvertible: IScriptObject
    {
        /// <summary>
        /// Attempts to convert the script object to the specified native .NET type.
        /// </summary>
        /// <param name="conversionType">The requested type.</param>
        /// <param name="result">A conversion result.</param>
        /// <returns><see langword="true"/> if conversion is possible; otherwise, <see langword="false"/>.</returns>
        bool TryConvertTo(Type conversionType, out object result);

        /// <summary>
        /// Attempts to convert the script object to the default underlying .NET type.
        /// </summary>
        /// <param name="result"></param>
        /// <returns></returns>
        bool TryConvert(out object result);
    }
}
