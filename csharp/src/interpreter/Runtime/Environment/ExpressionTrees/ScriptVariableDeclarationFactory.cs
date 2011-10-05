using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace DynamicScript.Runtime.Environment.ExpressionTrees
{
    using Compiler.Ast;
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;

    [ComVisible(false)]
    [Serializable]
    sealed class ScriptVariableDeclarationFactory : ScriptStatementFactory<ScriptCodeVariableDeclaration, ScriptVariableDeclaration>, IVariableDeclarationFactorySlots
    {
        #region Nested Types
        [ComVisible(false)]
        private sealed class ModifyAction : ModifyActionBase
        {
            private const string SecondParamName = "name";
            private const string ThirdParamName = "constant";
            private const string FourthParamName = "initExpr";
            private const string FifthParamName = "contractExpr";

            public ModifyAction()
                : base(Instance, new ScriptActionContract.Parameter(SecondParamName, ScriptNameTokenExpressionFactory.Instance),
                new ScriptActionContract.Parameter(ThirdParamName, ScriptBooleanContract.Instance),
                new ScriptActionContract.Parameter(FourthParamName, ScriptExpressionFactory.Instance),
                new ScriptActionContract.Parameter(FifthParamName, ScriptExpressionFactory.Instance))
            {
            }
        }

        [ComVisible(false)]
        private sealed class GetNameAction : CodeElementPartProvider<ScriptString>
        {
            public GetNameAction()
                : base(Instance, ScriptStringContract.Instance)
            {
            }

            protected override ScriptString Invoke(ScriptCodeVariableDeclaration element, InterpreterState state)
            {
                return element.Name;
            }
        }

        [ComVisible(false)]
        private sealed class IsConstantAction : CodeElementPartProvider<ScriptBoolean>
        {
            public IsConstantAction()
                : base(Instance, ScriptBooleanContract.Instance)
            {
            }

            protected override ScriptBoolean Invoke(ScriptCodeVariableDeclaration element, InterpreterState state)
            {
                return element.IsConst;
            }
        }

        [ComVisible(false)]
        private sealed class GetInitExpressionAction : CodeElementPartProvider<IScriptExpression<ScriptCodeExpression>>
        {
            public GetInitExpressionAction()
                : base(Instance, ScriptExpressionFactory.Instance)
            {
            }

            protected override IScriptExpression<ScriptCodeExpression> Invoke(ScriptCodeVariableDeclaration element, InterpreterState state)
            {
                return Convert(element.InitExpression) as IScriptExpression<ScriptCodeExpression>;
            }
        }

        [ComVisible(false)]
        private sealed class GetContractAction : CodeElementPartProvider<IScriptExpression<ScriptCodeExpression>>
        {
            public GetContractAction()
                : base(Instance, ScriptExpressionFactory.Instance)
            {
            }

            protected override IScriptExpression<ScriptCodeExpression> Invoke(ScriptCodeVariableDeclaration element, InterpreterState state)
            {
                return Convert(element.ContractBinding) as IScriptExpression<ScriptCodeExpression>;
            }
        }
        #endregion

        public new const string Name = "variable";
        private IRuntimeSlot m_modify;
        private IRuntimeSlot m_name;
        private IRuntimeSlot m_isconst;
        private IRuntimeSlot m_initexpr;
        private IRuntimeSlot m_getcontract;

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

        protected override IRuntimeSlot Modify
        {
            get { return CacheConst<ModifyAction>(ref m_modify); }
        }

        IRuntimeSlot IVariableDeclarationFactorySlots.Name
        {
            get { return CacheConst<GetNameAction>(ref m_name); }
        }

        IRuntimeSlot IVariableDeclarationFactorySlots.IsConst
        {
            get { return CacheConst<IsConstantAction>(ref m_isconst); }
        }

        IRuntimeSlot IVariableDeclarationFactorySlots.InitExpr
        {
            get { return CacheConst<GetInitExpressionAction>(ref m_initexpr); }
        }

        IRuntimeSlot IVariableDeclarationFactorySlots.Contract
        {
            get { return CacheConst<GetContractAction>(ref m_getcontract); }
        }
    }
}
