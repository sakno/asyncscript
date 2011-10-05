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
    sealed class ScriptObjectExpressionFactory: ScriptExpressionFactory<ScriptCodeObjectExpression, ScriptObjectExpression>, IObjectExpressionFactorySlots
    {
        #region Nested Types
        [ComVisible(false)]
        private sealed class ModifyAction : ModifyActionBase
        {
            private const string SecondParamName = "slots";

            public ModifyAction()
                : base(Instance, new ScriptActionContract.Parameter(SecondParamName, new ScriptArrayContract(ScriptVariableDeclarationFactory.Instance)))
            {
            }
        }

        [ComVisible(false)]
        private sealed class GetSlotsAction : CodeElementPartProvider<IScriptArray>
        {
            public GetSlotsAction()
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
        public new const string Name = "obj";

        private IRuntimeSlot m_modify;
        private IRuntimeSlot m_slots;

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

        protected override IRuntimeSlot Modify
        {
            get { return CacheConst<ModifyAction>(ref m_modify); }
        }

        IRuntimeSlot IObjectExpressionFactorySlots.Slots
        {
            get { return CacheConst<GetSlotsAction>(ref m_slots); }
        }
    }
}
