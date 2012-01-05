using System;

namespace DynamicScript.Runtime.Environment
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;

    /// <summary>
    /// Represents runtime slot that exposes access to the named object.
    /// </summary>
    [ComVisible(false)]
    public sealed class NamedRuntimeSlot : ScriptObject.RuntimeSlotBase, IStaticRuntimeSlot
    {
        /// <summary>
        /// Represents name of the accessible slot.
        /// </summary>
        public readonly string Name;

        /// <summary>
        /// Represents slot owner.
        /// </summary>
        public readonly IScriptObject This;
        
        /// <summary>
        /// Represents contract binding.
        /// </summary>
        public readonly IScriptContract ContractBinding;

        /// <summary>
        /// Initializes a new virtual slot that provides access to the named slot of the specified object.
        /// </summary>
        /// <param name="this">The slot owner.</param>
        /// <param name="slotName">The name of the accessible slot.</param>
        /// <param name="contractBinding">Optional contract binding.</param>
        public NamedRuntimeSlot(IScriptObject @this, string slotName, IScriptContract contractBinding = null)
        {
            Name = slotName;
            This = @this;
            ContractBinding = contractBinding ?? ScriptSuperContract.Instance;
        }

        /// <summary>
        /// Deletes the value of the runtime slot.
        /// </summary>
        /// <returns>This method always returns <see langword="false"/>.</returns>
        public override bool DeleteValue()
        {
            return false;
        }

        /// <summary>
        /// Reads value of the named slot.
        /// </summary>
        /// <param name="state"></param>
        /// <returns></returns>
        public override IScriptObject GetValue(InterpreterState state)
        {
            return This[Name, state];
        }

        /// <summary>
        /// Writes value to the named slot.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        public override IScriptObject SetValue(IScriptObject value, InterpreterState state)
        {
            return This[Name, state] = value;
        }

        IScriptContract IStaticRuntimeSlot.ContractBinding
        {
            get { return ContractBinding; }
        }
    }
}
