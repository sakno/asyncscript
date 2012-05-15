using System;


namespace DynamicScript.Runtime
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;

    /// <summary>
    /// Represents dynamic information about DynamicScript object interface.
    /// </summary>
    [ComVisible(false)]
    public interface IScriptContract : IScriptObject, IEquatable<IScriptContract>
    {
        /// <summary>
        /// Transforms the specified object according with this contract.
        /// </summary>
        /// <param name="conv">Conversion type.</param>
        /// <param name="value">The value to be converted.</param>
        /// <param name="state">Interpretation state.</param>
        /// <returns>Conversion result.</returns>
        /// <remarks>This method implements explicit conversion.</remarks>
        IScriptObject Convert(Conversion conv, IScriptObject value, InterpreterState state);

        /// <summary>
        /// Creates an object that represents void value according with the contract.
        /// </summary>
        /// <returns>The object that represents void value according with the contract.</returns>
        IScriptObject FromVoid(InterpreterState state);

        /// <summary>
        /// Returns relationship between the current contract and the specified.
        /// </summary>
        /// <param name="contract">The contract to compare. Cannot be <see langword="null"/>.</param>
        /// <returns>Relationship between the current contract and <paramref name="contract"/>.</returns>
        ContractRelationshipType GetRelationship(IScriptContract contract);

        /// <summary>
        /// Obtains type identifier.
        /// </summary>
        /// <returns></returns>
        ScriptTypeCode GetTypeCode();

#if USE_REL_MATRIX
        /// <summary>
        /// Gets a value that uniquely identifies this contract.
        /// </summary>
        ContractHandle RuntimeHandle { get; }
#endif
    }
}
