using System;

namespace DynamicScript.Runtime.Environment
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;

    /// <summary>
    /// Represents data type complementation.
    /// </summary>
    [ComVisible(false)]
    public interface IScriptComplementation: IScriptContract
    {
        /// <summary>
        /// Gets complemented data type.
        /// </summary>
        IScriptContract SourceContract { get; }
    }
}
