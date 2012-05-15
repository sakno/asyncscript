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
    using FunctionParameter = Compiler.Ast.ScriptCodeActionContractExpression.Parameter;

    /// <summary>
    /// Represents runtime statement factory.
    /// This class cannot be inherited.
    /// </summary>
    [ComVisible(false)]
    [Serializable]
    public sealed class ScriptStatementFactory: ScriptBuiltinContract, IScriptMetaContract
    {
        #region Nested Types

        [ComVisible(false)]
        private sealed class MiscSlotConverter : RuntimeConverter<ISlot>
        {
            public override bool Convert(ISlot input, out IScriptObject result)
            {
                if (input is ObjectSlot)
                    result = new ScriptVariableDeclaration(input);
                else if (input is FunctionParameter)
                    result = new ScriptVariableDeclaration(input);
                else result = null;
                return result != null;
            }
        }

        [ComVisible(false)]
        private sealed class CloneFunction : ScriptFunc<IScriptCodeElement<ScriptCodeStatement>>
        {
            public const string Name = "clone";
            private const string FirstParamName = "tree";

            public CloneFunction()
                : base(FirstParamName, Instance, Instance)
            {
            }

            protected override IScriptObject Invoke(IScriptCodeElement<ScriptCodeStatement> tree, InterpreterState state)
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
        private sealed class VisitFunction : ScriptFunc<IScriptCodeElement<ScriptCodeStatement>, IScriptFunction>
        {
            public const string Name = "visit";
            private const string FirstParamName = "tree";
            private const string SecondParamName = "visitor";

            public VisitFunction()
                : base(FirstParamName, Instance, SecondParamName, ScriptSuperContract.Instance, Instance)
            {
            }

            protected override IScriptObject Invoke(IScriptCodeElement<ScriptCodeStatement> tree, IScriptFunction visitor, InterpreterState state)
            {
                return Convert(tree.CodeObject.Visit(null, new ScriptSyntaxTreeVisitor(visitor, state)));
            }
        }

        [ComVisible(false)]
        private sealed class InitFunction : ScriptAction
        {
            public const string Name = "init";

            protected override void Invoke(InterpreterState state)
            {
                Instance.Clear();
            }
        }
        #endregion

        private static readonly AggregatedSlotCollection<ScriptStatementFactory> StaticSlots = new AggregatedSlotCollection<ScriptStatementFactory>
        {
            //functions
            {CloneFunction.Name, (owner, state) => LazyField<CloneFunction, IScriptFunction>(ref owner.m_clone)},
            {VisitFunction.Name, (owner, state) => LazyField<VisitFunction, IScriptFunction>(ref owner.m_visit)},
            {InitFunction.Name, (owner, state) => LazyField<InitFunction, IScriptFunction>(ref owner.m_init)},
            //slots
            {ScriptFaultStatementFactory.Name, (owner, state) => Fault},
            {ScriptContinueStatementFactory.Name, (owner, state) => Continue},
            {ScriptBreakStatementFactory.Name, (owner, state) => Leave},
            {ScriptReturnStatementFactory.Name, (owner, state) => Return},
            {ScriptEmptyStatementFactory.Name, (owner, state) => Empty},
            {ScriptExpressionStatementFactory.Name, (owner, state) => ExpressionStmt},
            {ScriptVariableDeclarationFactory.Name, (owner, state) => VariableDecl},
            {ScriptLoopVariableStatementFactory.Name, (owner, state) => LoopVar}
        };

        static ScriptStatementFactory()
        {
            RegisterConverter<StatementConverter>();
            RegisterConverter<MiscSlotConverter>();
        }

        /// <summary>
        /// Gets name of this contract.
        /// </summary>
        internal static string Name
        {
            get { return Keyword.Stmt; }
        }

        private IScriptFunction m_clone;
        private IScriptFunction m_visit;
        private IScriptFunction m_init;

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
        public override void Clear()
        {
            m_clone =
            m_visit =
            m_init = null;
            ScriptLoopVariableStatementFactory.Instance.Clear();
            ScriptReturnStatementFactory.Instance.Clear();
            ScriptBreakStatementFactory.Instance.Clear();
            ScriptFaultStatementFactory.Instance.Clear();
            ScriptExpressionStatementFactory.Instance.Clear();
            ScriptEmptyStatementFactory.Instance.Clear();
            ScriptContinueStatementFactory.Instance.Clear();
            ScriptVariableDeclarationFactory.Instance.Clear();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="slotName"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        public override IScriptObject this[string slotName, InterpreterState state]
        {
            get { return StaticSlots.GetValue(this, slotName, state); }
            set { StaticSlots.SetValue(this, slotName, value, state); }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="slotName"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        protected override IScriptObject GetSlotMetadata(string slotName, InterpreterState state)
        {
            return StaticSlots.GetSlotMetadata(this, slotName, state);
        }

        /// <summary>
        /// 
        /// </summary>
        public override ICollection<string> Slots
        {
            get { return StaticSlots.Keys; }
        }
    }
}
