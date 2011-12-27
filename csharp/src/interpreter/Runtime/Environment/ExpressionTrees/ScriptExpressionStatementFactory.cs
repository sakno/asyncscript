using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace DynamicScript.Runtime.Environment.ExpressionTrees
{
    using Compiler.Ast;
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;

    [ComVisible(false)]
    [Serializable]
    sealed class ScriptExpressionStatementFactory : ScriptStatementFactory<ScriptCodeExpressionStatement, ScriptExpressionStatement>
    {
        #region Nested Types
        [ComVisible(false)]
        private sealed class ModifyFunction : ModifyFunctionBase
        {
            private const string FirstParamName = "e";

            public ModifyFunction()
                : base(Instance, new ScriptFunctionContract.Parameter(FirstParamName, ScriptExpressionFactory.Instance))
            {
            }
        }

        [ComVisible(false)]
        private sealed class ExtractFunction : CodeElementPartProvider<IScriptExpression<ScriptCodeExpression>>
        {
            public const string Name = "extract";

            public ExtractFunction()
                : base(Instance, ScriptExpressionFactory.Instance)
            {
            }

            protected override IScriptExpression<ScriptCodeExpression> Invoke(ScriptCodeExpressionStatement element, InterpreterState state)
            {
                return Convert(element.Expression) as IScriptExpression<ScriptCodeExpression>;
            }
        }
        #endregion

        public new const string Name = "expression";

        private static readonly AggregatedSlotCollection<ScriptExpressionStatementFactory> StaticSlots = new AggregatedSlotCollection<ScriptExpressionStatementFactory>
        {
            {ModifyFunction.Name, (owner, state) => LazyField<ModifyFunction, IScriptFunction>(ref owner.m_modify)},
            {ExtractFunction.Name, (owner, state) => LazyField<ExtractFunction, IScriptFunction>(ref owner.m_extract)}
        };

        private IScriptFunction m_modify;
        private IScriptFunction m_extract;

        private ScriptExpressionStatementFactory(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        private ScriptExpressionStatementFactory()
            : base(Name)
        {
        }

        public static readonly ScriptExpressionStatementFactory Instance = new ScriptExpressionStatementFactory();

        public static ScriptExpressionStatement CreateStatement(IScriptObject expr)
        {
            var statement = ScriptExpressionStatement.CreateStatement(expr);
            return statement != null ? new ScriptExpressionStatement(statement) : null;
        }

        public override ScriptExpressionStatement CreateCodeElement(IList<IScriptObject> args, InterpreterState state)
        {
            return args.Count == 1 ? CreateStatement(args[0]) : null;
        }

        public override void Clear()
        {
            m_modify = m_extract = null;
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
