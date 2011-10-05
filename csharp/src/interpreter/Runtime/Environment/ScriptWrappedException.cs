using System;
using System.Collections.Generic;

namespace DynamicScript.Runtime.Environment
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;

    /// <summary>
    /// Represents DynamicScript object that wraps .NET exception.
    /// This class cannot be inherited.
    /// </summary>
    [ComVisible(false)]
    sealed class ScriptWrappedException: ScriptCompositeObject
    {
        #region Nested Types
        [ComVisible(false)]
        private sealed new class Slots: ObjectSlotCollection
        {
            private const string MessageSlot = "message";
            private const string StackTraceSlot = "stacktrace";
            private const string HelpSlot = "help";

            public Slots(Exception e)
            {
                if (e == null) throw new ArgumentNullException("e");
                AddConstant(MessageSlot, new ScriptString(e.Message ?? String.Empty));   //short exception message
                AddConstant(StackTraceSlot, new ScriptString(e.StackTrace ?? String.Empty)); //full description of the exception
                AddConstant(HelpSlot, new ScriptString(e.HelpLink ?? String.Empty));
            }
        }
        #endregion

        /// <summary>
        /// Initializes a new exception wrapper.
        /// </summary>
        /// <param name="e">The exception to wrap. Cannot be <see langword="null"/>.</param>
        public ScriptWrappedException(Exception e)
            : base(new Slots(e))
        {
        }
    }
}
