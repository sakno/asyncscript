using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Dynamic;
using System.Linq;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace DynamicScript.Runtime.Environment
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;
    using LinqExpression = System.Linq.Expressions.Expression;
    using InterpretationContext = Compiler.Ast.InterpretationContext;

    /// <summary>
    /// Represents complex object with slots.
    /// </summary>
    [ComVisible(false)]
    [Serializable]
    [TypeConverter(typeof(CompositeObjectConverter))]
    public class ScriptCompositeObject: ScriptObject, ISerializable, IScriptCompositeObject, IScriptSetFactory
    {
        #region Nested Types

        /// <summary>
        /// Represents an observable runtime slot that supports notification
        /// </summary>
        [ComVisible(false)]
        [Serializable]
        protected class ObservableSlot : RuntimeSlot
        {
            private ObservableSlot(SerializationInfo info, StreamingContext context)
                : base(info, context)
            {
            }

            private ObservableSlot(RuntimeSlot slot)
                : base(slot.ContractBinding)
            {
                if (slot == null) throw new ArgumentNullException("slot");
                Value = slot;
            }

            /// <summary>
            /// Initializes a new observable slot.
            /// </summary>
            /// <param name="contract">The contract binding for the slot.</param>
            public ObservableSlot(ScriptContract contract = null)
                : this(new ScriptVariable(contract))
            {

            }

            /// <summary>
            /// Initializes a new observable slot.
            /// </summary>
            /// <param name="value">The initial value written to the slot.</param>
            /// <param name="isConst">Specifies that the object stored in the slot cannot be changed.</param>
            public ObservableSlot(ScriptObject value, bool isConst = false)
                : this(isConst ? (RuntimeSlot)new ScriptConstant(value) : new ScriptVariable(value))
            {
            }

            /// <summary>
            /// Initializes a new observable slot.
            /// </summary>
            /// <param name="value">The initial value written to the slot.</param>
            /// <param name="contract">The contract binding for the slot.</param>
            /// <param name="isConst">Specifies that the object stored in the slot cannot be changed.</param>
            public ObservableSlot(ScriptObject value, ScriptContract contract, bool isConst = false)
                : this(isConst ? (RuntimeSlot)new ScriptConstant(value, contract) : new ScriptVariable(value, contract))
            {
            }

            private new RuntimeSlot Value
            {
                get { return (RuntimeSlot)base.Value; }
                set { base.Value = value; }
            }

            /// <summary>
            /// Gets a value indicating that the current slot is immutable.
            /// </summary>
            protected internal override bool IsConstant
            {
                get { return Value.IsConstant; }
            }

            /// <summary>
            /// Restores an object from the slot.
            /// </summary>
            /// <param name="state">Internal interpreter state.</param>
            /// <returns>An object restored from the slot.</returns>
            public sealed override IScriptObject GetValue(InterpreterState state)
            {
                return GetValue(Value.GetValue(state), state);
            }

            /// <summary>
            /// Provides processing of the value to be obtained from the slot.
            /// </summary>
            /// <param name="value">The value to be processed.</param>
            /// <param name="state">Internal interpreter state.</param>
            /// <returns>The preprocessed value.</returns>
            protected virtual IScriptObject GetValue(IScriptObject value, InterpreterState state)
            {
                return value;
            }

            /// <summary>
            /// Stores an object into the slot.
            /// </summary>
            /// <param name="value">The value to be stored.</param>
            /// <param name="state">Internal interpreter state.</param>
            public sealed override void SetValue(IScriptObject value, InterpreterState state)
            {
                SetValue(ref value, state);
                Value.SetValue(value, state);
            }

            /// <summary>
            /// Provides processing of the value to be stored to the slot.
            /// </summary>
            /// <param name="value">The value to set.</param>
            /// <param name="state">Internal interpreter state.</param>
            [CLSCompliant(false)]
            protected virtual void SetValue(ref IScriptObject value, InterpreterState state)
            {
            }

            /// <summary>
            /// Gets a value indicating that the slot contains value.
            /// </summary>
            public sealed override bool HasValue
            {
                get
                {
                    return Value.HasValue;
                }
                protected set
                {
                    throw new NotSupportedException();
                }
            }
        }

        /// <summary>
        /// Represents collection of object slots.
        /// </summary>
        [ComVisible(false)]
        [Serializable]
        public class ObjectSlotCollection : ICollection<KeyValuePair<string, IRuntimeSlot>>, IEquatable<ObjectSlotCollection>
        {
            private readonly IDictionary<string, IRuntimeSlot> m_slots;

            /// <summary>
            /// Initializes a new empty collection.
            /// </summary>
            public ObjectSlotCollection()
            {
                const int DefaultCapacity = 10;
                m_slots = new Dictionary<string, IRuntimeSlot>(DefaultCapacity, new StringEqualityComparer());
            }

            /// <summary>
            /// Initializes a new collection with predefined set of slots.
            /// </summary>
            /// <param name="slots">The collection with object slots.</param>
            public ObjectSlotCollection(IEnumerable<KeyValuePair<string, IRuntimeSlot>> slots)
                : this()
            {
                foreach (var s in slots ?? new KeyValuePair<string, IRuntimeSlot>[0])
                    Add(s);
            }

            /// <summary>
            /// Gets collection of slot names.
            /// </summary>
            public ICollection<string> SlotNames
            {
                get { return m_slots.Keys; }
            }

            internal static MethodCallExpression Add(Expression collection, KeyValuePair<string, Expression> slot)
            {
                var call = LinqHelpers.BodyOf<Action<ObjectSlotCollection, string, IRuntimeSlot>, MethodCallExpression>((slots, name, s) => slots.Add(name, s));
                return call.Update(collection, new[] { LinqHelpers.Constant(slot.Key), slot.Value });
            }

            private IEnumerable<KeyValuePair<string, ScriptCompositeContract.SlotMeta>> GetSlotSurrogates()
            {
                return m_slots.Select(t => new KeyValuePair<string, ScriptCompositeContract.SlotMeta>(t.Key, new ScriptCompositeContract.SlotMeta(t.Value.GetContractBinding(), (t.Value.Attributes & RuntimeSlotAttributes.Immutable) != 0)));
            }

            private void Add<T>(DynamicScriptProxy<T> proxy, Func<string, IScriptAction, bool> invoker)
                where T : class
            {
                if (proxy == null) throw new ArgumentNullException("proxy");
                if (invoker == null) throw new ArgumentNullException("invoker");
                foreach (var pair in proxy) invoker.Invoke(pair.Key, pair.Value);
            }

            internal bool Contains(IScriptObject value, bool byRef, InterpreterState state)
            {
                foreach (var slot in m_slots.Values)
                    if (byRef ? ReferenceEquals(value, slot.GetValue(state)) : Equals(value, slot.GetValue(state)))
                        return true;
                return false;
            }

            /// <summary>
            /// Imports constants from the specified proxy.
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <param name="proxy"></param>
            public void AddConstants<T>(DynamicScriptProxy<T> proxy)
                where T:class
            {
                Add<T>(proxy, AddConstant);
            }

            /// <summary>
            /// Imports variables from the specified proxy.
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <param name="proxy"></param>
            public void AddVariables<T>(DynamicScriptProxy<T> proxy)
                where T : class
            {
                Add<T>(proxy, AddVariable);
            }

            /// <summary>
            /// Adds a new constant slot.
            /// </summary>
            /// <param name="slotName">The name of the slot.</param>
            /// <param name="value">The value of the constant.</param>
            /// <returns><see langword="true"/> if slot is registered successfully; otherwise, <see langword="false"/>.</returns>
            public bool AddConstant(string slotName, IScriptObject value)
            {
                return Add(slotName, new ScriptConstant(value));
            }

            /// <summary>
            /// Adds a new constant slot.
            /// </summary>
            /// <typeparam name="TScriptObject"></typeparam>
            /// <param name="slotName"></param>
            /// <returns></returns>
            public bool AddConstant<TScriptObject>(string slotName)
                where TScriptObject : IScriptObject, new()
            {
                return AddConstant(slotName, new TScriptObject());
            }

            /// <summary>
            /// Adds a new constant slot.
            /// </summary>
            /// <param name="slotName">The name of the slot.</param>
            /// <param name="value">The value of the constant.</param>
            /// <param name="contract">The contract binding for the constant.</param>
            /// <returns><see langword="true"/> if slot is registered successfully; otherwise, <see langword="false"/>.</returns>
            public bool AddConstant(string slotName, IScriptObject value, IScriptContract contract)
            {
                return Add(slotName, new ScriptConstant(value, contract));
            }

            /// <summary>
            /// Adds a new variable slot.
            /// </summary>
            /// <param name="slotName">The name of the slot.</param>
            /// <param name="value">The initial value of the variable.</param>
            /// <returns><see langword="true"/> if slot is registered successfully; otherwise, <see langword="false"/>.</returns>
            public bool AddVariable(string slotName, IScriptObject value)
            {
                return Add(slotName, new ScriptVariable(value));
            }

            /// <summary>
            /// Adds a new variable slot.
            /// </summary>
            /// <param name="slotName">The name of the slot.</param>
            /// <param name="contract">The contract binding for the slot.</param>
            /// <returns><see langword="true"/> if slot is registered successfully; otherwise, <see langword="false"/>.</returns>
            public bool AddVariable(string slotName, IScriptContract contract)
            {
                return Add(slotName, new ScriptVariable(contract));
            }

            /// <summary>
            /// Adds a new variable slot.
            /// </summary>
            /// <param name="slotName">The name of the slot.</param>
            /// <param name="value">The initial value of the variable.</param>
            /// <param name="contract">The contract binding for the slot.</param>
            /// <returns><see langword="true"/> if slot is registered successfully; otherwise, <see langword="false"/>.</returns>
            public bool AddVariable(string slotName, IScriptObject value, IScriptContract contract)
            {
                return Add(slotName, new ScriptVariable(value, contract));
            }

            /// <summary>
            /// Adds a new slot to the collection.
            /// </summary>
            /// <param name="slotName">The name of the slot.</param>
            /// <param name="slot">DynamicScript object holder.</param>
            /// <returns><see langword="true"/> if slot is registered successfully; <see langword="false"/> 
            /// if <paramref name="slotName"/> is <see langword="null"/> or empty, or <paramref name="slot"/> is <see langword="null"/>
            /// or slot with the specified name is already registered.</returns>
            public bool Add(string slotName, IRuntimeSlot slot)
            {
                if (string.IsNullOrEmpty(slotName) || slot == null || m_slots.ContainsKey(slotName)) return false;
                m_slots.Add(slotName, slot);
                return true;
            }

            /// <summary>
            /// Adds a new slot to the collection.
            /// </summary>
            /// <typeparam name="TRuntimeSlot"></typeparam>
            /// <param name="slotName"></param>
            /// <returns></returns>
            public bool Add<TRuntimeSlot>(string slotName)
                where TRuntimeSlot : IRuntimeSlot, new()
            {
                return Add(slotName, new TRuntimeSlot());
            }

            /// <summary>
            /// Gets DynamicScript object holder using slot name.
            /// </summary>
            /// <param name="slotName">The slot name.</param>
            /// <returns>DynamicScript object holder; or <see langword="null"/> is slot with the specified name is not registered.</returns>
            public IRuntimeSlot this[string slotName]
            {
                get
                {
                    var slot = default(IRuntimeSlot);
                    return m_slots.TryGetValue(slotName, out slot) && slot != null ? slot : null;
                }
            }

            /// <summary>
            /// Adds a new named slot to this collection.
            /// </summary>
            /// <param name="namedSlot">A named slot to be added.</param>
            public void Add(KeyValuePair<string, IRuntimeSlot> namedSlot)
            {
                Add(namedSlot.Key, namedSlot.Value);
            }

            void ICollection<KeyValuePair<string, IRuntimeSlot>>.Add(KeyValuePair<string, IRuntimeSlot> item)
            {
                Add(item);
            }

            /// <summary>
            /// Removes all slots.
            /// </summary>
            public void Clear()
            {
                m_slots.Clear();
            }

            /// <summary>
            /// Determines whether the slot with the specified name is already registered.
            /// </summary>
            /// <param name="slotName">The name of the slot.</param>
            /// <returns><see langword="true"/> if slot with the specified name is registered in the collection; otherwise, <see langword="false"/>.</returns>
            public bool Contains(string slotName)
            {
                return m_slots.ContainsKey(slotName);
            }

            bool ICollection<KeyValuePair<string, IRuntimeSlot>>.Contains(KeyValuePair<string, IRuntimeSlot> item)
            {
                return m_slots.Contains(item);
            }

            void ICollection<KeyValuePair<string, IRuntimeSlot>>.CopyTo(KeyValuePair<string, IRuntimeSlot>[] array, int arrayIndex)
            {
                m_slots.CopyTo(array, arrayIndex);
            }

            /// <summary>
            /// Gets count of the registered slots.
            /// </summary>
            public int Count
            {
                get { return m_slots.Count; }
            }

            bool ICollection<KeyValuePair<string, IRuntimeSlot>>.IsReadOnly
            {
                get { return m_slots.IsReadOnly; }
            }

            /// <summary>
            /// Removes the slot by its name.
            /// </summary>
            /// <param name="slotName">The name of the slot to be removed.</param>
            /// <returns><see langword="true"/> if slot was exist and removed successfully; otherwise, <see langword="false"/>.</returns>
            public bool Remove(string slotName)
            {
                return m_slots.Remove(slotName);
            }

            bool ICollection<KeyValuePair<string, IRuntimeSlot>>.Remove(KeyValuePair<string, IRuntimeSlot> item)
            {
                return m_slots.Remove(item);
            }

            /// <summary>
            /// Returns an enumerator through registered slots.
            /// </summary>
            /// <returns>The enumerator through registered slots.</returns>
            public IEnumerator<KeyValuePair<string, IRuntimeSlot>> GetEnumerator()
            {
                return m_slots.GetEnumerator();
            }

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            /// <summary>
            /// Infers contract from collection of slots.
            /// </summary>
            /// <param name="collection">The collection of slots.</param>
            /// <returns>Inferred contract.</returns>
            public static explicit operator ScriptCompositeContract(ObjectSlotCollection collection)
            {
                return collection != null ? new ScriptCompositeContract(collection.GetSlotSurrogates()) : null;
            }

            /// <summary>
            /// Determines whether the current collection contains the same set of slots
            /// as other collection.
            /// </summary>
            /// <param name="other">Other collection to compare.</param>
            /// <returns></returns>
            public virtual bool Equals(ObjectSlotCollection other)
            {
                switch (other != null && Count == other.Count)
                {
                    case true:
                        foreach (var l in this)
                        {
                            var r = other[l.Key];
                            if (r == null || !l.Equals(r)) return false;
                        }
                        return true;
                    default: return false;
                }
            }

            /// <summary>
            /// Determines whether the current collection contains the same set of slots
            /// as other collection.
            /// </summary>
            /// <param name="other">Other collection to compare.</param>
            /// <returns></returns>
            public sealed override bool Equals(object other)
            {
                return Equals(other as ObjectSlotCollection);
            }

            /// <summary>
            /// Computes hash code of this collection. 
            /// </summary>
            /// <returns></returns>
            public override int GetHashCode()
            {
                var result = 0;
                foreach (var s in m_slots)
                    result = result ^ (StringEqualityComparer.GetHashCode(s.Key) << 1) ^ (s.Value.GetHashCode());
                return result;
            }
        }
        #endregion

        private const string SlotCollectionHolder = "Slots";
        private readonly ObjectSlotCollection m_slots;
        private IScriptContract m_contract;

        /// <summary>
        /// Deserializes composite object.
        /// </summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        protected ScriptCompositeObject(SerializationInfo info, StreamingContext context)
        {
            m_slots = (ObjectSlotCollection)info.GetValue(SlotCollectionHolder, typeof(ObjectSlotCollection));
        }

        /// <summary>
        /// Initializes a new custom object with the specified set of slots.
        /// </summary>
        /// <param name="slots">A collection with object slots.</param>
        public ScriptCompositeObject(IEnumerable<KeyValuePair<string, IRuntimeSlot>> slots)
        {
            m_slots = slots is ObjectSlotCollection ? ((ObjectSlotCollection)slots) : new ObjectSlotCollection(slots);
        }

        /// <summary>
        /// Defines a new constant slot.
        /// </summary>
        /// <param name="slotName">The name of the slot. Cannot be <see langword="null"/> or empty.</param>
        /// <param name="value">The value of the constant.</param>
        /// <returns>A new constant slot.</returns>
        /// <exception cref="System.ArgumentNullException"><paramref name="slotName"/> is <see langword="null"/> or empty.</exception>
        protected static KeyValuePair<string, IRuntimeSlot> Constant(string slotName, IScriptObject value)
        {
            if (string.IsNullOrEmpty(slotName)) throw new ArgumentNullException("slotName");
            if (value == null) value = Void;
            return new KeyValuePair<string, IRuntimeSlot>(slotName, new ScriptConstant(value));
        }

        /// <summary>
        /// Defines a new constant slot.
        /// </summary>
        /// <param name="slotName">The name of the slot. Cannot be <see langword="null"/> or empty.</param>
        /// <param name="value">The value of the constant.</param>
        /// <param name="contract">The static contract binding of the constant.</param>
        /// <returns>A new constant slot.</returns>
        /// <exception cref="System.ArgumentNullException"><paramref name="slotName"/> is <see langword="null"/> or empty.</exception>
        protected static KeyValuePair<string, IRuntimeSlot> Constant(string slotName, IScriptObject value, IScriptContract contract)
        {
            if (string.IsNullOrEmpty(slotName)) throw new ArgumentNullException("slotName");
            if (value == null) value = Void;
            if (contract == null) value = Void;
            return new KeyValuePair<string, IRuntimeSlot>(slotName, new ScriptConstant(value, contract));
        }

        /// <summary>
        /// Defines a new variable slot.
        /// </summary>
        /// <param name="slotName">The name of the slot. Cannot be <see langword="null"/> or empty.</param>
        /// <param name="value">The value of the variable.</param>
        /// <returns>A new variable slot.</returns>
        /// <exception cref="System.ArgumentNullException"><paramref name="slotName"/> is <see langword="null"/> or empty.</exception>
        protected static KeyValuePair<string, IRuntimeSlot> Variable(string slotName, IScriptObject value)
        {
            if (string.IsNullOrEmpty(slotName)) throw new ArgumentNullException("slotName");
            if (value == null) value = Void;
            return new KeyValuePair<string, IRuntimeSlot>(slotName, new ScriptVariable(value));
        }

        /// <summary>
        /// Defines a new variable slot.
        /// </summary>
        /// <param name="slotName">The name of the slot. Cannot be <see langword="null"/> or empty.</param>
        /// <param name="value">The value of the variable.</param>
        /// <param name="contract">The contract binding of the variable.</param>
        /// <returns>A new variable slot.</returns>
        /// <exception cref="System.ArgumentNullException"><paramref name="slotName"/> is <see langword="null"/> or empty.</exception>
        protected static KeyValuePair<string, IRuntimeSlot> Variable(string slotName, IScriptObject value, IScriptContract contract)
        {
            if (string.IsNullOrEmpty(slotName)) throw new ArgumentNullException("slotName");
            if (value == null) value = Void;
            if (contract == null) contract = Void;
            return new KeyValuePair<string, IRuntimeSlot>(slotName, new ScriptVariable(value, contract));
        }

        /// <summary>
        /// Defines a new variable slot.
        /// </summary>
        /// <param name="slotName">The name of the slot. Cannot be <see langword="null"/> or empty.</param>
        /// <param name="contract">The contract binding of the variable.</param>
        /// <returns>A new variable slot.</returns>
        /// <exception cref="System.ArgumentNullException"><paramref name="slotName"/> is <see langword="null"/> or empty.</exception>
        protected static KeyValuePair<string, IRuntimeSlot> Variable(string slotName, IScriptContract contract = null)
        {
            if (string.IsNullOrEmpty(slotName)) throw new ArgumentNullException("slotName");
            if (contract == null) contract = Void;
            return new KeyValuePair<string, IRuntimeSlot>(slotName, new ScriptVariable(contract));
        }

        /// <summary>
        /// Gets a collection of the object slots.
        /// </summary>
        public sealed override ICollection<string> Slots
        {
            get
            {
                return m_slots.SlotNames;
            }
        }

        /// <summary>
        /// Populates all objects stored in the slots of the composite object.
        /// </summary>
        /// <param name="state">Internal interpreter state.</param>
        /// <returns>The collection of populated objects.</returns>
        public IEnumerable<KeyValuePair<string, IScriptObject>> GetSlotValues(InterpreterState state)
        {
            foreach (var slot in m_slots)
                yield return new KeyValuePair<string, IScriptObject>(slot.Key, slot.Value.GetValue(state));
        }

        /// <summary>
        /// Creates a clone of the composite contract.
        /// </summary>
        /// <returns>The clone of the composite contract.</returns>
        protected override ScriptObject Clone()
        {
            return new ScriptCompositeObject(m_slots);
        }

        #region Runtime Helpers

        private static NewExpression New(Expression slots)
        {
            var ctor = LinqHelpers.BodyOf<IEnumerable<KeyValuePair<string, IRuntimeSlot>>, ScriptCompositeObject, NewExpression>(s => new ScriptCompositeObject(s));
            return ctor.Update(new[] { slots });
        }

        internal static BlockExpression Bind(IEnumerable<KeyValuePair<string, Expression>> slots, ParameterExpression @this)
        {
            ICollection<Expression> expressions = new LinkedList<Expression>();
            //Declare variable that stores collection of slots
            var runtimeSlots = Expression.Variable(typeof(ObjectSlotCollection));
            //runtimeSlots = new ObjectSlotCollection();
            expressions.Add(Expression.Assign(runtimeSlots, LinqHelpers.Restore<ObjectSlotCollection>()));
            //@this = new ScriptCompositeObject(runtimeSlots);
            expressions.Add(Expression.Assign(@this, New(runtimeSlots)));
            //add each slot to the collection
            foreach (var s in slots)
                expressions.Add(ObjectSlotCollection.Add(runtimeSlots, s));
            //return the newly constructed composite object
            expressions.Add(@this);
            return Expression.Block(new[] { runtimeSlots, @this }, expressions); 
        }

        internal static NewExpression Bind(IEnumerable<KeyValuePair<string, ParameterExpression>> slots)
        {
            return New(Expression.NewArrayInit(typeof(KeyValuePair<string, IRuntimeSlot>), Enumerable.Select(slots, s => LinqHelpers.CreateKeyValuePair<string, IRuntimeSlot>(s.Key, s.Value))));
        }

        /// <summary>
        /// Adds a new slot to the dictionary.
        /// </summary>
        /// <param name="slots"></param>
        /// <param name="slotName"></param>
        /// <param name="slot"></param>
        /// <returns></returns>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static IDictionary<string, IRuntimeSlot> RtlAppendSlot(IDictionary<string, IRuntimeSlot> slots, string slotName, IRuntimeSlot slot)
        {
            if (slots == null) slots = new Dictionary<string, IRuntimeSlot>(10, new StringEqualityComparer());
            slots[slotName] = slot;
            return slots;
        }
        #endregion

        /// <summary>
        /// Gets runtime slot by its name.
        /// </summary>
        /// <param name="slotName">The name of the slot.</param>
        /// <returns>Runtime slot holder.</returns>
        public IRuntimeSlot this[string slotName]
        {
            get { return m_slots[slotName] ?? RuntimeSlotBase.Missing(slotName); }
        }

        /// <summary>
        /// Gets runtime slot by its name.
        /// </summary>
        /// <param name="slotName">The name of the slot.</param>
        /// <param name="state">Internal interpreter state.</param>
        /// <returns>Runtime slot holder.</returns>
        public sealed override IRuntimeSlot this[string slotName, InterpreterState state]
        {
            get 
            {
                return this[slotName];
            }
        }

        /// <summary>
        /// Returns a contract binding of the complex object.
        /// </summary>
        /// <returns>The contract binding of the complex object.</returns>
        public sealed override IScriptContract GetContractBinding()
        {
            if (m_contract == null) m_contract = GetContractBinding(m_slots);
            return m_contract;
        }

        /// <summary>
        /// Extracts contract from the collection of slots.
        /// </summary>
        /// <param name="collection">The collection of slots.</param>
        /// <returns>The contract binding of the complex object.</returns>
        protected virtual ScriptCompositeContract GetContractBinding(ObjectSlotCollection collection)
        {
            return collection.Count == 0 ? ScriptCompositeContract.Empty : (ScriptCompositeContract)collection;
        }

        /// <summary>
        /// Extracts metadata of the specified slot.
        /// </summary>
        /// <param name="slotName">The name of the requested slot.</param>
        /// <param name="state">Internal interpreter state.</param>
        /// <returns>The object that encapsulated metadata information.</returns>
        protected sealed override IScriptObject GetSlotMetadata(string slotName, InterpreterState state)
        {
            var contract = (ScriptCompositeContract)GetContractBinding();
            return contract.GetSlotMetadata(slotName);
        }

        internal virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue(SlotCollectionHolder, m_slots, typeof(ObjectSlotCollection));
        }

        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
        {
            GetObjectData(info, context);
        }

        /// <summary>
        /// Returns composite object where each slot is immutable.
        /// </summary>
        /// <param name="state">Internal interpreter state.</param>
        /// <returns>The composite object where each slot is immutable.</returns>
        public ScriptCompositeObject AsReadOnly(InterpreterState state)
        {
            return new ScriptCompositeObject(m_slots.Select(t => new KeyValuePair<string, IRuntimeSlot>(t.Key, new ScriptConstant(t.Value.GetValue(state), t.Value.GetContractBinding(), state))));
        }

        private ScriptBoolean Equals(ScriptCompositeObject right, InterpreterState state)
        {
            return m_slots.Equals(right.m_slots);
        }

        /// <summary>
        /// Determines whether the current composite object is equal to another object.
        /// </summary>
        /// <param name="right"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        protected sealed override IScriptObject Equals(IScriptObject right, InterpreterState state)
        {
            if (right is ScriptCompositeObject)
                return Equals((ScriptCompositeObject)right, state);
            else if (state.Context == InterpretationContext.Unchecked)
                return Void;
            else throw new UnsupportedOperationException(state);
        }

        /// <summary>
        /// Determines whether the current composite object is not equal to another object.
        /// </summary>
        /// <param name="right"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        protected sealed override IScriptObject NotEquals(IScriptObject right, InterpreterState state)
        {
            if (right is ScriptCompositeObject)
                return !Equals((ScriptCompositeObject)right, state);
            else if (state.Context == InterpretationContext.Unchecked)
                return Void;
            else throw new UnsupportedOperationException(state);
        }

        IScriptCompositeObject IScriptCompositeObject.AsReadOnly(InterpreterState state)
        {
            return AsReadOnly(state);
        }

        /// <summary>
        /// Splits this composite object on its slots.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<ScriptCompositeObject> Split()
        {
            return m_slots.Select(pair => new ScriptCompositeObject(new[] { pair }));
        }

        /// <summary>
        /// Produces a new set of elements from each value in slots holded by
        /// this composite object.
        /// </summary>
        /// <param name="state">Internal interpreter state.</param>
        /// <returns>A new set of elements.</returns>
        public IScriptSet CreateSet(InterpreterState state)
        {
            return new ScriptSetContract(m_slots.Select(s => s.Value.GetValue(state)));
        }
    }
}
