using System;
using System.Runtime.Serialization;
using System.Collections.Generic;
using System.Linq;

namespace DynamicScript.Runtime.Environment
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;
    using InterpretationContext = Compiler.Ast.InterpretationContext;

    /// <summary>
    /// Represents unordered finite set of values.
    /// This class cannot be inherited.
    /// </summary>
    [ComVisible(false)]
    [Serializable]
    sealed class ScriptSetContract: ScriptContract, IScriptSet, ISerializable
    {
        private readonly HashSet<IScriptObject> m_set;
        private IScriptContract m_contract;

        private ScriptSetContract(SerializationInfo info, StreamingContext context)
        {
        }

        private ScriptSetContract(ISet<IScriptObject> set)
        {
            if (set == null) throw new ArgumentNullException("set");
            m_set = set is HashSet<IScriptObject> ? (HashSet<IScriptObject>)set : new HashSet<IScriptObject>(set);
            if (m_set.Count == 0) throw new ArgumentException();
        }

        /// <summary>
        /// Initializes a new set contract.
        /// </summary>
        /// <param name="objects">Collection of objects.</param>
        /// <exception cref="System.ArgumentException">The collection contains single or zero elements.</exception>
        public ScriptSetContract(IEnumerable<IScriptObject> objects)
            : this(new HashSet<IScriptObject>(objects))
        {
        }

        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
        {
            
        }

        public long Count
        {
            get { return m_set.Count; }
        }

        private ISet<IScriptObject> Elements
        {
            get { return m_set; }
        }

        /// <summary>
        /// Gets underlying contract of the set.
        /// </summary>
        public IScriptContract UnderlyingContract
        {
            get 
            {
                if (m_contract == null) m_contract = Infer(Elements.Select(e => e.GetContractBinding()));
                return m_contract;
            }
        }

        /// <summary>
        /// Determines whether a set is a subset of a specified collection.
        /// </summary>
        /// <param name="other">Other set to compare. Cannot be <see langword="null"/>.</param>
        /// <param name="strict">Specifies proper(strict) subset detection.</param>
        /// <returns><see langword="true"/> if the current set is a subset of <paramref name="other"/>; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="System.ArgumentNullException"><paramref name="other"/> is <see langword="null"/>.</exception>
        public bool IsSubsetOf(IEnumerable<IScriptObject> other, bool strict)
        {
            return strict ? Elements.IsProperSubsetOf(other) : Elements.IsSubsetOf(other);
        }

        /// <summary>
        /// Determines whether the current set is a superset of a specified collection.
        /// </summary>
        /// <param name="other">The collection to compare to the current set. Cannot be <see langword="null"/>.</param>
        /// <param name="strict">Specifies proper(strict) superset detection.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException"><paramref name="other"/> is <see langword="null"/>.</exception>
        public bool IsSupersetOf(IEnumerable<IScriptObject> other, bool strict)
        {
            return strict ? Elements.IsProperSupersetOf(other) : Elements.IsSupersetOf(other);
        }

        /// <summary>
        /// Determines whether the current set overlaps with the specified collection.
        /// </summary>
        /// <param name="other">The collection to compare to the current set. Cannot be <see langword="null"/>.</param>
        /// <returns><see langword="true"/> if the current set and other share at least one common element; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="System.ArgumentNullException"><paramref name="other"/> is <see langword="null"/>.</exception>
        public bool Overlaps(IEnumerable<IScriptObject> other)
        {
            return Elements.Overlaps(other);
        }

        /// <summary>
        /// Removes all elements in the specified collection from the current set.
        /// </summary>
        /// <param name="other">The collection of items to remove from the set. Cannot be <see langword="null"/>.</param>
        /// <returns>A new modified set; or <see langword="null"/> if the result set is empty.</returns>
        /// <exception cref="System.ArgumentNullException"><paramref name="other"/> is <see langword="null"/>.</exception>
        public IScriptSet ExceptWith(IEnumerable<IScriptObject> other)
        {
            var set = new HashSet<IScriptObject>(Elements);
            set.ExceptWith(other);
            return new ScriptSetContract(set);
        }

        /// <summary>
        /// Returns set with elements contained in the current set and the specified collection.
        /// </summary>
        /// <param name="other">The collection to compare to the current set. Cannot be <see langword="null"/>.</param>
        /// <returns>A new modified set; or <see langword="null"/> if the result set is empty.</returns>
        /// <exception cref="System.ArgumentNullException"><paramref name="other"/> is <see langword="null"/>.</exception>
        public IScriptSet IntersectWith(IEnumerable<IScriptObject> other)
        {
            var set = new HashSet<IScriptObject>(Elements);
            set.IntersectWith(other);
            return new ScriptSetContract(set);
        }

        /// <summary>
        /// Returns a new set so that it contains only elements that are present either in the current 
        /// set or in the specified collection, but not both.
        /// </summary>
        /// <param name="other">The collection to compare to the current set. Cannot be <see langword="null"/>.</param>
        /// <returns>A new modified set; or <see langword="null"/> if the result set is empty.</returns>
        /// <exception cref="System.ArgumentNullException"><paramref name="other"/> is <see langword="null"/>.</exception>
        public IScriptSet SymmetricExceptWith(IEnumerable<IScriptObject> other)
        {
            var set = new HashSet<IScriptObject>(Elements);
            set.SymmetricExceptWith(other);
            return new ScriptSetContract(set);
        }

        /// <summary>
        /// Returns a new set so that it contains all elements that are present in both the current 
        /// set and in the specified collection.
        /// </summary>
        /// <param name="other">The collection to compare to the current set. Cannot be <see langword="null"/>.</param>
        /// <exception cref="System.ArgumentNullException"><paramref name="other"/> is <see langword="null"/>.</exception>
        public IScriptSet UnionWith(IEnumerable<IScriptObject> other)
        {
            var set = new HashSet<IScriptObject>(Elements);
            set.UnionWith(other);
            return new ScriptSetContract(set);
        }

        /// <summary>
        /// Returns an enumerator through set elements.
        /// </summary>
        /// <returns>An enumerator through set elements.</returns>
        public new IEnumerator<IScriptObject> GetEnumerator()
        {
            return Elements.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        ///  Determines whether the current set and the specified collection contain the same elements.
        /// </summary>
        /// <param name="other">The collection to compare to the current set.</param>
        /// <returns><see langword="true"/> if the current set is equal to <paramref name="other"/>; otherwise, <see langword="false"/>.</returns>
        public bool Equals(IEnumerable<IScriptObject> other)
        {
            return Elements.SetEquals(other);
        }

        /// <summary>
        /// Returns relationship between this set and other set.
        /// </summary>
        /// <param name="contract"></param>
        /// <returns></returns>
        public ContractRelationshipType GetRelationship(IScriptSet contract)
        {
            //Join two slot set
            var equalSlots = Enumerable.Aggregate(from left in Elements
                                                  from right in contract
                                                  where Equals(left, right)
                                                  select 1,
                                                  (l, r) => l + r);
            switch (Count == contract.Count)
            {
                case true:
                    return equalSlots == Count ? ContractRelationshipType.TheSame : ContractRelationshipType.None;
                default:
                    if (equalSlots == Count)
                        return ContractRelationshipType.Superset;
                    else if (equalSlots == contract.Count)
                        return ContractRelationshipType.Subset;
                    return ContractRelationshipType.None;
            }
        }

        public override ContractRelationshipType GetRelationship(IScriptContract contract)
        {
            if (contract is IScriptSet)
                return GetRelationship((IScriptSet)contract);
            else if (contract is ScriptSuperContract)
                return ContractRelationshipType.Subset;
            else if (IsVoid(contract))
                return ContractRelationshipType.Superset;
            else if (UnderlyingContract.GetRelationship(contract) == ContractRelationshipType.TheSame)
                return ContractRelationshipType.Subset;
            else return ContractRelationshipType.None;
        }

        /// <summary>
        /// This method is not supported.
        /// </summary>
        /// <param name="args"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        public override IScriptObject CreateObject(IList<IScriptObject> args, InterpreterState state)
        {
            throw new UnsupportedOperationException(state);
        }

        protected override bool Mapping(ref IScriptObject value)
        {
            switch (UnderlyingContract.GetRelationship(value.GetContractBinding()))
            {
                case ContractRelationshipType.Superset:
                    value = UnderlyingContract.Convert(Conversion.Implicit, value, null);
                    return true;
                case ContractRelationshipType.TheSame: return true;
                default: return false;
            }
        }

        public override IScriptObject Convert(IScriptObject value, InterpreterState state)
        {
            if (Mapping(ref value))
                return value;
            else if (state.Context == InterpretationContext.Unchecked)
                return Void;
            else throw new UnsupportedOperationException(state);
        }

        public override IScriptContract GetContractBinding()
        {
            return ScriptFinSetContract.Instance;
        }
    }
}
