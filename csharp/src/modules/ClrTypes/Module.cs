using System;
using DynamicScript.Runtime;
using DynamicScript.Runtime.Environment;

namespace DynamicScript.Modules.ClrTypes
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;

    /// <summary>
    /// Represents implementation of ClrTypes module.
    /// This class cannot be inherited.
    /// </summary>
    [ComVisible(false)]
    public sealed class Module: ScriptCompositeObject
    {
        /// <summary>
        /// Executes an entry point of the module.
        /// </summary>
        /// <param name="state"></param>
        /// <returns></returns>
        public static IScriptObject Run(InterpreterState state)
        {
            return new Module();
        }
    }
}
