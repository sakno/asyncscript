using System;
using System.Runtime.Serialization;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace DynamicScript.Runtime.Environment
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;
    using InterpretationContext = Compiler.Ast.InterpretationContext;
    using Enumerable = System.Linq.Enumerable;
    using SystemMath = System.Math;
    using Operator = Compiler.Operator;

    /// <summary>
    /// Represents tuple.
    /// This class cannot be inherited.
    /// </summary>
    [ComVisible(false)]
    [Serializable]
    public sealed class ScriptTuple : ScriptObject, ISerializable
    {
        #region Nested Types

        [ComVisible(false)]
        [Serializable]
        private sealed class ScriptCartesianContract : ScriptContract, ISerializable, IEnumerable<IScriptContract>, IScriptCartesianProduct
        {
            private const string ContractsSerializationSlot = "Contracts";

            public static readonly ScriptArrayContract ContractBinding = new ScriptArrayContract();
            private readonly ReadOnlyCollection<IScriptContract> m_contracts;

            private ScriptCartesianContract(SerializationInfo info, StreamingContext context)
            {
                m_contracts = (ReadOnlyCollection<IScriptContract>)info.GetValue(ContractsSerializationSlot, typeof(ReadOnlyCollection<IScriptContract>));
            }

            public ScriptCartesianContract(IEnumerable<IScriptContract> contracts)
            {
                if (contracts is ReadOnlyCollection<IScriptContract>)
                    m_contracts = (ReadOnlyCollection<IScriptContract>)contracts;
                if (contracts is IList<IScriptContract>)
                    m_contracts = new ReadOnlyCollection<IScriptContract>((IList<IScriptContract>)contracts);
                else m_contracts = Array.AsReadOnly(Enumerable.ToArray(contracts));
            }

            private static IEnumerable<IScriptContract> GetContracts(IScriptContract left, IScriptContract right, IEnumerable<IScriptContract> contracts)
            {
                IEnumerable<IScriptContract> result = left is ScriptCartesianContract ? ((ScriptCartesianContract)left).Contracts : new[] { left };
                result = Enumerable.Concat(result, right is ScriptCartesianContract ? ((ScriptCartesianContract)right).Contracts : new[] { right });
                return Enumerable.Concat(result, contracts);
            }

            public ScriptCartesianContract(IScriptContract left, IScriptContract right, params IScriptContract[] contracts)
                : this(GetContracts(left, right, contracts))
            {
            }

            public override IScriptObject Convert(IScriptObject value, InterpreterState state)
            {
                if (Mapping(ref value))
                    return value;
                else if (state.Context == InterpretationContext.Unchecked)
                    return Void;
                else throw new UnsupportedOperationException(state);
            }

            /// <summary>
            /// Gets immutable list of contracts participated in cartesian product.
            /// </summary>
            public IList<IScriptContract> Contracts
            {
                get { return m_contracts; }
            }

            /// <summary>
            /// Returns relationship between this cartesian product and the specified cartesian product.
            /// </summary>
            /// <param name="cartesian"></param>
            /// <returns></returns>
            public ContractRelationshipType GetRelationship(ScriptCartesianContract cartesian)
            {
                //Compare each member of the cartesian product.
                if (!ParallelEqualityComparer<IScriptContract, IScriptContract>.MayBeEqual(Contracts, cartesian.Contracts))
                    return ContractRelationshipType.None;
                //compare a power of two cartesian products.
                var cmp = Contracts.Count.CompareTo(cartesian.Contracts.Count);
                if (cmp < 0) return ContractRelationshipType.Superset;
                else if (cmp > 0) return ContractRelationshipType.Subset;
                else return ContractRelationshipType.TheSame;
            }

            public override ContractRelationshipType GetRelationship(IScriptContract contract)
            {
                if (contract is ScriptSuperContract)
                    return ContractRelationshipType.Subset;
                else if (IsVoid(contract))
                    return ContractRelationshipType.Superset;
                if (contract is ScriptCartesianContract)
                    return GetRelationship((ScriptCartesianContract)contract);
                else return ContractRelationshipType.None;
            }

            /// <summary>
            /// Creates a new tuple.
            /// </summary>
            /// <param name="args"></param>
            /// <param name="state"></param>
            /// <returns></returns>
            public override IScriptObject CreateObject(IList<IScriptObject> args, InterpreterState state)
            {
                if (args.Count == 0)
                    return new ScriptTuple(Enumerable.Select(Contracts, c => c.FromVoid(state)));
                else if (args.Count == Contracts.Count)
                    return new ScriptTuple(args);
                else if (state.Context == InterpretationContext.Unchecked)
                    return Void;
                else throw new ActionArgumentsMistmatchException(state);
            }

            /// <summary>
            /// Returns underlying contract for this cartesian product.
            /// </summary>
            /// <returns></returns>
            public override IScriptContract GetContractBinding()
            {
                return ScriptMetaContract.Instance;
            }

            void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
            {
                info.AddValue(ContractsSerializationSlot, Contracts, typeof(ReadOnlyCollection<IScriptContract>));
            }

            /// <summary>
            /// Returns string representation of the contract.
            /// </summary>
            /// <returns></returns>
            public override string ToString()
            {
                return string.Join<string>(Operator.Asterisk, Enumerable.Select(Contracts, c => c.ToString()));
            }

            /// <summary>
            /// Returns an enumerator through all contracts participated in cartesian product.
            /// </summary>
            /// <returns>An enumerator through all contracts participated in cartesian product.</returns>
            public new IEnumerator<IScriptContract> GetEnumerator()
            {
                return Contracts.GetEnumerator();
            }

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }
        #endregion

        private const string ValuesSerializationSlot = "Values";
        private readonly ScriptList m_values;
        private IScriptContract m_contract;

        private ScriptTuple(SerializationInfo info, StreamingContext context)
        {
            m_values = (ScriptList)info.GetValue(ValuesSerializationSlot, typeof(ScriptList));
        }

        private ScriptTuple(IEnumerable<IScriptObject> values)
        {
            m_values = new ScriptList(10, values);
        }

        /// <summary>
        /// Initializes a new tuple.
        /// </summary>
        /// <param name="value1">The first value to store.</param>
        /// <param name="value2">The second value to store. </param>
        /// <param name="values"></param>
        public ScriptTuple(IScriptObject value1, IScriptObject value2, params IScriptObject[] values)
            : this(Enumerable.Concat(new[] { value1, value2 }, values ?? new IScriptObject[0]))
        {

        }

        /// <summary>
        /// Gets values in the tuple.
        /// </summary>
        public IList<IScriptObject> Values
        {
            get { return m_values; }
        }

        /// <summary>
        /// Returns cartesian product of the specified contracts.
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <param name="contracts"></param>
        /// <returns></returns>
        public static ScriptContract GetContractBinding(IScriptContract left, IScriptContract right, params IScriptContract[] contracts)
        {
            return new ScriptCartesianContract(left, right, contracts);
        }

        /// <summary>
        /// Gets collection of object slots.
        /// </summary>
        public override ICollection<string> Slots
        {
            get { return m_values.Slots; }
        }

        /// <summary>
        /// Gets slot by its name.
        /// </summary>
        /// <param name="slotName"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        public override IRuntimeSlot this[string slotName, InterpreterState state]
        {
            get { return m_values[slotName, state]; }
        }

        /// <summary>
        /// Gets tuple element accessor.
        /// </summary>
        /// <param name="args"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        public override RuntimeSlotBase this[IScriptObject[] args, InterpreterState state]
        {
            get { return m_values[args, state]; }
        }

        /// <summary>
        /// Returns contract for this tuple.
        /// </summary>
        /// <returns></returns>
        public override IScriptContract GetContractBinding()
        {
            if (m_contract == null) m_contract = new ScriptCartesianContract(Values.GetContractBindings());
            return m_contract;
        }

        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue(ValuesSerializationSlot, Values, typeof(ReadOnlyCollection<IScriptObject>));
        }
    }
}
