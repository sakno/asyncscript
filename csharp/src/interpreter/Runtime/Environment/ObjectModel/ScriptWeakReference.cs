using System;
using System.Dynamic;
using System.Collections.Generic;

namespace DynamicScript.Runtime.Environment
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;
    using ScriptCodeBinaryOperatorType = Compiler.Ast.ScriptCodeBinaryOperatorType;
    using ScriptCodeUnaryOperatorType = Compiler.Ast.ScriptCodeUnaryOperatorType;
    using LinqExpression = System.Linq.Expressions.Expression;

    /// <summary>
    /// Represents weak reference to another script object.
    /// This class cannot be inherited.
    /// </summary>
    [ComVisible(false)]
    sealed class ScriptWeakReference: ScriptCompositeObject
    {
        #region Nested Types
        [ComVisible(false)]
        private sealed class WeakReferenceHolder : WeakReference
        {
            public WeakReferenceHolder(IScriptObject obj)
                : base(IsVoid(obj) ? null : obj, false)
            {
            }

            public new IScriptObject Target
            {
                get { return base.Target as IScriptObject ?? Void; }
            }
        }

        [ComVisible(false)]
        private sealed class TargetSlot : RuntimeSlotBase, IEquatable<TargetSlot>
        {
            public const string Name = "target";
            private readonly WeakReferenceHolder m_reference;
            private readonly IScriptContract m_contract;

            public TargetSlot(WeakReferenceHolder reference, IScriptContract contract = null)
            {
                m_contract = contract ?? ScriptSuperContract.Instance;
                m_reference = reference ?? new WeakReferenceHolder(Void);
            }

            public override IScriptObject GetValue(InterpreterState state)
            {
                return m_reference.IsAlive ? m_reference.Target : m_contract.FromVoid(state);
            }

            public override void SetValue(IScriptObject value, InterpreterState state)
            {
                throw new ConstantCannotBeChangedException(state);
            }

            public override IScriptContract ContractBinding
            {
                get { return m_contract; }
            }

            public override RuntimeSlotAttributes Attributes
            {
                get { return RuntimeSlotAttributes.Immutable; }
            }

            protected override ICollection<string> Slots
            {
                get { return m_reference.Target.Slots; }
            }

            public override bool DeleteValue()
            {
                return false;
            }

            public bool Equals(TargetSlot other)
            {
                return other != null && ReferenceEquals(m_reference, other.m_reference);
            }

            public override bool Equals(IRuntimeSlot other)
            {
                return Equals(other as TargetSlot);
            }

            public override bool Equals(object other)
            {
                return Equals(other as TargetSlot);
            }

            public override int GetHashCode()
            {
                return m_reference.GetHashCode();
            }
        }

        [ComVisible(false)]
        private sealed class IsAliveSlot : RuntimeSlotBase, IEquatable<IsAliveSlot>
        {
            public const string Name = "isalive";
            private readonly WeakReferenceHolder m_reference;

            public IsAliveSlot(WeakReferenceHolder reference)
            {
                m_reference = reference;
            }

            public override IScriptObject GetValue(InterpreterState state)
            {
                return (ScriptBoolean)m_reference.IsAlive;
            }

            public override void SetValue(IScriptObject value, InterpreterState state)
            {
                throw new ConstantCannotBeChangedException(state);
            }

            public override IScriptContract ContractBinding
            {
                get { return ScriptBooleanContract.Instance; }
            }

            public override RuntimeSlotAttributes Attributes
            {
                get { return RuntimeSlotAttributes.Immutable; }
            }

            protected override ICollection<string> Slots
            {
                get { return m_reference.Target.Slots; }
            }

            public override bool DeleteValue()
            {
                return false;
            }

            public bool Equals(IsAliveSlot other)
            {
                return other != null && ReferenceEquals(m_reference, other.m_reference);
            }

            public override bool Equals(IRuntimeSlot other)
            {
                return Equals(other as IsAliveSlot);
            }

            public override bool Equals(object other)
            {
                return Equals(other as IsAliveSlot);
            }

            public override int GetHashCode()
            {
                return m_reference.GetHashCode();
            }
        }

        [ComVisible(false)]
        private new sealed class Slots : ObjectSlotCollection
        {
            public Slots(IScriptObject obj)
            {
                var weakref = new WeakReferenceHolder(obj);
                Add(TargetSlot.Name, new TargetSlot(weakref, obj.GetContractBinding()));
                Add(IsAliveSlot.Name, new IsAliveSlot(weakref));
            }
        }
        #endregion

        public ScriptWeakReference(IScriptObject obj)
            : base(new Slots(obj))
        {
        }
    }
}
