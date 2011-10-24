using System;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace DynamicScript.Runtime.Environment
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;

    /// <summary>
    /// Represents item getter.
    /// This class cannot be inherited.
    /// </summary>
    [ComVisible(false)]
    public abstract class ScriptGetItemAction: ScriptActionBase, IIndexerAccessor
    {
        #region Nested Types

        /// <summary>
        /// Represents item getter contract.
        /// This class cannot be inherited.
        /// </summary>
        [ComVisible(false)]
        [Serializable]
        private sealed class ActionContract : ScriptIndexerActionContract
        {
            private ActionContract(SerializationInfo info, StreamingContext context)
                : base(info, context)
            {
            }

            /// <summary>
            /// Initializes a new item setter contract.
            /// </summary>
            /// <param name="valueContract">The contract of the value to set.</param>
            /// <param name="indicies">The contracts of the indicies.</param>
            public ActionContract(IScriptContract valueContract, params IScriptContract[] indicies)
                : base(CreateSignature(valueContract, indicies), valueContract)
            {
            }

            private static IEnumerable<Parameter> CreateSignature(IScriptContract valueContract, IScriptContract[] indicies)
            {
                if (valueContract == null) throw new ArgumentNullException("valueContract");
                if (indicies == null) indicies = new IScriptContract[0];
                const string IndexParameterPrefix = "index{0}";
                var ics = new Parameter[indicies.Length];
                for (var i = 0; i < indicies.Length; i++)
                    ics[i] = new Parameter(String.Format(IndexParameterPrefix, i), indicies[i]);
                return ics;
            }

            /// <summary>
            /// Gets an array of the parameters that describes indicies.
            /// </summary>
            public override Parameter[] Indicies
            {
                get
                {
                    return Parameters.ToArray();
                }
            }
        }
        #endregion

        /// <summary>
        /// Initializes a new item getter.
        /// </summary>
        /// <param name="contract">The contract of the item getter.</param>
        /// <param name="this">The item getter owner.</param>
        private ScriptGetItemAction(ActionContract contract, IScriptObject @this = null)
            : base(contract, @this)
        {
        }

        /// <summary>
        /// Initializes a new item getter.
        /// </summary>
        /// <param name="valueContract">The contract of the item.</param>
        /// <param name="indicies">The indicies of the item getter.</param>
        /// <param name="this">The item getter owner.</param>
        public ScriptGetItemAction(IScriptContract valueContract, IScriptContract[] indicies, IScriptObject @this = null)
            : this(new ActionContract(valueContract, indicies), @this)
        {
        }

        /// <summary>
        /// Creates a new item setter contract.
        /// </summary>
        /// <param name="valueContract">The contract of the value to set.</param>
        /// <param name="indicies">The contracts of the indicies.</param>
        public static ScriptActionContract GetContractBinding(IScriptContract valueContract, params IScriptContract[] indicies)
        {
            return new ActionContract(valueContract, indicies);
        }

        /// <summary>
        /// Gets contract of the returning item.
        /// </summary>
        public IScriptContract ItemContract
        {
            get { return ReturnValueContract; }
        }

        /// <summary>
        /// Gets indicies.
        /// </summary>
        public IScriptContract[] Indicies
        {
            get { return Parameters.Select(p => p.ContractBinding).ToArray(); }
        }

        /// <summary>
        /// Returns an item identified by the specified indicies.
        /// </summary>
        /// <param name="indicies"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        protected abstract IScriptObject GetItem(IScriptObject[] indicies, InterpreterState state);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="args"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        protected sealed override IScriptObject InvokeCore(IList<IScriptObject> args, InterpreterState state)
        {
            var indicies = new IScriptObject[args.Count];
            args.CopyTo(0, indicies, 0, args.Count);
            return GetItem(indicies, state);
        }
    }

    [ComVisible(false)]
    internal abstract class ScriptGetItemAction<TOwner> : ScriptGetItemAction
        where TOwner : class, IScriptObject
    {
        /// <summary>
        /// Initializes a new item getter.
        /// </summary>
        /// <param name="valueContract">The contract of the item.</param>
        /// <param name="indicies">The indicies of the item getter.</param>
        /// <param name="this">The item getter owner.</param>
        public ScriptGetItemAction(IScriptContract valueContract, IScriptContract[] indicies, TOwner @this)
            : base(valueContract, indicies, @this)
        {
        }

        public new TOwner This
        {
            get { return (TOwner)base.This; }
        }
    }
}
