using System;
using System.Collections.Generic;

namespace DynamicScript.Runtime
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;

    /// <summary>
    /// Represents script composite object.
    /// </summary>
    [ComVisible(false)]
    public interface IScriptCompositeObject: IScriptObject
    {
        /// <summary>
        /// Gets slot by its name.
        /// </summary>
        /// <param name="slotName">The name of the slot.</param>
        /// <returns>An object that represents access to slot; or <see langword="null"/> if slot is not existed.</returns>
        IStaticRuntimeSlot this[string slotName]
        {
            get;
        }

        /// <summary>
        /// Returns a collection of slot values.
        /// </summary>
        /// <param name="state">Internal interpreter state.</param>
        /// <returns>A collection of slot values.</returns>
        IEnumerable<KeyValuePair<string, IScriptObject>> GetSlotValues(InterpreterState state);

        /// <summary>
        /// Returns composite object in which each slot is read-only.
        /// </summary>
        /// <param name="state">Internal interpreter state.</param>
        /// <returns>A new instance of composite object.</returns>
        IScriptCompositeObject AsReadOnly(InterpreterState state);

        /// <summary>
        /// Copies slots from the specified object into this object.
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="state">Internal interpreter state.</param>
        void Import(IScriptObject obj, InterpreterState state);
    }
}
