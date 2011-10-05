using System;
using System.Collections.Generic;

namespace DynamicScript.Runtime.Environment
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;

    /// <summary>
    /// Represents script object that implements collection functionality.
    /// </summary>
    [ComVisible(false)]
    [CLSCompliant(false)]
    public interface IScriptObjectCollection: IScriptContainer, ICollection<IScriptObject>
    {
    }
}
