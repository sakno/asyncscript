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
                m_contracts = contracts is ReadOnlyCollection<IScriptContract> ? (ReadOnlyCollection<IScriptContract>)contracts : Array.AsReadOnly<IScriptContract>(Enumerable.ToArray(contracts));
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
                for (var i = 0; i < SystemMath.Min(Contracts.Count, cartesian.Contracts.Count); i++)
                    switch (Contracts[i].Equals(cartesian.Contracts[i]))
                    {
                        case true: continue;
                        default: return ContractRelationshipType.None;
                    }
                switch (Contracts.Count == cartesian.Contracts.Count)
                {
                    case true: return ContractRelationshipType.TheSame;
                    default: return Contracts.Count > cartesian.Contracts.Count ? ContractRelationshipType.Subset : ContractRelationshipType.Superset;
                }
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
            public override ScriptObject CreateObject(IList<IScriptObject> args, InterpreterState state)
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
        private readonly ReadOnlyCollection<IScriptObject> m_values;
        [NonSerialized]
        private ScriptArray m_underlyingObject;

        private ScriptTuple(SerializationInfo info, StreamingContext context)
        {
            m_values = (ReadOnlyCollection<IScriptObject>)info.GetValue(ValuesSerializationSlot, typeof(ReadOnlyCollection<IScriptObject>));
        }

        private ScriptTuple(IEnumerable<IScriptObject> values)
        {
            m_values = values is ReadOnlyCollection<IScriptObject> ? (ReadOnlyCollection<IScriptObject>)values : Array.AsReadOnly<IScriptObject>(Enumerable.ToArray(values));
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
        public ICollection<IScriptObject> Values
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

        private ScriptArray UnderlyingObject
        {
            get
            {
                if (m_underlyingObject == null)
                    m_underlyingObject = new ScriptArray(ScriptCartesianContract.ContractBinding, Values.Count);
                return m_underlyingObject;
            }
        }

        /// <summary>
        /// Gets collection of object slots.
        /// </summary>
        public override ICollection<string> Slots
        {
            get
            {
                return UnderlyingObject.Slots;
            }
        }

        /// <summary>
        /// Gets slot by its name.
        /// </summary>
        /// <param name="slotName"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        public override IRuntimeSlot this[string slotName, InterpreterState state]
        {
            get
            {
                return UnderlyingObject[slotName, state];
            }
        }

        /// <summary>
        /// Returns contract for this tuple.
        /// </summary>
        /// <returns></returns>
        public override IScriptContract GetContractBinding()
        {
            return new ScriptCartesianContract(Enumerable.Select(Values, v => v.GetContractBinding()));
        }

        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue(ValuesSerializationSlot, Values, typeof(ReadOnlyCollection<IScriptObject>));
        }
    }
}
