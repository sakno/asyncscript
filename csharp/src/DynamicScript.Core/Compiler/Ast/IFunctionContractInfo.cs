using System;

namespace DynamicScript.Compiler.Ast
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;

    /// <summary>
    /// Represents compile-time information about function type.
    /// </summary>
    [ComVisible(false)]
    public interface IFunctionContractInfo: IWellKnownContractInfo
    {
        /// <summary>
        /// Returns an object that describes return type of the function.
        /// </summary>
        /// <returns></returns>
        IWellKnownContractInfo GetReturnType();
    }
}
