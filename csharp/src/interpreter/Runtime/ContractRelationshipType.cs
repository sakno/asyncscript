using System;

namespace DynamicScript.Runtime
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;

    /// <summary>
    /// Represents relationship between two contracts.
    /// </summary>
    [ComVisible(false)]
    public enum ContractRelationshipType : short
    {
        /// <summary>
        /// Two contracts are not compatible.
        /// </summary>
        None = 0,

        /// <summary>
        /// The first contract defines subset of the second contract.
        /// </summary>
        Subset = 0x01,

        /// <summary>
        /// The first contract defines superset of the second contract.
        /// </summary>
        Superset = 0x02,

        /// <summary>
        /// Two contracts are equal.
        /// </summary>
        TheSame = 0x04
    }
}
