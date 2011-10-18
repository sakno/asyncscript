using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace DynamicScript.Runtime.Debugging
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;
    using Resources = Properties.Resources;

    /// <summary>
    /// Represents call stack frame.
    /// This class cannot be inherited.
    /// </summary>
    [ComVisible(false)]
    public sealed class CallStackFrame: IEnumerable<KeyValuePair<string, RuntimeSlotWatcher>>
    {
        /// <summary>
        /// Represents an action associated with this call stack frame.
        /// </summary>
        public readonly IScriptAction Action;
        private readonly IDictionary<string, RuntimeSlotWatcher> m_watchers;
        private string m_id;

        private CallStackFrame(IScriptObject global, InterpreterState state = null)
        {
            if (state == null) state = InterpreterState.Initial;
            m_watchers = new Dictionary<string, RuntimeSlotWatcher>(10, new StringEqualityComparer());
            foreach (var s in global.Slots)
                m_watchers[s] = new RuntimeSlotWatcher(global[s, state]);
        }

        internal CallStackFrame(IScriptAction action, InterpreterState state)
            : this(state.Global, state)
        {
            Action = action;
        }

        internal static CallStackFrame CreateEntryPoint(IScriptObject global)
        {
            return new CallStackFrame(global);
        }

        /// <summary>
        /// Gets watcher for the specified variable or constant.
        /// </summary>
        /// <param name="storeName">The name of the variable or constant.</param>
        /// <returns>Variable or constant value watcher; or <see langword="null"/> if variable or constant
        /// with the specified name is not visible at the current stack frame.</returns>
        public RuntimeSlotWatcher this[string storeName]
        {
            get
            {
                var watcher = default(RuntimeSlotWatcher);
                return m_watchers.TryGetValue(storeName, out watcher) ? watcher : null;
            }
        }

        /// <summary>
        /// Registers a new named storage in the current stack frame.
        /// </summary>
        /// <param name="storageName">The name of the storage. Cannot be <see langword="null"/> or empty.</param>
        /// <param name="storage">The storage to watch. Cannot be <see langword="null"/>.</param>
        /// <exception cref="System.ArgumentNullException"><paramref name="storageName"/> is <see langword="null"/> or empty; or <paramref name="storage"/> is <see langword="null"/>.</exception>
        [MethodImpl(MethodImplOptions.Synchronized)]
        internal void RegisterStorage(string storageName, IRuntimeSlot storage)
        {
            if (string.IsNullOrEmpty(storageName)) throw new ArgumentNullException("storageName");
            if (storage == null) throw new ArgumentNullException("storage");
            m_watchers[storageName] = new RuntimeSlotWatcher(storage);
        }

        /// <summary>
        /// Gets collection of variables and constants visible at the current stack frame.
        /// </summary>
        public ICollection<string> Storages
        {
            get { return m_watchers.Keys; }
        }

        /// <summary>
        /// Gets or sets human-readable identifier of the call stack frame.
        /// </summary>
        public string ID
        {
            get 
            {
                switch (EntryPoint)
                {
                    case true: return Resources.EntryPointFrameID;
                    default: return string.IsNullOrEmpty(m_id) ? Action.ToString() : m_id;
                }
            }
            set
            {
                if (string.IsNullOrEmpty(m_id)) m_id = value;
            }
        }

        /// <summary>
        /// Gets a value indicating that the current frame describes root-level action.
        /// </summary>
        public bool EntryPoint
        {
            get { return Action == null; }
        }

        /// <summary>
        /// Returns a human-readable representation of the call stack frame.
        /// </summary>
        /// <returns>A human-readable representation of the call stack frame.</returns>
        public override string ToString()
        {
            return ID;
        }

        /// <summary>
        /// Returns an enumerator through all slot watchers.
        /// </summary>
        /// <returns></returns>
        public IEnumerator<KeyValuePair<string, RuntimeSlotWatcher>> GetEnumerator()
        {
            return m_watchers.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
