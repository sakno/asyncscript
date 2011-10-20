using System;
using System.Collections.Concurrent;
using System.Threading;

namespace DynamicScript.Runtime
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;
    using SystemEnvironment = System.Environment;

#if USE_REL_MATRIX
    /// <summary>
    /// Represents relationship matrix that stores type of the relationship
    /// between two contracts.
    /// This class cannot be inherited.
    /// </summary>
    /// <remarks>With this matrix, the relationship detection is O(1) operation and not depends
    /// on the contract nature.</remarks>
    [ComVisible(false)]
    sealed class ContractRelationshipMatrix : ConcurrentDictionary<ContractRelationshipHandle, ContractRelationshipType>
    {
        public ContractRelationshipMatrix(int capacity = 100)
            : base(SystemEnvironment.ProcessorCount + 1, capacity)
        {
        }

        public ContractRelationshipType GetRelationship(ContractRelationshipHandle handle, Func<ContractRelationshipType> relationshipProvider)
        {
            return GetOrAdd(handle, h => relationshipProvider());
        }

        public ContractRelationshipType GetRelationship(ContractHandle source, ContractHandle destination, Func<ContractRelationshipType> relationshipProvider)
        {
            return GetRelationship(new ContractRelationshipHandle(source, destination), relationshipProvider);
        }

        public ContractRelationshipType GetRelationship(IScriptContract source, IScriptContract destination, Func<ContractRelationshipType> relationshipProvider)
        {
            return GetRelationship(source.RuntimeHandle, destination.RuntimeHandle, relationshipProvider);
        }
    }
#endif

}
