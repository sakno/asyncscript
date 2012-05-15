using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace DynamicScript.Runtime.Environment.ExpressionTrees
{
    using Compiler.Ast;
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;

    [ComVisible(false)]
    [Serializable]
    sealed class ScriptComplexExpressionFactory : ScriptExpressionFactory<ScriptCodeComplexExpression, ScriptComplexExpression>
    {
        #region Nested Types
        [ComVisible(false)]
        private sealed class ModifyFunction : ModifyFunctionBase
        {
            private const string SecondParamName = "statements";

            public ModifyFunction()
                : base(Instance, new ScriptFunctionContract.Parameter(SecondParamName, new ScriptArrayContract(ScriptStatementFactory.Instance)))
            {
            }
        }

        [ComVisible(false)]
        private sealed class GetStatementsFunction : CodeElementPartProvider<IScriptArray>
        {
            public const string Name = "statements";

            public GetStatementsFunction()
                : base(Instance, new ScriptArrayContract(ScriptStatementFactory.Instance))
            {
            }

            protected override IScriptArray Invoke(ScriptCodeComplexExpression element, InterpreterState state)
            {
                return ScriptStatementFactory.CreateStatements(element.Body, state);
            }
        }
        #endregion

        private static readonly AggregatedSlotCollection<ScriptComplexExpressionFactory> StaticSlots = new AggregatedSlotCollection<ScriptComplexExpressionFactory>
        {
            {ModifyFunction.Name, (owner, state) =>LazyField<ModifyFunction, IScriptFunction>(ref owner.m_modify)},
            {GetStatementsFunction.Name, (owner, state) => LazyField<GetStatementsFunction, IScriptFunction>(ref owner.m_statements)}
        };

        public new const string Name = "cplx";

        private IScriptFunction m_modify;
        private IScriptFunction m_statements;

        private ScriptComplexExpressionFactory(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        private ScriptComplexExpressionFactory()
            : base(Name)
        {
        }

        public static readonly ScriptComplexExpressionFactory Instance = new ScriptComplexExpressionFactory();

        public static ScriptComplexExpression CreateExpression(IEnumerable<IScriptObject> statements)
        {
            return new ScriptComplexExpression(ScriptComplexExpression.CreateExpression(statements));
        }

        public override ScriptComplexExpression CreateCodeElement(IList<IScriptObject> args, InterpreterState state)
        {
            switch (args.Count)
            {
                case 0: return null;
                case 1: return CreateExpression(args[0] as IEnumerable<IScriptObject> ?? args);
                default: return CreateExpression(args);
            }
        }

        public override void Clear()
        {
            m_statements = m_modify = null;
        }

        public override ICollection<string> Slots
        {
            get { return StaticSlots.Keys; }
        }

        protected override IScriptObject GetSlotMetadata(string slotName, InterpreterState state)
        {
            return StaticSlots.GetSlotMetadata(this, slotName, state); 
        }

        public override IScriptObject this[string slotName, InterpreterState state]
        {
            get { return StaticSlots.GetValue(this, slotName, state); }
            set { StaticSlots.SetValue(this, slotName, value, state); }
        }
    }
}
