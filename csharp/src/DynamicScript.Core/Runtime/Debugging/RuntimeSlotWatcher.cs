using System;
using System.Collections.Generic;

namespace DynamicScript.Runtime.Debugging
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;
    using Resources = Properties.Resources;
    using QCodeBinaryOperatorType = Compiler.Ast.ScriptCodeBinaryOperatorType;
    using QCodeUnaryOperatorType = Compiler.Ast.ScriptCodeUnaryOperatorType;
    using DynamicMetaObject = System.Dynamic.DynamicMetaObject;
    using Expression = System.Linq.Expressions.Expression;
    using IDynamicMetaObjectProvider = System.Dynamic.IDynamicMetaObjectProvider;
    using IScopeVariable = Microsoft.Scripting.IScopeVariable;
    using ScriptSuperContract = Environment.ScriptSuperContract;
    using NamedRuntimeSlot = Environment.NamedRuntimeSlot;

    /// <summary>
    /// Represents runtime slot watcher that is used in debug mode.
    /// This class cannot be inherited.
    /// </summary>
    
    [ComVisible(false)]
    public sealed class RuntimeSlotWatcher: WeakReference, IDebuggerBrowsable, IEquatable<RuntimeSlotWatcher>
    {
        /// <summary>
        /// Initializes a new runtime slot watcher.
        /// </summary>
        /// <param name="slot">A reference to the runtime slot.</param>
        public RuntimeSlotWatcher(IRuntimeSlot slot)
            : base(slot, false)
        {
            if (slot == null) throw new ArgumentNullException("slot");
        }

        /// <summary>
        /// Initializes a new runtime slot watcher for the specified named slot.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="slotName"></param>
        public RuntimeSlotWatcher(IScriptObject value, string slotName)
            : this(new NamedRuntimeSlot(value, slotName))
        {
        }

        /// <summary>
        /// Gets target runtime slot.
        /// </summary>
        public new IRuntimeSlot Target
        {
            get { return (IRuntimeSlot)base.Target; }
        }

        /// <summary>
        /// Returns value stored in the slot.
        /// </summary>
        /// <param name="state">Internal interpreter state.</param>
        /// <returns>The value stored in the slot.</returns>
        /// <exception cref="System.InvalidOperationException">The runtime slot is unavailable at the current stack frame.</exception>
        public IScriptObject GetValue(InterpreterState state)
        {
            switch (IsAlive)
            {
                case true:
                    return Target.GetValue(state);
                default:
                    throw new InvalidOperationException(Resources.SlotIsOutOfScope);
            }
        }

        /// <summary>
        /// Stores value to the runtime slot.
        /// </summary>
        /// <param name="value">The value to store.</param>
        /// <param name="state">Internal interpreter state.</param>
        /// <exception cref="System.InvalidOperationException">The runtime slot is unavailable at the current stack frame.</exception>
        public IScriptObject SetValue(IScriptObject value, InterpreterState state)
        {
            switch (IsAlive)
            {
                case true:
                    return Target.SetValue(value, state);
                default:
                    throw new InvalidOperationException(Resources.SlotIsOutOfScope);
            }
        }

        /// <summary>
        /// Returns contract binding of the slot.
        /// </summary>
        /// <exception cref="System.InvalidOperationException">The runtime slot is unavailable at the current stack frame.</exception>
        public IScriptContract ContractBinding
        {
            get
            {
                switch (IsAlive)
                {
                    case true:
                        return Target is IStaticRuntimeSlot ? ((IStaticRuntimeSlot)Target).ContractBinding : ScriptSuperContract.Instance;
                    default:
                        throw new InvalidOperationException(Resources.SlotIsOutOfScope);
                }
            }
        }

        /// <summary>
        /// Gets runtime slot semantic.
        /// </summary>
        /// <exception cref="System.InvalidOperationException">The runtime slot is unavailable at the current stack frame.</exception>
        public RuntimeSlotAttributes Attributes
        {
            get 
            {
                switch (IsAlive)
                {
                    case true:
                        return Target.Attributes;
                    default:
                        throw new InvalidOperationException(Resources.SlotIsOutOfScope);
                } 
            }
        }

        bool IScopeVariable.DeleteValue()
        {
            return IsAlive && Target.DeleteValue();
        }

        /// <summary>
        /// Gets a value indicating that the runtime slot stores script object.
        /// </summary>
        public bool HasValue
        {
            get { return IsAlive && Target.HasValue; }
        }

        void IScopeVariable.SetValue(object value)
        {
            switch (IsAlive)
            {
                case true:
                    Target.SetValue(value);
                    break;
                default:
                    throw new InvalidOperationException(Resources.SlotIsOutOfScope);
            } 
        }

        bool IScopeVariable.TryGetValue(out dynamic value)
        {
            switch (IsAlive)
            {
                case true:
                    return Target.TryGetValue(out value);
                default:
                    throw new InvalidOperationException(Resources.SlotIsOutOfScope);
            } 
        }

        /// <summary>
        /// Tries to obtain value from the underlying named storage.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        public bool TryGetValue(InterpreterState state, out string value)
        {
            switch (IsAlive && Target is IDebuggerBrowsable)
            {
                case true:
                    return ((IDebuggerBrowsable)Target).TryGetValue(state, out value);
                default:
                    value = null;
                    return false;
            }
        }

        /// <summary>
        /// Determines whether the current watcher references the same runtime slot as other.
        /// </summary>
        /// <param name="other">Other slot watcher to compare.</param>
        /// <returns></returns>
        public bool Equals(RuntimeSlotWatcher other)
        {
            return other != null ? Equals(Target, other.Target) : false;
        }

        /// <summary>
        /// Determines whether the current watcher references the same runtime slot as other.
        /// </summary>
        /// <param name="other">Other slot watcher to compare.</param>
        /// <returns></returns>
        public bool Equals(IRuntimeSlot other)
        {
            return other is RuntimeSlotWatcher ? Equals(other as RuntimeSlotWatcher) : Equals(Target, other);
        }
    }
}
