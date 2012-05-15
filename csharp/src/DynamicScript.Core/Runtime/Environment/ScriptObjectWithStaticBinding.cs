using System;
using System.Dynamic;
using System.Linq.Expressions;
using System.ComponentModel;

namespace DynamicScript.Runtime.Environment
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;

    /// <summary>
    /// Represents DynamicScript object with static knowledge abouts its contract.
    /// </summary>
    /// <typeparam name="TContract">Type of the contract.</typeparam>
    [ComVisible(false)]
    public abstract class ScriptObjectWithStaticBinding<TContract>: ScriptObject
        where TContract: ScriptContract
    {
        /// <summary>
        /// Represents static contract binding.
        /// </summary>
        public readonly TContract ContractBinding;

        internal ScriptObjectWithStaticBinding(TContract contractBinding)
        {
            if (contractBinding == null) throw new ArgumentNullException("contractBinding");
            ContractBinding = contractBinding;
        }

        /// <summary>
        /// Returns a contract binding for the object.
        /// </summary>
        /// <returns>The contract binding for the object.</returns>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public sealed override IScriptContract GetContractBinding()
        {
            return ContractBinding;
        }
    }
}
