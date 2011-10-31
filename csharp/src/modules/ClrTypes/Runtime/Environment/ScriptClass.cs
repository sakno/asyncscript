using System;
using System.Collections.Generic;
using DynamicScript.Runtime;
using DynamicScript.Runtime.Environment;
using System.Reflection;

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
        private static readonly ScriptClass ObjectClass;
        private static readonly ScriptClass StringClass;
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

        private ScriptClass(Type nt)
        {
            if (nt == null) throw new ArgumentNullException("nt");
            NativeType = nt;
        }

        /// <summary>
        /// Returns relationship with other .NET type.
        /// </summary>
        /// <param name="class"></param>
        /// <returns></returns>
        public ContractRelationshipType GetRelationship(IScriptClass @class)
        {
            if (Equals(NativeType, @class.NativeType))
                return ContractRelationshipType.TheSame;
            else if (NativeType.IsAssignableFrom(@class.NativeType))
                return ContractRelationshipType.Superset;
            else if (@class.NativeType.IsAssignableFrom(NativeType))
                return ContractRelationshipType.Subset;
            else return ContractRelationshipType.None;
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
            return ScriptMetaContract.Instance;
        }

        Type IScriptClass.NativeType
        {
            get { return NativeType; }
        }

        public static explicit operator ScriptClass(Type t)
        {
            if (Equals(typeof(object), t))
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
