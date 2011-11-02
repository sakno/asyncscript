using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;

namespace DynamicScript.Runtime.Environment
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;

    /// <summary>
    /// Represents member accessor.
    /// This class cannot be inherited.
    /// </summary>
    [ComVisible(false)]
    sealed class ScriptMember: ScriptObject.RuntimeSlotBase, IEquatable<ScriptMember>
    {
        /// <summary>
        /// Represents wrapped member.
        /// </summary>
        public readonly MemberInfo[] Members;
        private readonly MemberTypes MemberType;
        private readonly INativeObject Owner;
        private readonly IScriptObject[] Arguments;

        public ScriptMember(MemberInfo[] mi, INativeObject @this = null, IScriptObject[] args = null)
        {
            if (mi == null) throw new ArgumentNullException("mi");
            Owner = @this;
            Members = mi;
            MemberType = mi[0].MemberType;
            Arguments = args ?? new IScriptObject[0];
        }

        private static IScriptObject GetValue(FieldInfo fi, object @this)
        {
            return NativeObject.ConvertFrom(fi.GetValue(@this));
        }

        private static IScriptObject GetValue(PropertyInfo pi, object @this, IScriptObject[] arguments, InterpreterState state)
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

        public override IScriptObject GetValue(InterpreterState state)
        {
            switch (MemberType)
            {
                case MemberTypes.Field:
                    return GetValue((FieldInfo)Members[0], Owner != null ? Owner.Instance : null);
                case MemberTypes.Property:
                    return GetValue((PropertyInfo)Members[0], Owner != null ? Owner.Instance : null, Arguments, state);
                case MemberTypes.NestedType:
                    return GetValue((Type)Members[0]);
                case MemberTypes.Event:
                    return GetValue((EventInfo)Members[0], Owner);
                case MemberTypes.Method:
                    return GetValue(Array.ConvertAll(Members, m => (MethodInfo)m), Owner, state);
                default:
                    throw new UnsupportedOperationException(state);
            }
        }

        private static void SetValue(IScriptObject value, FieldInfo fi, object @this, InterpreterState state)
        {
            var fieldObject = default(object);
            if (NativeObject.TryConvert(value, fi.FieldType, state, out fieldObject))
                fi.SetValue(@this, fieldObject);
            else throw new UnsupportedOperationException(state);
        }

        private static void SetValue(IScriptObject value, PropertyInfo pi, object @this, IScriptObject[] arguments, InterpreterState state)
        {
            var propertyIndicies = default(object[]);
            var propertyValue = default(object);
            var parameters = Array.ConvertAll(pi.GetIndexParameters(), p => p.ParameterType);
            switch (NativeObject.TryConvert(value, state, out propertyValue)&& NativeObject.TryConvert(arguments, out propertyIndicies, parameters, state))
            {
                case true:
                    pi.SetValue(@this, propertyValue, propertyIndicies);
                    return;
                default:
                    throw new UnsupportedOperationException(state);
            }
        }

        public override void SetValue(IScriptObject value, InterpreterState state)
        {
            switch (MemberType)
            {
                case MemberTypes.Field:
                     SetValue(value, (FieldInfo)Members[0], Owner != null ? Owner.Instance : null, state);return;
                case MemberTypes.Property:
                     SetValue(value, (PropertyInfo)Members[0], Owner != null ? Owner.Instance : null, Arguments, state);return;
                default: throw new UnsupportedOperationException(state);
            }
        }

        public override IScriptContract ContractBinding
        {
            get { return ScriptSuperContract.Instance; }
        }

        public override RuntimeSlotAttributes Attributes
        {
            get { return RuntimeSlotAttributes.Lazy; }
        }

        protected override ICollection<string> Slots
        {
            get { return Array.ConvertAll(Members, m => m.Name); }
        }

        public override bool DeleteValue()
        {
            return false;
        }

        public bool Equals(ScriptMember other)
        {
            return other != null &&
                Enumerable.SequenceEqual(Members, other.Members) &&
                Equals(Owner, other.Owner);
        }

        public override bool Equals(IRuntimeSlot other)
        {
            return Equals(other as ScriptMember);
        }
    }
}
