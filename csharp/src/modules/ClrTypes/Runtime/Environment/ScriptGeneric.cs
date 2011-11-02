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
        #region Nested Types
        /// <summary>
        /// Represents converter between <see cref="System.Type"/> object and
        /// its script wrapper.
        /// This class cannot be inherited.
        /// </summary>
        [ComVisible(false)]
        public sealed class GenericConverter : RuntimeConverter<Type>
        {
            public override bool Convert(Type input, out IScriptObject result)
            {
                result = input.IsGenericTypeDefinition ? new ScriptGeneric(input, null, false) : null;
                return result != null;
            }
        }
        #endregion

        public readonly IScriptClass BaseType;
        public readonly IScriptClass[] Interfaces;
        public readonly bool DefaultConstructor;

        public ScriptGeneric(IScriptClass baseType, IEnumerable<IScriptClass> interfaces = null, bool defaultConstructor = false)
        {
            BaseType = baseType ?? ScriptClass.ObjectClass;
            Interfaces = Enumerable.ToArray(interfaces ?? Enumerable.Empty<IScriptClass>());
            DefaultConstructor = defaultConstructor;
        }

        private static IEnumerable<Type> ExtractConstraints(IEnumerable<Type> interfaces)
        {
            foreach(var t in interfaces)
                switch (t.IsGenericTypeDefinition)
                {
                    case true:
                        foreach (var subt in ExtractConstraints(t.GetGenericParameterConstraints()))
                            yield return subt;
                        continue;
                    default: yield return t; continue;
                }
        }

        private static Type ExtractConstraints(Type genericParameter, ref IEnumerable<Type> interfaces, ref bool defaultConstructor)
        {
            defaultConstructor = defaultConstructor || (genericParameter.GenericParameterAttributes & GenericParameterAttributes.DefaultConstructorConstraint) != 0;
            interfaces = Enumerable.Concat(interfaces, ExtractConstraints(genericParameter.GetGenericParameterConstraints()));
            return genericParameter.BaseType;
        }

        /// <summary>
        /// Initializes a new generic description.
        /// </summary>
        /// <param name="baseType"></param>
        /// <param name="interfaces"></param>
        /// <param name="defaultConstructor"></param>
        public ScriptGeneric(Type baseType, IEnumerable<Type> interfaces, bool defaultConstructor)
        {
            if (interfaces == null) interfaces = Enumerable.Empty<Type>();
            if (baseType == null) baseType = typeof(object);
            else if (baseType.IsGenericTypeDefinition)
                BaseType = (ScriptClass)ExtractConstraints(baseType, ref interfaces, ref defaultConstructor);
            else
            {
                BaseType = (ScriptClass)baseType;
                interfaces = ExtractConstraints(interfaces);
            }
            Interfaces = Enumerable.ToArray(ScriptClass.ToCollection(interfaces));
            DefaultConstructor = defaultConstructor;
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
