using System;
using System.Collections.Generic;

namespace DynamicScript.Runtime.Environment
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;

    /// <summary>
    /// Represents a union of two or more data types.
    /// </summary>
    [ComVisible(false)]
    public interface IScriptUnionContract: IScriptContract, IEnumerable<IScriptContract>
    {
    }
}
