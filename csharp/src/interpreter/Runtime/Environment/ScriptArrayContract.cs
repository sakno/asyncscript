using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Runtime.Serialization;

namespace DynamicScript.Runtime.Environment
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;
    using Parallel = System.Threading.Tasks.Parallel;
    using Lexeme = Compiler.Lexeme;
    using SystemConverter = System.Convert;

    /// <summary>
    /// Represents array contract.
    /// This class cannot be inherited.
    /// </summary>
    [Serializable]
    [ComVisible(false)]
    public sealed class ScriptArrayContract : ScriptContract, IScriptArrayContract, ISerializable
    {
        /// <summary>
        /// Represents name of the slot that holds number of dimensions.
        /// </summary>
        public const string RankSlotName = "rank";

        /// <summary>
        /// Represents contract of the rank slot.
        /// </summary>
        public static readonly ScriptContract RankSlotContract = ScriptIntegerContract.Instance;

        /// <summary>
        /// Represents name of the slot that holds linear length.
        /// </summary>
        public const string LengthSlotName = "length";

        /// <summary>
        /// Represents name of the slot that holds dimension size resolver.
        /// </summary>
        public const string UpperBoundSlotName = "upperBound";

        #region Nested Types
        [ComVisible(false)]
        internal sealed class UppedBoundActionContract : ScriptActionContract
        {
            private const string DimensionParameterName = "dimension";

            public UppedBoundActionContract()
                : base(new[] { new Parameter(DimensionParameterName, ScriptIntegerContract.Instance) }, ScriptIntegerContract.Instance)
            {
            }
        }

        [ComVisible(false)]
        private sealed class ArrayContractPrototype : ScriptCompositeContract
        {
            public ArrayContractPrototype(IScriptContract elementContract, long rank)
                : base(Slots(elementContract, rank))
            {
            }

            private static new IEnumerable<KeyValuePair<string, SlotMeta>> Slots(IScriptContract elementContract, long rank)
            {
                yield return ScriptCompositeContract.DefineSlot(RankSlotName, RankSlotContract, true);
                yield return DefineSlot(LengthSlotName, ScriptIntegerContract.Instance, true);
                yield return DefineSlot(SetItemAction, ScriptSetItemAction.GetContractBinding(elementContract, ScriptIntegerContract.Instance.AsArray(rank)));
                yield return DefineSlot(GetItemAction, ScriptGetItemAction.GetContractBinding(elementContract, ScriptIntegerContract.Instance.AsArray(rank)));
                yield return DefineSlot(UpperBoundSlotName, new UppedBoundActionContract());
                yield return DefineSlot(IteratorAction, ScriptIteratorAction.GetContractBinding(elementContract), true);
            }
        }
        #endregion

        private const string RankSerializationSlot = "Rank";
        private const string ElementContractSerializationSlot = "ElementContract";

        private readonly ScriptIntegerContract[] m_indicies;
        private readonly IScriptContract m_elements;
#if USE_REL_MATRIX
        private int? m_hashCode;
#endif

        /// <summary>
        /// Initializes a new array contract.
        /// </summary>
        /// <param name="elementContract">The Contract of each element in array.</param>
        /// <param name="rank">The number of dimensions.</param>
        public ScriptArrayContract(IScriptContract elementContract = null, int rank = 1)
        {
            m_elements = elementContract ?? ScriptSuperContract.Instance;
            m_indicies = ScriptIntegerContract.Instance.AsArray(rank > 0 ? rank : 1);
        }

        private ScriptArrayContract(SerializationInfo info, StreamingContext context)
            : this(info.GetValue(ElementContractSerializationSlot, typeof(IScriptContract)) as IScriptContract, info.GetInt32(RankSerializationSlot))
        {
        }

        /// <summary>
        /// Returns relationship with other array contract.
        /// </summary>
        /// <param name="contract"></param>
        /// <returns></returns>
        public ContractRelationshipType GetRelationship(IScriptArrayContract contract)
        {
            return ElementContract.GetRelationship(contract.ElementContract);
        }

        private static ContractRelationshipType GetRelationship(ArrayContractPrototype prototype, ScriptCompositeContract contract)
        {
            var rels = prototype.GetRelationship(contract);
            return rels == ContractRelationshipType.TheSame ? ContractRelationshipType.Superset : rels;
        }

        /// <summary>
        /// Returns relationship with other composite contract.
        /// </summary>
        /// <param name="contract"></param>
        /// <returns></returns>
        public ContractRelationshipType GetRelationship(ScriptCompositeContract contract)
        {
            return GetRelationship(new ArrayContractPrototype(ElementContract, Rank), contract);
        }

        /// <summary>
        /// Returns relationship with other contract.
        /// </summary>
        /// <param name="contract"></param>
        /// <returns></returns>
        public override ContractRelationshipType GetRelationship(IScriptContract contract)
        {
            if (contract.OneOf<ScriptSuperContract, ScriptDimensionalContract>())
                return ContractRelationshipType.Subset;
            else if (contract is IScriptArrayContract)
                return GetRelationship((IScriptArrayContract)contract);
            else if (contract is ScriptCompositeContract)
                return GetRelationship((ScriptCompositeContract)contract);
            else if (contract.OneOf<IScriptComplementation, IScriptUnionContract, IScriptCartesianProduct>())
                return Inverse(contract.GetRelationship(this));
            else return ContractRelationshipType.None;
        }

        private static long[] CreateLengths(IList<IScriptObject> args, InterpreterState state)
        {
            var lengths = new long[args.Count];    
            for (var i = 0; i < args.Count; i++)
            {
                var temp = args[i];
                switch (ScriptIntegerContract.Convert(ref temp))
                {
                    case true:
                        lengths[i] = SystemConverter.ToInt64(temp);
                        continue;
                    default:
                        throw new ContractBindingException(temp, ScriptIntegerContract.Instance, state);
                }
            }
            return lengths;
        }

        /// <summary>
        /// Creates a new array.
        /// </summary>
        /// <param name="args">An array of length of each dimension.</param>
        /// <param name="state">Internal interpreter state.</param>
        /// <returns></returns>
        public override IScriptObject CreateObject(IList<IScriptObject> args, InterpreterState state)
        {
            if (args.Count != Rank)
                throw new ActionArgumentsMistmatchException(state);
            return new ScriptArray(this, CreateLengths(args, state));
        }

        internal static NewExpression New(Expression contractExpr, ConstantExpression rank)
        {
            contractExpr = Extract(contractExpr);
            var ctor = LinqHelpers.BodyOf<IScriptContract, int, ScriptArrayContract, NewExpression>((c, r) => new ScriptArrayContract(c, r));
            return ctor.Update(new[] { contractExpr, rank });
        }

        internal static NewExpression New(Expression contractExpr, int rank)
        {
            return New(contractExpr, LinqHelpers.Constant(rank));
        }

        /// <summary>
        /// Returns underlying contract of this array contract.
        /// </summary>
        /// <returns></returns>
        public override IScriptContract GetContractBinding()
        {
            return ScriptDimensionalContract.Instance;
        }

        /// <summary>
        /// Gets number of dimensions.
        /// </summary>
        public long Rank
        {
            get { return m_indicies.LongLength; }
        }

        /// <summary>
        /// Gets contract of each element in array.
        /// </summary>
        public IScriptContract ElementContract
        {
            get { return m_elements; }
        }

        internal ScriptContract[] Indicies
        {
            get { return m_indicies; }
        }

        /// <summary>
        /// Returns string representation of this contract.
        /// </summary>
        /// <returns>A string representation of this contract.</returns>
        public override string ToString()
        {
            return string.Concat(ElementContract, Lexeme.LeftSquareBracket, Rank > 1 ? new string(Lexeme.Comma, (int)Rank - 1) : string.Empty, Lexeme.RightSquareBracket);
        }

        /// <summary>
        /// Computes hash code for this contract.
        /// </summary>
        /// <returns>A hash code of this contract.</returns>
        public override int GetHashCode()
        {
#if USE_REL_MATRIX
            if (m_hashCode == null) m_hashCode = ElementContract.GetHashCode() << 1 ^ (int)Rank;
            return m_hashCode.Value;
#else
            return ElementContract.GetHashCode() << 1 ^ (int)Rank;
#endif
        }

        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue(RankSerializationSlot, Rank);
            info.AddValue(ElementContractSerializationSlot, ElementContract, typeof(IScriptContract));
        }
    }
}
