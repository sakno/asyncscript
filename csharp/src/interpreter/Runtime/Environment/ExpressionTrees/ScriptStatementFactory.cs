using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Linq;
using System.CodeDom;

namespace DynamicScript.Runtime.Environment.ExpressionTrees
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;
    using Keyword = Compiler.Keyword;
    using MemberExpression = System.Linq.Expressions.MemberExpression;
    using Compiler.Ast;
    using ObjectSlot = Compiler.Ast.ScriptCodeObjectExpression.Slot;
    using ActionParameter = Compiler.Ast.ScriptCodeActionContractExpression.Parameter;

    /// <summary>
    /// Represents runtime statement factory.
    /// This class cannot be inherited.
    /// </summary>
    [ComVisible(false)]
    [Serializable]
    public sealed class ScriptStatementFactory: ScriptBuiltinContract, IScriptMetaContract, IStatementFactorySlots
    {
        #region Nested Types

        [ComVisible(false)]
        private sealed class MiscSlotConverter : RuntimeConverter<ISlot>
        {
            public override bool Convert(ISlot input, out IScriptObject result)
            {
                if (input is ObjectSlot)
                    result = new ScriptVariableDeclaration(input);
                else if (input is ActionParameter)
                    result = new ScriptVariableDeclaration(input);
                else result = null;
                return result != null;
            }
        }

        [ComVisible(false)]
        private sealed class CloneAction : ScriptFunc<IScriptCodeElement<ScriptCodeStatement>>
        {
            private const string FirstParamName = "tree";

            public CloneAction()
                : base(FirstParamName, Instance, Instance)
            {
            }

            protected override IScriptObject Invoke(InvocationContext ctx, IScriptCodeElement<ScriptCodeStatement> tree)
            {
                return Convert(Extensions.Clone(tree.CodeObject));
            }
        }

        [ComVisible(false)]
        private sealed class StatementConverter : RuntimeConverter<ScriptCodeStatement>
        {
            public override bool Convert(ScriptCodeStatement input, out IScriptObject result)
            {
                if (input is ScriptCodeFaultStatement)
                    result = new ScriptFaultStatement((ScriptCodeFaultStatement)input);
                else if (input is ScriptCodeContinueStatement)
                    result = new ScriptContinueStatement((ScriptCodeContinueStatement)input);
                else if (input is ScriptCodeBreakLexicalScopeStatement)
                    result = new ScriptBreakStatement((ScriptCodeBreakLexicalScopeStatement)input);
                else if (input is ScriptCodeReturnStatement)
                    result = new ScriptReturnStatement((ScriptCodeReturnStatement)input);
                else if (input is ScriptCodeEmptyStatement)
                    result = ScriptEmptyStatement.Instance;
                else if (input is ScriptCodeExpressionStatement)
                    result = new ScriptExpressionStatement((ScriptCodeExpressionStatement)input);
                else if (input is ScriptCodeVariableDeclaration)
                    result = new ScriptVariableDeclaration((ScriptCodeVariableDeclaration)input);
                else if (input is ScriptCodeLoopWithVariableExpression.LoopVariable)
                    result = new ScriptLoopVariableStatement((ScriptCodeLoopWithVariableExpression.LoopVariable)input);
                else result = null;
                return result != null;
            }
        }

        [ComVisible(false)]
        private sealed class VisitAction : ScriptFunc<IScriptCodeElement<ScriptCodeStatement>, IScriptAction>
        {
            private const string FirstParamName = "tree";
            private const string SecondParamName = "visitor";

            public VisitAction()
                : base(FirstParamName, Instance, SecondParamName, ScriptSuperContract.Instance, Instance)
            {
            }

            protected override IScriptObject Invoke(InvocationContext ctx, IScriptCodeElement<ScriptCodeStatement> tree, IScriptAction visitor)
            {
                return Convert(tree.CodeObject.Visit(null, new ScriptSyntaxTreeVisitor(visitor, ctx.RuntimeState)));
            }
        }

        [ComVisible(false)]
        private sealed class InitAction : ScriptAction
        {
            protected override void Invoke(InvocationContext ctx)
            {
                Instance.Clear();
            }
        }
        #endregion

        static ScriptStatementFactory()
        {
            RuntimeHelpers.RegisterConverter<StatementConverter>();
            RuntimeHelpers.RegisterConverter<MiscSlotConverter>();
        }

        /// <summary>
        /// Gets name of this contract.
        /// </summary>
        internal static string Name
        {
            get { return Keyword.Stmt; }
        }

        private IRuntimeSlot m_fault;
        private IRuntimeSlot m_continue;
        private IRuntimeSlot m_leave;
        private IRuntimeSlot m_return;
        private IRuntimeSlot m_empty;
        private IRuntimeSlot m_expression;
        private IRuntimeSlot m_variable;
        private IRuntimeSlot m_init;
        private IRuntimeSlot m_clone;
        private IRuntimeSlot m_loopvar;
        private IRuntimeSlot m_visit;

        private ScriptStatementFactory(SerializationInfo info, StreamingContext context)
            : this()
        {
        }

        private ScriptStatementFactory()
        {
        }

        internal override Keyword Token
        {
            get { return Keyword.Stmt; }
        }



        private static ScriptCodeStatement CreateStatement(IScriptObject obj)
        {
            if (obj is IScriptCodeElement<ScriptCodeStatement>)
                return ((IScriptCodeElement<ScriptCodeStatement>)obj).CodeObject;
            else if (obj is IScriptCodeElement<ScriptCodeExpression>)
                return new ScriptCodeExpressionStatement(((IScriptCodeElement<ScriptCodeExpression>)obj).CodeObject);
            else return null;
        }

        internal static IEnumerable<ScriptCodeStatement> CreateStatements(IEnumerable<IScriptObject> statementsOrExpressions)
        {
            return statementsOrExpressions != null ? statementsOrExpressions.SelectNotNull(CreateStatement) : Enumerable.Empty<ScriptCodeStatement>();
        }

        internal static void CreateStatements(IEnumerable<IScriptObject> statementsOrExpressions, ScriptCodeStatementCollection output)
        {
            foreach (var stmt in CreateStatements(statementsOrExpressions))
                output.Add(stmt);
        }

        internal static IScriptArray CreateStatements(ScriptCodeStatementCollection statements, InterpreterState state)
        {
            var indicies = new long[1];
            var result = new ScriptArray(Instance, statements.Count);
            for (var i = 0; i < statements.Count; i++)
                result[indicies, state] = Convert(statements[i]);
            return result;
        }

        /// <summary>
        /// Represents singleton instance of the statement factory.
        /// </summary>
        public static readonly ScriptStatementFactory Instance = new ScriptStatementFactory();

        /// <summary>
        /// Determines relationship with the specified contract.
        /// </summary>
        /// <param name="contract"></param>
        /// <returns></returns>
        public override ContractRelationshipType GetRelationship(IScriptContract contract)
        {
            if (contract is ScriptStatementFactory)
                return ContractRelationshipType.TheSame;
            else if (contract.OneOf<ScriptMetaContract, ScriptSuperContract>())
                return ContractRelationshipType.Subset;
            else if (contract.OneOf<IScriptComplementation, IScriptUnionContract, IScriptCartesianProduct>())
                return Inverse(contract.GetRelationship(this));
            else return ContractRelationshipType.None;
        }

        /// <summary>
        /// Creates a new runtime representation of the FAULT statement.
        /// </summary>
        /// <param name="error"></param>
        /// <returns></returns>
        public static IScriptStatement<ScriptCodeFaultStatement> MakeFaultStatement(IScriptObject error)
        {
            return ScriptFaultStatementFactory.CreateStatement(error);
        }

        internal static MemberExpression Expression
        {
            get
            {
                return LinqHelpers.BodyOf<Func<ScriptStatementFactory>, MemberExpression>(() => Instance);
            }
        }

        /// <summary>
        /// Gets factory for runtime representation of FAULT statement.
        /// </summary>
        public static IScriptStatementContract<ScriptCodeFaultStatement> Fault
        {
            get { return ScriptFaultStatementFactory.Instance; }
        }

        /// <summary>
        /// Gets factory for runtime representation of FAULT statement.
        /// </summary>
        public static IScriptStatementContract<ScriptCodeContinueStatement> Continue
        {
            get { return ScriptContinueStatementFactory.Instance; }
        }

        /// <summary>
        /// Creates a new runtime representation of the CONTINUE statement.
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public static IScriptStatement<ScriptCodeContinueStatement> MakeContinueStatement(IEnumerable<IScriptObject> args=null)
        {
            return ScriptContinueStatementFactory.CreateStatement(args);
        }

        /// <summary>
        /// Gets factory of the LEAVE statement runtime representation.
        /// </summary>
        public static IScriptStatementContract<ScriptCodeBreakLexicalScopeStatement> Leave
        {
            get { return ScriptBreakStatementFactory.Instance; }
        }

        /// <summary>
        /// Creates a new runtime representation of the LEAVE statement.
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public static IScriptStatement<ScriptCodeBreakLexicalScopeStatement> MakeLeaveStatement(IEnumerable<IScriptObject> args = null)
        {
            return ScriptBreakStatementFactory.CreateStatement(args);
        }

        /// <summary>
        /// Creates a new runtime representation of the RETURn statement.
        /// </summary>
        /// <param name="retobj"></param>
        /// <returns></returns>
        public static IScriptStatement<ScriptCodeReturnStatement> MakeReturnStatement(IScriptObject retobj = null)
        {
            return ScriptReturnStatementFactory.CreateStatement(retobj);
        }

        /// <summary>
        /// Gets a factory of the RETURN statement runtime representation.
        /// </summary>
        public static IScriptStatementContract<ScriptCodeReturnStatement> Return
        {
            get { return ScriptReturnStatementFactory.Instance; }
        }

        /// <summary>
        /// Gets a factory that produces an empty statement.
        /// </summary>
        public static IScriptStatementContract<ScriptCodeEmptyStatement> Empty
        {
            get { return ScriptEmptyStatementFactory.Instance; }
        }

        /// <summary>
        /// Gets a factory that produces expression statement.
        /// </summary>
        public static IScriptStatementContract<ScriptCodeExpressionStatement> ExpressionStmt
        {
            get { return ScriptExpressionStatementFactory.Instance; }
        }

        /// <summary>
        /// Gets a factory that produces variable declaration.
        /// </summary>
        public static IScriptStatementContract<ScriptCodeVariableDeclaration> VariableDecl
        {
            get { return ScriptVariableDeclarationFactory.Instance; }
        }

        /// <summary>
        /// Gets a factory that produces loop variable.
        /// </summary>
        public static IScriptStatementContract<ScriptCodeLoopWithVariableExpression.LoopVariable> LoopVar
        {
            get{return ScriptLoopVariableStatementFactory.Instance;}
        }

        /// <summary>
        /// Releases a memory associated with statement factories.
        /// </summary>
        public void Clear()
        {
            m_continue =
                m_empty =
                m_expression =
                m_fault =
                m_leave =
                m_return =
                m_variable =
                m_clone =
                m_loopvar=
                m_visit=
                m_init = null;
            ScriptLoopVariableStatementFactory.Instance.Clear();
            ScriptReturnStatementFactory.Instance.Clear();
            ScriptBreakStatementFactory.Instance.Clear();
            ScriptFaultStatementFactory.Instance.Clear();
            ScriptExpressionStatementFactory.Instance.Clear();
            ScriptEmptyStatementFactory.Instance.Clear();
            ScriptContinueStatementFactory.Instance.Clear();
            ScriptVariableDeclarationFactory.Instance.Clear();
            GC.Collect();
        }

        #region Runtime Slots
        IRuntimeSlot IStatementFactorySlots.Visit
        {
            get { return CacheConst<VisitAction>(ref m_visit); }
        }

        IRuntimeSlot IStatementFactorySlots.LoopVar
        {
            get { return CacheConst(ref m_loopvar, () => LoopVar); }
        }

        IRuntimeSlot IStatementFactorySlots.Clone
        {
            get { return CacheConst<CloneAction>(ref m_clone); }
        }

        IRuntimeSlot IStatementFactorySlots.Init
        {
            get { return CacheConst<InitAction>(ref m_init); }
        }

        IRuntimeSlot IStatementFactorySlots.Variable
        {
            get { return CacheConst(ref m_variable, () => VariableDecl); }
        }

        IRuntimeSlot IStatementFactorySlots.Expression
        {
            get { return CacheConst(ref m_expression, () => ExpressionStmt); }
        }

        IRuntimeSlot IStatementFactorySlots.Empty
        {
            get { return CacheConst(ref m_empty, () => Empty); }
        }

        IRuntimeSlot IStatementFactorySlots.ReturnDef
        {
            get { return CacheConst(ref m_return, () => Return); }
        }

        IRuntimeSlot IStatementFactorySlots.LeaveDef
        {
            get { return CacheConst(ref m_leave, () => Leave); }
        }

        IRuntimeSlot IStatementFactorySlots.FaultDef
        {
            get { return CacheConst(ref m_fault, () => Fault); }
        }

        IRuntimeSlot IStatementFactorySlots.ContinueDef
        {
            get { return CacheConst(ref m_continue, () => Continue); }
        }

        #endregion
    }
}
