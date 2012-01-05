using System;

namespace DynamicScript.Compiler.Ast
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;

    /// <summary>
    /// Represents compile-time information about contract.
    /// </summary>
    [ComVisible(false)]
    public interface IWellKnownContractInfo: ISyntaxTreeNode
    {
        /// <summary>
        /// Gets type code.
        /// </summary>
        ScriptTypeCode GetTypeCode();
    }
}
