﻿using System;
using System.Collections.Generic;
using DynamicScript.Runtime;
using DynamicScript.Runtime.Environment;
using System.Reflection;
using System.Linq;

namespace DynamicScript.Runtime.Environment
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;

    /// <summary>
    /// Represents script-compliant wrapper of the .NET type.
    /// This class cannot be inherited.
    /// </summary>
    [ComVisible(false)]
    sealed class ScriptClass : ScriptContract, IScriptClass, IScriptMetaContract
    {
        #region Nested Types
        /// <summary>
        /// Represents converter between <see cref="System.Type"/> object and
        /// its script wrapper.
        /// This class cannot be inherited.
        /// </summary>
        [ComVisible(false)]
        private sealed class TypeConverter : RuntimeConverter<Type>
        {
            public override bool Convert(Type input, out IScriptObject result)
            {
                result = (ScriptClass)input;
                return true;
            }
        }
        #endregion
        public static readonly ScriptClass ObjectClass;
        public static readonly ScriptClass StringClass;
        private const BindingFlags MemberFlags = BindingFlags.Static | BindingFlags.Public | BindingFlags.FlattenHierarchy;

        static ScriptClass()
        {
            ObjectClass = new ScriptClass(typeof(object));
            StringClass = new ScriptClass(typeof(string));
            ScriptObject.RegisterConverter<TypeConverter>();
        }

        /// <summary>
        /// Represents wrapped .NET type.
        /// </summary>
        public readonly Type NativeType;
        private IScriptGeneric m_generic;

        private ScriptClass(Type nt)
        {
            if (nt == null) throw new ArgumentNullException("nt");
            NativeType = nt;
        }

        /// <summary>
        /// Converts an array of .NET types to an array of script wrappers.
        /// </summary>
        /// <param name="types"></param>
        /// <returns></returns>
        public static IScriptClass[] ToArray(Type[] types)
        {
            return Array.ConvertAll(types ?? new Type[0], t => (ScriptClass)t);
        }

        public static IEnumerable<IScriptClass> ToCollection(IEnumerable<Type> types)
        {
            return from t in types??Enumerable.Empty<Type>() select (ScriptClass)t;
        }

        public static Type GetType(IScriptContract contract)
        {
            if (contract is ScriptSuperContract)
                return typeof(object);
            else if (contract is ScriptVoid)
                return typeof(void);
            else if (contract is ScriptIntegerContract)
                return typeof(long);
            else if (contract is ScriptBooleanContract)
                return typeof(bool);
            else if (contract is ScriptRealContract)
                return typeof(double);
            else if (contract is ScriptMetaContract)
                return typeof(Type);
            else if (contract is ScriptCallableContract)
                return typeof(Delegate);
            else if (contract is ScriptStringContract)
                return typeof(string);
            else if (contract is IScriptClass)
                return ((IScriptClass)contract).NativeType;
            else return null;
        }

        /// <summary>
        /// Returns relationship with other .NET type.
        /// </summary>
        /// <param name="class"></param>
        /// <returns></returns>
        public ContractRelationshipType GetRelationship(IScriptClass @class)
        {
            return ReflectionEngine.GetRelationship(NativeType, @class.NativeType);
        }

        public static ContractRelationshipType GetRelationship(IScriptClass[] source, IScriptClass[] destination)
        {
            var relationship = ContractRelationshipType.None;
            for (var i = 0L; i < Math.Min(source.LongLength, destination.LongLength); i++)
            {
                var subrels = source[i].GetRelationship(destination[i]);
                if (subrels == ContractRelationshipType.None) return ContractRelationshipType.None;
                else if (relationship == ContractRelationshipType.None || relationship == ContractRelationshipType.TheSame)
                    relationship = subrels;
                else if (subrels == relationship || relationship == ContractRelationshipType.TheSame || relationship == ContractRelationshipType.None)
                    relationship = subrels;
                else return ContractRelationshipType.None;
            }
            if (source.LongLength == destination.LongLength)
                return relationship;
            else if (source.LongLength > destination.LongLength)
                return relationship == ContractRelationshipType.TheSame ? ContractRelationshipType.Subset : relationship;
            else return relationship == ContractRelationshipType.TheSame ? ContractRelationshipType.Superset : relationship;
        }

        /// <summary>
        /// Returns a relationship
        /// </summary>
        /// <param name="contract"></param>
        /// <returns></returns>
        public override ContractRelationshipType GetRelationship(IScriptContract contract)
        {
            if (contract is IScriptClass)
                return GetRelationship((IScriptClass)contract);
            else if (contract.OneOf<ScriptMetaContract, ScriptSuperContract>())
                return ContractRelationshipType.Subset;
            else if (contract is ScriptVoid)
                return ContractRelationshipType.Superset;
            else if (contract.OneOf<IScriptComplementation, IScriptUnionContract>())
                return Inverse(contract.GetRelationship(this));
            else return ContractRelationshipType.None;
        }

        /// <summary>
        /// Creates a new instance of the native .NET object.
        /// </summary>
        /// <param name="args">Constructor arguments.</param>
        /// <param name="state">Internal interpreter state.</param>
        /// <returns>A new instance of the native .NET object.</returns>
        public override IScriptObject CreateObject(IList<IScriptObject> args, InterpreterState state)
        {
            return NativeObject.New(args, NativeType, state);
        }

        /// <summary>
        /// Returns a new contract binding.
        /// </summary>
        /// <returns></returns>
        public override IScriptContract GetContractBinding()
        {
            if (m_generic == null) m_generic = new ScriptGeneric(this);
            return m_generic;
        }

        Type IScriptClass.NativeType
        {
            get { return NativeType; }
        }

        public static explicit operator ScriptClass(Type t)
        {
            if (t == null)
                return null;
            else if (Equals(typeof(object), t))
                return ObjectClass;
            else if (Equals(typeof(string), t))
                return StringClass;
            else return new ScriptClass(t);
        }

        /// <summary>
        /// Returns a string representation of the native .NET class.
        /// </summary>
        /// <returns>A string representation of the native .NET class.</returns>
        public override string ToString()
        {
            return NativeType.FullName;
        }

        /// <summary>
        /// Gets collection of static members.
        /// </summary>
        public override ICollection<string> Slots
        {
            get { return ReflectionEngine.GetMemberNames(NativeType, MemberFlags); }
        }

        /// <summary>
        /// Exposes access to the static member.
        /// </summary>
        /// <param name="slotName"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        public override IRuntimeSlot this[string slotName, InterpreterState state]
        {
            get
            {
                var member = NativeType.GetMember(slotName, MemberFlags);
                return member != null && member.LongLength > 0L ?
                    new ScriptMember(member) :
                    RuntimeSlotBase.Missing(slotName);
            }
        }
    }
}
