using System;


namespace DynamicScript.Runtime
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;

#if USE_REL_MATRIX
    /// <summary>
    /// Represents relationship between two contract handles.
    /// </summary>
    [ComVisible(false)]
    [Serializable]
    struct ContractRelationshipHandle
    {
        public readonly ContractHandle SourceContractHandle;
        public readonly ContractHandle DestContractHandle;

        public ContractRelationshipHandle(ContractHandle sourceToken, ContractHandle destinationToken)
        {
            SourceContractHandle = sourceToken;
            DestContractHandle = destinationToken;
        }
    }
#endif
}
