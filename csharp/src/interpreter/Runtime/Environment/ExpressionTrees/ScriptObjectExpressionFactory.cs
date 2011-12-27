using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace DynamicScript.Runtime.Environment.ExpressionTrees
{
    using Compiler.Ast;
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;
    using Enumerable = System.Linq.Enumerable;

    [ComVisible(false)]
    [Serializable]
    sealed class ScriptObjectExpressionFactory: ScriptExpressionFactory<ScriptCodeObjectExpression, ScriptObjectExpression>
    {
        #region Nested Types
        [ComVisible(false)]
        private sealed class ModifyFunction : ModifyFunctionBase
        {
            private const string SecondParamName = "slots";

            public ModifyFunction()
                : base(Instance, new ScriptFunctionContract.Parameter(SecondParamName, new ScriptArrayContract(ScriptVariableDeclarationFactory.Instance)))
            {
            }
        }

        [ComVisible(false)]
        private sealed class GetSlotsFunction : CodeElementPartProvider<IScriptArray>
        {
            public const string Name = "slots";

            public GetSlotsFunction()
                : base(Instance, new ScriptArrayContract(ScriptVariableDeclarationFactory.Instance))
            {
            }

            private static IScriptCodeElement<ScriptCodeVariableDeclaration> CreateSlot(ScriptCodeObjectExpression.Slot s)
            {
                return new ScriptVariableDeclaration(s);
            }

            protected override IScriptArray Invoke(ScriptCodeObjectExpression element, InterpreterState state)
            {
                return new ScriptArray(element.ToArray(CreateSlot));
            }
        }
        #endregion

        private static readonly AggregatedSlotCollection<ScriptObjectExpressionFactory> StaticSlots = new AggregatedSlotCollection<ScriptObjectExpressionFactory>
        {
             {ModifyFunction.Name, (owner, state) => LazyField<ModifyFunction, IScriptFunction>(ref owner.m_modify)},
             {GetSlotsFunction.Name, (owner, state) => LazyField<GetSlotsFunction, IScriptFunction>(ref owner.m_slots)},
        };

        public new const string Name = "obj";

        private IScriptFunction m_modify;
        private IScriptFunction m_slots;

        private ScriptObjectExpressionFactory(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        private ScriptObjectExpressionFactory()
            : base(Name)
        {
        }

        public static readonly ScriptObjectExpressionFactory Instance = new ScriptObjectExpressionFactory();

        public static ScriptObjectExpression CreateExpression(IEnumerable<IScriptObject> slots)
        {
            return new ScriptObjectExpression(ScriptObjectExpression.CreateExpression(slots));
        }

        public override ScriptObjectExpression CreateCodeElement(IList<IScriptObject> args, InterpreterState state)
        {
            switch (args.Count)
            {
                case 0: return CreateExpression(Enumerable.Empty<IScriptObject>());
                case 1: return CreateExpression(args[0] as IEnumerable<IScriptObject> ?? args);
                default: return CreateExpression(args);
            }
        }

        public override void Clear()
        {
            m_slots =
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
