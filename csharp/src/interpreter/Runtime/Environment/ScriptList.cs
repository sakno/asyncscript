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
    public sealed class ScriptList: ScriptObject, IList<IScriptObject>, IScriptObjectCollection, IArraySlots
    {
        private static readonly ScriptArrayContract ContractBinding = new ScriptArrayContract();

        #region Nested Types

        [ComVisible(false)]
        private sealed class ReadListElementAction : ScriptGetItemAction
        {
            private readonly IList<IScriptObject> m_elements;

            public ReadListElementAction(IScriptContract elementContract, IList<IScriptObject> elements)
                : base(elementContract, new[] { ScriptIntegerContract.Instance })
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

            protected override IScriptObject GetItem(InvocationContext ctx, IScriptObject[] indicies)
            {
                return indicies.LongLength == 1L ? GetItem(indicies[0] as ScriptInteger) : Void;
            }
        }

        [ComVisible(false)]
        private sealed class WriteListElementAction : ScriptSetItemAction
        {
            private readonly IList<IScriptObject> m_elements;

            public WriteListElementAction(IScriptContract elementContract, IList<IScriptObject> elements)
                : base(elementContract, new[] { ScriptIntegerContract.Instance })
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

            protected override void SetItem(InvocationContext ctx, IScriptObject value, IScriptObject[] indicies)
            {
                SetItem(m_elements, value, indicies[0] as ScriptInteger);
            }
        }

        [ComVisible(false)]
        private sealed class LengthSlot : RuntimeSlotBase, IEquatable<LengthSlot>
        {
            private readonly ICollection<IScriptObject> m_elements;

            public LengthSlot(ICollection<IScriptObject> elements)
            {
                m_elements = elements;
            }

            public ScriptInteger Value
            {
                get { return new ScriptInteger(m_elements.Count); } 
            }

            public override IScriptObject GetValue(InterpreterState state)
            {
                return Value;
            }

            public override void SetValue(IScriptObject value, InterpreterState state)
            {
                throw new ConstantCannotBeChangedException(state);
            }

            public override IScriptContract ContractBinding
            {
                get { return ScriptIntegerContract.Instance; }
            }

            protected override IScriptContract GetValueContract()
            {
                return ContractBinding;
            }

            public override RuntimeSlotAttributes Attributes
            {
                get { return RuntimeSlotAttributes.Immutable; }
            }

            protected override ICollection<string> Slots
            {
                get { return Value.Slots; }
            }

            public override bool DeleteValue()
            {
                return false;
            }

            public bool Equals(LengthSlot other)
            {
                return other != null && ReferenceEquals(m_elements, other.m_elements);
            }

            public override bool Equals(IRuntimeSlot other)
            {
                return Equals(other as LengthSlot);
            }
        }

        #endregion

        private volatile static int BufferSize = 100;
        private static readonly object SyncRoot = new object();

        private readonly List<IScriptObject> m_elements;

        private IRuntimeSlot m_length;
        private IRuntimeSlot m_rank;
        private IRuntimeSlot m_getter;
        private IRuntimeSlot m_setter;
        private IRuntimeSlot m_ubound;
        private IRuntimeSlot m_iterator;

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
        public void Add(IEnumerable<IScriptObject> items)
        {
            m_elements.AddRange(items ?? Enumerable.Empty<IScriptObject>());
            lock (SyncRoot) if (Capacity > BufferSize) BufferSize = Capacity;
        }

        /// <summary>
        /// Adds a new element to the list.
        /// </summary>
        /// <param name="item">The item to add.</param>
        public void Add(IScriptObject item)
        {
            Add(new[] { item });
        }

        /// <summary>
        /// Removes all items from collection.
        /// </summary>
        public void Clear()
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
        public new IEnumerator<IScriptObject> GetEnumerator()
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

        internal static Expression BindNew()
        {
            return LinqHelpers.BodyOf<Func<ScriptList>, NewExpression>(() => new ScriptList());
        }

        internal static Expression BindAdd(ParameterExpression list, IEnumerable<Expression> items)
        {
            var addMethod = LinqHelpers.BodyOf<Action<ScriptList, IEnumerable<IScriptObject>>, MethodCallExpression>((l, i) => l.Add(i));
            return addMethod.Update(list, new[] { Expression.NewArrayInit(typeof(IScriptObject), items) });
        }

        internal static Expression BindAdd(ParameterExpression list, Expression item)
        {
            return BindAdd(list, new[] { item });
        }

        /// <summary>
        /// Returns a string representation of the list.
        /// </summary>
        /// <returns>The string representation of the list.</returns>
        public override string ToString()
        {
            return ScriptArray.ToString(this);
        }

        #region Runtime Slots

        private IRuntimeSlot CreateLengthSlot()
        {
            return new LengthSlot(m_elements);
        }

        private ReadListElementAction CreateGetterSlot()
        {
            return new ReadListElementAction(ContractBinding.ElementContract, m_elements);
        }

        private WriteListElementAction CreateSetterSlot()
        {
            return new WriteListElementAction(ContractBinding.ElementContract, m_elements);
        }

        private ScriptArray.UpperBoundAction<List<IScriptObject>> CreateUpperBoundSlot()
        {
            return new ScriptArray.UpperBoundAction<List<IScriptObject>>(m_elements);
        }

        private ScriptIteratorAction CreateIteratorSlot()
        {
            return new ScriptIteratorAction(m_elements, ContractBinding.ElementContract);
        }

        IRuntimeSlot IArraySlots.Length
        {
            get { return Cache(ref m_length, CreateLengthSlot); }
        }

        IRuntimeSlot IArraySlots.Rank
        {
            get { return CacheConst(ref m_rank, () => new ScriptInteger(ContractBinding.Rank)); }
        }

        IRuntimeSlot IArraySlots.GetItem
        {
            get { return CacheConst(ref m_getter, CreateGetterSlot); }
        }

        IRuntimeSlot IArraySlots.SetItem
        {
            get { return CacheConst(ref m_setter, CreateSetterSlot); }
        }

        IRuntimeSlot IArraySlots.UpperBound
        {
            get { return CacheConst(ref m_ubound, CreateUpperBoundSlot); }
        }

        IRuntimeSlot IIterableSlots.Iterator
        {
            get { return CacheConst(ref m_iterator, CreateIteratorSlot); }
        }
        #endregion

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
    }
}
