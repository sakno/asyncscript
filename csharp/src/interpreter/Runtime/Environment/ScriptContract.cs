using System;
using System.Collections.Generic;
using System.Linq;

namespace DynamicScript.Runtime.Environment
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;
    using LinqExpression = System.Linq.Expressions.Expression;
    using BindingRestrictions = System.Dynamic.BindingRestrictions;
    using InterpretationContext = Compiler.Ast.InterpretationContext;
    using QCodeBinaryOperatorType = Compiler.Ast.ScriptCodeBinaryOperatorType;
    using QCodeUnaryOperatorType = Compiler.Ast.ScriptCodeUnaryOperatorType;
    using TypeConverterAttribute = System.ComponentModel.TypeConverterAttribute;
    using Operator = Compiler.Operator;

    /// <summary>
    /// Represents DynamicScript contract at runtime.
    /// </summary>
    [ComVisible(false)]
    [TypeConverter(typeof(ContractConverter))]
    public abstract class ScriptContract : ScriptObject, IScriptContract
    {
        #region Nested Types
        /// <summary>
        /// Represents definition for iterable contract.
        /// This class cannot be inherited.
        /// </summary>
        [ComVisible(false)]
        internal sealed class ScriptIterableContractDef
        {
            private readonly IScriptContract m_contract;

            public ScriptIterableContractDef(IScriptContract contract)
            {
                m_contract = contract;
            }

            public IScriptContract MakeIterable(Func<IScriptContract, IScriptContract> converter)
            {
                if (converter == null) throw new ArgumentNullException("converter");
                return converter.Invoke(m_contract);
            }
        }

        [ComVisible(false)]
        private sealed class ScriptUnion : ScriptContract, IScriptUnionContract
        {
            private readonly IScriptContract m_contract1;
            private readonly IScriptContract m_contract2;

            public ScriptUnion(IScriptContract left, IScriptContract right)
            {
                if (left == null) throw new ArgumentNullException("left");
                if (right == null) throw new ArgumentNullException("right");
                m_contract1 = left;
                m_contract2 = right;
            }

            public override ContractRelationshipType GetRelationship(IScriptContract contract)
            {
                switch (m_contract1.GetRelationship(contract))
                {
                    case ContractRelationshipType.TheSame:
                    case ContractRelationshipType.Superset:
                        return ContractRelationshipType.Superset;
                    case ContractRelationshipType.Subset:
                        return ContractRelationshipType.None;
                }
                switch (m_contract2.GetRelationship(contract))
                {
                    case ContractRelationshipType.TheSame:
                    case ContractRelationshipType.Superset:
                        return ContractRelationshipType.Superset;
                    case ContractRelationshipType.Subset:
                    default:
                        return ContractRelationshipType.None;
                }
            }

            public override IScriptContract GetContractBinding()
            {
                return ScriptMetaContract.Instance;
            }

            private static IEnumerator<IScriptContract> GetEnumerator(IEnumerable<IScriptContract> union)
            {
                foreach (var c in union)
                    if (c is IScriptUnionContract)
                        using (var enumerator = GetEnumerator((IScriptUnionContract)union))
                            while (enumerator.MoveNext())
                                yield return enumerator.Current;
                    else yield return c;
            }

            public new IEnumerator<IScriptContract> GetEnumerator()
            {
                return GetEnumerator(new[] { m_contract1, m_contract2 });
            }

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            public override ScriptObject CreateObject(IList<IScriptObject> args, InterpreterState state)
            {
                if (state.Context == InterpretationContext.Unchecked)
                    return Void;
                else throw new UnsupportedOperationException(state);
            }

            public override string ToString()
            {
                return string.Join<IScriptContract>(Operator.Union, this);
            }
        }

        [ComVisible(false)]
        private sealed class ScriptComplementation : ScriptContract, IScriptComplementation
        {
            public readonly IScriptContract NegatedContract;

            public ScriptComplementation(IScriptContract contract)
            {
                if (contract == null) throw new ArgumentNullException("contract");
                NegatedContract = contract;
            }

            IScriptContract IScriptComplementation.NegatedContract
            {
                get { return NegatedContract; }
            }

            public override ContractRelationshipType GetRelationship(IScriptContract contract)
            {
                //See Set Theory axiomatic
                if (contract is IScriptComplementation)
                    switch (NegatedContract.GetRelationship(((IScriptComplementation)contract).NegatedContract))
                    {
                        case ContractRelationshipType.TheSame: return ContractRelationshipType.TheSame;
                        case ContractRelationshipType.Subset: return ContractRelationshipType.Subset;
                        case ContractRelationshipType.Superset: return ContractRelationshipType.Superset;
                        default: return ContractRelationshipType.None;
                    }
                else if (contract is ScriptSuperContract) return ContractRelationshipType.Subset;
                else if (NegatedContract.GetRelationship(contract) == ContractRelationshipType.TheSame)
                    return ContractRelationshipType.None;
                else return ContractRelationshipType.Superset;
            }

            public override IScriptContract GetContractBinding()
            {
                return ScriptMetaContract.Instance;
            }

            internal override IScriptContract Unite(IScriptContract right, InterpreterState state)
            {
                if (right is IScriptContract)
                    return (right is IScriptComplementation || NegatedContract.GetRelationship(right) != ContractRelationshipType.TheSame) ? (IScriptContract)base.Or(right, state) : ScriptSuperContract.Instance;
                else if (state.Context == InterpretationContext.Unchecked)
                    return Void;
                else throw new UnsupportedOperationException(state);
            }

            public override ScriptObject CreateObject(IList<IScriptObject> args, InterpreterState state)
            {
                if (state.Context == InterpretationContext.Unchecked)
                    return Void;
                else throw new UnsupportedOperationException(state);
            }

            public override string ToString()
            {
                return string.Concat(Operator.Negotiation, NegatedContract);
            }
        }
        #endregion

        /// <summary>
        /// Initializes a new contract.
        /// </summary>
        internal ScriptContract()
        {
        }

        /// <summary>
        /// Creates an object that represents void value according with the contract.
        /// </summary>
        /// <param name="state">State of the interpreter.</param>
        /// <returns>The object that represents void value according with the contract.</returns>
        internal protected virtual ScriptObject FromVoid(InterpreterState state)
        {
            return Void;
        }

        /// <summary>
        /// Transforms the specified object according with the current contract.
        /// </summary>
        /// <param name="value">The value to be converted.</param>
        /// <param name="state">Interpretation state.</param>
        /// <returns>Conversion result.</returns>
        public virtual IScriptObject Convert(IScriptObject value, InterpreterState state)
        {
            switch (Mapping(ref value))
            {
                case true: return value;
                default:
                    switch (state.Context)
                    {
                        case InterpretationContext.Checked:
                            throw new ContractBindingException(value, this, state);
                        default:
                            return FromVoid(state);
                    }
            }
        }

        /// <summary>
        /// Computes a hash code for the contract.
        /// </summary>
        /// <returns>The hash code for the contract.</returns>
        public override int GetHashCode()
        {
            return GetType().MetadataToken;
        }

        /// <summary>
        /// Provides implicit conversion of the specified object.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        protected virtual bool Mapping(ref IScriptObject value)
        {
            var theSame = default(bool);
            return RuntimeHelpers.IsCompatible(this, value, out theSame);
        }

        #region IScriptContract Members

        /// <summary>
        /// Returns relationship between the current contract and the specified.
        /// </summary>
        /// <param name="contract">The contract to compare. Cannot be <see langword="null"/>.</param>
        /// <returns>Relationship between the current contract and <paramref name="contract"/>.</returns>
        public abstract ContractRelationshipType GetRelationship(IScriptContract contract);

        /// <summary>
        /// Determines whether the current contract is equal to another.
        /// </summary>
        /// <param name="other">Other contract to compare.</param>
        /// <returns><see langword="true"/> if the current contract is equal to another; otherwise, <see langword="false"/>.</returns>
        public bool Equals(IScriptContract other)
        {
            return GetRelationship(other) == ContractRelationshipType.TheSame;
        }

        IScriptObject IScriptContract.FromVoid(InterpreterState state)
        {
            return FromVoid(state);
        }

        IScriptObject IScriptContract.Convert(Conversion conv, IScriptObject value, InterpreterState state)
        {
            switch (conv)
            {
                case Conversion.Implicit: return Mapping(ref value) ? value : FromVoid(state);
                case Conversion.Explicit: return Convert(value, state);
                default: throw new UnsupportedOperationException(state);
            }
        }

        #endregion

        /// <summary>
        /// Determines whether the the current object is equal to another.
        /// </summary>
        /// <param name="right">Other object to be compared.</param>
        /// <param name="state">Internal interpreter state.</param>
        /// <returns>The comparison result.</returns>
        protected sealed override IScriptObject Equals(IScriptObject right, InterpreterState state)
        {
            if (right is IScriptContract)
                return Convert<bool>(Equals((IScriptContract)right));
            else if (state.Context == InterpretationContext.Unchecked)
                return Void;
            else throw new UnsupportedOperationException(state);
        }

        /// <summary>
        /// Determines whether the current object is not equal to another.
        /// </summary>
        /// <param name="right">The second operand.</param>
        /// <param name="state">Internal interpreter state.</param>
        /// <returns>The comparison result.</returns>
        protected sealed override IScriptObject NotEquals(IScriptObject right, InterpreterState state)
        {
            if (right is IScriptContract)
                return Convert<bool>(!Equals((IScriptContract)right));
            else if (state.Context == InterpretationContext.Unchecked)
                return Void;
            else throw new UnsupportedOperationException(state);
        }

        private IScriptObject GreaterThan(IScriptContract contract, InterpreterState state)
        {
            switch (GetRelationship(contract))
            {
                case ContractRelationshipType.Superset: return Convert<bool>(true);
                case ContractRelationshipType.TheSame:
                case ContractRelationshipType.Subset:
                default: return Convert<bool>(false);
            }
        }

        /// <summary>
        /// Determines whether the current object is greater than the specified object.
        /// </summary>
        /// <param name="right">The second object to compare.</param>
        /// <param name="state">Internal interpreter state.</param>
        /// <returns><see langword="true"/>the current object is greater than the specified object; otherwise, <see langword="false"/>.</returns>
        protected sealed override IScriptObject GreaterThan(IScriptObject right, InterpreterState state)
        {
            if (right is IScriptContract)
                return GreaterThan((IScriptContract)right, state);
            else if (state.Context == InterpretationContext.Unchecked)
                return Void;
            else throw new UnsupportedOperationException(state);
        }

        private IScriptObject GreaterThanOrEqual(IScriptContract contract, InterpreterState state)
        {
            switch (GetRelationship(contract))
            {
                case ContractRelationshipType.Superset:
                case ContractRelationshipType.TheSame: return Convert<bool>(true);
                case ContractRelationshipType.Subset:
                default: return Convert<bool>(false);
            }
        }

        /// <summary>
        /// Determines whether the current object is greater than or equal to the specified object.
        /// </summary>
        /// <param name="right">The second object to compare.</param>
        /// <param name="state">Internal interpreter state.</param>
        /// <returns><see langword="true"/>the current object is greater than or equal to the specified object; otherwise, <see langword="false"/>.</returns>
        protected sealed override IScriptObject GreaterThanOrEqual(IScriptObject right, InterpreterState state)
        {
            if (right is IScriptContract)
                return GreaterThanOrEqual((IScriptContract)right, state);
            else if (state.Context == InterpretationContext.Unchecked)
                return Void;
            else throw new UnsupportedOperationException(state);
        }

        private IScriptObject LessThan(IScriptContract contract, InterpreterState state)
        {
            switch (GetRelationship(contract))
            {
                case ContractRelationshipType.Subset: return Convert<bool>(true);
                case ContractRelationshipType.Superset:
                case ContractRelationshipType.TheSame: return Convert<bool>(false);
                default: return Convert<bool>(false);
            }
        }

        /// <summary>
        /// Determines whether the current object is less than the specified object.
        /// </summary>
        /// <param name="right">The second object to compare.</param>
        /// <param name="state">Internal interpreter state.</param>
        /// <returns><see langword="true"/>the current object is less than the specified object; otherwise, <see langword="false"/>.</returns>
        protected override IScriptObject LessThan(IScriptObject right, InterpreterState state)
        {
            if (right is IScriptContract)
                return LessThan((IScriptContract)right, state);
            else if (state.Context == InterpretationContext.Unchecked)
                return Void;
            else throw new UnsupportedOperationException(state);
        }

        private IScriptObject LessThanOrEqual(IScriptContract contract, InterpreterState state)
        {
            switch (GetRelationship(contract))
            {
                case ContractRelationshipType.Subset:
                case ContractRelationshipType.TheSame: return Convert<bool>(true);
                case ContractRelationshipType.Superset:
                default: return Convert<bool>(false);
            }
        }

        /// <summary>
        /// Determines whether the current object is less than or equal to the specified object.
        /// </summary>
        /// <param name="right">The second object to compare.</param>
        /// <param name="state">Internal interpreter state.</param>
        /// <returns><see langword="true"/>the current object is less than or equal to the specified object; otherwise, <see langword="false"/>.</returns>
        protected sealed override IScriptObject LessThanOrEqual(IScriptObject right, InterpreterState state)
        {
            if (right is IScriptContract)
                return LessThanOrEqual((IScriptContract)right, state);
            else if (state.Context == InterpretationContext.Unchecked)
                return Void;
            else throw new UnsupportedOperationException(state);
        }

        /// <summary>
        /// Unites the current contract with the specified contract.
        /// </summary>
        /// <param name="right"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        protected sealed override IScriptObject Or(IScriptObject right, InterpreterState state)
        {
            return right is IScriptContract ? Unite((IScriptContract)right, state) : right.Or(this, state);
        }

        internal virtual IScriptContract Unite(IScriptContract right, InterpreterState state)
        {
            var rels = GetRelationship(right);
            switch (rels)
            {
                case ContractRelationshipType.Superset:
                case ContractRelationshipType.TheSame: return this;
                case ContractRelationshipType.Subset: return right;
                case ContractRelationshipType.None: return new ScriptUnion(this, right);
                default: return Void;
            }
        }

        /// <summary>
        /// Intersects the current contract with the specified.
        /// </summary>
        /// <param name="right"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        protected sealed override IScriptObject And(IScriptObject right, InterpreterState state)
        {
            return right is IScriptContract ? Intersect((IScriptContract)right, state) : right.And(this, state);
        }

        internal virtual IScriptContract Intersect(IScriptContract right, InterpreterState state)
        {
            var rels = GetRelationship(right);
            switch (rels)
            {
                case ContractRelationshipType.Superset: return right;
                case ContractRelationshipType.Subset: return this;
                case ContractRelationshipType.None:
                default: return Void;
            }
        }

        /// <summary>
        /// Returns superset contract.
        /// </summary>
        /// <param name="state"></param>
        /// <returns></returns>
        protected sealed override IScriptObject Not(InterpreterState state)
        {
            return this is IScriptComplementation ? ((IScriptComplementation)this).NegatedContract : new ScriptComplementation(this);
        }

        internal virtual IScriptObject Complement(InterpreterState state)
        {
            return this is IScriptComplementation ? ((IScriptComplementation)this).NegatedContract : new ScriptComplementation(this);
        }

        private IScriptContract Cartesian(IScriptContract right, InterpreterState state)
        {
            return ScriptTuple.GetContractBinding(this, right);
        }

        /// <summary>
        /// Computes cartesian products of this contract with the specified contract.
        /// </summary>
        /// <param name="right"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        protected sealed override IScriptObject Multiply(IScriptObject right, InterpreterState state)
        {
            if (right is IScriptContract)
                return Cartesian((IScriptContract)right, state);
            else if (state.Context == InterpretationContext.Unchecked)
                return Void;
            else throw new UnsupportedOperationException(state);
        }

        private IScriptContract RelativeComplement(IScriptContract right, InterpreterState state)
        {
            switch (GetRelationship(right))
            {
                case ContractRelationshipType.Superset: return Unite((IScriptContract)right.UnaryOperation(QCodeUnaryOperatorType.Negate, state), state);
                case ContractRelationshipType.None: return this;
                default: return Void;
            }
        }

        private IScriptContract SymmetricDifference(IScriptContract right, InterpreterState state)
        {
            var left = RelativeComplement(right, state);
            right = (IScriptContract)right.BinaryOperation(QCodeBinaryOperatorType.Subtract, this, state);
            return (IScriptContract)left.BinaryOperation(QCodeBinaryOperatorType.Union, right, state);
        }

        /// <summary>
        /// Computes symmetric difference between this contract and the specified contract.
        /// </summary>
        /// <param name="right"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        protected sealed override IScriptObject ExclusiveOr(IScriptObject right, InterpreterState state)
        {
            if (right is IScriptContract)
                return SymmetricDifference((IScriptContract)right, state);
            else if (state.Context == InterpretationContext.Unchecked)
                return Void;
            else throw new UnsupportedOperationException(state);
        }

        /// <summary>
        /// Computes relative complement.
        /// </summary>
        /// <param name="right"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        protected sealed override IScriptObject Subtract(IScriptObject right, InterpreterState state)
        {
            if (right is IScriptContract)
                return RelativeComplement((IScriptContract)right, state);
            else if (state.Context == InterpretationContext.Unchecked)
                return Void;
            else throw new UnsupportedOperationException(state);
        }

        internal static IScriptContract Unite(IEnumerable<IScriptContract> contracts, InterpreterState state)
        {
            return (IScriptContract)ScriptObject.Unite(contracts, state);
        }

        internal static IScriptContract Unite(IEnumerable<IScriptContract> set1, IEnumerable<IScriptContract> set2, InterpreterState state)
        {
            return (IScriptContract)ScriptObject.Unite(set1, set2, state);
        }

        internal static IScriptContract Intersect(IEnumerable<IScriptContract> contracts, InterpreterState state)
        {
            return (IScriptContract)ScriptObject.Intersect(contracts, state);
        }

        internal static IScriptContract Intersect(IEnumerable<IScriptContract> set1, IEnumerable<IScriptContract> set2, InterpreterState state)
        {
            return (IScriptContract)ScriptObject.Intersect(set1, set1, state);
        }

        /// <summary>
        /// Inferts the common contract from the two specified contracts.
        /// </summary>
        /// <param name="contract1"></param>
        /// <param name="contract2"></param>
        /// <returns></returns>
        public static IScriptContract Infer(IScriptContract contract1, IScriptContract contract2)
        {
            if (contract1 == null) contract1 = ScriptSuperContract.Instance;
            if (contract2 == null) contract2 = ScriptSuperContract.Instance;
            switch (contract1.GetRelationship(contract2))
            {
                case ContractRelationshipType.Subset:
                    return contract2;
                case ContractRelationshipType.Superset:
                case ContractRelationshipType.TheSame:
                    return contract1;
                default: return ScriptSuperContract.Instance;
            }
        }

        /// <summary>
        /// Infers the common contract from the specified set of contracts.
        /// </summary>
        /// <param name="contracts"></param>
        /// <returns></returns>
        public static IScriptContract Infer(IEnumerable<IScriptContract> contracts)
        {
            try
            {
                return Enumerable.Aggregate(contracts, Infer);
            }
            catch (InvalidOperationException)
            {
                return ScriptSuperContract.Instance;
            }
        }

        /// <summary>
        /// Inverse relationship type.
        /// </summary>
        /// <param name="relationship">The relationship to inverse.</param>
        /// <returns>Inversed relationship type.</returns>
        /// <remarks>This method is invertible to itself, for example Inverse(Inverse(A)) == A.</remarks>
        protected static ContractRelationshipType Inverse(ContractRelationshipType relationship)
        {
            switch (relationship)
            {
                case ContractRelationshipType.Subset: return ContractRelationshipType.Superset;
                case ContractRelationshipType.Superset: return ContractRelationshipType.Subset;
                default: return relationship;
            }
        }

        /// <summary>
        /// Creates an object satisfied to this contract.
        /// </summary>
        /// <param name="args"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        public sealed override IScriptObject Invoke(IList<IScriptObject> args, InterpreterState state)
        {
            return CreateObject(args, state);
        }

        /// <summary>
        /// Creates an object satisfied to this contract.
        /// </summary>
        /// <param name="args"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        public abstract ScriptObject CreateObject(IList<IScriptObject> args, InterpreterState state);

        /// <summary>
        /// Converts an object to the contract.
        /// </summary>
        /// <param name="obj">The object to be converted.</param>
        /// <returns>The conversion result.</returns>
        public static IScriptContract RtlExtractContract(object obj)
        {
            switch (obj is IScriptContract)
            {
                case true: return (IScriptContract)obj;
                default: throw new ContractExpectedException();
            }
        }

        internal static LinqExpression Extract(LinqExpression contract)
        {
            return typeof(IScriptContract).IsAssignableFrom(contract.Type) ? contract : LinqHelpers.Call<object, IScriptContract>(obj => RtlExtractContract(obj), null, contract);
        }

        private static IScriptContract AsIterable(IScriptContract contract)
        {
            return Convert(new ScriptIterableContractDef(contract)) as IScriptContract ?? Void;
        }

        /// <summary>
        /// Returns a new iterable contract constructed from this contract.
        /// </summary>
        /// <param name="state">Internal interpreter state.</param>
        /// <returns></returns>
        protected sealed override IScriptObject PostSquareAssign(InterpreterState state)
        {
            return AsIterable(this);
        }

        /// <summary>
        /// Returns a new iterable contract constructed from this contract.
        /// </summary>
        /// <param name="state">Internal interpreter state.</param>
        /// <returns></returns>
        protected sealed override IScriptObject PreSquareAssign(InterpreterState state)
        {
            return AsIterable(this);
        }
    }
}
