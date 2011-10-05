using System;

namespace DynamicScript.Runtime.Debugging
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;

    /// <summary>
    /// Represents runtime slot that supports debug-time editing.
    /// </summary>
    [ComVisible(false)]
    interface IDebuggerBrowsable: IRuntimeSlot
    {
        bool TryGetValue(out string value, InterpreterState state);
    }
}
