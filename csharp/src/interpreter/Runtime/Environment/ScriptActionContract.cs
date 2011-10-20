using System;
using System.Dynamic;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Collections.ObjectModel;
using System.Linq;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace DynamicScript.Runtime.Environment
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;
    using Compiler;
    using LinqExpression = System.Linq.Expressions.Expression;
    using StringBuilder = System.Text.StringBuilder;
    using InterpretationContext = Compiler.Ast.InterpretationContext;
    using Enumerable = System.Linq.Enumerable;
    
    /// <summary>
    /// Represents action signature.
    /// </summary>
    [ComVisible(false)]
    [Serializable]
    public class ScriptActionContract : ScriptContract, ISerializable, IEquatable<ScriptActionContract>, IScriptActionContractSlots
    {
        #region Nested Types

        /// <summary>
        /// Represents action parameter.
        /// This class cannot be inherited.
        /// </summary>
        [ComVisible(false)]
        [Serializable]
        public sealed class Parameter: IEquatable<Parameter>, IEquatable<string>
        {
            private readonly string m_name;
            private readonly IScriptContract m_contract;

            /// <summary>
            /// Initializes a new information about action parameter.
            /// </summary>
            /// <param name="paramName">The name of the parameter. Cannot be <see langword="null"/> or empty.</param>
            /// <param name="paramContract">Parameter contract binding.</param>
            /// <exception cref="System.ArgumentNullException"><paramref name="paramName"/> is  <see langword="null"/> or empty.</exception>
            public Parameter(string paramName, IScriptContract paramContract = null)
            {
                if (string.IsNullOrEmpty(paramName)) throw new ArgumentNullException("paramName");
                m_contract = paramContract ?? ScriptSuperContract.Instance;
                m_name = paramName;
            }

            /// <summary>
            /// Gets name of the parameter.
            /// </summary>
            public string Name
            {
                get { return m_name; }
            }

            /// <summary>
            /// Gets contract binding of the parameter.
            /// </summary>
            public IScriptContract ContractBinding
            {
                get { return m_contract; }
            }

            #region IEquatable<Parameter> Members

            /// <summary>
            /// Determines whether the current object represents the same parameter
            /// as other.
            /// </summary>
            /// <param name="other">Other parameter to compare.</param>
            /// <returns><see langword="true"/> if the current object represents the same parameter
            /// as other; otherwise, <see langword="false"/>.</returns>
            public bool Equals(Parameter other)
            {
                return other != null ? Equals(other.Name) : false;
            }

            #endregion

            #region IEquatable<string> Members

            /// <summary>
            /// Determines whether the parameter has the same name as the specified.
            /// </summary>
            /// <param name="paramName">The name of the parameter to compare.</param>
            /// <returns><see langword ="true"/> if the parameter has the same name as the specified;
            /// otherwise, <see langword="false"/>.</returns>
            public bool Equals(string paramName)
            {
                return StringEqualityComparer.Equals(Name, paramName);
            }

            #endregion

            /// <summary>
            /// Returns a hash code for the parameter.
            /// </summary>
            /// <returns>The hash code for the parameter.</returns>
            public override int GetHashCode()
            {
                return StringEqualityComparer.GetHashCode(Name);
            }

            /// <summary>
            /// Returns a string representation of the parameter.
            /// </summary>
            /// <returns>The string that represents the parameter.</returns>
            public override string ToString()
            {
                return Name;
            }

            /// <summary>
            /// Determines whether the current object represents the same parameter
            /// as other.
            /// </summary>
            /// <param name="other">Other parameter to compare.</param>
            /// <returns><see langword="true"/> if the current object represents the same parameter
            /// as other; otherwise, <see langword="false"/>.</returns>
            public override bool Equals(object other)
            {
                if (other is string) return Equals((string)other);
                else if (other is Parameter) return Equals((Parameter)other);
                else return false;
            }

            internal static NewExpression Bind(ConstantExpression paramName, Expression contractBinding)
            {
                contractBinding = Extract(contractBinding);
                var ctor = LinqHelpers.BodyOf<string, IScriptContract, Parameter, NewExpression>((p, c) => new Parameter(p, c));
                return ctor.Update(new LinqExpression[] { paramName, contractBinding });
            }

            internal static NewExpression Bind(string paramName, Expression contractBinding)
            {
                return Bind(LinqHelpers.Constant<string>(paramName), contractBinding);
            }
        }

        /// <summary>
        /// Represents action signature.
        /// </summary>
        [ComVisible(false)]
        [Serializable]
        protected class Signature : KeyedCollection<string, Parameter>
        {
            private readonly IScriptContract m_return;

            /// <summary>
            /// Initializes a new signature.
            /// </summary>
            /// <param name="params">The collection of parameters to be added to the signature.</param>
            /// <param name="return">The contract of the returning value.</param>
            public Signature(IEnumerable<Parameter> @params = null, IScriptContract @return = null)
                : base(new StringEqualityComparer(), 0)
            {
                if (@params == null) @params = new Parameter[0];
                m_return = @return ?? Void;
                foreach (var p in @params)
                    switch (p == null)
                    {
                        case true: throw new ArgumentException();
                        default: Add(p); continue;
                    }
            }

            /// <summary>
            /// Gets contract of the return value.
            /// </summary>
            /// <remarks>If it is <see langword="null"/> then action doesn't return value.</remarks>
            public IScriptContract ReturnValueContract
            {
                get { return m_return; }
            }

            /// <summary>
            /// Extracts parameter name.
            /// </summary>
            /// <param name="p">The parameter of the action.</param>
            /// <returns>The parameter name.</returns>
            protected override string GetKeyForItem(Parameter p)
            {
                return p != null ? p.Name : string.Empty;
            }
        }
        #endregion

        private const string SignatureHolder = "Signature";
        private readonly Signature m_signature;
        private ReadOnlyCollection<Parameter> m_parameters;
#if USE_REL_MATRIX
        private int? m_hashCode;
#endif
        
        private IRuntimeSlot m_ret;

        private ScriptActionContract(Signature sig, bool copy = false)
        {
            if (sig == null) throw new ArgumentNullException("sig");
            m_signature = copy ? new Signature(sig, sig.ReturnValueContract) : sig;
        }

        /// <summary>
        /// Deserializes action contract.
        /// </summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        protected ScriptActionContract(SerializationInfo info, StreamingContext context)
            : this((Signature)info.GetValue(SignatureHolder, typeof(Signature)))
        {
        }

        /// <summary>
        /// Initializes a new action contract.
        /// </summary>
        /// <param name="parameters">The parameters of the action.</param>
        /// <param name="returnValue">The contract of the return value. Can be <see langword="null"/> if action doens't return any value.</param>
        public ScriptActionContract(IEnumerable<Parameter> parameters, IScriptContract returnValue = null)
            : this(new Signature(parameters, returnValue))
        {
        }

        /// <summary>
        /// Initializes a new action contract.
        /// </summary>
        /// <param name="parameters">The action parameters.</param>
        /// <param name="returnValue">The contract of the return value. Can be <see langword="null"/> if action doens't return any value.</param>
        public ScriptActionContract(IEnumerable<KeyValuePair<ParameterExpression, IScriptContract>> parameters, IScriptContract returnValue = null)
            : this(parameters.Select(p => new Parameter(p.Key.Name, p.Value)), returnValue)
        {
        }

        /// <summary>
        /// Gets contract of the return value.
        /// </summary>
        /// <remarks>If it is <see langword="null"/> then action doesn't return value.</remarks>
        public IScriptContract ReturnValueContract
        {
            get { return m_signature.ReturnValueContract; }
        }

        /// <summary>
        /// Gets parameters of the action.
        /// </summary>
        public ReadOnlyCollection<Parameter> Parameters
        {
            get 
            {
                if (m_parameters == null) m_parameters = new ReadOnlyCollection<Parameter>(m_signature);
                return m_parameters;
            }
        }

        /// <summary>
        /// Gets parameter by its name.
        /// </summary>
        /// <param name="paramName">The name of the parameter.</param>
        /// <returns>The parameter description.</returns>
        public Parameter GetParameterByName(string paramName)
        {
            return m_signature[paramName];
        }

        /// <summary>
        /// Returns an underlying contract for the current contract.
        /// </summary>
        /// <returns>The underlying contract for the current contract.</returns>
        public sealed override IScriptContract GetContractBinding()
        {
            return ScriptCallableContract.Instance;
        }

        /// <summary>
        /// Represents an empty set of the action parameters.
        /// </summary>
        protected static readonly IEnumerable<Parameter> EmptyParameters = new Parameter[0];

        /// <summary>
        /// Creates an empty implementation of the action.
        /// </summary>
        /// <param name="state">Internal interpreter state.</param>
        /// <returns>An empty implementation of the action.</returns>
        internal protected sealed override ScriptObject FromVoid(InterpreterState state)
        {
            return Void;
        }

        #region Runtime Helpers

        /// <summary>
        /// Creates a new action contract.
        /// </summary>
        /// <param name="signature">The signature of the action.</param>
        /// <param name="returnValue">The contract of the returning value.</param>
        /// <returns>A new action contract.</returns>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static ScriptActionContract RtlCreate(IEnumerable<Parameter> signature, IScriptContract returnValue)
        {
            return new ScriptActionContract(signature, returnValue);
        }
        #endregion

        internal static NewExpression Bind(NewArrayExpression signature, Expression @return)
        {
            @return = Extract(@return);
            var ctor = LinqHelpers.BodyOf<IEnumerable<Parameter>, IScriptContract, ScriptActionContract, NewExpression>((ps, c) => new ScriptActionContract(ps, c));
            return ctor.Update(new LinqExpression[] { signature, @return });
        }

        /// <summary>
        /// Serializes the action contract.
        /// </summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        protected virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue(SignatureHolder, m_signature, typeof(Signature));
        }

        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
        {
            GetObjectData(info, context);
        }

        /// <summary>
        /// Determines whether the current contract is equal to another.
        /// </summary>
        /// <param name="other">Other contract to compare.</param>
        /// <returns><see langword="true"/> if the current contract is equal to another; otherwise, <see langword="false"/>.</returns>
        public bool Equals(ScriptActionContract other)
        {
            return other != null && 
                Equals(ReturnValueContract, other.ReturnValueContract) && 
                Enumerable.SequenceEqual(Parameters, other.Parameters);
        }

        /// <summary>
        /// Determines whether action satisfied to this contract can be composed with the action
        /// satisfied to the specified contract.
        /// </summary>
        /// <param name="contract">The contract to check. Cannot be <see langword="null"/>.</param>
        /// <returns><see langword="true"/> if action satisfied to this contract can be composed with the action
        /// satisfied to the specified contract; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="System.ArgumentNullException"><paramref name="contract"/> is <see langword="null"/>.</exception>
        public bool IsComposable(ScriptActionContract contract)
        {
            if (contract == null) throw new ArgumentNullException("contract");
            if (contract.Parameters.Count == 0)
                return true;
            else switch (ReturnValueContract.GetRelationship(contract.Parameters[0].ContractBinding))
                {
                    case ContractRelationshipType.TheSame:
                    case ContractRelationshipType.Superset: return true;
                    default: return false;
                }
        }

        /// <summary>
        /// Provides implicit conversion.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        protected sealed override bool Mapping(ref IScriptObject value)
        {
            switch (GetRelationship(value.GetContractBinding()))
            {
                case ContractRelationshipType.Superset:
                case ContractRelationshipType.TheSame: return true;
                default: return false;
            };
        }

        /// <summary>
        /// Provides explicit conversion.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        public sealed override IScriptObject Convert(IScriptObject value, InterpreterState state)
        {
            if (Mapping(ref value))
                return value;
            else if (Parameters.Count == 0)
                switch (ReturnValueContract.GetRelationship(value.GetContractBinding()))
                {
                    case ContractRelationshipType.Superset:
                    case ContractRelationshipType.TheSame: return ScriptRuntimeAction.CreateConstantLambda(value);
                    default: break;
                }
            if (state.Context == InterpretationContext.Unchecked)
                return Void;
            else throw new UnsupportedOperationException(state);
        }

        private static ContractRelationshipType ComputeContravariance(IList<Parameter> left, IList<Parameter> right)
        {
            var result = ContractRelationshipType.TheSame;
            for (var i = 0; i < left.Count; i++)
            {
                //contravariance detection. Contravariance inverse morphism direction. For more info, see contravariance in Category Theory
                var rels = Inverse(left[i].ContractBinding.GetRelationship(right[i].ContractBinding));
                switch (rels)
                {
                    case ContractRelationshipType.TheSame: continue;
                    case ContractRelationshipType.None: return ContractRelationshipType.None;
                    default: if (result == ContractRelationshipType.TheSame || result == rels) result = rels;
                        else return ContractRelationshipType.None;
                        continue;
                }
            }
            return result;
        }

        private static ContractRelationshipType ComputeContravariance(dynamic twoListOfParams)
        {
            return ComputeContravariance(twoListOfParams.Left, twoListOfParams.Right);
        }

        private static ContractRelationshipType ComputeCovariance(IScriptContract left, IScriptContract right)
        {
            if (IsVoid(left))
                return IsVoid(right) ? ContractRelationshipType.TheSame : ContractRelationshipType.Superset;
            else return left.GetRelationship(right);
        }

        /// <summary>
        /// Returns relationship between the current contract and the specified.
        /// </summary>
        /// <param name="other">The contract to compare. Cannot be <see langword="null"/>.</param>
        /// <returns>Relationship between the current contract and <paramref name="other"/>.</returns>
        public ContractRelationshipType GetRelationship(ScriptActionContract other)
        {
            switch (Parameters.Count == other.Parameters.Count)
            {
                case true:
                    var contravariance = Task.Factory.StartNew<ContractRelationshipType>(ComputeContravariance, new { Left = Parameters, Right = other.Parameters });
                    var covariance = ComputeCovariance(ReturnValueContract, other.ReturnValueContract);
                    switch (covariance)
                    {
                        case ContractRelationshipType.TheSame: return contravariance.Result;
                        case ContractRelationshipType.None: return ContractRelationshipType.None;
                        default:
                            return contravariance.Result == ContractRelationshipType.TheSame || contravariance.Result == covariance ? covariance : ContractRelationshipType.None;
                    }
                default: return ContractRelationshipType.None;
            }

        }

        /// <summary>
        /// Returns relationship between the current contract and the specified.
        /// </summary>
        /// <param name="contract">The contract to compare. Cannot be <see langword="null"/>.</param>
        /// <returns>Relationship between the current contract and <paramref name="contract"/>.</returns>
        public sealed override ContractRelationshipType GetRelationship(IScriptContract contract)
        {
            if (contract is ScriptActionContract)
                return GetRelationship((ScriptActionContract)contract);
            else if (contract.OneOf<ScriptSuperContract, ScriptCallableContract>())
                return ContractRelationshipType.Subset;
            else if (contract is ScriptVoid)
                return ContractRelationshipType.Superset;
            else if (contract.OneOf<IScriptComplementation, IScriptUnionContract, IScriptCartesianProduct>())
                return Inverse(contract.GetRelationship(this));
            else return ContractRelationshipType.None;
        }

        /// <summary>
        /// Returns a string representation of the action contract.
        /// </summary>
        /// <returns>The string representation of the action contract.</returns>
        public sealed override string ToString()
        {
            const string ContractFormat = "@{0} -> {1}";
            return String.Format(ContractFormat, Parameters.Count > 0 ? string.Join<string>(Punctuation.Comma, Parameters.Select(t => string.Concat(t.Name, Punctuation.Colon, t.ContractBinding))) : Keyword.Void,
                ReturnValueContract, ContractFormat);
        }

        /// <summary>
        /// Computes hash code of this contract.
        /// </summary>
        /// <returns></returns>
        public sealed override int GetHashCode()
        {
#if USE_REL_MATRIX
            if (m_hashCode == null) m_hashCode = m_signature.GetHashCode() << 1 ^ ReturnValueContract.GetHashCode();
            return m_hashCode.Value;
#else
            return m_signature.GetHashCode() << 1 ^ ReturnValueContract.GetHashCode();
#endif
        }

        /// <summary>
        /// Creates a new empty implementation.
        /// </summary>
        /// <param name="args"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        public sealed override ScriptObject CreateObject(IList<IScriptObject> args, InterpreterState state)
        {
            return ScriptRuntimeAction.CreateEmptyImplementation(this);
        }

        IRuntimeSlot IScriptActionContractSlots.Ret
        {
            get { return CacheConst(ref m_ret, () => ReturnValueContract); }
        }

        private RuntimeSlotBase GetParameterByName(ScriptString paramName, InterpreterState state)
        {
            var p = GetParameterByName(paramName);
            return p != null ? new ScriptConstant(p.ContractBinding, state) : RuntimeSlotBase.Missing(paramName);
        }

        /// <summary>
        /// Returns type of the parameter.
        /// </summary>
        /// <param name="args"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        public sealed override RuntimeSlotBase this[IScriptObject[] args, InterpreterState state]
        {
            get
            {
                return args.LongLength == 1L && args[0] is ScriptString ? GetParameterByName((ScriptString)args[0], state) : base[args, state];
            }
        }

        /// <summary>
        /// Creates a new signature and removes the parameter.
        /// </summary>
        /// <param name="parameterName"></param>
        /// <returns></returns>
        public ScriptActionContract RemoveParameter(string parameterName)
        {
            switch (m_signature.Contains(parameterName))
            {
                case true:
                    var result = new ScriptActionContract(m_signature, true);
                    result.m_signature.Remove(parameterName);
                    return result;
                default: return this;
            }
        }

        /// <summary>
        /// Removes the parameter from the signature.
        /// </summary>
        /// <param name="right"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        protected sealed override IScriptObject Modulo(IScriptObject right, InterpreterState state)
        {
            if (right is ScriptString)
                return RemoveParameter((ScriptString)right);
            else if (state.Context == InterpretationContext.Unchecked)
                return Void;
            else throw new UnsupportedOperationException(state);
        }
    }
}
