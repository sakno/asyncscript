using System;

namespace DynamicScript.Runtime.Environment
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;

    /// <summary>
    /// Represents a contract that can be assignable to meta contract.
    /// </summary>
    [ComVisible(false)]
    public interface IScriptMetaContract: IScriptContract
    {
    }
}
