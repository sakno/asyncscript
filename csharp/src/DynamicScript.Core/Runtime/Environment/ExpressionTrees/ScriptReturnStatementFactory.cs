using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace DynamicScript.Runtime.Environment.ExpressionTrees
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;
    using Compiler.Ast;

    [ComVisible(false)]
    [Serializable]
    sealed class ScriptReturnStatementFactory : ScriptStatementFactory<ScriptCodeReturnStatement, ScriptReturnStatement>
    {
        #region Nested Types
        [ComVisible(false)]
        private sealed class ModifyFunction : ModifyFunctionBase
        {
            private const string SecondParamName = "retval";

            public ModifyFunction()
                : base(Instance, new ScriptFunctionContract.Parameter(SecondParamName, ScriptExpressionFactory.Instance))
            {
            }
        }

        [ComVisible(false)]
        private sealed class GetValueFunction : CodeElementPartProvider<IScriptExpression<ScriptCodeExpression>>
        {
            public const string Name = "value";

            public GetValueFunction()
                : base(Instance, ScriptSuperContract.Instance)
            {
            }

            protected override IScriptExpression<ScriptCodeExpression> Invoke(ScriptCodeReturnStatement element, InterpreterState state)
            {
                return Convert(element.Value) as IScriptExpression<ScriptCodeExpression>;
            }
        }
        #endregion

        private static readonly AggregatedSlotCollection<ScriptReturnStatementFactory> StaticSlots = new AggregatedSlotCollection<ScriptReturnStatementFactory>
        {
             {ModifyFunction.Name, (owner, state) => LazyField<ModifyFunction, IScriptFunction>(ref owner.m_modify)},
             {GetValueFunction.Name, (owner, state) => LazyField<GetValueFunction, IScriptFunction>(ref owner.m_retval)}
        };

        public new const string Name = "`return";
        private IScriptFunction m_retval;
        private IScriptFunction m_modify;

        private ScriptReturnStatementFactory()
            : base(Name)
        {
        }

        private ScriptReturnStatementFactory(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        public static readonly ScriptReturnStatementFactory Instance = new ScriptReturnStatementFactory();

        public static ScriptReturnStatement CreateStatement(IScriptObject retObj = null)
        {
            return new ScriptReturnStatement(ScriptReturnStatement.CreateStatement(retObj));
        }

        public override ScriptReturnStatement CreateCodeElement(IList<IScriptObject> args, InterpreterState state)
        {
            switch (args.Count)
            {
                case 0: return CreateStatement();
                case 1: return CreateStatement(args[0]);
                default: return null;
            }
        }

        public override void Clear()
        {
            m_retval = m_modify = null;
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
