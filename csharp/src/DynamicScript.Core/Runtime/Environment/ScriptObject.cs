using System;
using System.Dynamic;
using System.Reflection;
using System.Linq.Expressions;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace DynamicScript.Runtime.Environment
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;
    using CallSiteBinder = System.Runtime.CompilerServices.CallSiteBinder;
    using InterpretationContext = Compiler.Ast.InterpretationContext;
    using CodeAnalysisException = Compiler.CodeAnalysisException;
    using ScriptDebugInfo = Compiler.ScriptDebugInfo;
    using SystemConverter = System.Convert;
    using ScriptCodeBinaryOperatorType = Compiler.Ast.ScriptCodeBinaryOperatorType;
    using ScriptCodeUnaryOperatorType = Compiler.Ast.ScriptCodeUnaryOperatorType;
    using IScopeVariable = Microsoft.Scripting.IScopeVariable;
    using NativeGarbageCollector = System.GC;
    using SystemHelpers = System.Runtime.CompilerServices.RuntimeHelpers;

    /// <summary>
    /// Represents DynamicScript object at runtime.
    /// </summary>
    [ComVisible(false)]
    public abstract class ScriptObject : DynamicObject, 
        IScriptObject,
        IComparable<IScriptObject>,
        IEquatable<IScriptObject>, 
        ICloneable
    {
        #region Nested Types

        /// <summary>
        /// Represents metadata of the slot.
        /// </summary>
        [ComVisible(false)]
        [Serializable]
        internal protected struct SlotMeta : IEquatable<SlotMeta>
        {
            private readonly bool m_constant;
            private readonly IScriptContract m_contract;

            /// <summary>
            /// Initializes a new slot metadata.
            /// </summary>
            /// <param name="contract">The contract of the slot.</param>
            /// <param name="constant">Specifies that the contract is immutable.</param>
            public SlotMeta(IScriptContract contract, bool constant = false)
            {
                m_constant = constant;
                m_contract = contract;
            }

            /// <summary>
            /// Gets a value indicating that the slot is immutable.
            /// </summary>
            public bool IsConstant
            {
                get { return m_constant; }
            }

            /// <summary>
            /// Gets contract binding of the slot.
            /// </summary>
            public IScriptContract ContractBinding
            {
                get { return m_contract ?? Void; }
            }

            /// <summary>
            /// Determines whether the current slot metadata is equal to another.
            /// </summary>
            /// <param name="other">Other slot metadata to compare.</param>
            /// <returns><see langword="true"/> if the current slot metadata is equal to another; otherwise, <see langword="false"/>.</returns>
            public bool Equals(SlotMeta other)
            {
                return IsConstant == other.IsConstant && ContractBinding.Equals(other.ContractBinding);
            }
        }

        [ComVisible(false)]
        private sealed class ParallelConverter : ParallelSearch<object, IRuntimeConverter, IScriptObject>
        {
            public ParallelConverter(object valueToConvert)
                : base(valueToConvert)
            {
            }

            protected override bool Match(IRuntimeConverter converter, object valueToConvert, out IScriptObject result)
            {
                return converter.Convert(valueToConvert, out result);
            }

            public static bool TryConvert(object value, IEnumerable<IRuntimeConverter> converters, out IScriptObject result)
            {
                var pconv = new ParallelConverter(value);
                pconv.Find(converters);
                result = pconv.Result;
                return pconv.Success;
            }
        }

        /// <summary>
        /// Represents an abstract runtime slot.
        /// </summary>
        [ComVisible(false)]
        [Serializable]
        public abstract class RuntimeSlotBase : IRuntimeSlot, ISerializable
        {
            private const string InitializedHolder = "Initialized";
            private bool m_initialized;

            /// <summary>
            /// Deserializes runtime holder.
            /// </summary>
            /// <param name="info"></param>
            /// <param name="context"></param>
            protected RuntimeSlotBase(SerializationInfo info, StreamingContext context)
            {
                m_initialized = info.GetBoolean(InitializedHolder);
            }

            /// <summary>
            /// Initializes a new runtime slot.
            /// </summary>
            protected RuntimeSlotBase()
            {
                m_initialized = false;
            }


            /// <summary>
            /// Erases an object stored in the slot.
            /// </summary>
            /// <returns><see langword="true"/> if value erasure is supported by the current type of the slot; otherwise, <see langword="false"/>.</returns>
            public abstract bool DeleteValue();

            /// <summary>
            /// Gets a value indicating that slot stores the object.
            /// </summary>
            /// <remarks>This property can be overwritten in the derived class.</remarks>
            public virtual bool HasValue
            {
                get { return m_initialized; }
                protected set { m_initialized = value; }
            }

            /// <summary>
            /// Reads value from the slot.
            /// </summary>
            /// <param name="state">Internal interpreter state.</param>
            /// <returns></returns>
            public abstract IScriptObject GetValue(InterpreterState state);

            /// <summary>
            /// Stores value to the slot.
            /// </summary>
            /// <param name="value">A value to store.</param>
            /// <param name="state">Internal interpreter state.</param>
            /// <returns></returns>
            public abstract IScriptObject SetValue(IScriptObject value, InterpreterState state);

            /// <summary>
            /// Gets semantics of this slot.
            /// </summary>
            public virtual RuntimeSlotAttributes Attributes
            {
                get { return RuntimeSlotAttributes.None; }
            }

            void IScopeVariable.SetValue(object value)
            {
                SetValue(Convert<object>(value), InterpreterState.Current);
            }

            bool IScopeVariable.TryGetValue(out dynamic value)
            {
                switch (HasValue)
                {
                    case true:
                        value = GetValue(InterpreterState.Current);
                        return true;
                    default:
                        value = null;
                        return false;
                }
            }

            internal static Expression GetValue(Expression slotHolder, ParameterExpression stateVar)
            {
                switch (RuntimeHelpers.IsRuntimeVariable(slotHolder))
                {
                    case true:
                        var getValueMethod = LinqHelpers.BodyOf<IRuntimeSlot, InterpreterState, IScriptObject, MethodCallExpression>((slot, state) => slot.GetValue(state));
                        return getValueMethod.Update(slotHolder, new[] { stateVar });
                    default: return slotHolder;
                }
            }

            internal static Expression SetValue(Expression slotHolder, Expression value, ParameterExpression stateVar)
            {
                switch (RuntimeHelpers.IsRuntimeVariable(slotHolder))
                {
                    case true:
                        var setValueMethod = LinqHelpers.BodyOf<IRuntimeSlot, IScriptObject, InterpreterState, IScriptObject, MethodCallExpression>((slot, v, s) => slot.SetValue(v, s));
                        return setValueMethod.Update(slotHolder, new Expression[] { value, stateVar });
                    default: return slotHolder;
                }
            }

            private static Expression Initialized(Expression slotExpr)
            {
                var prop = LinqHelpers.BodyOf<IRuntimeSlot, bool, MemberExpression>(slot => slot.HasValue);
                return prop.Update(slotExpr);
            }

            internal static Expression Initialize(Expression slotHolder, Expression initialization, ParameterExpression stateVar)
            {
                return RuntimeHelpers.IsRuntimeVariable(slotHolder) ?
                    Expression.Condition(Initialized(slotHolder), GetValue(slotHolder, stateVar), SetValue(slotHolder, initialization, stateVar)) :
                    (Expression)slotHolder;
            }

            /// <summary>
            /// Serializes runtime slot.
            /// </summary>
            /// <param name="info"></param>
            /// <param name="context"></param>
            protected virtual void GetObjectData(SerializationInfo info, StreamingContext context)
            {
                info.AddValue(InitializedHolder, m_initialized);
            }

            void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
            {
                GetObjectData(info, context);
            }

            internal static MethodCallExpression Lookup(string variableName, ParameterExpression stateVar)
            {
                return ScriptObject.GetValue(InterpreterState.GlobalGetterExpression(stateVar), variableName, stateVar);
            }
        }

        /// <summary>
        /// Represents script object's slot.
        /// </summary>
        [ComVisible(false)]
        protected interface IAggregatedSlot<in TOwner>
        {
            /// <summary>
            /// 
            /// </summary>
            /// <param name="owner"></param>
            /// <param name="state"></param>
            /// <returns></returns>
            IScriptObject GetValue(TOwner owner, InterpreterState state);

            /// <summary>
            /// Gets semantic of this slot.
            /// </summary>
            RuntimeSlotAttributes Attributes { get; }

            /// <summary>
            /// Stores value to the aggregated slot.
            /// </summary>
            /// <param name="owner"></param>
            /// <param name="value"></param>
            /// <param name="state"></param>
            /// <returns></returns>
            IScriptObject SetValue(TOwner owner, IScriptObject value, InterpreterState state);

            /// <summary>
            /// Computes static contract binding of this slot.
            /// </summary>
            /// <param name="owner">Slot owner.</param>
            /// <param name="state"></param>
            /// <returns></returns>
            IScriptContract GetContractBinding(TOwner owner, InterpreterState state);
        }

        /// <summary>
        /// Represents aggregated slot.
        /// </summary>
        /// <typeparam name="TOwner">Type of the slot owner.</typeparam>
        /// <typeparam name="TValue">Type of the slot value.</typeparam>
        [ComVisible(false)]
        protected abstract class AggregatedSlot<TOwner, TValue> : IAggregatedSlot<TOwner>
            where TOwner : ScriptObject
            where TValue : IScriptObject
        {
            /// <summary>
            /// Reads value of the aggregated slot.
            /// </summary>
            /// <param name="owner"></param>
            /// <param name="state"></param>
            /// <returns></returns>
            public abstract TValue GetValue(TOwner owner, InterpreterState state);

            /// <summary>
            /// Writes value to the aggregated slot.
            /// </summary>
            /// <param name="owner"></param>
            /// <param name="value"></param>
            /// <param name="state"></param>
            public abstract void SetValue(TOwner owner, TValue value, InterpreterState state);

            /// <summary>
            /// Gets semantics of this slot.
            /// </summary>
            public virtual RuntimeSlotAttributes Attributes
            {
                get { return RuntimeSlotAttributes.None; }
            }

            IScriptObject IAggregatedSlot<TOwner>.GetValue(TOwner owner, InterpreterState state)
            {
                return GetValue(owner, state);
            }

            IScriptObject IAggregatedSlot<TOwner>.SetValue(TOwner owner, IScriptObject value, InterpreterState state)
            {
                if(value is TValue) SetValue(owner, (TValue)value, state);
                return value;
            }

            /// <summary>
            /// Returns statically known contract binding of the aggregated slot.
            /// </summary>
            /// <param name="owner"></param>
            /// <param name="state"></param>
            /// <returns></returns>
            public abstract IScriptContract GetContractBinding(TOwner owner, InterpreterState state);
        }

        /// <summary>
        /// Represents readonly aggregated object.
        /// </summary>
        /// <typeparam name="TOwner">Type of the slot owner.</typeparam>
        /// <typeparam name="TValue">Type of the stored object.</typeparam>
        [ComVisible(false)]
        protected abstract class ReadOnlyAggregatedSlot<TOwner, TValue> : AggregatedSlot<TOwner, TValue>
            where TOwner : ScriptObject
            where TValue: IScriptObject
        {
            /// <summary>
            /// Writes value to the aggregated slot.
            /// </summary>
            /// <param name="owner"></param>
            /// <param name="value"></param>
            /// <param name="state"></param>
            public sealed override void SetValue(TOwner owner, TValue value, InterpreterState state)
            {
                if (state.Context == InterpretationContext.Checked) throw new ConstantCannotBeChangedException(state);
            }

            /// <summary>
            /// Gets semantics of this slot.
            /// </summary>
            public sealed override RuntimeSlotAttributes Attributes
            {
                get { return RuntimeSlotAttributes.Immutable; }
            }

            /// <summary>
            /// Returns statically known contract binding of the slot.
            /// </summary>
            /// <param name="owner"></param>
            /// <param name="state"></param>
            /// <returns></returns>
            public override IScriptContract GetContractBinding(TOwner owner, InterpreterState state)
            {
                return GetValue(owner, state).GetContractBinding();
            }
        }

        [ComVisible(false)]
        private sealed class SimpleReadOnlyAggregatedSlot<TOwner, TValue> : ReadOnlyAggregatedSlot<TOwner, TValue>
            where TOwner : ScriptObject
            where TValue : IScriptObject
        {
            private readonly object m_target;
            private readonly MethodInfo m_implementation;
            private readonly IScriptContract m_contract;

            public SimpleReadOnlyAggregatedSlot(Func<TOwner, InterpreterState, TValue> reader, IScriptContract contractBinding = null)
            {
                if (reader == null) throw new ArgumentNullException("reader");
                m_target = reader.Target;
                m_implementation = reader.Method;
                m_contract = contractBinding;
            }

            public override TValue GetValue(TOwner owner, InterpreterState state)
            {
                return (TValue)m_implementation.Invoke(m_target, new object[] { owner, state });
            }

            public override IScriptContract GetContractBinding(TOwner owner, InterpreterState state)
            {
                return m_contract ?? base.GetContractBinding(owner, state);
            }
        }

        [ComVisible(false)]
        private sealed class AggregatedValue<TOwner, TValue> : ReadOnlyAggregatedSlot<TOwner, TValue>
            where TOwner : ScriptObject
            where TValue: IScriptObject
        {
            private Func<TValue> m_provider;
            private TValue Value;

            public AggregatedValue(TValue v)
            {
                if (v == null) throw new ArgumentNullException("v");
                Value = v;
            }

            public AggregatedValue(Func<TValue> provider)
            {
                if (provider == null) throw new ArgumentNullException("provider");
                m_provider = provider;
            }

            public override TValue GetValue(TOwner owner, InterpreterState state)
            {
                if (m_provider != null)
                {
                    Value = m_provider();
                    m_provider = null;
                }
                return Value;
            }
        }

        /// <summary>
        /// Represents collection
        /// </summary>
        /// <typeparam name="TOwner"></typeparam>
        [ComVisible(false)]
        protected class AggregatedSlotCollection<TOwner> : Dictionary<string, IAggregatedSlot<TOwner>>
            where TOwner : ScriptObject
        {
            /// <summary>
            /// Initializes a new collection of aggregated slots with the predefined capacity.
            /// </summary>
            /// <param name="capacity"></param>
            public AggregatedSlotCollection(int capacity = 10)
                : base(capacity, StringEqualityComparer.Instance)
            {
            }

            /// <summary>
            /// Initializes a new collection of aggregated slots from the specified dictionary.
            /// </summary>
            /// <param name="collection"></param>
            public AggregatedSlotCollection(IDictionary<string, IAggregatedSlot<TOwner>> collection)
                : base(collection, StringEqualityComparer.Instance)
            {
            }

            /// <summary>
            /// Adds a new readonly aggregated slot with the specified value.
            /// </summary>
            /// <typeparam name="TValue">Type of the value to store into the slot.</typeparam>
            /// <param name="slotName">The name of the slot.</param>
            /// <param name="valueProvider">The delegate that supplies the value.</param>
            public void Add<TValue>(string slotName, Func<TValue> valueProvider)
                where TValue : IScriptObject
            {
                Add(slotName, new AggregatedValue<TOwner, TValue>(valueProvider));
            }

            /// <summary>
            /// Adds a new readonly aggregated slot.
            /// </summary>
            /// <typeparam name="TValue">Type of the aggregated object.</typeparam>
            /// <param name="slotName">The name of the aggregated object.</param>
            /// <param name="reader">The delegate that provides access to the stored object.</param>
            /// <param name="contractBinding">The static binding of the aggregated slot.</param>
            public void Add<TValue>(string slotName, Func<TOwner, InterpreterState, TValue> reader, IScriptContract contractBinding = null)
                where TValue : IScriptObject
            {
                Add(slotName, new SimpleReadOnlyAggregatedSlot<TOwner, TValue>(reader, contractBinding));
            }

            /// <summary>
            /// Adds a new aggregated slot that contains default constructor.
            /// </summary>
            /// <typeparam name="TSlot"></typeparam>
            /// <param name="slotName"></param>
            public void Add<TSlot>(string slotName)
                where TSlot : IAggregatedSlot<TOwner>, new()
            {
                Add(slotName, new TSlot());
            }

            private static IScriptObject GetSlotMetadata(IAggregatedSlot<TOwner> slot, TOwner owner, string slotName, InterpreterState state)
            {
                return Convert(new KeyValuePair<string, SlotMeta>(slotName, new SlotMeta(slot.GetContractBinding(owner, state), (slot.Attributes & RuntimeSlotAttributes.Immutable) != 0)));
            }

            /// <summary>
            /// Returns a script object that describes metadata of the aggregated slot.
            /// </summary>
            /// <param name="owner"></param>
            /// <param name="slotName"></param>
            /// <param name="state"></param>
            /// <returns></returns>
            public virtual IScriptObject GetSlotMetadata(TOwner owner, string slotName, InterpreterState state)
            {
                var holder = default(IAggregatedSlot<TOwner>);
                if (TryGetValue(slotName, out holder))
                    return GetSlotMetadata(holder, owner, slotName, state);
                else if (state.Context == InterpretationContext.Unchecked)
                    return Void;
                else throw new SlotNotFoundException(slotName, state);
            }

            /// <summary>
            /// Returns value stored in the named aggregated slot.
            /// </summary>
            /// <param name="owner"></param>
            /// <param name="slotName"></param>
            /// <param name="state"></param>
            /// <returns></returns>
            public IScriptObject GetValue(TOwner owner, string slotName, InterpreterState state)
            {
                var holder = default(IAggregatedSlot<TOwner>);
                if (TryGetValue(slotName, out holder))
                    return holder.GetValue(owner, state);
                else if (state.Context == InterpretationContext.Unchecked)
                    return Void;
                else throw new SlotNotFoundException(slotName, state);
            }

            /// <summary>
            /// Saves the value to the named aggregated slot.
            /// </summary>
            /// <param name="owner"></param>
            /// <param name="slotName"></param>
            /// <param name="value"></param>
            /// <param name="state"></param>
            /// <returns></returns>
            public IScriptObject SetValue(TOwner owner, string slotName, IScriptObject value, InterpreterState state)
            {
                var holder = default(IAggregatedSlot<TOwner>);
                if (TryGetValue(slotName, out holder))
                    return holder.SetValue(owner, value, state);
                else if (state.Context == InterpretationContext.Unchecked)
                    return value;
                else throw new SlotNotFoundException(slotName, state);
            }
        }
        
        #endregion

        /// <summary>
        /// Represents name of the 'getItem' predefined action.
        /// </summary>
        public const string GetItemAction = "getItem";

        /// <summary>
        /// Represents name of the 'setItem' predefined action.
        /// </summary>
        public const string SetItemAction = "setItem";

        /// <summary>
        /// Gets name of the slot that stores the current action.
        /// </summary>
        public const string IteratorAction = "iterator";

        private static readonly ISet<IRuntimeConverter> m_converters;

        static ScriptObject()
        {
            RuntimeConverters.RegisterConverters(m_converters = new HashSet<IRuntimeConverter>());
        }

        /// <summary>
        /// Registers a new converter.
        /// </summary>
        /// <typeparam name="TConverter">Type of the converter to register.</typeparam>
        /// <returns><see langword="true"/> if converter is registered at first time; <see langword="false"/> if the specified
        /// converter is already registered.</returns>
        public static bool RegisterConverter<TConverter>()
            where TConverter : IRuntimeConverter, new()
        {
            return RuntimeConverters.RegisterConverter<TConverter>(m_converters);
        }

        /// <summary>
        /// Represents runtime behavior of this object.
        /// </summary>
        internal readonly ObjectBehavior Behavior;

        /// <summary>
        /// Initializes a new instance of DynamicScript object.
        /// </summary>
        internal ScriptObject(ObjectBehavior behavior = ObjectBehavior.None)
        {
            Behavior = behavior;
        }

        /// <summary>
        /// Determines whether the specified object represents void object.
        /// </summary>
        /// <param name="value">The value to check.</param>
        /// <returns></returns>
        public static bool IsVoid(object value)
        {
            return value == null || value is ScriptVoid;
        }

        /// <summary>
        /// Converts .NET Framework object to DynamicScript-compliant object.
        /// </summary>
        /// <param name="value">The value to be converted.</param>
        /// <param name="result">Conversion result.</param>
        /// <returns><see langword="true"/> if conversion is supported; otherwise, <see langword="false"/>.</returns>
        public static bool TryConvert(object value, out IScriptObject result)
        {
            result = null;
            if (value == null)
                result = Void;
            if (value is IScriptObject)
                result = (IScriptObject)value;
            else if (value is DynamicMetaObject)
                return TryConvert(((DynamicMetaObject)value).Value, out result);
            else return ParallelConverter.TryConvert(value, m_converters, out result);
            return result != null;
        }

        /// <summary>
        /// Converts .NET Framework object to its DynamicScript-compliant representation.
        /// </summary>
        /// <typeparam name="T">Type of the object to convert.</typeparam>
        /// <param name="value">The value to convert.</param>
        /// <param name="defval">A default value returned by the method if conversion is not supported.</param>
        /// <returns>Conversion result.</returns>
        public static IScriptObject Convert<T>(T value, IScriptObject defval)
        {
            var result = default(IScriptObject);
            return TryConvert(value, out result) && result != null ? result : defval;
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TValue"></typeparam>
        /// <typeparam name="TField"></typeparam>
        /// <param name="value"></param>
        /// <returns></returns>
        protected static TField LazyField<TValue, TField>(ref TField value)
            where TField : class, IScriptObject
            where TValue: TField, new()
        {
            if (value == null) value = new TValue();
            return value;
        }

        /// <summary>
        /// Converts .NET Framework object to its DynamicScript-compliant representation.
        /// </summary>
        /// <typeparam name="T">Type of the object to convert.</typeparam>
        /// <param name="value">The value to convert.</param>
        /// <returns>Conversion result.</returns>
        /// <exception cref="ConversionNotSupportedException">Conversion is not supported.</exception>
        public static IScriptObject Convert<T>(T value)
        {
            var result = Convert(value, null);
            switch (result != null)
            {
                case true: return result;
                default: throw new ConversionNotSupportedException(value);
            }
        }

        internal static MethodCallExpression MakeConverter(Expression value, ParameterExpression state)
        {
            if (value == null) throw new ArgumentNullException("value");
            value = AsRightSide(value, state);
            var invocation = LinqHelpers.BodyOf<object, IScriptObject, MethodCallExpression>(obj => Convert(obj)).Method;
            invocation = invocation.GetGenericMethodDefinition().MakeGenericMethod(value.Type);
            return Expression.Call(null, invocation, value);
        }

        internal static Expression MakeConverter(IRestorable restorable, ParameterExpression state)
        {
            return restorable != null ? MakeConverter(restorable.Restore(), state) : MakeVoid();
        }

        /// <summary>
        /// Converts an array of the .NET Framework objects to an array of DynamicScript-compliant objects.
        /// </summary>
        /// <typeparam name="T">Type of the array elements to be converted.</typeparam>
        /// <param name="values">An array to be converted.</param>
        /// <returns>An array of DynamicScript-compliant objects.</returns>
        public static IScriptObject[] Convert<T>(T[] values)
        {
            return Array.ConvertAll(values ?? new T[0], Convert<T>);
        }

        private static bool TryConvert<T>(T[] values, out IScriptObject[] result)
        {
            if (values == null) values = new T[0];
            result = new IScriptObject[values.LongLength];
            for (var i = 0L; i < result.LongLength; i++)
            {
                var current = default(IScriptObject);
                if (TryConvert(values[i], out current)) result[i] = current;
                else return false;
            }
            return true;
        }

        /// <summary>
        /// Gets an object that represents DynamicScript-compliant void expression.
        /// </summary>
        public static ScriptContract Void
        {
            get { return ScriptVoid.Instance; }
        }

        /// <summary>
        /// Returns an expression that produces the void object.
        /// </summary>
        /// <returns>The expression that produces the void object.</returns>
        internal static Expression MakeVoid()
        {
            return LinqHelpers.BodyOf<Func<ScriptContract>, MemberExpression>(() => Void);
        }

        #region Indexer Operations

        /// <summary>
        /// Gets or sets indexed value.
        /// </summary>
        /// <param name="indicies"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        public virtual IScriptObject this[IList<IScriptObject> indicies, InterpreterState state]
        {
            get { return this[GetItemAction, state].Invoke(indicies, state); }
            set
            {
                var args = new IScriptObject[indicies.Count + 1];
                args[0] = value;
                indicies.CopyTo(args, 1);
                this[SetItemAction, state].Invoke(args, state);
            }
        }

        #endregion

        #region Binary Operations
        internal static bool TryBinaryOperation(IScriptObject target, BinaryOperationBinder binder, object arg, out object result)
        {
            var scriptObject = default(IScriptObject);
            var state = binder.GetState();
            switch (ScriptObject.TryConvert(arg, out scriptObject))
            {
                case true:
                    try
                    {
                        result = RuntimeHelpers.BinaryOperation(target, binder.Operation, scriptObject, state);
                        return true;
                    }
                    catch (UnsupportedOperationException e)
                    {
                        result = e;
                        return false;
                    }
                default:
                    if (state.Context == InterpretationContext.Unchecked) result = ScriptObject.Void;
                    else throw new UnsupportedOperationException(state);
                    return true;
            }
        }

        /// <summary>
        /// Provides implementation for binary operations.
        /// </summary>
        /// <param name="binder">Provides information about the binary operation.</param>
        /// <param name="arg">The right operand for the binary operation.</param>
        /// <param name="result">The result of the binary operation.</param>
        /// <returns><see langword="true"/> if the operation is successful; otherwise, <see langword="false"/>. 
        /// If this method returns <see langword="false"/>, the run-time binder of the language determines the behavior. (In most
        /// cases, a language-specific run-time exception is thrown.)</returns>
        public sealed override bool TryBinaryOperation(BinaryOperationBinder binder, object arg, out object result)
        {
            return TryBinaryOperation(this, binder, arg, out result);
        }

        /// <summary>
        /// Performs binary operation.
        /// </summary>
        /// <param name="operator"></param>
        /// <param name="right"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        public IScriptObject BinaryOperation(ScriptCodeBinaryOperatorType @operator, IScriptObject right, InterpreterState state)
        {
            if (right is IScriptProxyObject) return ((IScriptProxyObject)right).Enqueue(this, @operator, state);
            else switch (@operator)
            {
                case ScriptCodeBinaryOperatorType.Add:
                    return Add(right, state);
                case ScriptCodeBinaryOperatorType.Intersection:
                    return And(right, state);
                case ScriptCodeBinaryOperatorType.AndAlso:
                    return Convert(RuntimeHelpers.IsTrue(this, state) && RuntimeHelpers.IsTrue(right, state));
                case ScriptCodeBinaryOperatorType.Union:
                    return Or(right, state);
                case ScriptCodeBinaryOperatorType.OrElse:
                    return Convert(RuntimeHelpers.IsTrue(this, state) || RuntimeHelpers.IsTrue(right, state));
                case ScriptCodeBinaryOperatorType.Coalesce:
                    return Coalesce(right, state);
                case ScriptCodeBinaryOperatorType.Divide:
                    return Divide(right, state);
                case ScriptCodeBinaryOperatorType.InstanceOf:
                    return InstanceOf(right, state);
                case ScriptCodeBinaryOperatorType.ValueEquality:
                    return Equals(right, state);
                case ScriptCodeBinaryOperatorType.ReferenceEquality:
                    return ReferenceEquals(right, state);
                case ScriptCodeBinaryOperatorType.ValueInequality:
                    return NotEquals(right, state);
                case ScriptCodeBinaryOperatorType.ReferenceInequality:
                    return ReferenceNotEquals(right, state);
                case ScriptCodeBinaryOperatorType.Exclusion:
                    return ExclusiveOr(right, state);
                case ScriptCodeBinaryOperatorType.GreaterThan:
                    return GreaterThan(right, state);
                case ScriptCodeBinaryOperatorType.GreaterThanOrEqual:
                    return GreaterThanOrEqual(right, state);
                case ScriptCodeBinaryOperatorType.LessThan:
                    return LessThan(right, state);
                case ScriptCodeBinaryOperatorType.LessThanOrEqual:
                    return LessThanOrEqual(right, state);
                case ScriptCodeBinaryOperatorType.Modulo:
                    return Modulo(right, state);
                case ScriptCodeBinaryOperatorType.Multiply:
                    return Multiply(right, state);
                case ScriptCodeBinaryOperatorType.Subtract:
                    return Subtract(right, state);
                case ScriptCodeBinaryOperatorType.TypeCast:
                    return Convert(right as IScriptContract, state);
                case ScriptCodeBinaryOperatorType.PartOf:
                    return PartOf(right, state);
                case ScriptCodeBinaryOperatorType.MetadataDiscovery:
                    return GetSlotMetadata(right, state);
                case ScriptCodeBinaryOperatorType.MemberAccess:
                    return RtlGetValue(this, right, state);
                case ScriptCodeBinaryOperatorType.Assign:
                    return Assign(right, state);
                case ScriptCodeBinaryOperatorType.DivideAssign:
                    return DivideAssign(right, state);
                case ScriptCodeBinaryOperatorType.ExclusionAssign:
                    return ExclusionAssign(right, state);
                case ScriptCodeBinaryOperatorType.AdditiveAssign:
                    return AddAssign(right, state);
                case ScriptCodeBinaryOperatorType.MultiplicativeAssign:
                    return MulAssign(right, state);
                case ScriptCodeBinaryOperatorType.Expansion:
                    return Expansion(right, state);
                case ScriptCodeBinaryOperatorType.Reduction:
                    return Reduction(right, state);
                case ScriptCodeBinaryOperatorType.SubtractiveAssign:
                    return SubAssign(right, state);
                case ScriptCodeBinaryOperatorType.Initializer:
                    return this;
                default:
                    throw new UnsupportedOperationException(state);
            }
        }

        private IScriptObject PartOf(IScriptCompositeObject collection, InterpreterState state)
        {
            foreach (var slot in collection.GetSlotValues(state))
                if (Equals(this, slot.Value)) return Convert(true);
            return Convert(false);
        }

        private IScriptObject PartOf(IScriptContainer collection, InterpreterState state)
        {
            return Convert(collection.Contains(this, false, state));
        }

        private IScriptObject PartOf(IScriptSet collection, InterpreterState state)
        {
            foreach (var obj in collection)
                if (Equals(this, obj)) return Convert(true);
            return Convert(false);
        }

        /// <summary>
        /// Determines whether this object contains in the following collection.
        /// </summary>
        /// <param name="collection"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        protected virtual IScriptObject PartOf(IScriptObject collection, InterpreterState state)
        {
            if (collection is IScriptContainer)
                return PartOf((IScriptContainer)collection, state);
            if (collection is IScriptCompositeObject)
                return PartOf((IScriptCompositeObject)collection, state);
            else if (collection is IScriptSet)
                return PartOf((IScriptSet)collection, state);
            else return Convert(false);
        }

        private IScriptObject SubAssign(IScriptObject right, InterpreterState state)
        {
            return Assign(Subtract(right, state), state);
        }

        private IScriptObject Reduction(IScriptObject right, InterpreterState state)
        {
            return Assign(And(right, state), state);
        }

        private IScriptObject Expansion(IScriptObject right, InterpreterState state)
        {
            return Assign(Or(right, state), state);
        }

        private IScriptObject MulAssign(IScriptObject right, InterpreterState state)
        {
            return Assign(Multiply(right, state), state);
        }

        private IScriptObject AddAssign(IScriptObject right, InterpreterState state)
        {
            return Assign(Add(right, state), state);
        }

        private IScriptObject ExclusionAssign(IScriptObject right, InterpreterState state)
        {
            return Assign(ExclusiveOr(right, state), state);
        }

        private IScriptObject DivideAssign(IScriptObject right, InterpreterState state)
        {
            return Assign(Divide(right, state), state);
        }

        /// <summary>
        /// Provides assignment operation.
        /// </summary>
        /// <param name="right">An argument to be aggregated by this object.</param>
        /// <param name="state"></param>
        /// <returns></returns>
        private IScriptObject Assign(IScriptObject right, InterpreterState state)
        {
            if (this is IAggregator)
                return ((IAggregator)right).Aggregate(right, state);
            else if (state.Context == InterpretationContext.Unchecked)
                return this;
            else throw new UnsupportedOperationException(state);
        }

        /// <summary>
        /// Determines whether this object is implicitly convertible to the specified contract.
        /// </summary>
        /// <param name="contract"></param>
        /// <param name="state">Internal interpreter state.</param>
        /// <returns></returns>
        protected IScriptObject InstanceOf(IScriptContract contract, InterpreterState state)
        {
            return contract is IScriptSet ? PartOf((IScriptSet)contract, state) :
                Convert(RuntimeHelpers.IsCompatible(contract, this));
        }

        /// <summary>
        /// Determines whether this object is implicitly convertible to the specified contract.
        /// </summary>
        /// <param name="contract"></param>
        /// <param name="state">Internal interpreter state.</param>
        /// <returns></returns>
        /// <remarks>You should not override this method.</remarks>
        [EditorBrowsable(EditorBrowsableState.Never)]
        internal virtual IScriptObject InstanceOf(IScriptObject contract, InterpreterState state)
        {
            return contract is IScriptContract ? InstanceOf((IScriptContract)contract, state) : Convert(false);
        }

        /// <summary>
        /// Determines whether the current object is equal to another by reference.
        /// </summary>
        /// <param name="right">The object to compare.</param>
        /// <param name="state">Internal interpreter state.</param>
        /// <returns>The result of the reference equality.</returns>
        protected virtual IScriptObject ReferenceEquals(IScriptObject right, InterpreterState state)
        {
            return Convert<bool>(ReferenceEquals(this, right));
        }

        /// <summary>
        /// Determines whether the current object is equal to another by reference.
        /// </summary>
        /// <param name="right">The object to compare.</param>
        /// <param name="state">Internal interpreter state.</param>
        /// <returns>The result of the reference equality.</returns>
        protected virtual IScriptObject ReferenceNotEquals(IScriptObject right, InterpreterState state)
        {
            var result = ReferenceEquals(right, state);
            return result.Not(state);
        }

        private IScriptObject GetSlotMetadata(IScriptObject slotIdentity, InterpreterState state)
        {
            switch (SystemConverter.GetTypeCode(slotIdentity))
            {
                case TypeCode.String: return GetSlotMetadata(SystemConverter.ToString(slotIdentity), state);
                default: throw new UnsupportedOperationException(state);
            }
        }

        /// <summary>
        /// Gets slot metadata by its name.
        /// </summary>
        /// <param name="slotName">The name of the slot.</param>
        /// <param name="state">Internal interpreter state.</param>
        /// <returns>The slot metadata.</returns>
        protected virtual IScriptObject GetSlotMetadata(string slotName, InterpreterState state)
        {
            return Void;
        }

        /// <summary>
        /// Converts the current object to the specified contract.
        /// </summary>
        /// <param name="contract">The conversion destination.</param>
        /// <param name="state">Internal interpreter state.</param>
        /// <returns>The conversion result.</returns>
        protected IScriptObject Convert(IScriptContract contract, InterpreterState state)
        {
            switch (contract != null)
            {
                case true: return contract.Convert(Conversion.Explicit, this, state);
                default:
                    if (state.Context == InterpretationContext.Unchecked)
                        return Void;
                    else throw new UnsupportedOperationException(state);
            }   
        }

        /// <summary>
        /// Computes subtraction between the current object and the specified object.
        /// </summary>
        /// <param name="right">The subtrahend.</param>
        /// <param name="state">Internal interpreter result.</param>
        /// <returns>The subtraction result.</returns>
        protected virtual IScriptObject Subtract(IScriptObject right, InterpreterState state)
        {
            throw new UnsupportedOperationException(state);
        }

        /// <summary>
        /// Determines whether the current object is not equal to another.
        /// </summary>
        /// <param name="right">The second operand.</param>
        /// <param name="state">Internal interpreter state.</param>
        /// <returns>The comparison result.</returns>
        /// <remarks>This method is implemented by default.</remarks>
        protected virtual IScriptObject NotEquals(IScriptObject right, InterpreterState state)
        {
            var value = Equals(right, state);
            return value.Not(state);
        }

        /// <summary>
        /// Computes multiplication between the current object and the specified object.
        /// </summary>
        /// <param name="right">The second operand.</param>
        /// <param name="state">Internal interpreter state.</param>
        /// <returns>The multiplication of the two objects.</returns>
        protected virtual IScriptObject Multiply(IScriptObject right, InterpreterState state)
        {
            throw new UnsupportedOperationException(state);
        }

        /// <summary>
        /// Computes the remainder after dividing the current object by the second.
        /// </summary>
        /// <param name="right">The right operand.</param>
        /// <param name="state">Internal interpreter state.</param>
        /// <returns>The remainder.</returns>
        protected virtual IScriptObject Modulo(IScriptObject right, InterpreterState state)
        {
            throw new UnsupportedOperationException(state);
        }

        /// <summary>
        /// Determines whether the current object is less than or equal to the specified object.
        /// </summary>
        /// <param name="right">The second object to compare.</param>
        /// <param name="state">Internal interpreter state.</param>
        /// <returns><see langword="true"/>the current object is less than or equal to the specified object; otherwise, <see langword="false"/>.</returns>
        protected virtual IScriptObject LessThanOrEqual(IScriptObject right, InterpreterState state)
        {
            throw new UnsupportedOperationException(state);
        }

        /// <summary>
        /// Determines whether the current object is less than the specified object.
        /// </summary>
        /// <param name="right">The second object to compare.</param>
        /// <param name="state">Internal interpreter state.</param>
        /// <returns><see langword="true"/>the current object is less than the specified object; otherwise, <see langword="false"/>.</returns>
        protected virtual IScriptObject LessThan(IScriptObject right, InterpreterState state)
        {
            throw new UnsupportedOperationException(state);
        }

        /// <summary>
        /// Determines whether the current object is greater than or equal to the specified object.
        /// </summary>
        /// <param name="right">The second object to compare.</param>
        /// <param name="state">Internal interpreter state.</param>
        /// <returns><see langword="true"/>the current object is greater than or equal to the specified object; otherwise, <see langword="false"/>.</returns>
        protected virtual IScriptObject GreaterThanOrEqual(IScriptObject right, InterpreterState state)
        {
            throw new UnsupportedOperationException(state);
        }

        /// <summary>
        /// Determines whether the current object is greater than the specified object.
        /// </summary>
        /// <param name="right">The second object to compare.</param>
        /// <param name="state">Internal interpreter state.</param>
        /// <returns><see langword="true"/>the current object is greater than the specified object; otherwise, <see langword="false"/>.</returns>
        protected virtual IScriptObject GreaterThan(IScriptObject right, InterpreterState state)
        {
            throw new UnsupportedOperationException(state);
        }

        #region IComparable<IScriptObject> Members

        int IComparable<IScriptObject>.CompareTo(IScriptObject right)
        {
            switch (Equals((object)right))
            {
                case true: return 0;
                default: return SystemConverter.ToBoolean(RuntimeHelpers.GreaterThan(this, right)) ? 1 : -1;
            }
        }

        #endregion

        /// <summary>
        /// Computes exclusive or, or difference between two objects.
        /// </summary>
        /// <param name="right">The second operand.</param>
        /// <param name="state">Internal interpreter state.</param>
        /// <returns>The computation result.</returns>
        protected virtual IScriptObject ExclusiveOr(IScriptObject right, InterpreterState state)
        {
            throw new UnsupportedOperationException(state);
        }

        /// <summary>
        /// Determines whether the the current object is equal to another.
        /// </summary>
        /// <param name="right">Other object to be compared.</param>
        /// <param name="state">Internal interpreter state.</param>
        /// <returns>The comparison result.</returns>
        protected virtual IScriptObject Equals(IScriptObject right, InterpreterState state)
        {
            return ReferenceEquals(right, state);
        }

        /// <summary>
        /// Divides the current object using the specified.
        /// </summary>
        /// <param name="right">The right operand of the division operator.</param>
        /// <param name="state">Internal interpreter state.</param>
        /// <returns>The division result.</returns>
        protected virtual IScriptObject Divide(IScriptObject right, InterpreterState state)
        {
            throw new UnsupportedOperationException(state);
        }

        /// <summary>
        /// Returns coalesce result.
        /// </summary>
        /// <param name="right">The right operand of coalescing operation.</param>
        /// <param name="state">Internal interpreter state.</param>
        /// <returns>The operation result.</returns>
        protected virtual IScriptObject Coalesce(IScriptObject right, InterpreterState state)
        {
            return this;
        }

        /// <summary>
        /// Computes logical or, bitwise or, or union.
        /// </summary>
        /// <param name="right">The right operand.</param>
        /// <param name="state">Internal interpreter state.</param>
        /// <returns>The result of the binary operation.</returns>
        protected virtual IScriptObject Or(IScriptObject right, InterpreterState state)
        {
            throw new UnsupportedOperationException(state);
        }

        /// <summary>
        /// Computies logical and, bitwise and, or intersection.
        /// </summary>
        /// <param name="right">The right operand.</param>
        /// <param name="state">Internal interpreter state.</param>
        /// <returns>The result of the binary operation.</returns>
        protected virtual IScriptObject And(IScriptObject right, InterpreterState state)
        {
            throw new UnsupportedOperationException(state);
        }

        /// <summary>
        /// Computes the sum, or union of the current object with the specified.
        /// </summary>
        /// <param name="right">The second operand of the addition operation.</param>
        /// <param name="state">Internal interpretation state.</param>
        /// <returns>The result of the binary operation interpretation.</returns>
        protected virtual IScriptObject Add(IScriptObject right, InterpreterState state)
        {
            throw new UnsupportedOperationException(state);
        }

        #endregion

        #region Invocation Operation

        internal static bool TryInvoke(IScriptObject target, InvokeBinder binder, object[] args, out object result)
        {
            try
            {
                result = target.Invoke(Convert<object>(args), binder.GetState());
                return true;
            }
            catch (ConversionNotSupportedException e)
            {
                result = e;
                return false;
            }
        }

        /// <summary>
        /// Provides the implementation for operations that invoke an object.
        /// </summary>
        /// <param name="binder">Provides information about the invoke operation.</param>
        /// <param name="args">The arguments that are passed to the object during the invoke operation.</param>
        /// <param name="result">The result of the object invocation.</param>
        /// <returns><see langword="true"/> if the operation is successful; otherwise, <see langword="false"/>.</returns>
        public sealed override bool TryInvoke(InvokeBinder binder, object[] args, out object result)
        {
            return TryInvoke(this, binder, args, out result);
        }

        /// <summary>
        /// Performs application operator to the current object.
        /// </summary>
        /// <param name="args">An array of application arguments.</param>
        /// <param name="state">Internal interpretation state.</param>
        /// <returns>Application result.</returns>
        /// <remarks>In the default implementation, this method provides clone of the object.</remarks>
        public virtual IScriptObject Invoke(IList<IScriptObject> args, InterpreterState state)
        {
            switch (args.Count)
            {
                case 0: return Clone();
                default: if (state.Context == InterpretationContext.Unchecked) return null;
                    else throw new FunctionArgumentsMistmatchException(state);
            }
        }
        
        #endregion

        #region Special Members Invocation

        internal static bool TryGetMember(IScriptObject target, GetMemberBinder binder, out object result)
        {
            try
            {
                result = target[binder.Name, binder.GetState()];
            }
            catch (SlotNotFoundException)
            {
                result = null;
            }
            return true;
        }

        internal static bool TrySetIndex(IScriptObject target, SetIndexBinder binder, object[] indexes, object value)
        {
            var args = default(IScriptObject[]);
            var v = default(IScriptObject);
            switch (TryConvert(indexes, out args) && TryConvert(value, out v))
            {
                case true:
                    try
                    {
                        target[args, binder.GetState()] = v;
                        return true;
                    }
                    catch (SlotNotFoundException)
                    {
                        return false;
                    }
                default: return false;
            }
        }

        /// <summary>
        /// Binds to the indexer setter operation.
        /// </summary>
        /// <param name="binder"></param>
        /// <param name="indexes"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public sealed override bool TrySetIndex(SetIndexBinder binder, object[] indexes, object value)
        {
            return TrySetIndex(this, binder, indexes, value);
        }

        internal static bool TryGetIndex(IScriptObject target, GetIndexBinder binder, object[] indexes, out object result)
        {
            var args = default(IScriptObject[]);
            switch (TryConvert(indexes, out args))
            {
                case true:
                    try
                    {
                        result = target[args, binder.GetState()];
                    }
                    catch (SlotNotFoundException)
                    {
                        result = null;
                    }
                    return result!=null;
                default:
                    result = null;
                    return false;
            }
        }

        /// <summary>
        /// Binds to the indexer getter.
        /// </summary>
        /// <param name="binder"></param>
        /// <param name="indexes"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public sealed override bool TryGetIndex(GetIndexBinder binder, object[] indexes, out object result)
        {
            return TryGetIndex(this, binder, indexes, out result);
        }

        /// <summary>
        /// Provides the implementation for operations that get member values.
        /// </summary>
        /// <param name="binder">Provides information about the object that called the dynamic operation.</param>
        /// <param name="result">The result of the get operation.</param>
        /// <returns><see langword="true"/> if the operation is successful; otherwise, <see langword="false"/>.</returns>
        public sealed override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            return TryGetMember(this, binder, out result);
        }

        internal static bool TrySetMember(IScriptObject target, SetMemberBinder binder, object value)
        {
            var scriptObject = default(IScriptObject);
            switch (TryConvert(value, out scriptObject))
            {
                case true:
                    try
                    {
                        target[binder.Name, binder.GetState()] = scriptObject;
                        return true;
                    }
                    catch (SlotNotFoundException)
                    {
                        return false;
                    }
                default:
                    return false;
            }
        }

        /// <summary>
        /// Provides the implementation for operations that set member values.
        /// </summary>
        /// <param name="binder">Provides information about the object that called the dynamic operation.</param>
        /// <param name="value">The value to set to the member.</param>
        /// <returns><see langword="true"/> if the operation is successful; otherwise, <see langword="false"/>.</returns>
        public sealed override bool TrySetMember(SetMemberBinder binder, object value)
        {
            return TrySetMember(this, binder, value);
        }

        internal static bool TryInvokeMember(IScriptObject target, InvokeMemberBinder binder, object[] args, out object result)
        {
            var state = binder.GetState();
            var a = default(IScriptObject[]);
            switch (TryConvert(args, out a))
            {
                case true:
                    var slot = target[binder.Name, state];
                    result = slot.Invoke(a, state);
                    return true;
                default:
                    result = null;
                    return false;
            }
        }

        /// <summary>
        /// Provides the implementation for operations that invoke a member.
        /// </summary>
        /// <param name="binder">Provides information about the dynamic operation.</param>
        /// <param name="args">The arguments that are passed to the object member during the invoke operation.</param>
        /// <param name="result">The result of the member invocation.</param>
        /// <returns><see langword="true"/> if the operation is successful; otherwise, <see langword="false"/>.</returns>
        public sealed override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
        {
            return TryInvokeMember(this, binder, args, out result);
        }

        internal static bool TryConvert(IScriptObject target, ConvertBinder binder, out object result)
        {
            switch (binder.ReturnType.IsAssignableFrom(target.GetType()))
            {
                case true:
                    result = target;
                    return true;
                default:
                    result = null;
                    return false;
            }
        }

        /// <summary>
        /// Provides implementation for type conversion operations.
        /// </summary>
        /// <param name="binder">Provides information about the conversion operation.</param>
        /// <param name="result">The result of the type conversion operation.</param>
        /// <returns><see langword="true"/> if the operation is successful; otherwise, <see langword="false"/>.</returns>
        public override bool TryConvert(ConvertBinder binder, out object result)
        {
            return TryConvert(this, binder, out result);
        }
        #endregion

        #region Unary Operations

        internal static bool TryUnaryOperation(IScriptObject target, UnaryOperationBinder binder, out object result)
        {
            try
            {
                result = RuntimeHelpers.UnaryOperation(target, binder.Operation, binder.GetState());
                return true;
            }
            catch (UnsupportedOperationException e)
            {
                result = e;
                return false;
            }
        }

        /// <summary>
        /// Provides implementation for unary operations.
        /// </summary>
        /// <param name="binder">Provides information about the unary operation.</param>
        /// <param name="result">The result of the unary operation.</param>
        /// <returns><see langword="true"/> if the operation is successful; otherwise, <see langword="false"/>.</returns>
        public sealed override bool TryUnaryOperation(UnaryOperationBinder binder, out object result)
        {
            return TryUnaryOperation(this, binder, out result);
        }

        /// <summary>
        /// Performs unary operation.
        /// </summary>
        /// <param name="operator"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        public IScriptObject UnaryOperation(ScriptCodeUnaryOperatorType @operator, InterpreterState state)
        {
            switch (@operator)
            {
                case ScriptCodeUnaryOperatorType.IncrementPrefix:
                    return PreIncrementAssign(state);
                case ScriptCodeUnaryOperatorType.IncrementPostfix:
                    return PostIncrementAssign(state);
                case ScriptCodeUnaryOperatorType.DecrementPrefix:
                    return PreDecrementAssign(state);
                case ScriptCodeUnaryOperatorType.DecrementPostfix:
                    return PostDecrementAssign(state);
                case ScriptCodeUnaryOperatorType.Plus:
                    return UnaryPlus(state);
                case ScriptCodeUnaryOperatorType.Minus:
                    return UnaryMinus(state);
                case ScriptCodeUnaryOperatorType.Negate:
                    return Not(state);
                case ScriptCodeUnaryOperatorType.SquarePrefix:
                    return PreSquareAssign(state);
                case ScriptCodeUnaryOperatorType.SquarePostfix:
                    return PostSquareAssign(state);
                case ScriptCodeUnaryOperatorType.TypeOf:
                    return GetContractBinding();
                case ScriptCodeUnaryOperatorType.VoidCheck:
                    return IsVoid(state);
                case ScriptCodeUnaryOperatorType.Intern:
                    return Intern(state);
                default:
                    throw new UnsupportedOperationException(state);
            }
        }

        internal virtual ScriptObject Intern(InterpreterState state)
        {
            return this;
        }

        /// <summary>
        /// Determines whether the current object is void.
        /// </summary>
        /// <param name="state">Internal interpreter state.</param>
        /// <returns></returns>
        protected virtual IScriptObject IsVoid(InterpreterState state)
        {
            return Convert<bool>(IsVoid(this));
        }

        /// <summary>
        /// Applies postfixed ** operator to the current object.
        /// </summary>
        /// <param name="state">Internal interpreter state.</param>
        /// <returns>The operation result</returns>
        protected virtual IScriptObject PostSquareAssign(InterpreterState state)
        {
            throw new UnsupportedOperationException(state);
        }

        /// <summary>
        /// Applies prefixed ** operator to the current object.
        /// </summary>
        /// <param name="state">Internal interpreter state.</param>
        /// <returns>The operation result</returns>
        protected virtual IScriptObject PreSquareAssign(InterpreterState state)
        {
            throw new UnsupportedOperationException(state);
        }

        /// <summary>
        /// Applies bitwise complement or logicat negation.
        /// </summary>
        /// <param name="state">Internal interpreter state.</param>
        /// <returns>The operation result.</returns>
        protected virtual IScriptObject Not(InterpreterState state)
        {
            throw new UnsupportedOperationException(state);
        }

        /// <summary>
        /// Applies negation to the object.
        /// </summary>
        /// <param name="state">Internal interpreter state.</param>
        /// <returns>Negation result.</returns>
        protected virtual IScriptObject UnaryMinus(InterpreterState state)
        {
            throw new UnsupportedOperationException(state);
        }

        /// <summary>
        /// Applies unary plus to the object.
        /// </summary>
        /// <param name="state">Internal interpreter state.</param>
        /// <returns>The operation result.</returns>
        protected virtual IScriptObject UnaryPlus(InterpreterState state)
        {
            throw new UnsupportedOperationException(state);
        }

        /// <summary>
        /// Performs prefixed decrement on the object.
        /// </summary>
        /// <param name="state">Internal interpreter state.</param>
        /// <returns>The decremented object.</returns>
        protected virtual IScriptObject PreDecrementAssign(InterpreterState state)
        {
            throw new UnsupportedOperationException(state);
        }

        /// <summary>
        /// Performs postfixed decrement on the object.
        /// </summary>
        /// <param name="state">Internal interpreter state.</param>
        /// <returns>The decremented object.</returns>
        protected virtual IScriptObject PostDecrementAssign(InterpreterState state)
        {
            throw new UnsupportedOperationException(state);
        }

        /// <summary>
        /// Performs prefixed increment on the object.
        /// </summary>
        /// <param name="state">Internal interpreter state.</param>
        /// <returns>The incremented object.</returns>
        protected virtual IScriptObject PreIncrementAssign(InterpreterState state)
        {
            throw new UnsupportedOperationException(state);
        }

        /// <summary>
        /// Performs postfixed increment on the object.
        /// </summary>
        /// <param name="state">Internal interpreter state.</param>
        /// <returns>The incremented object.</returns>
        protected virtual IScriptObject PostIncrementAssign(InterpreterState state)
        {
            throw new UnsupportedOperationException(state);
        }

        #endregion

        /// <summary>
        /// Returns a collection of the dynamic members.
        /// </summary>
        /// <returns></returns>
        public sealed override IEnumerable<string> GetDynamicMemberNames()
        {
            return Slots;
        }

        /// <summary>
        /// Returns a collection of the available slots.
        /// </summary>
        /// <returns>The collection of the available slots.</returns>
        /// <remarks>In the default implementation this property returns an empty collection.</remarks>
        public virtual ICollection<string> Slots
        {
            get { return new string[0]; }
        }

        /// <summary>
        /// Gets or sets value to the aggregated object.
        /// </summary>
        /// <param name="slotName"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        public virtual IScriptObject this[string slotName, InterpreterState state]
        {
            get
            {
                if (state.Context == InterpretationContext.Unchecked)
                    return Void;
                else throw new SlotNotFoundException(slotName, state);
            }
            set { if (state.Context == InterpretationContext.Checked)throw new SlotNotFoundException(slotName, state); }
        }

        /// <summary>
        /// Gets a contract binding for the object.
        /// </summary>
        /// <returns>The contract binding.</returns>
        public abstract IScriptContract GetContractBinding();

        #region IEquatable<IScriptObject> Members

        private bool EqualsFast(IScriptObject other)
        {
            if (other is IScriptProxyObject)
                other = ((IScriptProxyObject)other).Unwrap(InterpreterState.Current);
            other = Equals(other, InterpreterState.Current);
            return SystemConverter.GetTypeCode(other) == TypeCode.Boolean && SystemConverter.ToBoolean(other);
        }

        bool IEquatable<IScriptObject>.Equals(IScriptObject other)
        {
            return EqualsFast(other);
        }

        #endregion

        /// <summary>
        /// Determines whether the current object is equal to the other.
        /// </summary>
        /// <param name="other">Other object to be compared.</param>
        /// <returns><see langword="true"/> if the current object is equal to another; otherwise, <see langword="false"/>.</returns>
        public sealed override bool Equals(object other)
        {
            return other is IScriptObject && EqualsFast((IScriptObject)other);
        }

        /// <summary>
        /// Computes a hash code for the object.
        /// </summary>
        /// <returns>The hash code for the object.</returns>
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        /// <summary>
        /// Produces clone of the current object.
        /// </summary>
        /// <returns>The clone of the current object.</returns>
        protected virtual ScriptObject Clone()
        {
            return MemberwiseClone() as ScriptObject;
        }

        /// <summary>
        /// Nullifies all cached internal data.
        /// </summary>
        public virtual void Clear()
        {
        }

        #region ICloneable Members

        object ICloneable.Clone()
        {
            return Clone();
        }

        #endregion

        private static readonly MethodInfo SlotSetter = typeof(IScriptObject).GetMethod("set_Item", new[] { typeof(string), typeof(InterpreterState), typeof(IScriptObject) });
        private static readonly MethodInfo IndexerSetter = typeof(IScriptObject).GetMethod("set_Item", new[] { typeof(IList<IScriptObject>), typeof(InterpreterState), typeof(IScriptObject) });
        private static readonly MethodInfo SlotGetter = typeof(IScriptObject).GetMethod("get_Item", new[] { typeof(string), typeof(InterpreterState) });
        private static readonly MethodInfo IndexerGetter = typeof(IScriptObject).GetMethod("get_Item", new[] { typeof(IList<IScriptObject>), typeof(InterpreterState) });

        internal static Expression AsRightSide(Expression expr, ParameterExpression stateHolder)
        {
            if (RuntimeHelpers.IsRuntimeVariable(expr))
                expr = RuntimeSlotBase.GetValue(expr, stateHolder);
            else if (expr is MethodCallExpression)
            {
                var call = (MethodCallExpression)expr;
                if (call.Method == SlotSetter)
                    expr = Expression.Block(expr, LinqHelpers.BodyOf<IScriptObject, string, InterpreterState, IScriptObject, MethodCallExpression>((obj, name, state) => obj[name, state]).
                        Update(call.Object, new[] { call.Arguments[0], call.Arguments[1] }));
                else if (call.Method == IndexerSetter)
                    expr = Expression.Block(expr, LinqHelpers.BodyOf<IScriptObject, IList<IScriptObject>, InterpreterState, IScriptObject, MethodCallExpression>((obj, indicies, state) => obj[indicies, state]).
                        Update(call.Object, new[] { call.Arguments[0], call.Arguments[1] }));
            }
            return expr;
        }

        internal static IEnumerable<Expression> AsRightSide(IEnumerable<Expression> expressions, ParameterExpression stateHolder)
        {
            return from e in expressions select AsRightSide(e, stateHolder);
        }

        private static Expression AsLeftSide(Expression left, Expression right, ParameterExpression stateHolder)
        {
            if (RuntimeHelpers.IsRuntimeVariable(left))
                return RuntimeSlotBase.SetValue(left, right, stateHolder);
            else if (left is MethodCallExpression)
            {
                var callexpr = (MethodCallExpression)left;
                if (callexpr.Method == IndexerGetter)   //convert to setter
                    return Expression.Call(callexpr.Object, IndexerSetter, callexpr.Arguments[0], callexpr.Arguments[1], right);
                else if (callexpr.Method == SlotGetter)
                    return Expression.Call(callexpr.Object, SlotSetter, callexpr.Arguments[0], callexpr.Arguments[1], right);
            }
            return left;
        }

        internal static Expression BinaryOperation(Expression left, ScriptCodeBinaryOperatorType @operator, Expression right, ParameterExpression stateVar)
        {
            right = AsRightSide(right, stateVar);
            switch (@operator)
            {
                case ScriptCodeBinaryOperatorType.Assign:
                    return AsLeftSide(left, right, stateVar);
                case ScriptCodeBinaryOperatorType.AdditiveAssign:
                    right = BinaryOperation(left, ScriptCodeBinaryOperatorType.Add, right, stateVar);
                    return AsLeftSide(left, right, stateVar);
                case ScriptCodeBinaryOperatorType.OrElse:
                case ScriptCodeBinaryOperatorType.AndAlso:
                    left = AsRightSide(left, stateVar);
                    break;
                case ScriptCodeBinaryOperatorType.SubtractiveAssign:
                    right = BinaryOperation(left, ScriptCodeBinaryOperatorType.Subtract, right, stateVar);
                    return AsLeftSide(left, right, stateVar);
                case ScriptCodeBinaryOperatorType.DivideAssign:
                    return RuntimeHelpers.IsRuntimeVariable(left) ?
                        BinaryOperation(left, ScriptCodeBinaryOperatorType.Assign, BinaryOperation(left, ScriptCodeBinaryOperatorType.Divide, right, stateVar), stateVar) :
                        left;
                case ScriptCodeBinaryOperatorType.ExclusionAssign:
                    right = BinaryOperation(left, ScriptCodeBinaryOperatorType.Exclusion, right, stateVar);
                    return AsLeftSide(left, right, stateVar);
                case ScriptCodeBinaryOperatorType.Expansion:
                    right = BinaryOperation(left, ScriptCodeBinaryOperatorType.Union, right, stateVar);
                    return AsLeftSide(left, right, stateVar);
                case ScriptCodeBinaryOperatorType.Initializer:
                    return RuntimeHelpers.IsRuntimeVariable(left) ?
                        RuntimeSlotBase.Initialize(left, right, stateVar) :
                        left;
                case ScriptCodeBinaryOperatorType.ModuloAssign:
                    return RuntimeHelpers.IsRuntimeVariable(left) ?
                         BinaryOperation(left, ScriptCodeBinaryOperatorType.Assign, BinaryOperation(left, ScriptCodeBinaryOperatorType.Modulo, right, stateVar), stateVar) :
                        left;
                case ScriptCodeBinaryOperatorType.MultiplicativeAssign:
                    right = BinaryOperation(left, ScriptCodeBinaryOperatorType.Multiply, right, stateVar);
                    return AsLeftSide(left, right, stateVar);
                case ScriptCodeBinaryOperatorType.Reduction:
                    right = BinaryOperation(left, ScriptCodeBinaryOperatorType.Intersection, right, stateVar);
                    return AsLeftSide(left, right, stateVar);
                default: left = AsRightSide(left, stateVar); break;
            }
            var binaryOp = LinqHelpers.BodyOf<IScriptObject, ScriptCodeBinaryOperatorType, IScriptObject, InterpreterState, IScriptObject, MethodCallExpression>((l, op, r, s) => l.BinaryOperation(op, r, s));
            return binaryOp.Update(left, new Expression[] { LinqHelpers.Constant(@operator), right, stateVar });
        }

        internal static Expression UnaryOperation(Expression operand, ScriptCodeUnaryOperatorType @operator, ParameterExpression stateVar)
        {
            switch (@operator)
            {
                case ScriptCodeUnaryOperatorType.DecrementPrefix:
                case ScriptCodeUnaryOperatorType.IncrementPrefix:
                case ScriptCodeUnaryOperatorType.SquarePrefix:
                    var right = AsRightSide(operand, stateVar);
                    if(!ReferenceEquals(operand, right))
                    return AsLeftSide(operand, UnaryOperation(right, @operator, stateVar), stateVar);
                    break;
                case ScriptCodeUnaryOperatorType.DecrementPostfix:
                case ScriptCodeUnaryOperatorType.IncrementPostfix:
                case ScriptCodeUnaryOperatorType.SquarePostfix:
                    var temp = ParameterExpression.Parameter(typeof(IScriptObject));
                    right = AsRightSide(operand, stateVar);
                    if (!ReferenceEquals(right, operand))
                        return Expression.Block(new[] { temp },
                        Expression.Assign(temp, right),    //save value to the temp variable
                        AsLeftSide(operand, UnaryOperation(temp, @operator, stateVar), stateVar),   //save the modified value to the slot
                        temp);  //return the unmodified object
                    break;
            }
            var unaryOp = LinqHelpers.BodyOf<IScriptObject, ScriptCodeUnaryOperatorType, InterpreterState, IScriptObject, MethodCallExpression>((l, op, s) => l.UnaryOperation(op, s));
            return unaryOp.Update(AsRightSide(operand, stateVar), new Expression[] { LinqHelpers.Constant(@operator), stateVar });
        }

        internal static MethodCallExpression BindInvoke(Expression target, IEnumerable<Expression> args, ParameterExpression stateVar)
        {
            target= AsRightSide(target, stateVar);
            args = AsRightSide(args ?? Enumerable.Empty<Expression>(), stateVar);
            var invoker = LinqHelpers.BodyOf<IScriptObject, IScriptObject[], InterpreterState, IScriptObject, MethodCallExpression>((tgt, a, s) => tgt.Invoke(a, s));
            return invoker.Update(target, new Expression[] { Expression.NewArrayInit(typeof(IScriptObject), args), stateVar });
        }

        internal static MethodCallExpression GetValue(Expression target, string slotName, ParameterExpression stateVar)
        {
            target = AsRightSide(target, stateVar);
            var indexer = LinqHelpers.BodyOf<IScriptObject, string, InterpreterState, IScriptObject, MethodCallExpression>((@this, n, s) => @this[n, s]);
            return indexer.Update(target, new Expression[] { LinqHelpers.Constant(slotName), stateVar });
        }

        internal static MethodCallExpression SetValue(Expression target, string slotName, Expression value, ParameterExpression stateVar)
        {
            return Expression.Call(AsRightSide(target, stateVar), SlotSetter, LinqHelpers.Constant(slotName), stateVar, AsRightSide(value, stateVar));
        }

        internal static MethodCallExpression GetValue(Expression target, IEnumerable<Expression> indicies, ParameterExpression stateVar)
        {
            target = AsRightSide(target, stateVar);
            indicies = AsRightSide(indicies ?? Enumerable.Empty<Expression>(), stateVar);
            return LinqHelpers.BodyOf<IScriptObject, IList<IScriptObject>, InterpreterState, IScriptObject, MethodCallExpression>((obj, i, s) => obj[i, s]).
                Update(target, new Expression[] { Expression.NewArrayInit(typeof(IScriptObject), indicies), stateVar });
        }

        internal static MethodCallExpression SetValue(Expression target, IEnumerable<Expression> indicies, Expression value, ParameterExpression stateVar)
        {
            target = AsRightSide(target, stateVar);
            indicies = AsRightSide(indicies ?? Enumerable.Empty<Expression>(), stateVar);
            value = AsRightSide(value, stateVar);
            return Expression.Call(target, IndexerSetter, new Expression[] { Expression.NewArrayInit(typeof(IScriptObject), indicies), stateVar, value });
        }

        internal static MethodCallExpression BindIsVoid(Expression target)
        {
            var isv = LinqHelpers.BodyOf<object, bool, MethodCallExpression>(ob => IsVoid(ob));
            return isv.Update(null, new[] { target });
        }

        /// <summary>
        /// Gets value of the named slot.
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="slotName"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        public static IScriptObject RtlGetValue(IScriptObject obj, IScriptObject slotName, InterpreterState state)
        {
            if (SystemConverter.GetTypeCode(slotName) == TypeCode.String)
                return obj[slotName.ToString(), state];
            else if (state.Context == InterpretationContext.Unchecked)
                return ScriptObject.Void;
            else throw new SlotNotFoundException(slotName.ToString(), state);
        }

        internal static MethodCallExpression RtlGetValue(Expression obj, Expression slotName, ParameterExpression state)
        {
            obj = AsRightSide(obj, state);
            slotName = AsRightSide(slotName, state);
            return LinqHelpers.BodyOf<IScriptObject, IScriptObject, InterpreterState, IScriptObject, MethodCallExpression>((o, n, s) => RtlGetValue(o, n, s)).
                Update(null, new[] { obj, slotName, state });
        }

        /// <summary>
        /// Sets value of the named slot.
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="slotName"></param>
        /// <param name="value"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        public static IScriptObject RtlSetValue(IScriptObject obj, IScriptObject slotName, IScriptObject value, InterpreterState state)
        {
            if (SystemConverter.GetTypeCode(slotName) == TypeCode.String)
                return obj[slotName.ToString(), state] = value;
            else if (state.Context == InterpretationContext.Unchecked)
                return ScriptObject.Void;
            else throw new SlotNotFoundException(slotName.ToString(), state);
        }

        internal static MethodCallExpression RtlSetValue(Expression obj, Expression slotName, Expression value, ParameterExpression state)
        {
            obj = AsRightSide(obj, state);
            slotName = AsRightSide(slotName, state);
            value = AsRightSide(value, state);
            return LinqHelpers.BodyOf<IScriptObject, IScriptObject, IScriptObject, InterpreterState, IScriptObject, MethodCallExpression>((o, n, v, s) => RtlSetValue(o, n, v, s)).
                Update(null, new[] { obj, slotName, value });
        }

        internal static Expression Null
        {
            get { return Expression.Constant(null, typeof(IScriptObject)); }
        }

        /// <summary>
        /// Returns default string representation of DynamicScript object.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        protected static string ToString(IScriptObject obj)
        {
            var hash = (obj ?? Void).GetHashCode();
            return String.Concat("0x", hash.ToString("X"));
        }

        /// <summary>
        /// Returns a string representation of DynamicScript object.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return ToString(this);
        }

        internal static IScriptObject Unite(IEnumerable<IScriptObject> objects, InterpreterState state)
        {
            if (objects == null) objects = Enumerable.Empty<IScriptObject>();
            return objects.Aggregate((left, right) => left.BinaryOperation(ScriptCodeBinaryOperatorType.Union, right, state));
        }

        internal static IScriptObject Unite(IEnumerable<IScriptObject> set1, IEnumerable<IScriptObject> set2, InterpreterState state)
        {
            return Unite(new[] { Unite(set1, state), Unite(set2, state) }, state);
        }

        internal static IScriptObject Intersect(IEnumerable<IScriptObject> objects, InterpreterState state)
        {
            if (objects == null) objects = Enumerable.Empty<IScriptObject>();
            return objects.Aggregate((left, right) => left.BinaryOperation(ScriptCodeBinaryOperatorType.Intersection, right, state));
        }

        internal static IScriptObject Intersect(IEnumerable<IScriptObject> set1, IEnumerable<IScriptObject> set2, InterpreterState state)
        {
            return Unite(new[] { Unite(set1, state), Unite(set2, state) }, state);
        }

        internal static bool ReferenceEquals(IScriptObject value1, IScriptObject value2)
        {
            return object.ReferenceEquals(value1, value2);
        }

        /// <summary>
        /// Represents an empty array of script objects.
        /// </summary>
        internal protected static readonly IScriptObject[] EmptyArray = new IScriptObject[0];
    }
}
