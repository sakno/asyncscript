using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DynamicScript.Runtime.Environment
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;
    using InterpretationContext = Compiler.Ast.InterpretationContext;
    using IScopeVariable = Microsoft.Scripting.IScopeVariable;

    /// <summary>
    /// Represents dynamic property slot.
    /// This class cannot be inherited.
    /// </summary>
    [ComVisible(false)]
    sealed class ScriptProperty: ScriptObject.RuntimeSlotBase, IScriptProperty
    {
        public readonly IScriptFunction Getter;
        public readonly IScriptFunction Setter;
        public readonly IScriptContract ContractBinding;

        public ScriptProperty(IScriptContract slotContract, IScriptFunction getter, IScriptFunction setter)
        {
            if (slotContract == null) throw new ArgumentNullException("slotContract");
            Getter = getter;
            Setter = setter;
            ContractBinding = slotContract;
        }

        IScriptContract IStaticRuntimeSlot.ContractBinding
        {
            get { return ContractBinding; }
        }

        public override IScriptObject GetValue(InterpreterState state)
        {
            if (Getter != null) return Getter.Invoke(ScriptObject.EmptyArray, state);
            else if (state.Context == InterpretationContext.Unchecked) return ScriptObject.Void;
            else throw new UnsupportedOperationException(state);
        }

        public override IScriptObject SetValue(IScriptObject value, InterpreterState state)
        {
            if (Setter != null) return Setter.Invoke(new[] { value }, state);
            else if (state.Context == InterpretationContext.Unchecked) return ScriptObject.Void;
            else throw new ConstantCannotBeChangedException(state);
        }

        IScriptFunction IScriptProperty.Getter
        {
            get { return Getter; }
        }

        IScriptFunction IScriptProperty.Setter
        {
            get { return Setter; }
        }

        public override bool DeleteValue()
        {
            return false;
        }
    }
}
