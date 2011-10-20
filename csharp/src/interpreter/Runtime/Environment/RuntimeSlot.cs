using System;
using System.Dynamic;
using System.Linq.Expressions;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace DynamicScript.Runtime.Environment
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;
    using InterpretationContext = Compiler.Ast.InterpretationContext;
    using WaitHandle = System.Threading.WaitHandle;
    using IScopeVariable = Microsoft.Scripting.IScopeVariable;
    using QCodeBinaryOperatorType = Compiler.Ast.ScriptCodeBinaryOperatorType;
    using QCodeUnaryOperatorType = Compiler.Ast.ScriptCodeUnaryOperatorType;
    using NativeGarbageCollector = System.GC;
    using IDebuggerEditable = Debugging.IDebuggerBrowsable;
    using ScriptObjectConverter = Debugging.ScriptObjectConverter;

    /// <summary>
    /// Represents DynamicScript object runtime storage, such as variable or constant.
    /// </summary>
    [ComVisible(false)]
    [Serializable]
    public abstract class RuntimeSlot: ScriptObject.RuntimeSlotBase, ISynchronizable, IDebuggerEditable, IEquatable<IRuntimeSlot>, IEquatable<RuntimeSlot>
    {
        private const string ContractBindingHolder = "ContractBinding";
        private const string ValueHolder = "Value";
        private readonly IScriptContract m_contract;
        /// <summary>
        /// Represents internal storage of the runtime slot.
        /// </summary>
        protected IScriptObject Value;

        /// <summary>
        /// Initializes a new runtime storage.
        /// </summary>
        /// <param name="contract">The contract binding for the storage.</param>
        protected RuntimeSlot(IScriptContract contract = null)
        {
            m_contract = contract ?? ScriptSuperContract.Instance;
        }

        /// <summary>
        /// Deserializes runtime slot.
        /// </summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        protected RuntimeSlot(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            m_contract = (IScriptContract)info.GetValue(ContractBindingHolder, typeof(IScriptContract));
            Value = (IScriptObject)info.GetValue(ValueHolder, typeof(IScriptObject));
        }

        /// <summary>
        /// Gets slot semantic.
        /// </summary>
        public sealed override RuntimeSlotAttributes Attributes
        {
            get
            {
                var result = default(RuntimeSlotAttributes);
                if (IsConstant) result |= RuntimeSlotAttributes.Immutable;
                if (Value is IScriptProxyObject) result |= RuntimeSlotAttributes.Lazy;
                return result;
            }
        }

        /// <summary>
        /// Gets a value indicating that the current slot is immutable.
        /// </summary>
        internal protected abstract bool IsConstant
        {
            get;
        }

        /// <summary>
        /// Returns contract binding of the stored value.
        /// </summary>
        /// <returns>The contract binding of the stored value.</returns>
        protected sealed override IScriptContract GetValueContract()
        {
            return Value != null ? Value.GetContractBinding() : ScriptObject.Void;
        }

        /// <summary>
        /// Gets contract binding.
        /// </summary>
        public sealed override IScriptContract ContractBinding
        {
            get { return m_contract; }
        }

        /// <summary>
        /// Gets collection of slots.
        /// </summary>
        protected sealed override ICollection<string> Slots
        {
            get { return Value != null ? Value.Slots : new string[0]; }
        }

        /// <summary>
        /// Stores the specified value directly to the slot.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="state"></param>
        /// <remarks>This method ignores custom semantic defined in overridden <see cref="SetValue"/> method.</remarks>
        [MethodImpl(MethodImplOptions.Synchronized)]
        protected void SetValueFast(IScriptObject value, InterpreterState state)
        {
            var theSame = default(bool);
            if (value == null)
                Value = null;
            else if (value is ScriptVoid)
                Value = ContractBinding.FromVoid(state);
            else if (value is IRuntimeSlot)
                SetValueFast(((IRuntimeSlot)value).GetValue(state), state);
            else if (value is IScriptProxyObject)
                Value = value;
            else if (ContractBinding.IsCompatible(value, out theSame))
                Value = theSame ? value : ContractBinding.Convert(Conversion.Implicit, value, state);
            else if (state.Context == InterpretationContext.Unchecked)
                Value = ContractBinding.FromVoid(state);
            else
                throw new ContractBindingException(value, ContractBinding, state);
            HasValue = true;
        }

        /// <summary>
        /// Gets a value indicating whether this runtime slot holds a script object.
        /// </summary>
        public override bool HasValue
        {
            get { return base.HasValue && Value != null; }
            protected set { base.HasValue = value; }
        }

        /// <summary>
        /// Stores value in the holder.
        /// </summary>
        /// <param name="value">The value to store.</param>
        /// <param name="state">Internal interpreter state.</param>
        /// <exception cref="ContractBindingException">The input object has incompatible contract with the slot and
        /// assignment is located in the checked context.</exception>
        public override void SetValue(IScriptObject value, InterpreterState state)
        {
            SetValueFast(value, state);
        }

        /// <summary>
        /// Reads value stored in the slot.
        /// </summary>
        /// <param name="state">Internal interpretation state.</param>
        /// <returns>The value restored from the slot.</returns>
        /// <exception cref="UnassignedSlotReadingException"> when external code attempts to read
        /// value from unassigned slot in the checked context.</exception>
        public override IScriptObject GetValue(InterpreterState state)
        {
            switch (HasValue)
            {
                case true:
                    if (Value is IScriptProxyObject)
                        SetValueFast(((IScriptProxyObject)Value).Unwrap(), state);
                    return Value;
                default:
                    if (state.Context == InterpretationContext.Unchecked) return ContractBinding.FromVoid(state);
                    else throw new UnassignedSlotReadingException(state);
            }
        }

        /// <summary>
        /// Erases an object stored in the slot.
        /// </summary>
        /// <param name="forceGarbageCollection">Specifies that the garbage collection should be forced after erasure.</param>
        /// <returns><see langword="true"/> if value erasure is supported by the current type of the slot; otherwise, <see langword="false"/>.</returns>
        public bool DeleteValue(bool forceGarbageCollection)
        {
            if (Value == null || IsConstant) return false;
            else if (forceGarbageCollection)
            {
                var generation = NativeGarbageCollector.GetGeneration(Value);
                if (Value is IDisposable) ((IDisposable)Value).Dispose();
                Value = null;
                if (forceGarbageCollection) NativeGarbageCollector.Collect(generation);
                HasValue = false;
            }
            else
            {
                Value = null;
                HasValue = false;
            }
            return true;
        }

        /// <summary>
        /// Erases an object stored in the slot.
        /// </summary>
        /// <returns><see langword="true"/> if value erasure is supported by the current type of the slot; otherwise, <see langword="false"/>.</returns>
        public sealed override bool DeleteValue()
        {
            return DeleteValue(false);
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        bool ISynchronizable.Await(WaitHandle handle, TimeSpan timeout)
        {
            return Value is ISynchronizable ? ((ISynchronizable)Value).Await(handle, timeout) : true;
        }

        /// <summary>
        /// Gets string representation of the runtime slot.
        /// </summary>
        /// <returns></returns>
        public sealed override string ToString()
        {
            return Value != null ? Value.ToString() : String.Empty;
        }

        /// <summary>
        /// Serializes runtime slot.
        /// </summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        protected sealed override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue(ContractBindingHolder, ContractBinding);
            info.AddValue(ValueHolder, Value, typeof(IScriptObject));
            base.GetObjectData(info, context);
        }

        bool IDebuggerEditable.TryGetValue(out string value, InterpreterState state)
        {
            var v = GetValue(state);
            var converter = TypeDescriptor.GetConverter(GetValue(state), true) as ScriptObjectConverter;
            switch (converter != null && converter.CanConvertTo(typeof(string)))
            {
                case true:
                    converter.SetState(state);
                    value = converter.ConvertToString(v);
                    return true;
                default:
                    value = null;
                    return false;
            }
        }

        /// <summary>
        /// Determines whether the current runtime slot holds the same value as other.
        /// </summary>
        /// <param name="other">Other runtime slot to compare.</param>
        /// <returns><see langword="true"/> if the current slot holds the same value as other; otherwise, <see langword="false"/>.</returns>
        public bool Equals(RuntimeSlot other)
        {
            if (other == null) return false;
            switch (ContractBinding.Equals(ContractBinding))
            {
                case true:
                    return HasValue && other.HasValue ? Value.Equals(other.Value) : HasValue == other.HasValue;
                default: 
                    return false;
            }
        }

        /// <summary>
        /// Determines whether the current runtime slot holds the same value as other.
        /// </summary>
        /// <param name="other">Other runtime slot to compare.</param>
        /// <returns><see langword="true"/> if the current slot holds the same value as other; otherwise, <see langword="false"/>.</returns>
        public sealed override bool Equals(IRuntimeSlot other)
        {
            return Equals(other as RuntimeSlot);
        }

        /// <summary>
        /// Determines whether the current runtime slot holds the same value as other.
        /// </summary>
        /// <param name="other">Other runtime slot to compare.</param>
        /// <returns><see langword="true"/> if the current slot holds the same value as other; otherwise, <see langword="false"/>.</returns>
        public sealed override bool Equals(object other)
        {
            switch (other is IScriptObject)
            {
                case true:
                    return HasValue && Value != null ? Equals(Value, other) : false;
                default: return Equals(other as RuntimeSlot);
            }
        }

        /// <summary>
        /// Computes hash code of this runtime slot.
        /// </summary>
        /// <returns>A hash code of this runtime slot.</returns>
        public sealed override int GetHashCode()
        {
            var result = ContractBinding.GetHashCode();
            return HasValue ? result ^ Value.GetHashCode() : result;
        }
    }
}
