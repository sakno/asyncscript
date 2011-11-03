using System;

namespace DynamicScript.Runtime.Environment
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;

    /// <summary>
    /// Represents asynchronous object that implements Future pattern.
    /// </summary>
    [ComVisible(false)]
    interface IScriptAsyncObject: IScriptProxyObject, ISynchronizable
    {
    }
}
