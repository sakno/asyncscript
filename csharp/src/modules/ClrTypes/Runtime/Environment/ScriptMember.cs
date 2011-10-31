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
        private readonly object Owner;

        public ScriptMember(MemberInfo[] mi, object @this = null)
        {
            if (mi == null) throw new ArgumentNullException("mi");
            Owner = @this;
            Members = mi;
        }

        protected override IScriptContract GetValueContract()
        {
            throw new NotImplementedException();
        }

        public override IScriptObject GetValue(InterpreterState state)
        {
            throw new NotImplementedException();
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
