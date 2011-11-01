using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace DynamicScript.Runtime.Environment
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;
    using StringBuilder = System.Text.StringBuilder;

    /// <summary>
    /// Represents generic definition.
    /// This class cannot be inherited.
    /// </summary>
    [ComVisible(false)]
    sealed class ScriptGeneric : ScriptContract, IScriptMetaContract, IScriptGeneric
    {
        public readonly IScriptClass BaseType;
        public readonly IScriptClass[] Interfaces;
        public readonly bool DefaultConstructor;

        public ScriptGeneric(IScriptClass baseType, IEnumerable<IScriptClass> interfaces = null, bool defaultConstructor = false)
        {
            if (baseType.NativeType.IsGenericParameter)
            {
                var genericParam = baseType.NativeType;
                DefaultConstructor = (genericParam.GenericParameterAttributes & GenericParameterAttributes.DefaultConstructorConstraint) != 0;
                BaseType = (genericParam.GenericParameterAttributes & GenericParameterAttributes.NotNullableValueTypeConstraint) != 0 ? (ScriptClass)typeof(ValueType) : (ScriptClass)typeof(object);
                Interfaces = Array.ConvertAll(genericParam.GetGenericParameterConstraints(), t => (ScriptClass)t);
            }
            else
            {
                BaseType = baseType ?? ScriptClass.ObjectClass;
                Interfaces = Enumerable.ToArray(interfaces ?? Enumerable.Empty<IScriptClass>());
                DefaultConstructor = defaultConstructor;
            }
        }

        /// <summary>
        /// Initializes a new generic description.
        /// </summary>
        /// <param name="baseType"></param>
        /// <param name="interfaces"></param>
        /// <param name="defaultConstructor"></param>
        public ScriptGeneric(Type baseType, IEnumerable<Type> interfaces, bool defaultConstructor)
            : this((ScriptClass)baseType, ScriptClass.ToCollection(interfaces), defaultConstructor)
        {
        }

        /// <summary>
        /// Initializes a new generic description.
        /// </summary>
        /// <param name="baseGeneric"></param>
        /// <param name="interfaces"></param>
        /// <param name="defaultConstructor"></param>
        public ScriptGeneric(IScriptGeneric baseGeneric, IEnumerable<IScriptClass> interfaces, bool defaultConstructor)
            : this(baseGeneric.BaseType, Enumerable.Concat(baseGeneric.Interfaces, interfaces??Enumerable.Empty<IScriptClass>()), defaultConstructor)
        {

        }

        public ContractRelationshipType GetRelationship(IScriptGeneric generic)
        {
            var relationship = BaseType.GetRelationship(generic.BaseType);
            switch (relationship)
            {
                case ContractRelationshipType.None: return ContractRelationshipType.None;
                default:
                    var ifacesRels = ScriptClass.GetRelationship(Interfaces, generic.Interfaces);
                    if (relationship == ContractRelationshipType.TheSame)
                        relationship = ifacesRels;
                    else if (relationship != ifacesRels)
                        return ContractRelationshipType.None;
                    break;
            }
            switch (DefaultConstructor)
            {
                case true: return generic.DefaultConstructor ? relationship : ContractRelationshipType.None;
                default: return generic.DefaultConstructor || relationship == ContractRelationshipType.TheSame ? ContractRelationshipType.Subset : relationship;
            }
        }

        public override ContractRelationshipType GetRelationship(IScriptContract contract)
        {
            if (contract is IScriptGeneric)
                return GetRelationship((IScriptGeneric)contract);
            else if (contract.OneOf<ScriptSuperContract, ScriptMetaContract>())
                return ContractRelationshipType.Subset;
            else if (contract is ScriptVoid)
                return ContractRelationshipType.Superset;
            else if (contract.OneOf<IScriptComplementation, IScriptUnionContract>())
                return Inverse(contract.GetRelationship(this));
            else return ContractRelationshipType.TheSame;
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

        /// <summary>
        /// Returns an underlying contract binding.
        /// </summary>
        /// <returns></returns>
        public override IScriptContract GetContractBinding()
        {
            return ScriptMetaContract.Instance;
        }

        IScriptClass IScriptGeneric.BaseType
        {
            get { return BaseType; }
        }

        IScriptClass[] IScriptGeneric.Interfaces
        {
            get { return Interfaces; }
        }

        bool IScriptGeneric.DefaultConstructor
        {
            get { return DefaultConstructor; }
        }

        /// <summary>
        /// Returns a string representation of the generic definition.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            var result = string.Join(", ", Enumerable.Concat(new[] { BaseType }, Interfaces));
            return DefaultConstructor ? string.Concat(result, ", ctor()") : result;
        }
    }
}
