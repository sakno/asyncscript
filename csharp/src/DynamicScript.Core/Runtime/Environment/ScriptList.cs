using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Linq;

namespace DynamicScript.Runtime.Environment
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;
    using InterpretationContext = Compiler.Ast.InterpretationContext;
    using SystemConverter = System.Convert;

    /// <summary>
    /// Represents list of objects that imitates single-dimensional array.
    /// This class cannot be inherited.
    /// </summary>
    [ComVisible(false)]
    [CLSCompliant(false)]
    public sealed class ScriptList: ScriptObject, IList<IScriptObject>, IScriptObjectCollection, IScriptArray
    {
        #region Nested Types

        [ComVisible(false)]
        private sealed class ReadListElementFunction : ScriptGetItemFunction
        {
            private readonly IList<IScriptObject> m_elements;

            public ReadListElementFunction(IScriptArrayContract elementContract, IList<IScriptObject> elements)
                : base(elementContract.ElementContract, new[] { ScriptIntegerContract.Instance })
            {
                m_elements = elements;
            }

            private IScriptObject GetItem(ScriptInteger index)
            {
                switch (index != null && index.IsInt32)
                {
                    case true: return m_elements[SystemConverter.ToInt32(index)];
                    default: throw new ArgumentOutOfRangeException();
                }
            }

            protected override IScriptObject GetItem(IScriptObject[] indicies, InterpreterState state)
            {
                return indicies.LongLength == 1L ? GetItem(indicies[0] as ScriptInteger) : Void;
            }
        }

        [ComVisible(false)]
        private sealed class WriteListElementFunction : ScriptSetItemFunction
        {
            private readonly IList<IScriptObject> m_elements;

            public WriteListElementFunction(IScriptArrayContract elementContract, IList<IScriptObject> elements)
                : base(elementContract.ElementContract, new[] { ScriptIntegerContract.Instance })
            {
                m_elements = elements;
            }

            private bool SetItem(IList<IScriptObject> elements, IScriptObject value, ScriptInteger index)
            {
                switch (index != null && index.IsInt32)
                {
                    case true:
                        m_elements[SystemConverter.ToInt32(index)] = value;
                        return true;
                    default:
                        return false;
                }
            }

            protected override void SetItem(IScriptObject value, IScriptObject[] indicies, InterpreterState state)
            {
                SetItem(m_elements, value, indicies[0] as ScriptInteger);
            }
        }

        #endregion

        private static readonly AggregatedSlotCollection<ScriptList> StaticSlots = new AggregatedSlotCollection<ScriptList>
        {
            {ScriptArrayContract.LengthSlotName, (owner, state) => new ScriptInteger(owner.Count), ScriptIntegerContract.Instance},
            {ScriptArrayContract.RankSlotName, (owner, state) => ScriptInteger.One, ScriptIntegerContract.Instance},
            //Getter
            {GetItemAction, (owner, state) => 
                {
                    if(owner.m_getter == null)owner.m_getter=new ReadListElementFunction(owner.ContractBinding, owner.m_elements);
                    return owner.m_getter;
                }},
            //Setter
            {SetItemAction, (owner, state) =>
                {
                    if (owner.m_setter == null) owner.m_setter = new WriteListElementFunction(owner.ContractBinding, owner.m_elements);
                    return owner.m_setter;
                }},
                {ScriptArrayContract.UpperBoundSlotName, (owner, state) =>
                    {
                        if (owner.m_ubound == null) owner.m_ubound = new ScriptArray.UpperBoundFunction<List<IScriptObject>>(owner.m_elements);
                        return owner.m_ubound;
                    }},
                    {ScriptArrayContract.IteratorAction, (owner, state) =>
                        {
                            if (owner.m_iterator == null) owner.m_iterator = new ScriptIteratorFunction(owner.m_elements, owner.ContractBinding.ElementContract);
                            return owner.m_iterator;
                        }}
        };

        private volatile static int BufferSize = 100;
        private static readonly object SyncRoot = new object();

        private readonly List<IScriptObject> m_elements;
        private ScriptArrayContract m_contract;

        private IScriptFunction m_getter;
        private IScriptFunction m_setter;
        private IScriptFunction m_ubound;
        private IScriptFunction m_iterator;

        private ScriptList(List<IScriptObject> elements)
        {
            if (elements == null) throw new ArgumentNullException("elements");
            m_elements = elements;
        }

        /// <summary>
        /// Initializes a new empty collection with the specified capacity.
        /// </summary>
        /// <param name="capacity">The capacity of the list.</param>
        public ScriptList(int capacity)
            : this(new List<IScriptObject>(capacity))
        {
        }

        /// <summary>
        /// Initializes a new empty collection with optimal capacity.
        /// </summary>
        public ScriptList()
            : this(BufferSize)
        {
        }

        /// <summary>
        /// Initializes a new collection with the predefined elements.
        /// </summary>
        /// <param name="capacity"></param>
        /// <param name="elements"></param>
        public ScriptList(int capacity, IEnumerable<IScriptObject> elements)
            : this(capacity)
        {
            m_elements.AddRange(elements);
        }

        private ScriptArrayContract ContractBinding
        {
            get
            {
                if (m_contract == null) m_contract = new ScriptArrayContract(ScriptArray.InferContract(m_elements));
                return m_contract;
            }
        }

        /// <summary>
        /// Returns contract binding for this list.
        /// </summary>
        /// <returns></returns>
        public override IScriptContract GetContractBinding()
        {
            return ContractBinding;
        }

        /// <summary>
        /// Gets contract for all elements in the array.
        /// </summary>
        public IScriptContract ElementContract
        {
            get { return ContractBinding.ElementContract; }
        }

        /// <summary>
        /// Extracts .NET Framework list from the DynamicScript-compliant list.
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        public static implicit operator List<IScriptObject>(ScriptList list)
        {
            return list != null ? list.m_elements : null;
        }

        int IList<IScriptObject>.IndexOf(IScriptObject item)
        {
            return m_elements.IndexOf(item ?? Void);
        }

        void IList<IScriptObject>.Insert(int index, IScriptObject item)
        {
            m_elements.Insert(index, item ?? Void);
        }

        void IList<IScriptObject>.RemoveAt(int index)
        {
            m_elements.RemoveAt(index);
        }

        /// <summary>
        /// Gets or sets element at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the element to get or set.</param>
        /// <returns>The element of the list.</returns>
        public IScriptObject this[int index]
        {
            get
            {
                return m_elements[index];
            }
            set
            {
                m_elements[index] = value ?? Void;
            }
        }

        /// <summary>
        /// Gets capacity of the list.
        /// </summary>
        public int Capacity
        {
            get { return m_elements.Capacity; }
        }

        /// <summary>
        /// Adds elements to the list.
        /// </summary>
        /// <param name="items">The collection of items to add.</param>
        /// <param name="state">Specifies that the void result should be omitted.</param>
        public void RtlAdd(IEnumerable<IScriptObject> items, InterpreterState state)
        {
            foreach (var i in items ?? Enumerable.Empty<IScriptObject>()) 
                RtlAdd(i, state);
        }

        /// <summary>
        /// Adds a new element to the list.
        /// </summary>
        /// <param name="item">The item to add.</param>
        /// <param name="state">Internal interpreter state.</param>
        public bool RtlAdd(IScriptObject item, InterpreterState state)
        {
            if (item is IRuntimeSlot)
                return RtlAdd(((IRuntimeSlot)item).GetValue(state), state);
            else if (state.Behavior.OmitVoidYieldInLoops && IsVoid(item)) return false;
            else
            {
                Add(item);
                return true;
            }
        }

        /// <summary>
        /// Adds a new element to the list.
        /// </summary>
        /// <param name="item"></param>
        public void Add(IScriptObject item)
        {
            m_contract = null;
            m_elements.Add(item);
        }

        /// <summary>
        /// Removes all items from collection.
        /// </summary>
        public new void Clear()
        {
            m_elements.Clear();
        }

        private bool Contains(IScriptObject item, bool byRef)
        {
            return byRef ? m_elements.FirstOrDefault(other => ReferenceEquals(item, other)) != null : m_elements.Contains(item ?? Void);
        }

        bool IScriptContainer.Contains(IScriptObject item, bool byRef, InterpreterState state)
        {
            return Contains(item, byRef);
        }

        bool ICollection<IScriptObject>.Contains(IScriptObject obj)
        {
            return Contains(obj, false);
        }

        void ICollection<IScriptObject>.CopyTo(IScriptObject[] array, int arrayIndex)
        {
            m_elements.CopyTo(array, arrayIndex);
        }

        /// <summary>
        /// Gets count of elements in the list.
        /// </summary>
        public int Count
        {
            get { return m_elements.Count; }
        }

        bool ICollection<IScriptObject>.IsReadOnly
        {
            get { return false; }
        }

        /// <summary>
        /// Removes the specified element from the list.
        /// </summary>
        /// <param name="item">The item to remove.</param>
        /// <returns><see langword="true"/> if item is removed successfully; otherwise, <see langword="false"/>.</returns>
        public bool Remove(IScriptObject item)
        {
            return m_elements.Remove(item ?? Void);
        }

        /// <summary>
        /// Returns an enumerator through list elements.
        /// </summary>
        /// <returns>An enumerator through list elements.</returns>
        public IEnumerator<IScriptObject> GetEnumerator()
        {
            return m_elements.GetEnumerator();
        }

        IScriptArray IScriptArray.ToSingleDimensional()
        {
            return this;
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        internal static Expression New()
        {
            return LinqHelpers.BodyOf<Func<ScriptList>, NewExpression>(() => new ScriptList());
        }

        internal static Expression Add(ParameterExpression list, IEnumerable<Expression> items, ParameterExpression stateVar)
        {
            var addMethod = LinqHelpers.BodyOf<Action<ScriptList, IEnumerable<IScriptObject>, InterpreterState>, MethodCallExpression>((l, i, s) => l.RtlAdd(i, s));
            return addMethod.Update(list, new Expression[] { Expression.NewArrayInit(typeof(IScriptObject), items), stateVar });
        }

        internal static Expression Add(ParameterExpression list, Expression item, ParameterExpression stateVar)
        {
            return Add(list, new[] { item }, stateVar);
        }

        /// <summary>
        /// Concatenates a two arrays.
        /// </summary>
        /// <param name="right"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        protected override IScriptObject Add(IScriptObject right, InterpreterState state)
        {
            if (right is IScriptArray)
                return ScriptArray.Concat(this, (IScriptArray)right, state);
            else if (state.Context == InterpretationContext.Unchecked)
                return Void;
            else throw new UnsupportedOperationException(state);
        }

        /// <summary>
        /// Returns a string representation of the list.
        /// </summary>
        /// <returns>The string representation of the list.</returns>
        public override string ToString()
        {
            return ScriptArray.ToString(this);
        }

        #region IScriptArray Members

        IScriptObject IScriptArray.this[long[] indicies, InterpreterState state]
        {
            get
            {
                if (indicies == null) indicies = new long[0];
                return indicies.LongLength == 1L ? this[(int)indicies[0]] : null;
            }
            set
            {
                if (indicies == null) indicies = new long[0];
                if (indicies.LongLength == 1L)
                    this[(int)indicies[0]] = value;
            }
        }

        long IScriptArray.GetLength(int dimension)
        {
            return dimension == 0 ? Count : 0;
        }

        IScriptArrayContract IScriptArray.GetContractBinding()
        {
            return ContractBinding;
        }

        #endregion

        /// <summary>
        /// 
        /// </summary>
        /// <param name="slotName"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        public override IScriptObject this[string slotName, InterpreterState state]
        {
            get { return StaticSlots.GetValue(this, slotName, state); }
            set { StaticSlots.SetValue(this, slotName, value, state); }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="slotName"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        protected override IScriptObject GetSlotMetadata(string slotName, InterpreterState state)
        {
            return StaticSlots.GetSlotMetadata(this, slotName, state);
        }

        /// <summary>
        /// 
        /// </summary>
        public override ICollection<string> Slots
        {
            get { return StaticSlots.Keys; }
        }
    }
}
