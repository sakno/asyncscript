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
    using ScriptContractProvider = System.Func<InterpreterState, IScriptContract>;

    /// <summary>
    /// Represents runtime storage for variables.
    /// This class cannot be inherited.
    /// </summary>
    [ComVisible(false)]
    [Serializable]
    public sealed class ScriptVariable : RuntimeSlot
    {
        private ScriptVariable(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        /// <summary>
        /// Initializes a new runtime variable.
        /// </summary>
        /// <param name="contract">The contract binding for variable.</param>
        public ScriptVariable(IScriptContract contract = null)
            : base(contract)
        {
        }

        /// <summary>
        /// Initializes a new runtime variable.
        /// </summary>
        /// <param name="value">Initial variable value.</param>
        public ScriptVariable(IScriptObject value)
            : this(value, InterpreterState.Current)
        {
        }

        /// <summary>
        /// Initializes a new variable.
        /// </summary>
        /// <param name="value">The initial value of the variable.</param>
        /// <param name="contract">The contract binding for the variable.</param>
        public ScriptVariable(IScriptObject value, IScriptContract contract)
            : this(value, contract, InterpreterState.Current)
        {
        }

        private ScriptVariable(IScriptObject value, IScriptContract contract, InterpreterState state)
            : base(contract)
        {
            SetValue(value, state);
        }

        private ScriptVariable(IScriptObject value, InterpreterState state)
            : this(value, value != null ? value.GetContractBinding() : null, state)
        {
        }

        /// <summary>
        /// Gets a value indicating that the current slot is immutable.
        /// </summary>
        protected internal override bool IsConstant
        {
            get { return false; }
        }

        /// <summary>
        /// Returns a value stored in the slot.
        /// </summary>
        /// <param name="state">Internal interpreter state.</param>
        /// <returns></returns>
        public override IScriptObject GetValue(InterpreterState state)
        {
            lock (this)
                if (HasValue) return base.GetValue(state);
                else switch (state.Context)
                    {
                        case InterpretationContext.Unchecked:
                            return ContractBinding.FromVoid(state);
                        default:
                            throw new UnassignedSlotReadingException(state);
                    }
        }

        #region Runtime Helpers

        private static IRuntimeSlot IntrnlBindToVariable(IRuntimeSlot slot, string variableName, IScriptObject value, object contractBinding, InterpreterState state)
        {
            if (slot == null)
            {
                slot = new ScriptVariable(value, contractBinding is ScriptContractProvider ? ((ScriptContractProvider)contractBinding).Invoke(state) : contractBinding as IScriptContract, state);
                //Register variable at the current stack frame.
                if (!string.IsNullOrEmpty(variableName) && CallStack.Current != null)
                    CallStack.Current.RegisterStorage(variableName, slot);
            }
            else if (value != null) slot.SetValue(value, state);
            return slot;
        }

        /// <summary>
        /// Binds to the variable slot through contract and value.
        /// </summary>
        /// <param name="slot">Variable slot to bind.</param>
        /// <param name="variableName">The name of the constant storage. This parameter is used for debugging purposes only.</param>
        /// <param name="initialValue">Value to be assigned to the variable.</param>
        /// <param name="contractBinding">Contract binding for the variable.</param>
        /// <param name="state">Internal interpreter state.</param>
        /// <returns>Bounded variable slot.</returns>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static IRuntimeSlot RtlBindToVariable(IRuntimeSlot slot, string variableName, IScriptObject initialValue, ScriptContractProvider contractBinding, InterpreterState state)
        {
            return IntrnlBindToVariable(slot, variableName, initialValue, contractBinding, state);
        }

        /// <summary>
        /// Binds to the variable slot through contract and value.
        /// </summary>
        /// <param name="slot">Variable slot to bind.</param>
        /// <param name="variableName">The name of the constant storage. This parameter is used for debugging purposes only.</param>
        /// <param name="initialValue">Value to be assigned to the variable.</param>
        /// <param name="contractBinding">Contract binding for the variable.</param>
        /// <param name="state">Internal interpreter state.</param>
        /// <returns>Bounded variable slot.</returns>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static IRuntimeSlot RtlBindToVariable(IRuntimeSlot slot, string variableName, IScriptObject initialValue, IScriptContract contractBinding, InterpreterState state)
        {
            return IntrnlBindToVariable(slot, variableName, initialValue, contractBinding, state);
        }

        internal static Expression BindToVariable(Expression varRef, string variableName, Expression initValue, Expression contractBinding, ParameterExpression state)
        {
            if (initValue == null) initValue = LinqHelpers.Null<IScriptObject>();
            if (contractBinding == null) contractBinding = LinqHelpers.Null<IScriptContract>();
            var rvalue = default(MethodCallExpression);
            rvalue = contractBinding is Expression<ScriptContractProvider> ?
                LinqHelpers.BodyOf<IRuntimeSlot, string, IScriptObject, ScriptContractProvider, InterpreterState, IRuntimeSlot, MethodCallExpression>((v, n, b, c, s) => RtlBindToVariable(v, n, b, c, s)) :
                LinqHelpers.BodyOf<IRuntimeSlot, string, IScriptObject, IScriptContract, InterpreterState, IRuntimeSlot, MethodCallExpression>((v, n, b, c, s) => RtlBindToVariable(v, n, b, c, s));
            rvalue = rvalue.Update(null, new Expression[] { varRef, LinqHelpers.Constant(variableName), initValue, contractBinding, state });
            return varRef is ParameterExpression ? (Expression)Expression.Assign(varRef, rvalue) : rvalue;
        }
        #endregion
    }
}
