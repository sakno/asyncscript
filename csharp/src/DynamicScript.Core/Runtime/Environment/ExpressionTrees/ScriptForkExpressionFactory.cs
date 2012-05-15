using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace DynamicScript.Runtime.Environment.ExpressionTrees
{
    using Compiler.Ast;
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;

    [ComVisible(false)]
    [Serializable]
    sealed class ScriptForkExpressionFactory : ScriptExpressionFactory<ScriptCodeForkExpression, ScriptForkExpression>
    {
        #region Nested Types
        [ComVisible(false)]
        private sealed class ModifyFunction : ModifyFunctionBase
        {
            private const string FirstParamName = "stmts";

            public ModifyFunction()
                : base(Instance, new ScriptFunctionContract.Parameter(FirstParamName, ScriptExpressionFactory.Instance))
            {
            }
        }

        [ComVisible(false)]
        private sealed class GetBodyFunction : CodeElementPartProvider<IScriptCodeElement<ScriptCodeExpression>>
        {
            public const string Name = "body";

            public GetBodyFunction()
                : base(Instance, new ScriptArrayContract(ScriptStatementFactory.Instance))
            {
            }

            protected override IScriptCodeElement<ScriptCodeExpression> Invoke(ScriptCodeForkExpression element, InterpreterState state)
            {
                return Convert(element.Body.Expression) as IScriptCodeElement<ScriptCodeExpression>;
            }
        }
        #endregion

        private static readonly AggregatedSlotCollection<ScriptForkExpressionFactory> StaticSlots = new AggregatedSlotCollection<ScriptForkExpressionFactory>
        {
             {ModifyFunction.Name, (owner, state) => LazyField<ModifyFunction, IScriptFunction>(ref owner.m_modify)},
             {GetBodyFunction.Name, (owner, state) => LazyField<GetBodyFunction, IScriptFunction>(ref owner.m_getbody)},
        };

        public new const string Name = "`fork";

        private IScriptFunction m_modify;
        private IScriptFunction m_getbody;

        private ScriptForkExpressionFactory(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        private ScriptForkExpressionFactory()
            : base(Name)
        {
        }

        public static readonly ScriptForkExpressionFactory Instance = new ScriptForkExpressionFactory();

        public static ScriptForkExpression CreateExpression(IScriptCodeElement<ScriptCodeExpression> body)
        {
            return new ScriptForkExpression(ScriptForkExpression.CreateExpression(body));
        }

        public override ScriptForkExpression CreateCodeElement(IList<IScriptObject> args, InterpreterState state)
        {
            return args.Count == 1 ? CreateExpression(args[0] as IScriptCodeElement<ScriptCodeExpression>) : null;
        }

        public override void Clear()
        {
            m_getbody = m_modify = null;
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
