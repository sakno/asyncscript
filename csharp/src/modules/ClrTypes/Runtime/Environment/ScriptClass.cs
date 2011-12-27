using System;
using System.Collections.Generic;
using DynamicScript.Runtime;
using DynamicScript.Runtime.Environment;
using System.Reflection;
using System.Linq;

namespace DynamicScript.Runtime.Environment
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;
    using IEnumerator = System.Collections.IEnumerator;

    /// <summary>
    /// Represents script-compliant wrapper of the .NET type.
    /// This class cannot be inherited.
    /// </summary>
    [ComVisible(false)]
    sealed class ScriptClass : ScriptContract, IScriptClass, IScriptMetaContract, IScriptIterable
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
            NativeType = nt.IsByRef ? nt.GetElementType() : nt;
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
                return source.LongLength == 0L ? ContractRelationshipType.TheSame : relationship;
            else if (source.LongLength > destination.LongLength)
                return relationship == ContractRelationshipType.TheSame ? ContractRelationshipType.Subset : relationship;
            else return relationship == ContractRelationshipType.TheSame ? ContractRelationshipType.Superset : relationship;
        }

        private static ContractRelationshipType GetRelationship(ScriptFunctionContract sourceSignature, ScriptFunctionContract destSignature)
        {
            return sourceSignature.GetRelationship(destSignature);
        }

        private static ContractRelationshipType GetRelationship(Type sourceDelegate, ScriptFunctionContract destSignature)
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
            switch (Type.GetTypeCode(NativeType))
            {
                case TypeCode.Int32:
                case TypeCode.Int16:
                case TypeCode.Int64:
                    return ScriptIntegerContract.Instance.GetRelationship(contract);
                case TypeCode.Boolean:
                    return ScriptBooleanContract.Instance.GetRelationship(contract);
                case TypeCode.Double:
                case TypeCode.Single:
                    return ScriptRealContract.Instance.GetRelationship(contract);
                case TypeCode.String:
                    return ScriptStringContract.Instance.GetRelationship(contract);
                default:
                    if (contract is IScriptClass)
                        return GetRelationship((IScriptClass)contract);
                    else if (contract.OneOf<ScriptMetaContract, ScriptSuperContract>())
                        return ContractRelationshipType.Subset;
                    else if (contract is ScriptVoid)
                        return ContractRelationshipType.Superset;
                    else if (contract.OneOf<IScriptComplementation, IScriptUnionContract>())
                        return Inverse(contract.GetRelationship(this));
                    else if (contract is ScriptFunctionContract && typeof(Delegate).IsAssignableFrom(NativeType))
                        return GetRelationship(NativeType, (ScriptFunctionContract)contract);
                    else return ContractRelationshipType.None;
            }
        }

        private ScriptClass MakeGenericType(IList<IScriptObject> args)
        {
            var genericTypes = from a in args
                               let type = GetType(a as IScriptContract)
                               where type != null
                               select type;
            return new ScriptClass(NativeType.MakeGenericType(Enumerable.ToArray(genericTypes)));
        }

        private static IScriptObject CreateDelegate(Type delegateType, IScriptFunction implementation, InterpreterState state)
        {
            return new NativeObject(ScriptMethod.CreateDelegate(delegateType, implementation, state));
        }

        private static IScriptObject CreateDelegate(Type delegateType, IList<IScriptObject> args, InterpreterState state)
        {
            return args.Count == 1 ? CreateDelegate(delegateType, args[0] as IScriptFunction, state) : Void;
        }

        private INativeObject CreateArray(Type arrayType, IList<IScriptObject> args, InterpreterState state)
        {
            var lengths = default(Array);
            switch (NativeObject.TryConvert(args, out lengths, typeof(long), state))
            {
                case true:
                    return new NativeObject(Array.CreateInstance(arrayType.GetElementType(), (long[])lengths), arrayType);
                default:
                    throw new UnsupportedOperationException(state);
            }
        }

        /// <summary>
        /// Creates a new instance of the native .NET object.
        /// </summary>
        /// <param name="args">Constructor arguments.</param>
        /// <param name="state">Internal interpreter state.</param>
        /// <returns>A new instance of the native .NET object.</returns>
        public override IScriptObject CreateObject(IList<IScriptObject> args, InterpreterState state)
        {
            if (NativeType.IsGenericTypeDefinition)
                return MakeGenericType(args);
            else if (NativeType.IsArray)
                return CreateArray(NativeType, args, state);
            else if (typeof(Delegate).IsAssignableFrom(NativeType))
                return CreateDelegate(NativeType, args, state);
            else return NativeObject.New(args, NativeType, state);
        }

        /// <summary>
        /// Returns a new contract binding.
        /// </summary>
        /// <returns></returns>
        public override IScriptContract GetContractBinding()
        {
            if (m_generic == null) m_generic = new ScriptGeneric(this, defaultConstructor: NativeType.GetConstructor(Type.EmptyTypes) != null);
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

        #region Member Accessors

        private static IScriptObject GetValue(FieldInfo fi, object @this)
        {
            return NativeObject.ConvertFrom(fi.GetValue(@this));
        }

        private static IScriptObject GetValue(PropertyInfo pi, object @this, IList<IScriptObject> arguments, InterpreterState state)
        {
            var propertyIndicies = default(object[]);
            var parameters = Array.ConvertAll(pi.GetIndexParameters(), p => p.ParameterType);
            switch (NativeObject.TryConvert(arguments, out propertyIndicies, parameters, state))
            {
                case true:
                    var result = pi.GetValue(@this, propertyIndicies);
                    return pi.PropertyType == null || Equals(pi.PropertyType, typeof(void)) ? ScriptObject.Void : NativeObject.ConvertFrom(result, pi.PropertyType);
                default:
                    throw new UnsupportedOperationException(state);
            }
        }

        private static IScriptClass GetValue(Type nestedType)
        {
            return (ScriptClass)nestedType;
        }

        private static IScriptCompositeObject GetValue(EventInfo ei, INativeObject owner)
        {
            return new ScriptEvent(ei, owner);
        }

        private static IScriptObject GetValue(MethodInfo[] methods, INativeObject @this, InterpreterState state)
        {
            return methods.LongLength > 1L ? ScriptMethod.Overload(methods, @this, state) : new ScriptMethod(methods[0], @this);
        }

        private static void SetValue(IScriptObject value, FieldInfo fi, object @this, InterpreterState state)
        {
            var fieldObject = default(object);
            if (NativeObject.TryConvert(value, fi.FieldType, state, out fieldObject))
                fi.SetValue(@this, fieldObject);
            else throw new UnsupportedOperationException(state);
        }

        private static void SetValue(IScriptObject value, PropertyInfo pi, object @this, IList<IScriptObject> arguments, InterpreterState state)
        {
            var propertyIndicies = default(object[]);
            var propertyValue = default(object);
            var parameters = Array.ConvertAll(pi.GetIndexParameters(), p => p.ParameterType);
            switch (NativeObject.TryConvert(value, state, out propertyValue) && NativeObject.TryConvert(arguments, out propertyIndicies, parameters, state))
            {
                case true:
                    pi.SetValue(@this, propertyValue, propertyIndicies);
                    return;
                default:
                    throw new UnsupportedOperationException(state);
            }
        }
        #endregion

        public IScriptObject this[string slotName, BindingFlags flags, INativeObject @this, InterpreterState state]
        {
            get
            {
                var members = NativeType.GetMember(slotName, flags);
                if (members.LongLength > 0L)
                    switch (members[0].MemberType)
                    {
                        case MemberTypes.Field:
                            return GetValue((FieldInfo)members[0], @this != null ? @this.Instance : null);
                        case MemberTypes.Property:
                            return GetValue((PropertyInfo)members[0], @this != null ? @this.Instance : null, EmptyArray, state);
                        case MemberTypes.NestedType:
                            return GetValue((Type)members[0]);
                        case MemberTypes.Event:
                            return GetValue((EventInfo)members[0], @this);
                        case MemberTypes.Method:
                            return GetValue(Array.ConvertAll(members, m => (MethodInfo)m), @this, state);
                        default:
                            throw new UnsupportedOperationException(state);
                    }
                else if (state.Context == Compiler.Ast.InterpretationContext.Unchecked)
                    return Void;
                else throw new SlotNotFoundException(slotName, state);
            }
            set
            {
                var members = NativeType.GetMember(slotName, flags);
                if (members.LongLength > 0L)
                    switch (members[0].MemberType)
                    {
                        case MemberTypes.Field:
                            SetValue(value, (FieldInfo)members[0], @this != null ? @this.Instance : null, state); return;
                        case MemberTypes.Property:
                            SetValue(value, (PropertyInfo)members[0], @this != null ? @this.Instance : null, EmptyArray, state); return;
                        default: throw new UnsupportedOperationException(state);
                    }
                else if (state.Context == Compiler.Ast.InterpretationContext.Checked)
                    throw new SlotNotFoundException(slotName, state);
            }
        }

        /// <summary>
        /// Exposes access to the static member.
        /// </summary>
        /// <param name="slotName"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        public override IScriptObject this[string slotName, InterpreterState state]
        {
            get { return this[slotName, MemberFlags, null, state]; }
            set { this[slotName, MemberFlags, null, state] = value; }
        }

        public IScriptObject GetSlotMetadata(string slotName, BindingFlags flags, InterpreterState state)
        {
            var members = NativeType.GetMember(slotName, flags);
            switch (members.LongLength)
            {
                case 0L: return Void;
                case 1L: return NativeObject.ConvertFrom(members[0]);
                default:
                    return new ScriptArray(Array.ConvertAll(members, m => NativeObject.ConvertFrom(m)));
            }
        }

        protected override IScriptObject GetSlotMetadata(string slotName, InterpreterState state)
        {
            return GetSlotMetadata(slotName, MemberFlags, state);
        }

        public IScriptObject this[IList<IScriptObject> indicies, INativeObject @this, InterpreterState state]
        {
            get
            {
                var members = NativeType.GetDefaultMembers();
                if (members.LongLength > 0L)
                    switch (members[0].MemberType)
                    {
                        case MemberTypes.Field:
                            return GetValue((FieldInfo)members[0], @this != null ? @this.Instance : null);
                        case MemberTypes.Property:
                            return GetValue((PropertyInfo)members[0], @this != null ? @this.Instance : null, indicies, state);
                        case MemberTypes.NestedType:
                            return GetValue((Type)members[0]);
                        case MemberTypes.Event:
                            return GetValue((EventInfo)members[0], @this);
                        case MemberTypes.Method:
                            return GetValue(Array.ConvertAll(members, m => (MethodInfo)m), @this, state);
                        default:
                            throw new UnsupportedOperationException(state);
                    }
                else if (NativeType.IsArray && @this != null)
                {
                    var args = default(Array);
                    switch (NativeObject.TryConvert(indicies, out args, typeof(long), state))
                    {
                        case true:
                            return NativeObject.ConvertFrom(((Array)@this.Instance).GetValue((long[])args), NativeType);
                        default:
                            throw new UnsupportedOperationException(state);
                    }
                }
                else if (state.Context == Compiler.Ast.InterpretationContext.Unchecked)
                    return Void;
                else throw new SlotNotFoundException(GetItemAction, state);
            }
            set
            {
                var members = NativeType.GetDefaultMembers();
                if (members.LongLength > 0L)
                    switch (members[0].MemberType)
                    {
                        case MemberTypes.Field:
                            SetValue(value, (FieldInfo)members[0], @this != null ? @this.Instance : null, state); return;
                        case MemberTypes.Property:
                            SetValue(value, (PropertyInfo)members[0], @this != null ? @this.Instance : null, indicies, state); return;
                        default: throw new UnsupportedOperationException(state);
                    }
                else if (NativeType.IsArray && @this != null)
                {
                    var args = default(Array);
                    var v = default(object);
                    switch (NativeObject.TryConvert(value, NativeType, state, out v) && NativeObject.TryConvert(indicies, out args, typeof(long), state))
                    {
                        case true:
                            ((Array)@this.Instance).SetValue(v, (long[])args); return;
                        default:
                            throw new UnsupportedOperationException(state);
                    }
                }
                else if (state.Context == Compiler.Ast.InterpretationContext.Checked)
                    throw new SlotNotFoundException(SetItemAction, state);
            }
        }

        public override IScriptObject this[IList<IScriptObject> indicies, InterpreterState state]
        {
            get { return this[indicies, null, state]; }
            set { this[indicies, null, state] = value; }
        }

        IEnumerator IScriptIterable.GetIterator(InterpreterState state)
        {
            if (NativeType.BaseType != null) yield return (ScriptClass)NativeType.BaseType;
            foreach (var iface in NativeType.GetInterfaces())
                yield return (ScriptClass)iface;
        }
    }
}
