using System;

namespace DynamicScript.Runtime
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;

    /// <summary>
    /// Represents behavior options used to control DynamicScript runtime.
    /// This class cannot be inherited;
    /// </summary>
    [ComVisible(false)]
    [Serializable]
    public sealed class RuntimeBehavior
    {
        internal const string DataSlotName = "{B2C9C465-EC7E-48BB-BE92-051DB35935AF}";
        internal const bool DefaultOmitVoidYieldInLoops = true;

        /// <summary>
        /// Represents a value indicating whether the loop doesn't return VOID value returned
        /// from the single iteration.
        /// </summary>
        /// <remarks>The default value of this option is <see langword="true"/>.</remarks>
        public bool OmitVoidYieldInLoops = DefaultOmitVoidYieldInLoops;
    }
}
