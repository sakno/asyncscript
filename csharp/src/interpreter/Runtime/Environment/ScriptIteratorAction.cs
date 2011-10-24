using System;

namespace DynamicScript.Runtime.Environment
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;
    using IEnumerable = System.Collections.IEnumerable;

    /// <summary>
    /// Represents action that returns iterator through DynamicScript objects.
    /// This class cannot be inherited.
    /// </summary>
    [ComVisible(false)]
    public sealed class ScriptIteratorAction: ScriptFunc
    {
        #region Nested Types

        [ComVisible(false)]
        private sealed class IteratorActionContract : ScriptActionContract
        {
            public IteratorActionContract(IScriptContract elementContract)
                : base(EmptyParameters, ScriptIterator.GetContractBinding(elementContract))
            {
            }
        }
        #endregion

        private readonly IEnumerable m_enumerable;
        private readonly IScriptContract m_elementContract;

        /// <summary>
        /// Initializes a new script object that produces an iterator.
        /// </summary>
        /// <param name="collection"></param>
        /// <param name="elementContract"></param>
        /// <param name="this"></param>
        public ScriptIteratorAction(IEnumerable collection, IScriptContract elementContract, IScriptObject @this = null)
            : base(GetContractBinding(elementContract).ReturnValueContract, @this)
        {
            if (collection == null) throw new ArgumentNullException("collection");
            m_enumerable = collection;
            m_elementContract = elementContract;
        }

        /// <summary>
        /// Returns contract of the action.
        /// </summary>
        /// <param name="elementContract"></param>
        /// <returns></returns>
        public static ScriptActionContract GetContractBinding(IScriptContract elementContract)
        {
            return new IteratorActionContract(elementContract);
        }

        /// <summary>
        /// Determines whether the specified object is iterable.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static bool IsIterable(IScriptObject obj)
        {
            return obj != null && obj.Slots.Contains(IteratorAction);
        }

        /// <summary>
        /// Creates a new script iterator.
        /// </summary>
        /// <param name="state"></param>
        /// <returns></returns>
        protected override IScriptObject Invoke(InterpreterState state)
        {
            return new ScriptIterator(m_enumerable, m_elementContract);
        }
    }
}
