using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace DynamicScript.Runtime.Environment.ExpressionTrees
{
    using Compiler.Ast;
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;

    [ComVisible(false)]
    [Serializable]
    sealed class ScriptFunctionExpressionFactory : ScriptExpressionFactory<ScriptCodeFunctionExpression, ScriptFunctionExpression>
    {
        #region Nested Types

        [ComVisible(false)]
        private sealed class ModifyFunction : ModifyFunctionBase
        {
            private const string SecondParamName = "sig";
            private const string ThirdParamName = "body";

            public ModifyFunction()
                : base(Instance, new ScriptFunctionContract.Parameter(SecondParamName, ScriptFunctionSignatureExpressionFactory.Instance),
                new ScriptFunctionContract.Parameter(ThirdParamName, ScriptExpressionFactory.Instance))
            {
            }
        }

        [ComVisible(false)]
        private sealed class GetBodyFunction : CodeElementPartProvider<IScriptCodeElement<ScriptCodeExpression>>
        {
            public const string Name = "body";

            public GetBodyFunction()
                :base(Instance, new ScriptArrayContract(ScriptStatementFactory.Instance))
            {
            }

            protected override IScriptCodeElement<ScriptCodeExpression> Invoke(ScriptCodeFunctionExpression element, InterpreterState state)
            {
                return Convert(element.Body) as IScriptCodeElement<ScriptCodeExpression>;
            }
        }

        [ComVisible(false)]
        private sealed class GetSignatureFunction : CodeElementPartProvider<IScriptCodeElement<ScriptCodeActionContractExpression>>
        {
            public const string Name = "signature";

            public GetSignatureFunction()
                : base(Instance, ScriptFunctionSignatureExpressionFactory.Instance)
            {
            }

            protected override IScriptCodeElement<ScriptCodeActionContractExpression> Invoke(ScriptCodeFunctionExpression element, InterpreterState state)
            {
                return new ScriptFunctionSignatureExpression(element.Signature);
            }
        }
        #endregion

        private static readonly AggregatedSlotCollection<ScriptFunctionExpressionFactory> StaticSlots = new AggregatedSlotCollection<ScriptFunctionExpressionFactory>
        {
             {ModifyFunction.Name, (owner, state) => LazyField<ModifyFunction, IScriptFunction>(ref owner.m_modify)},
             {GetBodyFunction.Name, (owner, state) => LazyField<GetBodyFunction, IScriptFunction>(ref owner.m_body)},
             {GetSignatureFunction.Name, (owner, state) => LazyField<GetSignatureFunction, IScriptFunction>(ref owner.m_sig)},
        };

        public new const string Name = "function";

        private IScriptFunction m_modify;
        private IScriptFunction m_sig;
        private IScriptFunction m_body; 

        private ScriptFunctionExpressionFactory(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        private ScriptFunctionExpressionFactory()
            : base(Name)
        {
        }

        public static readonly ScriptFunctionExpressionFactory Instance = new ScriptFunctionExpressionFactory();

        public static ScriptFunctionExpression CreateExpression(IScriptCodeElement<ScriptCodeActionContractExpression> signature, IScriptCodeElement<ScriptCodeExpression> body)
        {
            return new ScriptFunctionExpression(ScriptFunctionExpression.CreateExpression(signature, body));
        }

        public override ScriptFunctionExpression CreateCodeElement(IList<IScriptObject> args, InterpreterState state)
        {
            return args.Count == 2 ? CreateExpression(args[0] as IScriptCodeElement<ScriptCodeActionContractExpression>, args[1] as IScriptCodeElement<ScriptCodeExpression>) : null;
        }

        public override void Clear()
        {
            m_body = m_modify = m_sig = null;
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
