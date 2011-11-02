using System;
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
        public sealed class TypeConverter : RuntimeConverter<Type>
        {
            public override bool Convert(Type input, out IScriptObject result)
            {
                result = input.IsGenericTypeDefinition ? null : (ScriptClass)input;
                return result != null;
            }
        }
        #endregion
        public static readonly ScriptClass ObjectClass;
        public static readonly ScriptClass StringClass;
        private const BindingFlags MemberFlags = BindingFlags.Static | BindingFlags.Public | BindingFlags.FlattenHierarchy | BindingFlags.IgnoreCase;

        static ScriptClass()
        {
            ObjectClass = new ScriptClass(typeof(object));
            StringClass = new ScriptClass(typeof(string));
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

        public static IScriptContract GetContractBinding(Type t)
        {
            if (Equals(typeof(object), t))
                return ScriptSuperContract.Instance;
            else if (Equals(typeof(Type), t))
                return ScriptMetaContract.Instance;
            else if (Equals(typeof(string), t))
                return ScriptStringContract.Instance;
            else if (t.Is<long, int, byte>())
                return ScriptIntegerContract.Instance;
            else if (Equals(typeof(bool), t))
                return ScriptBooleanContract.Instance;
            else if (Equals(typeof(double), t))
                return ScriptRealContract.Instance;
            else if (Equals(typeof(Delegate), t))
                return ScriptCallableContract.Instance;
            else if (Equals(typeof(void), t))
                return ScriptObject.Void;
            else return (ScriptClass)t;
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

        private static ContractRelationshipType GetRelationship(ScriptActionContract sourceSignature, ScriptActionContract destSignature)
        {
            return sourceSignature.GetRelationship(destSignature);
        }

        private static ContractRelationshipType GetRelationship(Type sourceDelegate, ScriptActionContract destSignature)
        {
            var invokeMethod = sourceDelegate.GetMethod("Invoke");
            return invokeMethod != null ?
                GetRelationship(ScriptMethod.GetContractBinding(invokeMethod), destSignature) :
                ContractRelationshipType.None;
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
            else if (contract is ScriptActionContract && typeof(Delegate).IsAssignableFrom(NativeType))
                return GetRelationship(NativeType, (ScriptActionContract)contract);
            else return ContractRelationshipType.None;
        }

        private ScriptClass MakeGenericType(IList<IScriptObject> args)
        {
            var genericTypes = from a in args
                               let type = GetType(a as IScriptContract)
                               where type != null
                               select type;
            return new ScriptClass(NativeType.MakeGenericType(Enumerable.ToArray(genericTypes)));
        }

        /// <summary>
        /// Creates a new instance of the native .NET object.
        /// </summary>
        /// <param name="args">Constructor arguments.</param>
        /// <param name="state">Internal interpreter state.</param>
        /// <returns>A new instance of the native .NET object.</returns>
        public override IScriptObject CreateObject(IList<IScriptObject> args, InterpreterState state)
        {
            return NativeType.IsGenericTypeDefinition ? MakeGenericType(args) : NativeObject.New(args, NativeType, state);
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

        public IRuntimeSlot this[string slotName, BindingFlags flags, INativeObject @this, InterpreterState state]
        {
            get
            {
                var members = NativeType.GetMember(slotName, flags);
                return members.LongLength > 0L ? new ScriptMember(members, @this) : RuntimeSlotBase.Missing(slotName);
            }
        }

        /// <summary>
        /// Exposes access to the static member.
        /// </summary>
        /// <param name="slotName"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        public override IRuntimeSlot this[string slotName, InterpreterState state]
        {
            get { return this[slotName, MemberFlags, null, state]; }
        }
    }
}
