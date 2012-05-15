using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace DynamicScript.Runtime.Environment.ExpressionTrees
{
    using Compiler.Ast;
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;

    [ComVisible(false)]
    [Serializable]
    sealed class ScriptIndexerExpressionFactory : ScriptExpressionFactory<ScriptCodeIndexerExpression, ScriptIndexerExpression>
    {
        #region Nested Types
        [ComVisible(false)]
        private sealed class ModifyFunction : ModifyFunctionBase
        {
            private const string SecondParamName = "args";

            public ModifyFunction()
                : base(Instance, new ScriptFunctionContract.Parameter(SecondParamName, new ScriptArrayContract(ScriptExpressionFactory.Instance)))
            {
            }
        }

        [ComVisible(false)]
        private sealed class GetArgumentsFunction : CodeElementPartProvider<IScriptArray>
        {
            public const string Name = "args";

            public GetArgumentsFunction()
                : base(Instance, new ScriptArrayContract(ScriptExpressionFactory.Instance))
            {
            }

            protected override IScriptArray Invoke(ScriptCodeIndexerExpression element, InterpreterState state)
            {
                return ScriptExpressionFactory.CreateExpressions(element.ArgList, state);
            }
        }
        #endregion

        private static readonly AggregatedSlotCollection<ScriptIndexerExpressionFactory> StaticSlots = new AggregatedSlotCollection<ScriptIndexerExpressionFactory>
        {
             {ModifyFunction.Name, (owner, state) => LazyField<ModifyFunction, IScriptFunction>(ref owner.m_modify)},
             {GetArgumentsFunction.Name, (owner, state) => LazyField<GetArgumentsFunction, IScriptFunction>(ref owner.m_args)}
        };

        public new const string Name = "indexer";
        private IScriptFunction m_modify;
        private IScriptFunction m_args;

        private ScriptIndexerExpressionFactory(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        private ScriptIndexerExpressionFactory()
            : base(Name)
        {
        }

        public static readonly ScriptIndexerExpressionFactory Instance = new ScriptIndexerExpressionFactory();

        public static ScriptIndexerExpression CreateExpression(IEnumerable<IScriptObject> args)
        {
            return new ScriptIndexerExpression(ScriptIndexerExpression.CreateExpression(args));
        }

        public override void Clear()
        {
            m_args = m_modify = null;
        }

        public override ScriptIndexerExpression CreateCodeElement(IList<IScriptObject> args, InterpreterState state)
        {
            switch (args.Count)
            {
                case 0: return CreateExpression(EmptyArray);
                case 1: return CreateExpression(args[0] as IEnumerable<IScriptObject> ?? args);
                default: return CreateExpression(args);
            }
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
