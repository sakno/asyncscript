using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace DynamicScript.Runtime.Environment
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;

    /// <summary>
    /// Represents an abstract class for indexer action.
    /// </summary>
    [ComVisible(false)]
    
    public abstract class ScriptIndexerActionContract: ScriptActionContract
    {
        internal ScriptIndexerActionContract(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        internal ScriptIndexerActionContract(IEnumerable<Parameter> parameters, IScriptContract returnValue = null)
            : base(parameters, returnValue)
        {
        }

        /// <summary>
        /// Gets indicies of the indexer.
        /// </summary>
        public abstract Parameter[] Indicies
        {
            get;
        }
    }
}
