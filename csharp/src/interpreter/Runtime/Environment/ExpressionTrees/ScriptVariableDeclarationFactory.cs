using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace DynamicScript.Runtime.Environment.ExpressionTrees
{
    using Compiler.Ast;
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;

    [ComVisible(false)]
    [Serializable]
    sealed class ScriptVariableDeclarationFactory : ScriptStatementFactory<ScriptCodeVariableDeclaration, ScriptVariableDeclaration>
    {
        #region Nested Types
        [ComVisible(false)]
        private sealed class ModifyFunction : ModifyFunctionBase
        {
            private const string SecondParamName = "name";
            private const string ThirdParamName = "constant";
            private const string FourthParamName = "initExpr";
            private const string FifthParamName = "contractExpr";

            public ModifyFunction()
                : base(Instance, new ScriptFunctionContract.Parameter(SecondParamName, ScriptNameTokenExpressionFactory.Instance),
                new ScriptFunctionContract.Parameter(ThirdParamName, ScriptBooleanContract.Instance),
                new ScriptFunctionContract.Parameter(FourthParamName, ScriptExpressionFactory.Instance),
                new ScriptFunctionContract.Parameter(FifthParamName, ScriptExpressionFactory.Instance))
            {
            }
        }

        [ComVisible(false)]
        private sealed class GetNameFunction : CodeElementPartProvider<ScriptString>
        {
            public const string Name = "name";

            public GetNameFunction()
                : base(Instance, ScriptStringContract.Instance)
            {
            }

            protected override ScriptString Invoke(ScriptCodeVariableDeclaration element, InterpreterState state)
            {
                return element.Name;
            }
        }

        [ComVisible(false)]
        private sealed class IsConstantFunction : CodeElementPartProvider<ScriptBoolean>
        {
            public const string Name = "isconst";

            public IsConstantFunction()
                : base(Instance, ScriptBooleanContract.Instance)
            {
            }

            protected override ScriptBoolean Invoke(ScriptCodeVariableDeclaration element, InterpreterState state)
            {
                return element.IsConst;
            }
        }

        [ComVisible(false)]
        private sealed class GetInitExpressionFunction : CodeElementPartProvider<IScriptExpression<ScriptCodeExpression>>
        {
            public const string Name = "initexpr";

            public GetInitExpressionFunction()
                : base(Instance, ScriptExpressionFactory.Instance)
            {
            }

            protected override IScriptExpression<ScriptCodeExpression> Invoke(ScriptCodeVariableDeclaration element, InterpreterState state)
            {
                return Convert(element.InitExpression) as IScriptExpression<ScriptCodeExpression>;
            }
        }

        [ComVisible(false)]
        private sealed class GetContractFunction : CodeElementPartProvider<IScriptExpression<ScriptCodeExpression>>
        {
            public const string Name = "contract";

            public GetContractFunction()
                : base(Instance, ScriptExpressionFactory.Instance)
            {
            }

            protected override IScriptExpression<ScriptCodeExpression> Invoke(ScriptCodeVariableDeclaration element, InterpreterState state)
            {
                return Convert(element.ContractBinding) as IScriptExpression<ScriptCodeExpression>;
            }
        }
        #endregion

        private static readonly AggregatedSlotCollection<ScriptVariableDeclarationFactory> StaticSlots = new AggregatedSlotCollection<ScriptVariableDeclarationFactory>
        {
             {ModifyFunction.Name, (owner, state) => LazyField<ModifyFunction, IScriptFunction>(ref owner.m_modify)},
             {GetNameFunction.Name, (owner, state) => LazyField<GetNameFunction, IScriptFunction>(ref owner.m_name)},
             {IsConstantFunction.Name, (owner, state) => LazyField<IsConstantFunction, IScriptFunction>(ref owner.m_isconst)},
             {GetInitExpressionFunction.Name, (owner, state) => LazyField<GetInitExpressionFunction, IScriptFunction>(ref owner.m_initexpr)},
             {GetContractFunction.Name, (owner, state) => LazyField<GetContractFunction, IScriptFunction>(ref owner.m_getcontract)}
        };

        public new const string Name = "variable";
        private IScriptFunction m_modify;
        private IScriptFunction m_name;
        private IScriptFunction m_isconst;
        private IScriptFunction m_initexpr;
        private IScriptFunction m_getcontract;

        private ScriptVariableDeclarationFactory(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        private ScriptVariableDeclarationFactory()
            : base(Name)
        {
        }

        public static readonly ScriptVariableDeclarationFactory Instance = new ScriptVariableDeclarationFactory();

        public static ScriptVariableDeclaration CreateStatement(IScriptObject variableName, ScriptBoolean constant, IScriptObject initExpr, IScriptObject contractBinding)
        {
            return new ScriptVariableDeclaration(ScriptVariableDeclaration.CreateStatement(variableName, constant, initExpr, contractBinding));
        }

        public override ScriptVariableDeclaration CreateCodeElement(IList<IScriptObject> args, InterpreterState state)
        {
            return args.Count == 4 ? CreateStatement(args[0], args[1] as ScriptBoolean, args[2], args[3]) : null;
        }

        public override void Clear()
        {
            m_modify =
                m_name =
                m_isconst =
                m_initexpr = null;
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
