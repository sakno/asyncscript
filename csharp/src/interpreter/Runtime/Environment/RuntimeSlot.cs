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
    using ScriptObjectConverterAttribute = Debugging.ScriptObjectConverterAttribute;

    /// <summary>
    /// Represents DynamicScript object runtime storage, such as variable or constant.
    /// </summary>
    [ComVisible(false)]
    [Serializable]
    public abstract class RuntimeSlot: ScriptObject.RuntimeSlotBase, 
        IDebuggerEditable,
        IStaticRuntimeSlot,
        IEquatable<RuntimeSlot>,
        IEquatable<IStaticRuntimeSlot>
    {
        private const string ContractBindingHolder = "ContractBinding";
        private const string ValueHolder = "Value";

        /// <summary>
        /// Represents static contract binding.
        /// </summary>
        public readonly IScriptContract ContractBinding;

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
            ContractBinding = contract ?? ScriptSuperContract.Instance;
        }

        /// <summary>
        /// Deserializes runtime slot.
        /// </summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        protected RuntimeSlot(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            ContractBinding = (IScriptContract)info.GetValue(ContractBindingHolder, typeof(IScriptContract));
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
        /// Gets contract binding.
        /// </summary>
        IScriptContract IStaticRuntimeSlot.ContractBinding
        {
            get { return ContractBinding; }
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        private IScriptObject SetValueDirect(IScriptObject v)
        {
            HasValue = v != null;
            return Value = v;
        }

        private IScriptObject SetValueDirect(IScriptProxyObject value, InterpreterState state)
        {
            switch (value.IsCompleted)
            {
                case true: return SetValueDirect(value.Unwrap(state), state); 
                default:
                    value.RequiresContract(ContractBinding, state);
                    return SetValueDirect(value);
            }
        }


        /// <summary>
        /// Stores the specified value directly to the slot.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="state"></param>
        /// <remarks>This method ignores custom semantic defined in overridden <see cref="SetValue"/> method.</remarks>
        private IScriptObject SetValueDirect(IScriptObject value, InterpreterState state)
        {
            var theSame = default(bool);
            if (value == null)
            { SetValueDirect(null); return ContractBinding.FromVoid(state); }
            else if (value is IScriptProxyObject)
                return SetValueDirect((IScriptProxyObject)value, state);
            else if (ContractBinding.IsCompatible(value, out theSame))
                return SetValueDirect(theSame ? value : ContractBinding.Convert(Conversion.Implicit, value, state));
            else if (value is ScriptVoid || state.Context == InterpretationContext.Unchecked)
                return SetValueDirect(ContractBinding.FromVoid(state));
            else
                throw new ContractBindingException(value, ContractBinding, state);
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
        public override IScriptObject SetValue(IScriptObject value, InterpreterState state)
        {
            return SetValueDirect(value, state);
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
            if (HasValue)
                return Value;
            else if (state.Context == InterpretationContext.Unchecked) return ContractBinding.FromVoid(state);
            else throw new UnassignedSlotReadingException(state);
        }

        /// <summary>
        /// Erases an object stored in the slot.
        /// </summary>
        /// <param name="forceGarbageCollection">Specifies that the garbage collection should be forced after erasure.</param>
        /// <returns><see langword="true"/> if value erasure is supported by the current type of the slot; otherwise, <see langword="false"/>.</returns>
        [MethodImpl(MethodImplOptions.Synchronized)]
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

        /// <summary>
        /// Determines whether the current slot represents the same value and contract binding as the specified slot.
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public bool Equals(RuntimeSlot s)
        {
            return s != null && Equals(ContractBinding, s.ContractBinding) && Equals(Value, s.Value);
        }

        /// <summary>
        /// Determines whether the current slot represents the same value and contract binding as the specified slot.
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public bool Equals(IStaticRuntimeSlot s)
        {

            return s != null &&
                Equals(ContractBinding, s.ContractBinding) &&
                (HasValue == s.HasValue ? Equals(GetValue(InterpreterState.Current), s.GetValue(InterpreterState.Current)) : true);
        }

        /// <summary>
        /// Determines whether the current slot represents the same value and contract binding as the specified slot.
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public sealed override bool Equals(object s)
        {
            if (s is RuntimeSlot)
                return Equals((RuntimeSlot)s);
            else if (s is IStaticRuntimeSlot)
                return Equals((IStaticRuntimeSlot)s);
            else return false;
        }

        /// <summary>
        /// Computes hash code for this runtime slot.
        /// </summary>
        /// <returns></returns>
        public sealed override int GetHashCode()
        {
            return Value != null ? Value.GetHashCode() : 0;
        }

        bool IDebuggerEditable.TryGetValue(InterpreterState state, out string value)
        {
            return ScriptObjectConverterAttribute.ConvertTo<string>(GetValue(state), state, out value);
        }
    }
}
