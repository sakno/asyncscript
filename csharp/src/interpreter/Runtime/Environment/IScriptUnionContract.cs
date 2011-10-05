using System;
using System.Collections.Generic;

namespace DynamicScript.Runtime.Environment
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;

    [ComVisible(false)]
    interface IScriptUnionContract: IScriptContract, IEnumerable<IScriptContract>
    {
    }
}
