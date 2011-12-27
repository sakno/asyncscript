using System;
using System.Collections.Generic;

namespace DynamicScript.Runtime.Environment.ExpressionTrees
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;
    using ScriptCodeVariableReference = Compiler.Ast.ScriptCodeVariableReference;

    /// <summary>
    /// Represents contract of name token runtime representation.
    /// This class cannot be inherited.
    /// </summary>
    [ComVisible(false)]
    sealed class ScriptNameTokenExpressionFactory : ScriptExpressionFactory<ScriptCodeVariableReference, ScriptNameTokenExpression>
    {
        #region Nested Types

        [ComVisible(false)]
        private sealed class ModifyFunction : ModifyFunctionBase
        {
            private const string SecondParamName = "newname";

            public ModifyFunction()
                : base(Instance,new ScriptFunctionContract.Parameter(SecondParamName, ScriptStringContract.Instance))
            {
            }
        }
        #endregion

        private static readonly AggregatedSlotCollection<ScriptNameTokenExpressionFactory> StaticSlots = new AggregatedSlotCollection<ScriptNameTokenExpressionFactory>
        {
             {ModifyFunction.Name, (owner, state) => LazyField<ModifyFunction, IScriptFunction>(ref owner.m_modify)},
        };

        /// <summary>
        /// Represents name of this contract.
        /// </summary>
        public new const string Name = "nmtoken";

        private IScriptFunction m_modify;

        private ScriptNameTokenExpressionFactory()
            : base(Name)
        {
        }

        public static readonly ScriptNameTokenExpressionFactory Instance = new ScriptNameTokenExpressionFactory();

        /// <summary>
        /// Creates a new runtime representation of 
        /// </summary>
        /// <param name="variableName"></param>
        /// <returns></returns>
        public static ScriptNameTokenExpression CreateExpression(ScriptString variableName)
        {
            return new ScriptNameTokenExpression(ScriptNameTokenExpression.CreateExpression(variableName));
        }

        public override ScriptNameTokenExpression CreateCodeElement(IList<IScriptObject> args, InterpreterState state)
        {
            if (args.Count == 0)
                switch (args[0] is ScriptString)
                {
                    case true: return CreateExpression((ScriptString)args[0]);
                    default: throw new ContractBindingException(args[0], ScriptStringContract.Instance, state);
                }
            else throw new FunctionArgumentsMistmatchException(state);
        }

        public override IScriptObject Convert(IScriptObject value, InterpreterState state)
        {
            if (value is ScriptNameTokenExpression)
                return value;
            else if (value is ScriptString)
                return CreateExpression((ScriptString)value);
            else return Void;
        }

        public override void Clear()
        {
            m_modify = null;
        }

        public override ICollection<string> Slots
        {
            get { return StaticSlots.Keys; }
        }

        public override IScriptObject this[string slotName, InterpreterState state]
        {
            get { return StaticSlots.GetValue(this, slotName, state); }
            set { StaticSlots.SetValue(this, slotName, value, state); }
        }

        protected override IScriptObject GetSlotMetadata(string slotName, InterpreterState state)
        {
            return StaticSlots.GetSlotMetadata(this, slotName, state);
        }
    }
}
