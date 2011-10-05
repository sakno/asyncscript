using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace DynamicScript.Runtime.Environment.ExpressionTrees
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;
    using Compiler.Ast;

    /// <summary>
    /// Represents FAULT statement factory.
    /// This class cannot be inherited.
    /// </summary>
    [ComVisible(false)]
    [Serializable]
    sealed class ScriptFaultStatementFactory: ScriptStatementFactory<ScriptCodeFaultStatement, ScriptFaultStatement>,
        IFaultStatementFactorySlots
    {
        #region Nested Types
        [ComVisible(false)]
        private sealed class ModifyAction : ModifyActionBase
        {
            private const string SecondParamName = "error";

            public ModifyAction()
                : base( Instance, new ScriptActionContract.Parameter(SecondParamName, ScriptExpressionFactory.Instance))
            {
            }
        }

        [ComVisible(false)]
        private sealed class ExecuteAction : ScriptFunc<IScriptStatement<ScriptCodeFaultStatement>, IScriptCompositeObject>
        {
            private const string FirstParamName = "faultstmt";
            private const string SecondParamName = "obj";

            public ExecuteAction()
                : base(FirstParamName, Instance, SecondParamName, ScriptCompositeContract.Empty, 
                ScriptBooleanContract.Instance)
            {
            }

            protected override IScriptObject Invoke(InvocationContext ctx, IScriptStatement<ScriptCodeFaultStatement> faultStmt, IScriptCompositeObject obj)
            {
                return (ScriptBoolean)ScriptFaultStatement.Execute(faultStmt, obj, ctx.RuntimeState);
            }
        }

        [ComVisible(false)]
        private sealed class GetErrorAction : CodeElementPartProvider<IScriptExpression<ScriptCodeExpression>>
        {
            public GetErrorAction()
                : base( Instance, ScriptSuperContract.Instance)
            {
            }

            protected override IScriptExpression<ScriptCodeExpression> Invoke(ScriptCodeFaultStatement element, InterpreterState state)
            {
                return Convert(element.Error) as IScriptExpression<ScriptCodeExpression>;
            }
        }
        #endregion

        /// <summary>
        /// Represents name of this contract.
        /// </summary>
        public new const string Name = "faultdef";

        private IRuntimeSlot m_exec;
        private IRuntimeSlot m_geterr;
        private IRuntimeSlot m_modify;

        private ScriptFaultStatementFactory(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        private ScriptFaultStatementFactory()
            : base(Name)
        {
        }

        public static readonly ScriptFaultStatementFactory Instance = new ScriptFaultStatementFactory();

        public static ScriptFaultStatement CreateStatement(IScriptObject error, bool emitDefault = true)
        {
            var statement = ScriptFaultStatement.CreateStatement(error);
            return statement != null || emitDefault ?
                new ScriptFaultStatement(statement ?? new ScriptCodeFaultStatement { Error = ScriptCodeVoidExpression.Instance }) : null;
        }

        public override ScriptFaultStatement CreateCodeElement(IList<IScriptObject> args, InterpreterState state)
        {
            return args.Count == 1 ? CreateStatement(args[0]) : null;
        }

        public override void Clear()
        {
            m_exec = m_geterr = m_modify = null;
        }

        #region Runtime Slots

        IRuntimeSlot IFaultStatementFactorySlots.Exec
        {
            get { return CacheConst<ExecuteAction>(ref m_exec); }
        }

        IRuntimeSlot IFaultStatementFactorySlots.Error
        {
            get { return CacheConst<GetErrorAction>(ref m_geterr); }
        }

        #endregion

        protected override IRuntimeSlot Modify
        {
            get { return CacheConst<ModifyAction>(ref m_modify); }
        }
    }
}
