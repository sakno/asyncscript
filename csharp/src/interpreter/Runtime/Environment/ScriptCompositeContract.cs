using System;
using System.Dynamic;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Collections;
using System.Runtime.Serialization;
using System.Linq;

namespace DynamicScript.Runtime.Environment
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;
    using InterpretationContext = Compiler.Ast.InterpretationContext;
    using Compiler;
    using Interlocked = System.Threading.Interlocked;

    /// <summary>
    /// Represents composite contract with slots.
    /// </summary>
    /// <example>
    /// The following example demonstrates how to define composite contract:
    /// <code language="C#">
    /// sealed class MyContract : ScriptCompositeContract
    /// {
    ///     private static readonly KeyValuePair&lt;string, IScriptContract&gt; Slot1 = DefineSlot("slot1");
    ///     private static readonly KeyValuePair&lt;string, IScriptContract&gt; Slot2 = DefineSlot("slot2", ScriptIntegerContract.Instance);
    /// 
    ///     public MyContract()
    ///         : base(Slot1, Slot2)
    ///     {
    ///     }
    /// }
    /// </code>
    /// </example>
    [ComVisible(false)]
    [Serializable]
    public class ScriptCompositeContract : ScriptContract, IEnumerable, ISerializable
    {
        #region Nested Types

        /// <summary>
        /// Represents metadata of the slot.
        /// </summary>
        [ComVisible(false)]
        [Serializable]
        internal protected struct SlotMeta: IEquatable<SlotMeta>
        {
            private readonly bool m_constant;
            private readonly IScriptContract m_contract;

            /// <summary>
            /// Initializes a new slot metadata.
            /// </summary>
            /// <param name="contract">The contract of the slot.</param>
            /// <param name="constant">Specifies that the contract is immutable.</param>
            public SlotMeta(IScriptContract contract, bool constant = false)
            {
                m_constant = constant;
                m_contract = contract;
            }

            /// <summary>
            /// Gets a value indicating that the slot is immutable.
            /// </summary>
            public bool IsConstant 
            {
                get { return m_constant; }
            }

            /// <summary>
            /// Gets contract binding of the slot.
            /// </summary>
            public IScriptContract ContractBinding
            {
                get { return m_contract ?? Void; }
            }

            /// <summary>
            /// Creates a new runtime slot using this metadata specification.
            /// </summary>
            /// <returns>A new runtime slot using this metadata specification.</returns>
            public IRuntimeSlot CreateSlot()
            {
                switch (IsConstant)
                {
                    case true: return new ScriptConstant(Void, ContractBinding);
                    default: return new ScriptVariable(Void, ContractBinding);
                }
            }

            /// <summary>
            /// Determines whether the current slot metadata is equal to another.
            /// </summary>
            /// <param name="other">Other slot metadata to compare.</param>
            /// <returns><see langword="true"/> if the current slot metadata is equal to another; otherwise, <see langword="false"/>.</returns>
            public bool Equals(SlotMeta other)
            {
                return IsConstant == other.IsConstant && ContractBinding.Equals(other.ContractBinding);
            }
        }

        [ComVisible(false)]
        [Serializable]
        private sealed class SlotAggregator : ParallelAggregator<KeyValuePair<string, SlotMeta>, long>
        {
            protected override void Aggregate(KeyValuePair<string, SlotMeta> slot1, KeyValuePair<string, SlotMeta> slot2, ref long result)
            {
                if (StringEqualityComparer.Equals(slot1.Key, slot2.Key) && slot1.Value.ContractBinding.Equals(slot2.Value.ContractBinding))
                    Interlocked.Increment(ref result);
            }

            public static new long Aggregate(IEnumerable<KeyValuePair<string, SlotMeta>> slots1, IEnumerable<KeyValuePair<string, SlotMeta>> slots2)
            {
                return Aggregate<SlotAggregator>(slots1, slots2);
            }
        }
        #endregion
        private const string SlotsHolder = "Slots";

        private readonly IDictionary<string, SlotMeta> m_slots;
#if USE_REL_MATRIX
        private int? m_hashCode;
#endif

        /// <summary>
        /// Deserializes composite object contract.
        /// </summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        protected ScriptCompositeContract(SerializationInfo info, StreamingContext context)
        {
            m_slots = (IDictionary<string, SlotMeta>)info.GetValue(SlotsHolder, typeof(Dictionary<string, SlotMeta>));
        }

        /// <summary>
        /// Initializes a new contract with the specified set of slots.
        /// </summary>
        /// <param name="slots">A collection of the slot definitions.</param>
        internal protected ScriptCompositeContract(IEnumerable<KeyValuePair<string, SlotMeta>> slots)
        {
            m_slots = new Dictionary<string, SlotMeta>(new StringEqualityComparer());
            foreach (var s in slots ?? Enumerable.Empty<KeyValuePair<string, SlotMeta>>()) m_slots.Add(s.Key, s.Value);
        }

        /// <summary>
        /// Initializes a new composite contract.
        /// </summary>
        /// <param name="slots"></param>
        public ScriptCompositeContract(IEnumerable<KeyValuePair<string, IScriptContract>> slots)
            : this(from s in slots select new KeyValuePair<string, SlotMeta>(s.Key, new SlotMeta(s.Value)))
        {
        }

        internal static NewExpression New(IEnumerable<KeyValuePair<string, Expression>> slots)
        {
            var ctor = LinqHelpers.BodyOf<IEnumerable<KeyValuePair<string, IScriptContract>>, ScriptCompositeContract, NewExpression>(s => new ScriptCompositeContract(s));
            return ctor.Update(new[] { Expression.NewArrayInit(typeof(KeyValuePair<string, IScriptContract>), from s in slots
                                                                                                            select LinqHelpers.CreateKeyValuePair<string, IScriptContract>(LinqHelpers.Constant(s.Key), RequiresContract(s.Value))) });
        }

        /// <summary>
        /// Represents composite contract that doesn't containt any slot.
        /// </summary>
        public static readonly ScriptCompositeContract Empty = new ScriptCompositeContract(Enumerable.Empty<KeyValuePair<string, SlotMeta>>());


        internal static MemberExpression EmptyField
        {
            get { return LinqHelpers.BodyOf<Func<ScriptCompositeContract>, MemberExpression>(() => Empty); }
        }

        /// <summary>
        /// Delcares a new contract slot.
        /// </summary>
        /// <param name="slotName">The slot name.</param>
        /// <param name="contract">Slot contract binding.</param>
        /// <param name="constant">Specifies that the defined slot is immutable.</param>
        /// <returns>Contract slot definition.</returns>
        protected static KeyValuePair<string, SlotMeta> DefineSlot(string slotName, IScriptContract contract, bool constant = false)
        {
            return new KeyValuePair<string, SlotMeta>(slotName, new SlotMeta(contract, constant));
        }

        /// <summary>
        /// Gets a contract binding for the current object.
        /// </summary>
        /// <returns>The contract binding for the current object.</returns>
        /// <remarks>This method always returns reference to 'type' contract.</remarks>
        public sealed override IScriptContract GetContractBinding()
        {
            return ScriptMetaContract.Instance;
        }

        /// <summary>
        /// Creates a clone of the composite contract.
        /// </summary>
        /// <returns>The clone of the composite contract.</returns>
        protected override ScriptObject Clone()
        {
            return new ScriptCompositeContract(m_slots);
        }

        /// <summary>
        /// Returns a collection of reflected slots.
        /// </summary>
        /// <returns>A collection of reflected slots.</returns>
        public IEnumerable<IScriptObject> Reflect()
        {
            foreach (var name in m_slots.Keys)
                yield return GetSlotMetadata(name);
        }

        /// <summary>
        /// Returns slot metadata.
        /// </summary>
        /// <param name="slotName"></param>
        /// <returns></returns>
        public IScriptObject GetSlotMetadata(string slotName)
        {
            var s = default(SlotMeta);
            switch (m_slots.TryGetValue(slotName, out s))
            {
                case true:
                    var pair = new KeyValuePair<string, SlotMeta>(slotName, s);
                    return Convert(pair);
                default: return Void;
            }
        }

        /// <summary>
        /// Gets collection of declared slots.
        /// </summary>
        public new ICollection<string> Slots
        {
            get
            {
                return m_slots.Keys;
            }
        }

        /// <summary>
        /// Returns slot metadata.
        /// </summary>
        /// <param name="slotName"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        protected sealed override IScriptObject GetSlotMetadata(string slotName, InterpreterState state)
        {
            return GetSlotMetadata(slotName);
        }

        #region IEnumerable Members

        IEnumerator IEnumerable.GetEnumerator()
        {
            return m_slots.Values.GetEnumerator();
        }

        #endregion

        /// <summary>
        /// Serializes composite object contract.
        /// </summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        protected virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue(SlotsHolder, m_slots, typeof(Dictionary<string, SlotMeta>));
        }

        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
        {
            GetObjectData(info, context);
        }

        /// <summary>
        /// Returns relationship between the current contract and the specified.
        /// </summary>
        /// <param name="contract">The contract to compare. Cannot be <see langword="null"/>.</param>
        /// <returns>Relationship between the current contract and <paramref name="contract"/>.</returns>
        public ContractRelationshipType GetRelationship(ScriptCompositeContract contract)
        {
            //Join two slot set
            switch (m_slots.Count == contract.m_slots.Count)
            {
                case true:
                    var equalSlots = SlotAggregator.Aggregate(m_slots, contract.m_slots);
                    return equalSlots == m_slots.Count ? ContractRelationshipType.TheSame : ContractRelationshipType.None;
                default:
                    equalSlots = SlotAggregator.Aggregate(m_slots, contract.m_slots);
                    if (equalSlots == m_slots.Count)
                        return ContractRelationshipType.Superset;
                    else if (equalSlots == contract.m_slots.Count)
                        return ContractRelationshipType.Subset;
                    return ContractRelationshipType.None;
            }
        }

        /// <summary>
        /// Returns relationship between the current contract and the specified.
        /// </summary>
        /// <param name="contract">The contract to compare. Cannot be <see langword="null"/>.</param>
        /// <returns>Relationship between the current contract and <paramref name="contract"/>.</returns>
        public sealed override ContractRelationshipType GetRelationship(IScriptContract contract)
        {
            if (contract is ScriptCompositeContract)
                return GetRelationship((ScriptCompositeContract)contract);
            else if (contract is ScriptVoid)
                return ContractRelationshipType.Superset;
            else if (contract is ScriptSuperContract)
                return ContractRelationshipType.Subset;
            else if (contract.OneOf<IScriptComplementation, IScriptUnionContract, IScriptCartesianProduct>())
                return Inverse(contract.GetRelationship(this));
            else return ContractRelationshipType.None;
        }

        private static IEnumerable<KeyValuePair<string, SlotMeta>> StructuralUnion(IEnumerable<KeyValuePair<string, SlotMeta>> left, IEnumerable<KeyValuePair<string, SlotMeta>> right, InterpreterState state)
        {
            foreach(var l in left)
                foreach(var r in right)
                    switch (StringEqualityComparer.Equals(l.Key, r.Key))
                    {
                        case true:
                            yield return new KeyValuePair<string, SlotMeta>(l.Key, new SlotMeta(Unite(new[] { l.Value.ContractBinding, r.Value.ContractBinding }, state)));
                            continue;
                        default:
                            yield return l;
                            yield return r;
                            continue;
                    }
        }

        /// <summary>
        /// Computes structural union.
        /// </summary>
        /// <param name="right"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        protected sealed override IScriptObject Add(IScriptObject right, InterpreterState state)
        {
            if (right is ScriptCompositeContract)
                return new ScriptCompositeContract(StructuralUnion(m_slots, ((ScriptCompositeContract)right).m_slots, state));
            else if (state.Context == InterpretationContext.Unchecked)
                return Void;
            else throw new UnsupportedOperationException(state);
        }

        internal virtual ScriptCompositeObject CreateCompositeObject(IList<IScriptObject> args, InterpreterState state)
        {
            if (args.Count == 0)
                return new ScriptCompositeObject(from s in m_slots select new KeyValuePair<string, IRuntimeSlot>(s.Key, s.Value.CreateSlot()));
            else throw new ActionArgumentsMistmatchException(state);
        }

        /// <summary>
        /// Creates a new composite object.
        /// </summary>
        /// <param name="args"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        public sealed override IScriptObject CreateObject(IList<IScriptObject> args, InterpreterState state)
        {
            return CreateCompositeObject(args, state);
        }

        /// <summary>
        /// Returns a string representation of the current contract.
        /// </summary>
        /// <returns></returns>
        public sealed override string ToString()
        {
            const string SlotBindingFormat = "{0}: {1}";
            return String.Concat(Operator.TypeOf, Punctuation.LeftBrace, string.Join<string>(Punctuation.Comma, m_slots.Select(t => String.Format(SlotBindingFormat, t.Key, t.Value.ContractBinding))), Punctuation.RightBrace);
        }

        /// <summary>
        /// Computes hash code for this contract.
        /// </summary>
        /// <returns></returns>
        public sealed override int GetHashCode()
        {
#if USE_REL_MATRIX
            if (m_hashCode == null) m_hashCode = m_slots.GetHashCode();
            return m_hashCode.Value;
#else
            return m_slots.GetHashCode();
#endif
        }
    }
}
