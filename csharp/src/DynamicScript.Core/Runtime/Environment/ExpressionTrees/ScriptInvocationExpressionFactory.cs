using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace DynamicScript.Runtime.Environment.ExpressionTrees
{
    using Compiler.Ast;
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;

    [Serializable]
    [ComVisible(false)]
    sealed class ScriptInvocationExpressionFactory : ScriptExpressionFactory<ScriptCodeInvocationExpression, ScriptInvocationExpression>
    {
        #region Nested Types
        [ComVisible(false)]
        private sealed class ModifyFunction : ModifyFunctionBase
        {
            private const string FirstParamName = "args";

            public ModifyFunction()
                : base(Instance, new ScriptFunctionContract.Parameter(FirstParamName, new ScriptArrayContract(ScriptExpressionFactory.Instance)))
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

            protected override IScriptArray Invoke(ScriptCodeInvocationExpression element, InterpreterState state)
            {
                return ScriptExpressionFactory.CreateExpressions(element.ArgList, state);
            }
        }
        #endregion

        private static readonly AggregatedSlotCollection<ScriptInvocationExpressionFactory> StaticSlots = new AggregatedSlotCollection<ScriptInvocationExpressionFactory>
        {
             {ModifyFunction.Name, (owner, state) => LazyField<ModifyFunction, IScriptFunction>(ref owner.m_modify)},
             {GetArgumentsFunction.Name, (owner, state) => LazyField<GetArgumentsFunction, IScriptFunction>(ref owner.m_args)}
        };

        public new const string Name = "invocation";

        private IScriptFunction m_modify;
        private IScriptFunction m_args;

        private ScriptInvocationExpressionFactory(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        private ScriptInvocationExpressionFactory()
            : base(Name)
        {
        }

        public static readonly ScriptInvocationExpressionFactory Instance = new ScriptInvocationExpressionFactory();

        public static ScriptInvocationExpression CreateExpression(IEnumerable<IScriptObject> args)
        {
            return new ScriptInvocationExpression(ScriptInvocationExpression.CreateExpression(args));
        }

        public override ScriptInvocationExpression CreateCodeElement(IList<IScriptObject> args, InterpreterState state)
        {
            switch (args.Count)
            {
                case 0: return CreateExpression(EmptyArray);
                case 1: return CreateExpression(args[0] as IEnumerable<IScriptObject> ?? args);
                default: return CreateExpression(args);
            }
        }

        public override void Clear()
        {
            m_modify = m_args = null;
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
