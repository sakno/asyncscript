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
    sealed class ScriptFaultStatementFactory: ScriptStatementFactory<ScriptCodeFaultStatement, ScriptFaultStatement>
    {
        #region Nested Types
        [ComVisible(false)]
        private sealed class ModifyFunction : ModifyFunctionBase
        {
            private const string SecondParamName = "error";

            public ModifyFunction()
                : base( Instance, new ScriptFunctionContract.Parameter(SecondParamName, ScriptExpressionFactory.Instance))
            {
            }
        }

        [ComVisible(false)]
        private sealed class ExecuteFunction : ScriptFunc<IScriptStatement<ScriptCodeFaultStatement>, IScriptCompositeObject>
        {
            public const string Name = "execute";
            private const string FirstParamName = "faultstmt";
            private const string SecondParamName = "obj";

            public ExecuteFunction()
                : base(FirstParamName, Instance, SecondParamName, ScriptCompositeContract.Empty, 
                ScriptBooleanContract.Instance)
            {
            }

            protected override IScriptObject Invoke(IScriptStatement<ScriptCodeFaultStatement> faultStmt, IScriptCompositeObject obj, InterpreterState state)
            {
                return (ScriptBoolean)ScriptFaultStatement.Execute(faultStmt, obj, state);
            }
        }

        [ComVisible(false)]
        private sealed class GetErrorFunction : CodeElementPartProvider<IScriptExpression<ScriptCodeExpression>>
        {
            public const string Name = "error";
            public GetErrorFunction()
                : base( Instance, ScriptSuperContract.Instance)
            {
            }

            protected override IScriptExpression<ScriptCodeExpression> Invoke(ScriptCodeFaultStatement element, InterpreterState state)
            {
                return Convert(element.Error) as IScriptExpression<ScriptCodeExpression>;
            }
        }
        #endregion

        private static readonly AggregatedSlotCollection<ScriptFaultStatementFactory> StaticSlots = new AggregatedSlotCollection<ScriptFaultStatementFactory>
        {
            {ModifyFunction.Name, (owner, state) => LazyField<ModifyFunction, IScriptFunction>(ref owner.m_modify)},
            {ExecuteFunction.Name, (owner, state) => LazyField<ExecuteFunction, IScriptFunction>(ref owner.m_exec)},
            {GetErrorFunction.Name, (owner, state) => LazyField<GetErrorFunction, IScriptFunction>(ref owner.m_geterr)}
        };

        /// <summary>
        /// Represents name of this contract.
        /// </summary>
        public new const string Name = "`fault";

        private IScriptFunction m_exec;
        private IScriptFunction m_geterr;
        private IScriptFunction m_modify;

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
