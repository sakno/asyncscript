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
    using QDebugInfo = Compiler.ScriptDebugInfo;
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
        IEnumerable<KeyValuePair<string, IRuntimeSlot>>, 
        IComparable<IScriptObject>,
        IEquatable<IScriptObject>, 
        ICloneable
    {
        #region Nested Types
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
        /// Represents slot searcher that iterates through properties of implemented interfaces
        /// in parallel manner.
        /// This class cannot be inherited.
        /// </summary>
        [ComVisible(false)]
        private sealed class SlotSearcher : ParallelSearch<string, PropertyInfo, PropertyInfo>
        {
            private SlotSearcher(string slotName)
                : base(slotName)
            {
            }

            protected override bool Match(PropertyInfo element, string slotName, out PropertyInfo prop)
            {
                switch (StringEqualityComparer.Equals(slotName, element.Name))
                {
                    case true:
                        prop = element;
                        return true;
                    default:
                        prop = null;
                        return false;
                }
            }

            public static IEnumerable<PropertyInfo> GetSlotHolders(object owner)
            {
                return from iface in owner.GetType().GetInterfaces()
                       where Attribute.IsDefined(iface, typeof(SlotStoreAttribute))
                       from prop in iface.GetProperties()
                       select prop;
            }

            public static IRuntimeSlot Find(object owner, string slotName)
            {
                var searcher = new SlotSearcher(slotName);
                searcher.Find(GetSlotHolders(owner));
                return searcher.Success ? searcher.Result.GetValue(owner, null) as IRuntimeSlot : null;
            }
        }

        /// <summary>
        /// Represents an abstract runtime slot.
        /// </summary>
        [ComVisible(false)]
        [Serializable]
        public abstract class RuntimeSlotBase : DynamicObject, IRuntimeSlot, ISerializable
        {
            #region Nested Types
            [ComVisible(false)]
            [Serializable]
            private sealed class MissingSlot : RuntimeSlotBase, IEquatable<MissingSlot>
            {
                private const string SlotNameHolder = "Name";
                private readonly string m_slotName;

                public MissingSlot(string slotName)
                {
                    m_slotName = slotName;
                }

                private MissingSlot(SerializationInfo info, StreamingContext context)
                    : this(info.GetString(SlotNameHolder))
                {

                }

                public bool Equals(MissingSlot other)
                {
                    return other != null ? StringEqualityComparer.Equals(Name, other.Name) : false;
                }

                public override bool Equals(IRuntimeSlot other)
                {
                    return Equals(other as MissingSlot);
                }

                public override bool Equals(object obj)
                {
                    return Equals(obj as MissingSlot);
                }

                public override int GetHashCode()
                {
                    return StringEqualityComparer.GetHashCode(Name);
                }

                public string Name
                {
                    get { return m_slotName; }
                }

                protected override void GetObjectData(SerializationInfo info, StreamingContext context)
                {
                    info.AddValue(SlotNameHolder, Name);
                }

                public override IScriptObject GetValue(InterpreterState state)
                {
                    switch (state.Context)
                    {
                        case InterpretationContext.Unchecked: return Void;
                        default: throw new SlotNotFoundException(Name, state);
                    }
                }

                public override void SetValue(IScriptObject value, InterpreterState state)
                {
                    switch (state.Context)
                    {
                        case InterpretationContext.Unchecked: return;
                        default: throw new SlotNotFoundException(Name, state);
                    }
                }

                public override IScriptContract ContractBinding
                {
                    get { return Void; }
                }

                public override RuntimeSlotAttributes Attributes
                {
                    get { return RuntimeSlotAttributes.None; }
                }

                protected override IRuntimeSlot this[string slotName, InterpreterState state]
                {
                    get { return new MissingSlot(slotName); }
                }

                protected override IRuntimeSlot this[IScriptObject[] args, InterpreterState state]
                {
                    get { throw new UnsupportedOperationException(state); }
                }

                protected override ICollection<string> Slots
                {
                    get { return new string[0]; }
                }

                public override bool DeleteValue()
                {
                    return false;
                }

                public sealed override string ToString()
                {
                    return Name;
                }
            }
            #endregion
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
            /// Provides the implementation for operations that invoke an object.
            /// </summary>
            /// <param name="binder">Provides information about the invoke operation.</param>
            /// <param name="args">The arguments that are passed to the object during the invoke operation.</param>
            /// <param name="result">The result of the object invocation.</param>
            /// <returns><see langword="true"/> if the operation is successful; otherwise, <see langword="false"/>.</returns>
            public sealed override bool TryInvoke(InvokeBinder binder, object[] args, out object result)
            {
                return ScriptObject.TryInvoke(GetValue(binder.GetState()), binder, args, out result);
            }

            /// <summary>
            /// Creates a new instance of the missing slot.
            /// </summary>
            /// <param name="slotName">The name of the missing slot.</param>
            /// <returns></returns>
            public static RuntimeSlotBase Missing(string slotName)
            {
                return new MissingSlot(slotName);
            }

            internal static bool IsMissing(IRuntimeSlot slot)
            {
                return slot == null || slot is MissingSlot;
            }

            /// <summary>
            /// Sets value to the uninitialized slot.
            /// </summary>
            /// <param name="value">The value to set.</param>
            /// <param name="state">Internal interpreter state.</param>
            /// <returns>A value used to initialize slot; or the value that is already stored in the slot.</returns>
            protected IScriptObject Initialize(IScriptObject value, InterpreterState state)
            {
                switch (HasValue)
                {
                    case true:
                        return GetValue(state);
                    default:
                        SetValue(value, state);
                        return value;
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
                return ScriptObject.TryBinaryOperation(this, binder, arg, out result);
            }

            /// <summary>
            /// Binds to the member value.
            /// </summary>
            /// <param name="binder"></param>
            /// <param name="result"></param>
            /// <returns></returns>
            public sealed override bool TryGetMember(GetMemberBinder binder, out object result)
            {
                return ScriptObject.TryGetMember(GetValue(binder.GetState()), binder, out result);
            }


            /// <summary>
            /// Binds to the member assignment.
            /// </summary>
            /// <param name="binder"></param>
            /// <param name="value"></param>
            /// <returns></returns>
            public sealed override bool TrySetMember(SetMemberBinder binder, object value)
            {
                return ScriptObject.TrySetMember(GetValue(binder.GetState()), binder, value);
            }

            /// <summary>
            /// Provides implementation for unary operations.
            /// </summary>
            /// <param name="binder">Provides information about the unary operation.</param>
            /// <param name="result">The result of the unary operation.</param>
            /// <returns><see langword="true"/> if the operation is successful; otherwise, <see langword="false"/>.</returns>
            public sealed override bool TryUnaryOperation(UnaryOperationBinder binder, out object result)
            {
                return TryUnaryOperation(binder, out result);
            }

            /// <summary>
            /// Peforms conversion through dynamic context.
            /// </summary>
            /// <param name="binder"></param>
            /// <param name="result"></param>
            /// <returns></returns>
            public sealed override bool TryConvert(ConvertBinder binder, out object result)
            {
                return ScriptObject.TryConvert(GetValue(InterpreterState.Current), binder, out result);
            }

            /// <summary>
            /// Binds to the member invocation.
            /// </summary>
            /// <param name="binder"></param>
            /// <param name="args"></param>
            /// <param name="result"></param>
            /// <returns></returns>
            public sealed override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
            {
                return ScriptObject.TryInvokeMember(GetValue(binder.GetState()), binder, args, out result);
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
                return ScriptObject.TryGetIndex(GetValue(binder.GetState()), binder, indexes, out result);
            }

            /// <summary>
            /// Binds to the indexer setter.
            /// </summary>
            /// <param name="binder"></param>
            /// <param name="indexes"></param>
            /// <param name="value"></param>
            /// <returns></returns>
            public sealed override bool TrySetIndex(SetIndexBinder binder, object[] indexes, object value)
            {
                return ScriptObject.TrySetIndex(GetValue(binder.GetState()), binder, indexes, value);
            }

            IScriptObject IScriptObject.GetRuntimeDescriptor(string slotName, InterpreterState state)
            {
                return GetValue(state).GetRuntimeDescriptor(slotName, state);
            }

            private IScriptObject BinaryOperation(ScriptCodeBinaryOperatorType @operator, IScriptProxyObject arg, InterpreterState state)
            {
                return arg.Enqueue(this, @operator, state);
            }

            private IScriptObject BinaryOperation(ScriptCodeBinaryOperatorType @operator, IScriptObject arg, InterpreterState state)
            {
                if (arg is IScriptProxyObject)
                    return BinaryOperation(@operator, (IScriptProxyObject)arg, state);
                else switch (@operator)
                {
                    case ScriptCodeBinaryOperatorType.Assign:
                        SetValue(arg, state);
                        return this;
                    case ScriptCodeBinaryOperatorType.Expansion:
                        return BinaryOperation(ScriptCodeBinaryOperatorType.Assign, BinaryOperation(ScriptCodeBinaryOperatorType.Union, arg, state), state);
                    case ScriptCodeBinaryOperatorType.AdditiveAssign:
                        return BinaryOperation(ScriptCodeBinaryOperatorType.Assign, BinaryOperation(ScriptCodeBinaryOperatorType.Add, arg, state), state);
                    case ScriptCodeBinaryOperatorType.Reduction:
                        return BinaryOperation(ScriptCodeBinaryOperatorType.Assign, BinaryOperation(ScriptCodeBinaryOperatorType.Intersection, arg, state), state);
                    case ScriptCodeBinaryOperatorType.DivideAssign:
                        return BinaryOperation(ScriptCodeBinaryOperatorType.Assign, BinaryOperation(ScriptCodeBinaryOperatorType.Divide, arg, state), state);
                    case ScriptCodeBinaryOperatorType.ModuloAssign:
                        return BinaryOperation(ScriptCodeBinaryOperatorType.Assign, BinaryOperation(ScriptCodeBinaryOperatorType.ModuloAssign, arg, state), state);
                    case ScriptCodeBinaryOperatorType.MultiplicativeAssign:
                        return BinaryOperation(ScriptCodeBinaryOperatorType.Assign, BinaryOperation(ScriptCodeBinaryOperatorType.Multiply, arg, state), state);
                    case ScriptCodeBinaryOperatorType.SubtractiveAssign:
                        return BinaryOperation(ScriptCodeBinaryOperatorType.Assign, BinaryOperation(ScriptCodeBinaryOperatorType.Subtract, arg, state), state);
                    case ScriptCodeBinaryOperatorType.Initializer:
                        return Initialize(arg, state);
                    default:
                        return GetValue(state).BinaryOperation(@operator, arg, state);
                }

            }

            private IScriptObject UnaryOperation(ScriptCodeUnaryOperatorType @operator, InterpreterState state)
            {
                switch (@operator)
                {
                    case ScriptCodeUnaryOperatorType.DecrementPrefix:
                        var value = GetValue(state).UnaryOperation(ScriptCodeUnaryOperatorType.DecrementPrefix, state);
                        TrySetValue(value, state);
                        return value;
                    case ScriptCodeUnaryOperatorType.IncrementPrefix:
                        value = GetValue(state).UnaryOperation(ScriptCodeUnaryOperatorType.IncrementPrefix, state);
                        TrySetValue(value, state);
                        return value;
                    case ScriptCodeUnaryOperatorType.SquarePrefix:
                        value = GetValue(state).UnaryOperation(ScriptCodeUnaryOperatorType.SquarePrefix, state);
                        TrySetValue(value, state);
                        return value;
                    case ScriptCodeUnaryOperatorType.DecrementPostfix:
                        value = GetValue(state);
                        TrySetValue(value.UnaryOperation(ScriptCodeUnaryOperatorType.DecrementPostfix, state), state);
                        return value;
                    case ScriptCodeUnaryOperatorType.IncrementPostfix:
                        value = GetValue(state);
                        TrySetValue(value.UnaryOperation(ScriptCodeUnaryOperatorType.IncrementPostfix, state), state);
                        return value;
                    case ScriptCodeUnaryOperatorType.SquarePostfix:
                        value = GetValue(state);
                        TrySetValue(value.UnaryOperation(ScriptCodeUnaryOperatorType.SquarePostfix, state), state);
                        return value;
                    default:
                        return GetValue(state).UnaryOperation(@operator, state);
                }
            }

            private IScriptObject Invoke(IList<IScriptObject> args, InterpreterState state)
            {
                return GetValue(state).Invoke(args, state);
            }

            #region IScriptObject Members

            IScriptObject IScriptObject.BinaryOperation(ScriptCodeBinaryOperatorType @operator, IScriptObject right, InterpreterState state)
            {
                return BinaryOperation(@operator, right, state);
            }

            IScriptObject IScriptObject.UnaryOperation(ScriptCodeUnaryOperatorType @operator, InterpreterState state)
            {
                return UnaryOperation(@operator, state);
            }

            IScriptObject IScriptObject.Invoke(IList<IScriptObject> args, InterpreterState state)
            {
                return Invoke(args, state);
            }

            IScriptContract IScriptObject.GetContractBinding()
            {
                return ContractBinding;
            }

            IRuntimeSlot IScriptObject.this[string slotName, InterpreterState state]
            {
                get { return this[slotName, state]; }
            }

            IRuntimeSlot IScriptObject.this[IScriptObject[] args, InterpreterState state]
            {
                get { return this[args, state]; }
            }

            ICollection<string> IScriptObject.Slots
            {
                get { return Slots; }
            }

            #endregion

            /// <summary>
            /// Reads value stored in the slot.
            /// </summary>
            /// <param name="state">Internal interpretation state.</param>
            /// <returns>The value restored from the slot.</returns>
            public abstract IScriptObject GetValue(InterpreterState state);

            /// <summary>
            /// Stores value in the holder.
            /// </summary>
            /// <param name="value">The value to store.</param>
            /// <param name="state">Internal interpreter state.</param>
            public abstract void SetValue(IScriptObject value, InterpreterState state);

            /// <summary>
            /// Attempts to save value into this runtime slot.
            /// </summary>
            /// <param name="value"></param>
            /// <param name="state"></param>
            /// <returns></returns>
            protected bool TrySetValue(IScriptObject value, InterpreterState state)
            {
                switch ((Attributes & RuntimeSlotAttributes.Immutable) == 0)
                {
                    case true: SetValue(value, state); return true;
                    default: return false;
                }
            }

            /// <summary>
            /// Gets static contract binding of the slot.
            /// </summary>
            public abstract IScriptContract ContractBinding
            {
                get;
            }

            IScriptContract IRuntimeSlot.GetContractBinding()
            {
                return ContractBinding;
            }

            /// <summary>
            /// Gets slot semantic.
            /// </summary>
            public abstract RuntimeSlotAttributes Attributes
            {
                get;
            }

            /// <summary>
            /// Extracts slot accessor.
            /// </summary>
            /// <param name="slotName">The name of the slot.</param>
            /// <param name="state">Internal interpreter state.</param>
            /// <returns></returns>
            protected virtual IRuntimeSlot this[string slotName, InterpreterState state]
            {
                get { return GetValue(state)[slotName, state]; }
            }

            /// <summary>
            /// Extracts element accessor.
            /// </summary>
            /// <param name="args">The position of the element.</param>
            /// <param name="state">Internal interpreter state.</param>
            /// <returns>The element accessor.</returns>
            protected virtual IRuntimeSlot this[IScriptObject[] args, InterpreterState state]
            {
                get
                {
                    return GetValue(state)[args, state];
                }
            }

            /// <summary>
            /// Gets collection of the slots.
            /// </summary>
            protected abstract ICollection<string> Slots
            {
                get;
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
                switch (typeof(IRuntimeSlot).IsAssignableFrom(slotHolder.Type))
                {
                    case true:
                        var getValueMethod = LinqHelpers.BodyOf<IRuntimeSlot, InterpreterState, IScriptObject, MethodCallExpression>((slot, state) => slot.GetValue(state));
                        return getValueMethod.Update(slotHolder, new[] { stateVar });
                    default: return slotHolder;
                }
            }

            internal static Expression SetValue(Expression slotHolder, Expression value, ParameterExpression stateVar)
            {
                switch (typeof(IRuntimeSlot).IsAssignableFrom(slotHolder.Type))
                {
                    case true:
                        var setValueMethod = LinqHelpers.BodyOf<Action<IRuntimeSlot, IScriptObject, InterpreterState>, MethodCallExpression>((slot, v, s) => slot.SetValue(v, s));
                        return setValueMethod.Update(slotHolder, new Expression[] { value, stateVar });
                    default: return slotHolder;
                }
            }

            private static Expression Initialized(Expression slotExpr)
            {
                var prop = LinqHelpers.BodyOf<IRuntimeSlot, bool, MemberExpression>(slot => slot.HasValue);
                return prop.Update(slotExpr);
            }

            internal static Expression Initialize(Expression slotExpr, Expression initialization, ParameterExpression stateVar)
            {
                return RuntimeHelpers.IsRuntimeVariable(slotExpr) ?
                    Expression.Condition(Initialized(slotExpr), GetValue(slotExpr, stateVar), BindBinaryOperation(slotExpr, ScriptCodeBinaryOperatorType.Assign, initialization, stateVar)) :
                    slotExpr;
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

            /// <summary>
            /// Determines whether the current slot contains the same value as other.
            /// </summary>
            /// <param name="other">Other contract to compare.</param>
            /// <returns></returns>
            public abstract bool Equals(IRuntimeSlot other);
        }

        /// <summary>
        /// Represents runtime slot based on method from derived class.
        /// This class cannot be inherited.
        /// </summary>
        /// <typeparam name="TOwner">Type of the derived script object.</typeparam>
        [ComVisible(false)]
        [Serializable]
        protected sealed class WeakRuntimeSlot<TOwner> : RuntimeSlotBase, IEquatable<WeakRuntimeSlot<TOwner>>
            where TOwner: ScriptObject
        {
            private readonly WeakFunc<IScriptObject> m_getter;
            private readonly WeakAction<IScriptObject> m_setter;
            private readonly IScriptContract m_contract;

            /// <summary>
            /// Initializes a new runtime slot based on method from derived class.
            /// </summary>
            /// <param name="contractBinding">Static contract binding for this slot. Cannot be <see langword="null"/>.</param>
            /// <param name="getter">A delegate that represents implementation of getter method.</param>
            /// <param name="setter">A delegate that represents implementation of setter method.</param>
            /// <exception cref="System.ArgumentNullException"><paramref name="contractBinding"/> is <see langword="null"/>.</exception>
            public WeakRuntimeSlot(IScriptContract contractBinding, Func<IScriptObject> getter, Action<IScriptObject> setter=null)
            {
                if (contractBinding == null) throw new ArgumentNullException("contractBinding");
                m_getter = new WeakFunc<IScriptObject>(getter);
                m_setter = new WeakAction<IScriptObject>(setter);
                m_contract = contractBinding;
            }

            private static void MakePropertyAccessors(TOwner obj, Expression<Func<TOwner, IScriptObject>> getterExpr, Expression<Action<TOwner, IScriptObject>> setterExpr, out Func<IScriptObject> getter, out Action<IScriptObject> setter)
            {
                getter = getterExpr != null && getterExpr.Body is MethodCallExpression ?
                    Delegate.CreateDelegate(typeof(Func<IScriptObject>), obj, ((MethodCallExpression)getterExpr.Body).Method) as Func<IScriptObject> :
                    null;
                setter = setterExpr != null && setterExpr.Body is MethodCallExpression ?
                    Delegate.CreateDelegate(typeof(Action<IScriptObject>), obj, ((MethodCallExpression)setterExpr.Body).Method) as Action<IScriptObject> :
                    null;
            }

            /// <summary>
            /// Initializes a new runtime slot based on method from derived class.
            /// </summary>
            /// <param name="owner"></param>
            /// <param name="contractBinding"></param>
            /// <param name="getterExpr"></param>
            /// <param name="setterExpr"></param>
            public WeakRuntimeSlot(TOwner owner, IScriptContract contractBinding, Expression<Func<TOwner, IScriptObject>> getterExpr, Expression<Action<TOwner, IScriptObject>> setterExpr = null)
            {
                if (owner == null) throw new ArgumentNullException("owner");
                if (contractBinding == null) throw new ArgumentNullException("contractBinding");
                var getter = default(Func<IScriptObject>);
                var setter = default(Action<IScriptObject>);
                MakePropertyAccessors(owner, getterExpr, setterExpr, out getter, out setter);
                m_setter = setter != null ? new WeakAction<IScriptObject>(setter) : null;
                m_getter = getter != null ? new WeakFunc<IScriptObject>(getter) : null;
            }

            /// <summary>
            /// Gets setter method.
            /// </summary>
            public MethodInfo Setter
            {
                get { return m_setter != null ? m_setter.Method : null; }
            }

            /// <summary>
            /// Gets getter method.
            /// </summary>
            public MethodInfo Getter
            {
                get { return m_getter != null ? m_getter.Method : null; }
            }

            /// <summary>
            /// Gets runtime slot owner.
            /// </summary>
            public TOwner Owner
            {
                get
                {
                    if (m_getter != null && m_getter.IsAlive)
                        return m_getter.Target as TOwner;
                    else if (m_setter != null && m_setter.IsAlive)
                        return m_setter.Target as TOwner;
                    else return null;
                }
            }

            /// <summary>
            /// Restores value from slot.
            /// </summary>
            /// <param name="state">Internal interpreter state.</param>
            /// <returns></returns>
            public override IScriptObject GetValue(InterpreterState state)
            {
                switch (m_getter != null && m_getter.IsAlive)
                {
                    case true: return m_getter.Invoke();
                    default: throw new UnsupportedOperationException(state);
                }
            }

            /// <summary>
            /// Sets value in this slot.
            /// </summary>
            /// <param name="value">A value to store.</param>
            /// <param name="state">Internal interpreter state.</param>
            public override void SetValue(IScriptObject value, InterpreterState state)
            {
                switch (m_setter != null && m_setter.IsAlive)
                {
                    case true: 
                        m_setter.Invoke(value);
                        break;
                    default: throw new ConstantCannotBeChangedException(state);
                }
            }

            /// <summary>
            /// Gets static contract binding.
            /// </summary>
            public override IScriptContract ContractBinding
            {
                get { return m_contract; }
            }

            /// <summary>
            /// Gets semantic of this slot.
            /// </summary>
            public override RuntimeSlotAttributes Attributes
            {
                get { return m_setter != null ? RuntimeSlotAttributes.None : RuntimeSlotAttributes.Immutable; }
            }

            /// <summary>
            /// Gets collection of stored value slots.
            /// </summary>
            protected override ICollection<string> Slots
            {
                get { return m_getter != null && m_getter.IsAlive ? m_getter.Invoke().Slots : new string[0]; }
            }

            /// <summary>
            /// Deletes slot value.
            /// </summary>
            /// <returns><see langword="true"/> if slot value is deleted successfully; otherwise, <see langword="false"/>.</returns>
            public override bool DeleteValue()
            {
                var success = true;
                foreach (var weakref in new WeakReference[] { m_setter, m_getter })
                    if (weakref != null && weakref.IsAlive)
                    {
                        weakref.Target = null;
                        success = true;
                    }
                return success;
            }

            /// <summary>
            /// Determines whether this slot is equal to another.
            /// </summary>
            /// <param name="other">Other runtime slot to compare.</param>
            /// <returns><see langword="true"/> if this slot is equal to the specified slot; otherwise, <see langword="false"/>.</returns>
            public override bool Equals(IRuntimeSlot other)
            {
                return Equals(other as WeakRuntimeSlot<TOwner>);
            }

            /// <summary>
            /// Determines whether this slot is equal to another.
            /// </summary>
            /// <param name="other">Other runtime slot to compare.</param>
            /// <returns><see langword="true"/> if this slot is equal to the specified slot; otherwise, <see langword="false"/>.</returns>
            public bool Equals(WeakRuntimeSlot<TOwner> other)
            {
                return other != null &&
                    Equals(ContractBinding, other.ContractBinding) &&
                    ReferenceEquals(Owner, other.Owner) &&
                    Equals(Getter, other.Getter) &&
                    Equals(Setter, other.Setter);
            }
        }

        /// <summary>
        /// Represents an abstract runtime slot that is used to construct
        /// custom runtime slot.
        /// </summary>
        /// <typeparam name="T">Type of underlying object. Derived runtime slot
        /// represents access to this type.</typeparam>
        /// <typeparam name="TValue">Type of the stored value.</typeparam>
        [ComVisible(false)]
        [Serializable]
        public abstract class RuntimeSlotBase<T, TValue> : RuntimeSlotBase, IEquatable<RuntimeSlotBase<T, TValue>>
            where T:class
            where TValue: class, IScriptObject
        {
            private readonly RuntimeSlotAttributes m_attributes;
            private readonly T m_obj;
            private readonly IScriptContract m_contract;

            /// <summary>
            /// Initializes a new runtime slot.
            /// </summary>
            /// <param name="obj">An instance of underlying object. Cannot be <see langword="null"/>.</param>
            /// <param name="contractBinding">Static contract binding. Cannot be <see langword="null"/>.</param>
            /// <param name="attributes">Attributes associated with this slot.</param>
            /// <exception cref="System.ArgumentNullException"><paramref name="obj"/> or <paramref name="contractBinding"/> is <see langword="null"/>.</exception>
            protected RuntimeSlotBase(T obj, IScriptContract contractBinding, RuntimeSlotAttributes attributes)
            {
                if (obj == null) throw new ArgumentNullException("obj");
                if (contractBinding == null) throw new ArgumentNullException("contractBinding");
                m_attributes = attributes;
                m_obj = obj;
                m_contract = contractBinding;
            }

            /// <summary>
            /// Gets underlying object.
            /// </summary>
            protected T Target
            {
                get { return m_obj; }
            }

            /// <summary>
            /// Gets or sets value holded by this slot.
            /// </summary>
            public abstract TValue Value
            {
                set;
                get;
            }

            /// <summary>
            /// Stores value in this slot.
            /// </summary>
            /// <param name="value"></param>
            /// <param name="state"></param>
            public sealed override void SetValue(IScriptObject value, InterpreterState state)
            {
                switch ((Attributes & RuntimeSlotAttributes.Immutable) == 0)
                {
                    case true: Value = value as TValue; return;
                    default: throw new ConstantCannotBeChangedException(state);
                }
                
            }

            /// <summary>
            /// Returns value stored in this slot.
            /// </summary>
            /// <param name="state"></param>
            /// <returns></returns>
            public sealed override IScriptObject GetValue(InterpreterState state)
            {
                return (IScriptObject)Value ?? ContractBinding.FromVoid(state);
            }

            /// <summary>
            /// Returns static contract binding.
            /// </summary>
            public sealed override IScriptContract ContractBinding
            {
                get { return m_contract; }
            }

            /// <summary>
            /// Gets semantic of this slot.
            /// </summary>
            public sealed override RuntimeSlotAttributes Attributes
            {
                get { return m_attributes; }
            }

            /// <summary>
            /// Gets collection of runtime slots provided by encapsulated value.
            /// </summary>
            protected sealed override ICollection<string> Slots
            {
                get { return Value.Slots; }
            }

            /// <summary>
            /// Deletes value of this slot.
            /// </summary>
            /// <returns><see langword="true"/> if value is removed successfully; otherwise, <see langword="false"/>.</returns>
            /// <remarks>In the default implementation this method always returns <see langword="false"/>.</remarks>
            public override bool DeleteValue()
            {
                return false;
            }

            /// <summary>
            /// Determines whether this slot is equal to another.
            /// </summary>
            /// <param name="other">The slot to compare.</param>
            /// <returns></returns>
            public virtual bool Equals(RuntimeSlotBase<T, TValue> other)
            {
                return other != null && ReferenceEquals(Target, other.Target);
            }

            /// <summary>
            /// Determines whether this slot is equal to another.
            /// </summary>
            /// <param name="other">The slot to compare.</param>
            /// <returns></returns>
            public sealed override bool Equals(IRuntimeSlot other)
            {
                return Equals(other as RuntimeSlotBase<T, TValue>);
            }

            /// <summary>
            /// Determines whether this slot is equal to another.
            /// </summary>
            /// <param name="other">The slot to compare.</param>
            /// <returns></returns>
            public sealed override bool Equals(object other)
            {
                return Equals(other as RuntimeSlotBase<T, TValue>);
            }

            /// <summary>
            /// Computes a hash code of this instance.
            /// </summary>
            /// <returns></returns>
            public sealed override int GetHashCode()
            {
                return Target.GetHashCode();
            }
        }

        /// <summary>
        /// Represents an abstract class for building indexer accessor.
        /// </summary>
        [ComVisible(false)]
        protected abstract class Indexer : RuntimeSlotBase
        {
            private readonly ScriptContract m_contract;

            /// <summary>
            /// Initializes a new indexer accessor.
            /// </summary>
            /// <param name="valueContract">The contract binding of the value. Cannot be <see langword="null"/>.</param>
            /// <exception cref="System.ArgumentNullException"><paramref name="valueContract"/> is <see langword="null"/>.</exception>
            protected Indexer(ScriptContract valueContract)
            {
                if (valueContract == null) throw new ArgumentNullException("valueContract");
                m_contract = valueContract;
            }

            /// <summary>
            /// Gets a value indicating that this indexer accessor is read-only.
            /// </summary>
            protected abstract bool IsReadOnly
            {
                get;
            }

            /// <summary>
            /// This property is not implemented.
            /// </summary>
            /// <remarks>This property will never used by interpeter.</remarks>
            public sealed override IScriptContract ContractBinding
            {
                get { return m_contract; }
            }

            /// <summary>
            /// Gets semantic of this accessor.
            /// </summary>
            public sealed override RuntimeSlotAttributes Attributes
            {
                get { return IsReadOnly ? RuntimeSlotAttributes.Immutable : RuntimeSlotAttributes.None; }
            }

            /// <summary>
            /// Deletes indexer value.
            /// </summary>
            /// <returns>Always is <see langword="false"/>.</returns>
            public sealed override bool DeleteValue()
            {
                return false;
            }

            /// <summary>
            /// Gets runtime slot by its name.
            /// </summary>
            /// <param name="slotName">The name of the slot.</param>
            /// <param name="state">Internal interpreter state.</param>
            /// <returns></returns>
            protected sealed override IRuntimeSlot this[string slotName, InterpreterState state]
            {
                get
                {
                    return GetValue(state)[slotName, state];
                }
            }

            /// <summary>
            /// Gets indexer accessor.
            /// </summary>
            /// <param name="args"></param>
            /// <param name="state"></param>
            /// <returns></returns>
            protected sealed override IRuntimeSlot this[IScriptObject[] args, InterpreterState state]
            {
                get { return GetValue(state)[args, state]; }
            }

            /// <summary>
            /// Gets collection of access slots.
            /// </summary>
            protected sealed override ICollection<string> Slots
            {
                get { return new string[0]; }
            }
        }

        [ComVisible(false)]
        private sealed class ActionBasedIndexer : Indexer, IEquatable<ActionBasedIndexer>
        {
            private readonly IScriptObject m_getter;
            private readonly IScriptObject m_setter;
            private readonly IScriptObject[] m_indicies;

            public ActionBasedIndexer(IScriptObject[] indicies, IScriptObject getter = null, IScriptObject setter = null)
                : base(ScriptSuperContract.Instance)
            {
                if (indicies == null) throw new ArgumentNullException("indicies");
                m_indicies = indicies;
                m_getter = getter;
                m_setter = setter;
            }

            public bool Equals(ActionBasedIndexer other)
            {
                return other != null && Equals(Getter, other.Getter) && Equals(Setter, other.Setter) && Enumerable.SequenceEqual(Indicies, other.Indicies);
            }

            public override bool Equals(IRuntimeSlot other)
            {
                return Equals(other as ActionBasedIndexer);
            }

            public override bool Equals(object other)
            {
                return Equals(other as ActionBasedIndexer);
            }

            public override int GetHashCode()
            {
                return (Getter != null ? Getter.GetHashCode() : 0) ^ (Setter != null ? Setter.GetHashCode() : 0);
            }

            public IScriptObject Getter
            {
                get { return m_getter; }
            }

            public IScriptObject Setter
            {
                get { return m_setter; }
            }

            public IScriptObject[] Indicies
            {
                get { return m_indicies; }
            }

            public override IScriptObject GetValue(InterpreterState state)
            {
                switch (Getter != null)
                {
                    case true:
                        return Getter.Invoke(Indicies, state);
                    default:
                        if (state.Context == InterpretationContext.Unchecked)
                            return Void;
                        else throw new UnsupportedOperationException(state);
                }
            }

            public override void SetValue(IScriptObject value, InterpreterState state)
            {
                switch (Setter != null)
                {
                    case true:
                        var args = new List<IScriptObject>(Indicies.Length + 1) { value };
                        args.AddRange(Indicies);
                        Setter.Invoke(args, state);
                        return;
                    default:
                        if (state.Context == InterpretationContext.Unchecked)
                            return;
                        else throw new UnsupportedOperationException(state);
                }
            }

            protected override bool IsReadOnly
            {
                get { return Setter == null; }
            }
        }

        /// <summary>
        /// Marks runtime slot storage.
        /// </summary>
        [ComVisible(false)]
        [AttributeUsage(AttributeTargets.Interface, AllowMultiple = false, Inherited = true)]
        public sealed class SlotStoreAttribute : Attribute
        {
        }

        /// <summary>
        /// Represents runtime slots that provides read-only access to the encapsulated object.
        /// </summary>
        /// <typeparam name="TObject">Type of the stored object.</typeparam>
        [ComVisible(false)]
        internal sealed class RuntimeSlotWrapper<TObject> : RuntimeSlotBase, IEquatable<RuntimeSlotWrapper<TObject>>
            where TObject: class, IScriptObject
        {
            public readonly TObject Value;

            public RuntimeSlotWrapper(TObject obj)
            {
                if (obj == null) throw new ArgumentNullException("obj");
                Value = obj;
            }

            public override IScriptObject GetValue(InterpreterState state)
            {
                return Value;
            }

            public override void SetValue(IScriptObject value, InterpreterState state)
            {
                throw new ConstantCannotBeChangedException(state);
            }

            public override IScriptContract ContractBinding
            {
                get { return Value.GetContractBinding(); }
            }

            public override RuntimeSlotAttributes Attributes
            {
                get { return RuntimeSlotAttributes.Immutable; }
            }

            protected override ICollection<string> Slots
            {
                get { return Value.Slots; }
            }

            public override bool DeleteValue()
            {
                return false;
            }

            public bool Equals(RuntimeSlotWrapper<TObject> other)
            {
                return other != null && Equals(Value, other.Value);
            }

            public override bool Equals(IRuntimeSlot other)
            {
                return Equals(other as RuntimeSlotWrapper<TObject>);
            }

            public override int GetHashCode()
            {
                return Value.GetHashCode();
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

        internal static MethodCallExpression MakeConverter(Expression value)
        {
            if (value == null) throw new ArgumentNullException("value");
            var invocation = LinqHelpers.BodyOf<object, IScriptObject, MethodCallExpression>(obj => Convert(obj)).Method;
            invocation = invocation.GetGenericMethodDefinition().MakeGenericMethod(value.Type);
            return Expression.Call(null, invocation, value);
        }

        internal static Expression MakeConverter(IRestorable restorable)
        {
            return restorable != null ? MakeConverter(restorable.Restore()) : MakeVoid();
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

        IRuntimeSlot IScriptObject.this[IScriptObject[] args, InterpreterState state]
        {
            get 
            {
                return this[args, state];
            }
        }

        /// <summary>
        /// Gets element accessor.
        /// </summary>
        /// <param name="args">The position of the element.</param>
        /// <param name="state">Internal interpreter state.</param>
        /// <returns>The element accessor.</returns>
        public virtual RuntimeSlotBase this[IScriptObject[] args, InterpreterState state]
        {
            get { return new ActionBasedIndexer(args, this[GetItemAction, state], this[SetItemAction, state]); }
        }

        #endregion

        IScriptObject IScriptObject.GetRuntimeDescriptor(string slotName, InterpreterState state)
        {
            return GetSlotMetadata(slotName, state);
        }

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
        /// <param name="arg"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        public IScriptObject BinaryOperation(ScriptCodeBinaryOperatorType @operator, IScriptObject arg, InterpreterState state)
        {
            if (arg is IScriptProxyObject) return ((IScriptProxyObject)arg).Enqueue(this, @operator, state);
            else switch (@operator)
            {
                case ScriptCodeBinaryOperatorType.Add:
                    return Add(arg, state);
                case ScriptCodeBinaryOperatorType.Intersection:
                case ScriptCodeBinaryOperatorType.AndAlso:
                    return And(arg, state);
                case ScriptCodeBinaryOperatorType.Union:
                case ScriptCodeBinaryOperatorType.OrElse:
                    return Or(arg, state);
                case ScriptCodeBinaryOperatorType.Coalesce:
                    return Coalesce(arg, state);
                case ScriptCodeBinaryOperatorType.Divide:
                    return Divide(arg, state);
                case ScriptCodeBinaryOperatorType.InstanceOf:
                    return InstanceOf(arg, state);
                case ScriptCodeBinaryOperatorType.ValueEquality:
                    return Equals(arg, state);
                case ScriptCodeBinaryOperatorType.ReferenceEquality:
                    return ReferenceEquals(arg, state);
                case ScriptCodeBinaryOperatorType.ValueInequality:
                    return NotEquals(arg, state);
                case ScriptCodeBinaryOperatorType.ReferenceInequality:
                    return ReferenceNotEquals(arg, state);
                case ScriptCodeBinaryOperatorType.Exclusion:
                    return ExclusiveOr(arg, state);
                case ScriptCodeBinaryOperatorType.GreaterThan:
                    return GreaterThan(arg, state);
                case ScriptCodeBinaryOperatorType.GreaterThanOrEqual:
                    return GreaterThanOrEqual(arg, state);
                case ScriptCodeBinaryOperatorType.LessThan:
                    return LessThan(arg, state);
                case ScriptCodeBinaryOperatorType.LessThanOrEqual:
                    return LessThanOrEqual(arg, state);
                case ScriptCodeBinaryOperatorType.Modulo:
                    return Modulo(arg, state);
                case ScriptCodeBinaryOperatorType.Multiply:
                    return Multiply(arg, state);
                case ScriptCodeBinaryOperatorType.Subtract:
                    return Subtract(arg, state);
                case ScriptCodeBinaryOperatorType.TypeCast:
                    return Convert(arg as IScriptContract, state);
                case ScriptCodeBinaryOperatorType.PartOf:
                    return PartOf(arg, state);
                case ScriptCodeBinaryOperatorType.MetadataDiscovery:
                    return GetSlotMetadata(arg, state);
                case ScriptCodeBinaryOperatorType.MemberAccess:
                    return GetSlot(arg, state);
                case ScriptCodeBinaryOperatorType.Assign:
                    return Assign(arg, state);
                case ScriptCodeBinaryOperatorType.DivideAssign:
                    return DivideAssign(arg, state);
                case ScriptCodeBinaryOperatorType.ExclusionAssign:
                    return ExclusionAssign(arg, state);
                case ScriptCodeBinaryOperatorType.AdditiveAssign:
                    return AddAssign(arg, state);
                case ScriptCodeBinaryOperatorType.MultiplicativeAssign:
                    return MulAssign(arg, state);
                case ScriptCodeBinaryOperatorType.Expansion:
                    return Expansion(arg, state);
                case ScriptCodeBinaryOperatorType.Reduction:
                    return Reduction(arg, state);
                case ScriptCodeBinaryOperatorType.SubtractiveAssign:
                    return SubAssign(arg, state);
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

        private IScriptObject GetSlot(IScriptObject slotIdentity, InterpreterState state)
        {
            switch (SystemConverter.GetTypeCode(slotIdentity))
            {
                case TypeCode.String:
                    var runtimeSlot = GetSlot(SystemConverter.ToString(slotIdentity), state);
                    return (Behavior & ObjectBehavior.UnwrapSlotValue) == 0 ? runtimeSlot : runtimeSlot.GetValue(state);
                default:
                    throw new UnsupportedOperationException(state);
            }
        }

        private IRuntimeSlot GetSlot(string slotName, InterpreterState state)
        {
            return this[slotName, state] ?? RuntimeSlotBase.Missing(slotName);
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
                    else throw new ActionArgumentsMistmatchException(state);
            }
        }
        
        #endregion

        #region Special Members Invocation

        internal static bool TryGetMember(IScriptObject target, GetMemberBinder binder, out object result)
        {
            var slot = target[binder.Name, InterpreterState.Current];
            var state = binder.GetState();
            switch (slot != null)
            {
                case true:
                    result = slot.GetValue(state);
                    return true;
                default:
                    if (state.Context == InterpretationContext.Unchecked)
                        result = Void;
                    else throw new SlotNotFoundException(binder.Name, state);
                    return true;
            }
        }

        internal static bool TrySetIndex(IScriptObject target, SetIndexBinder binder, object[] indexes, object value)
        {
            var state = binder.GetState();
            var args = default(IScriptObject[]);
            var v = default(IScriptObject);
            switch (TryConvert(indexes, out args) && TryConvert(value, out v))
            {
                case true:
                    var slot = target[args, state];
                    slot.SetValue(v, state);
                    return true;
                default:return false;
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
            return base.TrySetIndex(binder, indexes, value);
        }

        internal static bool TryGetIndex(IScriptObject target, GetIndexBinder binder, object[] indexes, out object result)
        {
            var state = binder.GetState();
            var args = default(IScriptObject[]);
            switch (TryConvert(indexes, out args))
            {
                case true:
                    var slot = target[args, state];
                    result = slot.GetValue(state);
                    return true;
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
            var state = binder.GetState();
            switch (TryConvert(value, out scriptObject) && BindSetMember(target, binder, scriptObject))
            {
                case true: return true;
                default:
                    if (state.Context == InterpretationContext.Unchecked) return true;
                    else throw new SlotNotFoundException(binder.Name, state);
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

        private static bool BindSetMember(IScriptObject target, SetMemberBinder binder, IScriptObject value)
        {
            var slot = target[binder.Name, binder.GetState()];
            switch (slot != null)
            {
                case true:
                    slot.SetValue(value, binder.GetState());
                    return true;
                default: return false;
            }
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
            get
            {
                return Enumerable.ToArray(Enumerable.Select(SlotSearcher.GetSlotHolders(this), p => p.Name));
            }
        }

        /// <summary>
        /// Initializes field cache if it is necessary.
        /// </summary>
        /// <param name="slot">Reference to field that stores runtime slot.</param>
        /// <param name="initializer">Runtime slot initializer.</param>
        protected static IRuntimeSlot Cache(ref IRuntimeSlot slot, Func<IRuntimeSlot> initializer)
        {
            if (slot == null) slot = initializer.Invoke();
            return slot;
        }

        /// <summary>
        /// Initializes field cache if it is necessary.
        /// </summary>
        /// <typeparam name="TSlot"></typeparam>
        /// <param name="slot"></param>
        /// <returns></returns>
        protected static IRuntimeSlot Cache<TSlot>(ref IRuntimeSlot slot)
            where TSlot: class, IRuntimeSlot, new()
        {
            if (slot == null) slot = new TSlot();
            return slot;
        }

        /// <summary>
        /// Initializes field read-only cache if it is necessary.
        /// </summary>
        /// <typeparam name="TObject">Type of the object that should be created if the specified slot is <see langword="null"/>.</typeparam>
        /// <param name="slot"></param>
        /// <returns></returns>
        protected static IRuntimeSlot CacheConst<TObject>(ref IRuntimeSlot slot)
            where TObject : class, IScriptObject, new()
        {
            if (slot == null) slot = new RuntimeSlotWrapper<TObject>(new TObject());
            return slot;
        }

        /// <summary>
        /// Initializes field read-only cache if it is necessary.
        /// </summary>
        /// <typeparam name="TObject"></typeparam>
        /// <param name="slot"></param>
        /// <param name="initializer"></param>
        /// <returns></returns>
        protected static IRuntimeSlot CacheConst<TObject>(ref IRuntimeSlot slot, Func<TObject> initializer)
            where TObject : class, IScriptObject
        {
            if (slot == null) slot = new RuntimeSlotWrapper<TObject>(initializer.Invoke());
            return slot;
        }

        /// <summary>
        /// Gets slot exposed by this object.
        /// </summary>
        /// <param name="slotName">The name of the slot.</param>
        /// <param name="state">Internal interpreter state.</param>
        /// <returns>The runtime slot exposed by this object.</returns>
        public virtual IRuntimeSlot this[string slotName, InterpreterState state]
        {
            get
            {
                var slotHolder = SlotSearcher.Find(this, slotName);
                if (slotHolder != null)
                    return slotHolder;
                else if (state.Context == InterpretationContext.Unchecked)
                    return RuntimeSlotBase.Missing(slotName);
                else throw new SlotNotFoundException(slotName, state);
            }
        }

        /// <summary>
        /// Gets a contract binding for the object.
        /// </summary>
        /// <returns>The contract binding.</returns>
        public abstract IScriptContract GetContractBinding();

        #region IEquatable<IScriptObject> Members

        bool IEquatable<IScriptObject>.Equals(IScriptObject other)
        {
            return SystemConverter.ToBoolean(Equals(other, InterpreterState.Current)); 
        }

        #endregion

        /// <summary>
        /// Determines whether the current object is equal to the other.
        /// </summary>
        /// <param name="other">Other object to be compared.</param>
        /// <returns><see langword="true"/> if the current object is equal to another; otherwise, <see langword="false"/>.</returns>
        public sealed override bool Equals(object other)
        {
            if (other is IScriptProxyObject)
                other = ((IScriptProxyObject)other).Unwrap(InterpreterState.Current);
            return SystemConverter.ToBoolean(Equals(Convert(other), InterpreterState.Current));
        }

        /// <summary>
        /// Computes a hash code for the object.
        /// </summary>
        /// <returns>The hash code for the object.</returns>
        public override int GetHashCode()
        {
            return Slots.GetHashCode();
        }

        #region IEnumerable<KeyValuePair<string, IRuntimeSlot>> Members

        /// <summary>
        /// Returns an enumerator through slots.
        /// </summary>
        /// <returns>The enumerator through slots.</returns>
        public IEnumerator<KeyValuePair<string, IRuntimeSlot>> GetEnumerator()
        {
            foreach (var slotName in Slots)
                yield return new KeyValuePair<string, IRuntimeSlot>(slotName, this[slotName, InterpreterState.Current]);
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        /// <summary>
        /// Produces clone of the current object.
        /// </summary>
        /// <returns>The clone of the current object.</returns>
        protected virtual ScriptObject Clone()
        {
            return MemberwiseClone() as ScriptObject;
        }

        #region ICloneable Members

        object ICloneable.Clone()
        {
            return Clone();
        }

        #endregion

        internal static Expression AsRightSide(Expression expr, ParameterExpression stateHolder)
        {
            return RuntimeHelpers.IsRuntimeVariable(expr)? RuntimeSlotBase.GetValue(expr, stateHolder):expr;
        }

        private static IEnumerable<Expression> AsRightSide(IEnumerable<Expression> expressions, ParameterExpression stateHolder)
        {
            return from e in expressions select AsRightSide(e, stateHolder);
        }

        internal static MethodCallExpression BindBinaryOperation(Expression left, ConstantExpression @operator, Expression right, ParameterExpression stateVar)
        {
            right = AsRightSide(right, stateVar);
            var binaryOp = LinqHelpers.BodyOf<IScriptObject, ScriptCodeBinaryOperatorType, IScriptObject, InterpreterState, IScriptObject, MethodCallExpression>((l, op, r, s) => l.BinaryOperation(op, r, s));
            return binaryOp.Update(left, new Expression[] { @operator, right, stateVar });
        }

        internal static MethodCallExpression BindBinaryOperation(Expression left, ScriptCodeBinaryOperatorType @operator, Expression right, ParameterExpression stateVar)
        {
            switch (@operator)
            {
                case ScriptCodeBinaryOperatorType.AdditiveAssign:
                case ScriptCodeBinaryOperatorType.Assign:
                case ScriptCodeBinaryOperatorType.DivideAssign:
                case ScriptCodeBinaryOperatorType.ExclusionAssign:
                case ScriptCodeBinaryOperatorType.Expansion:
                case ScriptCodeBinaryOperatorType.Initializer:
                case ScriptCodeBinaryOperatorType.ModuloAssign:
                case ScriptCodeBinaryOperatorType.MultiplicativeAssign:
                case ScriptCodeBinaryOperatorType.Reduction:
                case ScriptCodeBinaryOperatorType.SubtractiveAssign: break;
                default:
                    left = AsRightSide(left, stateVar); break;
            }
            return BindBinaryOperation(left, LinqHelpers.Constant<ScriptCodeBinaryOperatorType>(@operator), right, stateVar);
        }

        internal static MethodCallExpression BindUnaryOperation(Expression operand, ConstantExpression @operator, ParameterExpression stateVar)
        {
            operand = AsRightSide(operand, stateVar);
            var unaryOp = LinqHelpers.BodyOf<IScriptObject, ScriptCodeUnaryOperatorType, InterpreterState, IScriptObject, MethodCallExpression>((l, op, s) => l.UnaryOperation(op, s));
            return unaryOp.Update(operand, new Expression[] { @operator, stateVar });
        }

        internal static MethodCallExpression BindUnaryOperation(Expression operand, ScriptCodeUnaryOperatorType @operator, ParameterExpression stateVar)
        {
            return BindUnaryOperation(operand, LinqHelpers.Constant<ScriptCodeUnaryOperatorType>(@operator), stateVar);
        }

        internal static MethodCallExpression BindInvoke(Expression target, IEnumerable<Expression> args, ParameterExpression stateVar)
        {
            target= AsRightSide(target, stateVar);
            args = AsRightSide(args ?? Enumerable.Empty<Expression>(), stateVar);
            var invoker = LinqHelpers.BodyOf<IScriptObject, IScriptObject[], InterpreterState, IScriptObject, MethodCallExpression>((tgt, a, s) => tgt.Invoke(a, s));
            return invoker.Update(target, new Expression[] { Expression.NewArrayInit(typeof(IScriptObject), args), stateVar });
        }

        internal static MethodCallExpression BindSlotAccess(Expression target, string slotName, ParameterExpression stateVar)
        {
            target = AsRightSide(target, stateVar);
            var indexer = LinqHelpers.BodyOf<IScriptObject, string, InterpreterState, IRuntimeSlot, MethodCallExpression>((@this, n, s) => @this[n, s]);
            return indexer.Update(target, new Expression[] { LinqHelpers.Constant<string>(slotName), stateVar });
        }

        internal static MethodCallExpression BindSlotMetadata(Expression target, string slotName, ParameterExpression stateVar)
        {
            target = AsRightSide(target, stateVar);
            var resolver = LinqHelpers.BodyOf<IScriptObject, string, InterpreterState, IScriptObject, MethodCallExpression>((@this, n, s) => @this.GetRuntimeDescriptor(n, s));
            return resolver.Update(target, new Expression[] { LinqHelpers.Constant<string>(slotName), stateVar });
        }

        internal static MethodCallExpression BindIndexer(Expression target, IEnumerable<Expression> indicies, ParameterExpression stateVar)
        {
            target = AsRightSide(target, stateVar);
            indicies = AsRightSide(indicies ?? Enumerable.Empty<Expression>(), stateVar);
            var indexer = LinqHelpers.BodyOf<IScriptObject, IScriptObject[], InterpreterState, IRuntimeSlot, MethodCallExpression>((o, i, s) => o[i, s]);
            return indexer.Update(target, new Expression[] { Expression.NewArrayInit(typeof(IScriptObject), indicies), stateVar });
        }

        internal static MethodCallExpression BindIsVoid(Expression target)
        {
            var isv = LinqHelpers.BodyOf<object, bool, MethodCallExpression>(ob => IsVoid(ob));
            return isv.Update(null, new[] { target });
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
    }
}
