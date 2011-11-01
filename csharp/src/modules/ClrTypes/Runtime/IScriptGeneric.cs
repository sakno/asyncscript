using System;
using System.Collections.Generic;

namespace DynamicScript.Runtime
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;

    /// <summary>
    /// Represents generic definition.
    /// </summary>
    [ComVisible(false)]
    public interface IScriptGeneric: IScriptContract
    {
        /// <summary>
        /// Gets base type constraint.
        /// </summary>
        IScriptClass BaseType { get; }

        /// <summary>
        /// Gets a collection of interfaces that should be implemented
        /// by the type.
        /// </summary>
        IScriptClass[] Interfaces { get; }

        /// <summary>
        /// Gets a value indicating whether the target type
        /// should have the default constructor.
        /// </summary>
        bool DefaultConstructor { get; }

        /// <summary>
        /// Returns relationship with other generic description.
        /// </summary>
        /// <param name="generic"></param>
        /// <returns></returns>
        ContractRelationshipType GetRelationship(IScriptGeneric generic);
    }
}
