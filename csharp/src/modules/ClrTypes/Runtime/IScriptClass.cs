using System;

namespace DynamicScript.Runtime
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;

    /// <summary>
    /// Represents the script wrapper of the native .NET type.
    /// </summary>
    [ComVisible(false)]
    public interface IScriptClass: IScriptContract
    {
        /// <summary>
        /// Gets underlying type.
        /// </summary>
        Type NativeType { get; }

        /// <summary>
        /// Returns relationship with the specified class.
        /// </summary>
        /// <param name="class"></param>
        /// <returns></returns>
        ContractRelationshipType GetRelationship(IScriptClass @class);
    }
}
