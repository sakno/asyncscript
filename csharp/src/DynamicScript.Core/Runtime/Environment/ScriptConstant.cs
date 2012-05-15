using System;
using System.Dynamic;
using System.Linq.Expressions;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace DynamicScript.Runtime.Environment
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;
    using InterpretationContext = Compiler.Ast.InterpretationContext;
    using CallStack = Debugging.CallStack;
    using ScriptValueProvider = System.Func<InterpreterState, IScriptObject>;
    using ScriptContractProvider = System.Func<InterpreterState, IScriptContract>;

    /// <summary>
    /// Represents DynamicScript constant at runtime.
    /// This class cannot be inherited.
    /// </summary>
    [ComVisible(false)]
    [Serializable]
    public sealed class ScriptConstant: RuntimeSlot
    {
        private ScriptConstant(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            HasValue = true;
        }

        internal ScriptConstant(IScriptObject value, IScriptContract contract, InterpreterState state)
            : base(contract ?? value.GetContractBinding())
        {
            base.SetValue(value, state);
        }

        internal ScriptConstant(IScriptObject value, InterpreterState state)
            : this(value, null, state)
        {
        }

        /// <summary>
        /// Initializes a new runtime constant.
        /// </summary>
        /// <param name="value">The value of the constant.</param>
        /// <param name="contract">The constant contract binding.</param>
        public ScriptConstant(IScriptObject value, IScriptContract contract)
            : this(value, contract, InterpreterState.Current)
        {
        }

        /// <summary>
        /// Initializes a new runtime constant.
        /// </summary>
        /// <param name="value">The value of the constant.</param>
        public ScriptConstant(IScriptObject value)
            : this(value, InterpreterState.Current)
        {
        }

        /// <summary>
        /// Gets a value indicating that the current slot is immutable.
        /// </summary>
        protected internal override bool IsConstant
        {
            get { return true; }
        }

        /// <summary>
        /// Saves the object to storage.
        /// </summary>
        /// <param name="value">The object to be stored.</param>
        /// <param name="state">Internal interpreter state.</param>
        /// <exception cref="ConstantCannotBeChangedException">The value of the constant cannot be changed.</exception>
        public override IScriptObject SetValue(IScriptObject value, InterpreterState state)
        {
            switch (state.Context)
            {
                case InterpretationContext.Checked:
                default:
                    throw new ConstantCannotBeChangedException(state);
                case InterpretationContext.Unchecked:
                    return GetValue(state);
            }
        }

        #region Runtime Helpers
        private static IStaticRuntimeSlot IntrnlBindToConstant(IStaticRuntimeSlot slot, string constantName, object value, object contractBinding, InterpreterState state)
        {
            if (slot == null)
            {
                slot = new ScriptConstant(value is ScriptValueProvider ? ((ScriptValueProvider)value).Invoke(state) : value as IScriptObject, contractBinding is ScriptContractProvider ? ((ScriptContractProvider)contractBinding).Invoke(state) : contractBinding as IScriptContract, state);
                //Register named slot in the call stack
                if (!string.IsNullOrEmpty(constantName) && CallStack.Current != null)
                    CallStack.Current.RegisterStorage(constantName, slot);
            }
            return slot;
        }
        
        /// <summary>
        /// Binds to the runtime constant.
        /// </summary>
        /// <param name="slot"></param>
        /// <param name="constantName"></param>
        /// <param name="value"></param>
        /// <param name="contractBinding">Contract binding of the runtime constant. Can be omitted.</param>
        /// <param name="state"></param>
        /// <returns></returns>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static IStaticRuntimeSlot RtlBindToConstant(IStaticRuntimeSlot slot, string constantName, IScriptObject value, IScriptContract contractBinding, InterpreterState state)
        {
            return IntrnlBindToConstant(slot, constantName, value, contractBinding, state);
        }

        /// <summary>
        /// Binds to the constant through constant value and its contract binding.
        /// </summary>
        /// <param name="slot">Constant slot to bind.</param>
        /// <param name="constantName">The name of the constant storage. This parameter is used for debugging purposes only.</param>
        /// <param name="value">The value of the constant.</param>
        /// <param name="contractBinding">The contract binding for the constant.</param>
        /// <param name="state">Internal interpreter state.</param>
        /// <returns>The runtime bounded constant.</returns>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static IStaticRuntimeSlot RtlBindToConstant(IStaticRuntimeSlot slot, string constantName, ScriptValueProvider value, ScriptContractProvider contractBinding, InterpreterState state)
        {
            return IntrnlBindToConstant(slot, constantName, value, contractBinding, state);
        }

        /// <summary>
        /// Binds to the constant through constant value and its contract binding.
        /// </summary>
        /// <param name="slot">Constant slot to bind.</param>
        /// <param name="constantName">The name of the constant storage. This parameter is used for debugging purposes only.</param>
        /// <param name="value">The value of the constant.</param>
        /// <param name="contractBinding">The contract binding for the constant.</param>
        /// <param name="state">Internal interpreter state.</param>
        /// <returns>The runtime bounded constant.</returns>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static IStaticRuntimeSlot RtlBindToConstant(IStaticRuntimeSlot slot, string constantName, ScriptValueProvider value, IScriptContract contractBinding, InterpreterState state)
        {
            return IntrnlBindToConstant(slot, constantName, value, contractBinding, state);
        }

        /// <summary>
        /// Binds to the constant through constant value and its contract binding.
        /// </summary>
        /// <param name="slot">Constant slot to bind.</param>
        /// <param name="constantName">The name of the constant storage. This parameter is used for debugging purposes only.</param>
        /// <param name="value">The value of the constant.</param>
        /// <param name="contractBinding">The contract binding for the constant.</param>
        /// <param name="state">Internal interpreter state.</param>
        /// <returns>The runtime bounded constant.</returns>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static IStaticRuntimeSlot RtlBindToConstant(IStaticRuntimeSlot slot, string constantName, IScriptObject value, ScriptContractProvider contractBinding, InterpreterState state)
        {
            return IntrnlBindToConstant(slot, constantName, value, contractBinding, state);
        }

        internal static BinaryExpression BindToConstant(ParameterExpression constantRef, string constantName, Expression value, Expression contractBinding, ParameterExpression state)
        {
            if (contractBinding == null) contractBinding = LinqHelpers.Null<IScriptContract>();
            var rvalue = default(MethodCallExpression);
            switch (value is Expression<ScriptValueProvider>)
            {
                case true:
                    rvalue = contractBinding is Expression<ScriptContractProvider> ?
                        LinqHelpers.BodyOf<IStaticRuntimeSlot, string, ScriptValueProvider, ScriptContractProvider, InterpreterState, IStaticRuntimeSlot, MethodCallExpression>((c, n, v, t, s) => RtlBindToConstant(c, n, v, t, s)) :
                        LinqHelpers.BodyOf<IStaticRuntimeSlot, string, ScriptValueProvider, IScriptContract, InterpreterState, IStaticRuntimeSlot, MethodCallExpression>((c, n, v, t, s) => RtlBindToConstant(c, n, v, t, s));
                    break;
                default:
                    rvalue = contractBinding is Expression<ScriptContractProvider> ?
                        LinqHelpers.BodyOf<IStaticRuntimeSlot, string, IScriptObject, ScriptContractProvider, InterpreterState, IStaticRuntimeSlot, MethodCallExpression>((c, n, v, t, s) => RtlBindToConstant(c, n, v, t, s)) :
                        LinqHelpers.BodyOf<IStaticRuntimeSlot, string, IScriptObject, IScriptContract, InterpreterState, IStaticRuntimeSlot, MethodCallExpression>((c, n, v, t, s) => RtlBindToConstant(c, n, v, t, s));
                    break;
            }
            rvalue = rvalue.Update(null, new Expression[] { constantRef, LinqHelpers.Constant(constantName), value, contractBinding, state });
            return Expression.Assign(constantRef, rvalue);
        }

        #endregion
    }
}
