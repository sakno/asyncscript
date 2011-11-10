using System;
using System.Collections.Generic;

namespace DynamicScript.Runtime.Environment
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;

    [ComVisible(false)]
    sealed class ScriptArrayIndexer: ScriptObject.RuntimeSlotBase, IEquatable<ScriptArrayIndexer>
    {
        public readonly Array Instance;
        private readonly IList<IScriptObject> Arguments;
        private readonly IScriptClass m_contract;

        public ScriptArrayIndexer(Array inst, IList<IScriptObject> args)
        {
            Instance = inst;
            Arguments = args;
            m_contract = (ScriptClass)Instance.GetType().GetElementType();
        }

        public override IScriptObject GetValue(InterpreterState state)
        {
            var indicies = default(Array);
            switch (NativeObject.TryConvert(Arguments, out indicies, typeof(long), state))
            {
                case true:
                    return NativeObject.ConvertFrom(Instance.GetValue((long[])indicies), m_contract.NativeType);
                default:
                    throw new UnsupportedOperationException(state);
            }
        }

        public override void SetValue(IScriptObject value, InterpreterState state)
        {
            var indicies = default(Array);
            var v = default(object);
            switch (NativeObject.TryConvert(value, m_contract.NativeType, state, out v)&& NativeObject.TryConvert(Arguments, out indicies,  typeof(long), state))
            {
                case true:
                    Instance.SetValue(v, (long[])indicies);return;
                default:
                    throw new UnsupportedOperationException(state);
            }
        }

        public override IScriptContract ContractBinding
        {
            get { return m_contract; }
        }

        public override RuntimeSlotAttributes Attributes
        {
            get { return RuntimeSlotAttributes.None; }
        }

        protected override ICollection<string> Slots
        {
            get { return new string[0]; }
        }

        public override bool DeleteValue()
        {
            return false;
        }

        public bool Equals(ScriptArrayIndexer other)
        {
            return other != null && Equals(Instance, other.Instance);
        }

        public override bool Equals(IRuntimeSlot other)
        {
            return Equals(other as ScriptArrayIndexer);
        }

        public override int GetHashCode()
        {
            return Instance.GetHashCode();
        }
    }
}
