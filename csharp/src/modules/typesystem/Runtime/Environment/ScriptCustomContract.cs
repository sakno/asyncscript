using System;
using System.Collections.Generic;
using System.Dynamic;

namespace DynamicScript.Runtime.Environment
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;
    using Compiler.Ast;

    /// <summary>
    /// Represents custom contract.
    /// This class cannot be inherited.
    /// </summary>
    [ComVisible(false)]
    sealed class ScriptCustomContract : ScriptContract, IScriptCustomContract, IScriptCustomContractSlots
    {
        #region Nested Types
        /// <summary>
        /// Represents instance of the custom object.
        /// This class cannot be inherited.
        /// </summary>
        [ComVisible(false)]
        private sealed class ScriptCustomObject : DynamicObject, IScriptObject
        {
            public readonly IScriptCustomContract ContractBinding;
            public readonly IScriptObject UnderlyingObject;

            public ScriptCustomObject(IScriptCustomContract contractBinding, IScriptObject underlyingObject)
            {
                ContractBinding = contractBinding;
                UnderlyingObject = underlyingObject;
            }

            public IScriptObject BinaryOperation(ScriptCodeBinaryOperatorType @operator, IScriptObject right, InterpreterState state)
            {
                var overloading = ContractBinding.GetOverloadedOperator(@operator);
                return overloading != null ? overloading.Invoke(new[] { UnderlyingObject, right }, state) : UnderlyingObject.BinaryOperation(@operator, right, state);
            }

            public IScriptObject UnaryOperation(ScriptCodeUnaryOperatorType @operator, InterpreterState state)
            {
                var overloading = ContractBinding.GetOverloadedOperator(@operator);
                return overloading != null ? overloading.Invoke(new[] { UnderlyingObject }, state) : UnderlyingObject.UnaryOperation(@operator, state);
            }

            public IScriptObject Invoke(IList<IScriptObject> args, InterpreterState state)
            {
                var overloading = ContractBinding.OverloadedInvoke;
                return overloading != null ? overloading.Invoke(args, state) : UnderlyingObject.Invoke(args, state);
            }

            public IRuntimeSlot this[string slotName, InterpreterState state]
            {
                get { return UnderlyingObject[slotName, state]; }
            }

            public IScriptObject GetRuntimeDescriptor(string slotName, InterpreterState state)
            {
                return UnderlyingObject.GetRuntimeDescriptor(slotName, state);
            }

            public IRuntimeSlot this[IScriptObject[] args, InterpreterState state]
            {
                get { return UnderlyingObject[args, state]; }
            }

            public ICollection<string> Slots
            {
                get { return UnderlyingObject.Slots; }
            }

            IScriptContract IScriptObject.GetContractBinding()
            {
                return ContractBinding;
            }
        }
        #endregion

        /// <summary>
        /// Represents custom object constructor.
        /// </summary>
        public readonly IScriptObject Constructor;

        public ScriptCustomContract(IScriptObject constructor)
        {
            if (constructor == null) throw new ArgumentNullException("constructor");
            Constructor = constructor;
        }

        public ContractRelationshipType GetRelationship(ScriptCustomContract contract)
        {
            if (ReferenceEquals(this, contract) || ReferenceEquals(Constructor, contract.Constructor))
                return ContractRelationshipType.TheSame;
            else return ContractRelationshipType.None;
        }

        public override ContractRelationshipType GetRelationship(IScriptContract contract)
        {
            if (IsVoid(contract))
                return ContractRelationshipType.Superset;
            else if (contract is ScriptCustomContract)
                return GetRelationship((ScriptCustomContract)contract);
            else if (contract.OneOf<ScriptSuperContract, ScriptMetaContract>())
                return ContractRelationshipType.Superset;
            else return ContractRelationshipType.None;
        }

        public override IScriptObject CreateObject(IList<IScriptObject> args, InterpreterState state)
        {
            return new ScriptCustomObject(this, Constructor.Invoke(args, state));
        }

        public override IScriptContract GetContractBinding()
        {
            return ScriptMetaContract.Instance;
        }

        public IScriptFunction GetOverloadedOperator(ScriptCodeUnaryOperatorType @operator)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets or sets a function that implements invocation operation.
        /// </summary>
        public IScriptFunction OverloadedInvoke
        {
            get;
            set;
        }

        public IScriptFunction GetOverloadedOperator(ScriptCodeBinaryOperatorType @operator)
        {
            throw new NotImplementedException();
        }

        #region Runtime Slots

        IRuntimeSlot IScriptCustomContractSlots.Constructor
        {
            get { return new ScriptConstant(Constructor); }
        }

        #endregion


        public IRuntimeSlot Aggregates
        {
            get { throw new NotImplementedException(); }
        }
    }
}
