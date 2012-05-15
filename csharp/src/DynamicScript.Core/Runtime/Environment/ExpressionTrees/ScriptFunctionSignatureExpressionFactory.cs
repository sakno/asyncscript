using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace DynamicScript.Runtime.Environment.ExpressionTrees
{
    using Compiler.Ast;
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;

    [ComVisible(false)]
    [Serializable]
    sealed class ScriptFunctionSignatureExpressionFactory : ScriptExpressionFactory<ScriptCodeActionContractExpression, ScriptFunctionSignatureExpression>
    {
        #region Nested Types
        [ComVisible(false)]
        private sealed class ModifyFunction : ModifyFunctionBase
        {
            private const string SecondParamName = "params";
            private const string ThirdParamName = "retval";

            public ModifyFunction()
                : base(Instance, new ScriptFunctionContract.Parameter(SecondParamName, new ScriptArrayContract(ScriptVariableDeclarationFactory.Instance)), new ScriptFunctionContract.Parameter(ThirdParamName, ScriptExpressionFactory.Instance))
            {
            }
        }

        [ComVisible(false)]
        private sealed class GetReturnTypeFunction : CodeElementPartProvider<IScriptCodeElement<ScriptCodeExpression>>
        {
            public const string Name = "ret";

            public GetReturnTypeFunction()
                : base(Instance)
            {
            }

            protected override IScriptCodeElement<ScriptCodeExpression> Invoke(ScriptCodeActionContractExpression element, InterpreterState state)
            {
                if (element.NoReturnValue) return null;
                else if (element.IsAsynchronous) return Convert(new ScriptCodeAsyncExpression(element.ReturnType)) as IScriptCodeElement<ScriptCodeExpression>;
                else return Convert(element.ReturnType) as IScriptCodeElement<ScriptCodeExpression>;
            }
        }

        [ComVisible(false)]
        private sealed class GetParametersFunction : CodeElementPartProvider<IScriptArray>
        {
            public const string Name = "parameters";

            public GetParametersFunction()
                : base(Instance)
            {
            }

            private static IScriptCodeElement<ScriptCodeVariableDeclaration> CreateParameter(ScriptCodeActionContractExpression.Parameter p)
            {
                return new ScriptVariableDeclaration(p);
            }

            protected override IScriptArray Invoke(ScriptCodeActionContractExpression element, InterpreterState state)
            {
                return new ScriptArray(element.ParamList.ToArray(CreateParameter));
            }
        }
        #endregion

        private static readonly AggregatedSlotCollection<ScriptFunctionSignatureExpressionFactory> StaticSlots = new AggregatedSlotCollection<ScriptFunctionSignatureExpressionFactory>
        {
             {ModifyFunction.Name, (owner, state) => LazyField<ModifyFunction, IScriptFunction>(ref owner.m_modify)},
             {GetReturnTypeFunction.Name, (owner, state) => LazyField<GetReturnTypeFunction, IScriptFunction>(ref owner.m_retval)},
             {GetParametersFunction.Name, (owner, state) => LazyField<GetParametersFunction, IScriptFunction>(ref owner.m_params)},
        };

        public new const string Name = "signature";

        private ScriptFunctionSignatureExpressionFactory(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        private ScriptFunctionSignatureExpressionFactory()
            : base(Name)
        {
        }

        private IScriptFunction m_modify;
        private IScriptFunction m_retval;
        private IScriptFunction m_params;

        public static readonly ScriptFunctionSignatureExpressionFactory Instance = new ScriptFunctionSignatureExpressionFactory();

        public override ScriptFunctionSignatureExpression CreateCodeElement(IList<IScriptObject> args, InterpreterState state)
        {
            return args.Count == 2 ? CreateExpression(args[0] as IEnumerable<IScriptObject>, args[1] as IScriptCodeElement<ScriptCodeExpression>) : null;
        }

        public static ScriptFunctionSignatureExpression CreateExpression(IEnumerable<IScriptObject> parameters, IScriptCodeElement<ScriptCodeExpression> returnType)
        {
            return new ScriptFunctionSignatureExpression(ScriptFunctionSignatureExpression.CreateExpression(parameters, returnType));
        }

        public override void Clear()
        {
            m_modify = m_params = m_retval = null;
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
