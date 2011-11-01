using System;
using System.Reflection;

namespace DynamicScript.Runtime.Environment
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;

    /// <summary>
    /// Represents member accessor.
    /// This class cannot be inherited.
    /// </summary>
    [ComVisible(false)]
    sealed class ScriptMember: ScriptObject.RuntimeSlotBase
    {
        /// <summary>
        /// Represents wrapped member.
        /// </summary>
        public readonly MemberInfo[] Members;
        private readonly MemberTypes MemberType;
        private readonly INativeObject Owner;

        public ScriptMember(MemberInfo[] mi, INativeObject @this = null)
        {
            if (mi == null) throw new ArgumentNullException("mi");
            Owner = @this;
            Members = mi;
            MemberType = mi[0].MemberType;
        }

        private static IScriptObject GetValue(FieldInfo fi, object @this)
        {
            return NativeObject.ConvertFrom(fi.GetValue(@this));
        }

        private static IScriptObject GetValue(PropertyInfo pi, object @this)
        {
            return NativeObject.ConvertFrom(pi.GetValue(@this, null));
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
                    return GetValue((PropertyInfo)Members[0], Owner != null ? Owner.Instance : null);
                case MemberTypes.NestedType:
                    return GetValue((Type)Members[0]);
                case MemberTypes.Event:
                    return GetValue((EventInfo)Members[0], Owner);
                case MemberTypes.Method:
                    return GetValue(Array.ConvertAll(Members, m => (MethodInfo)m), Owner, state);
                default:
                    return ScriptObject.Void;
            }
        }

        public override void SetValue(IScriptObject value, InterpreterState state)
        {
            throw new NotImplementedException();
        }

        public override IScriptContract ContractBinding
        {
            get { throw new NotImplementedException(); }
        }

        public override RuntimeSlotAttributes Attributes
        {
            get { throw new NotImplementedException(); }
        }

        protected override System.Collections.Generic.ICollection<string> Slots
        {
            get { throw new NotImplementedException(); }
        }

        public override bool DeleteValue()
        {
            throw new NotImplementedException();
        }

        public override bool Equals(IRuntimeSlot other)
        {
            throw new NotImplementedException();
        }
    }
}
