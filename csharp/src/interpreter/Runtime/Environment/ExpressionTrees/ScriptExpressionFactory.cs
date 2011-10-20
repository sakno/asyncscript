using System;
using System.Runtime.Serialization;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace DynamicScript.Runtime.Environment.ExpressionTrees
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;
    using Compiler.Ast;
    using Keyword = Compiler.Keyword;
    using CodeExpressionCollection = System.CodeDom.CodeExpressionCollection;
    using Enumerable = System.Linq.Enumerable;
    using CodeStatement = System.CodeDom.CodeStatement;

    /// <summary>
    /// Represents runtime expression factory.
    /// This class cannot be inherited.
    /// </summary>
    [ComVisible(false)]
    [Serializable]
    public sealed class ScriptExpressionFactory : ScriptBuiltinContract, IScriptMetaContract, IExpressionFactorySlots
    {
        #region Nested Types
        [ComVisible(false)]
        private sealed class DeduceAction : ScriptFunc<IScriptCodeElement<ScriptCodeExpression>, IScriptArray>
        {
            #region Nested Types
            [ComVisible(false)]
            private sealed class SyntaxNodeReplacement
            {
                private readonly IScriptArray m_expressions;
                private readonly InterpreterState m_state;
                private readonly long[] m_indicies;
                private readonly long m_length;

                public SyntaxNodeReplacement(IScriptArray expressions, InterpreterState state)
                {
                    m_expressions = expressions;
                    m_state = state;
                    m_indicies = new long[1];
                    m_length = expressions.GetLength(0);
                }

                private ISyntaxTreeNode Visit(long id)
                {
                    switch (id < m_length)
                    {
                        case true:
                            m_indicies[0] = id;
                            var node = m_expressions[m_indicies, m_state];
                            if (node is IScriptCodeElement<ScriptCodeExpression>)
                                return ((IScriptCodeElement<ScriptCodeExpression>)node).CodeObject;
                            else if (node is IScriptCodeElement<ScriptCodeStatement>)
                                return ((IScriptCodeElement<ScriptCodeStatement>)node).CodeObject;
                            else return null;
                        default: return null;
                    }
                }

                private ISyntaxTreeNode Visit(ISyntaxTreeNode node)
                {
                    return node is ScriptCodePlaceholderExpression ? Visit(((ScriptCodePlaceholderExpression)node).PlaceholderID) ?? node : node;
                }

                public static implicit operator Converter<ISyntaxTreeNode, ISyntaxTreeNode>(SyntaxNodeReplacement repl)
                {
                    return repl != null ? new Converter<ISyntaxTreeNode, ISyntaxTreeNode>(repl.Visit) : null;
                }
            }
            #endregion

            private const string FirstParamName = "tree";
            private const string SecondParamName = "expressions";

            public DeduceAction()
                : base(FirstParamName, Instance, SecondParamName, new ScriptArrayContract(Instance), Instance)
            {
            }

            private static IScriptObject Invoke(ScriptCodeExpression tree, IScriptArray expressions, InterpreterState state)
            {
                //clone into-the-deep an expression tree
                tree = Extensions.Clone(tree);
                //visit expression tree nodes and replace placeholders
                return Convert(tree.Visit(null, new SyntaxNodeReplacement(expressions, state)));
            }

            protected override IScriptObject Invoke(InvocationContext ctx, IScriptCodeElement<ScriptCodeExpression> tree, IScriptArray expressions)
            {
                return Invoke(tree.CodeObject, expressions, ctx.RuntimeState);
            }
        }

        [ComVisible(false)]
        private sealed class VisitAction : ScriptFunc<IScriptCodeElement<ScriptCodeExpression>, IScriptAction>
        {
            private const string FirstParamName = "tree";
            private const string SecondParamName = "visitor";

            public VisitAction()
                : base(FirstParamName, Instance, SecondParamName, ScriptSuperContract.Instance, Instance)
            {
            }

            protected override IScriptObject Invoke(InvocationContext ctx, IScriptCodeElement<ScriptCodeExpression> tree, IScriptAction visitor)
            {
                return Convert(tree.CodeObject.Visit(null, new ScriptSyntaxTreeVisitor(visitor, ctx.RuntimeState)));
            }
        }

        [ComVisible(false)]
        private sealed class CloneAction : ScriptFunc<IScriptCodeElement<ScriptCodeExpression>>
        {
            private const string FirstParamName = "tree";

            public CloneAction()
                : base(FirstParamName, Instance, Instance)
            {
            }

            protected override IScriptObject Invoke(InvocationContext ctx, IScriptCodeElement<ScriptCodeExpression> tree)
            {
                return Convert(Extensions.Clone(tree.CodeObject));
            }
        }

        [ComVisible(false)]
        private sealed class ParseAction : ScriptFunc<ScriptString>
        {
            private const string FirstParamName = "code";

            public ParseAction()
                : base(FirstParamName, ScriptStringContract.Instance, Instance)
            {
            }

            protected override IScriptObject Invoke(InvocationContext ctx, ScriptString sourceCode)
            {
                return Parse(sourceCode);
            }
        }

        [ComVisible(false)]
        private sealed class ReduceAction : ScriptFunc<IScriptExpression<ScriptCodeExpression>>
        {
            private const string FirstParamName = "e";

            public ReduceAction()
                : base(FirstParamName, Instance, Instance)
            {
            }

            protected override IScriptObject Invoke(InvocationContext ctx, IScriptExpression<ScriptCodeExpression> runtimeExpr)
            {
                return Reduce(runtimeExpr, ctx.RuntimeState);
            }
        }

        [ComVisible(false)]
        private sealed class CompileAction : ScriptFunc<IScriptExpression<ScriptCodeExpression>>
        {
            private const string FirstParamName = "expression";

            public CompileAction()
                : base(FirstParamName, Instance, ScriptSuperContract.Instance)
            {
            }

            protected override IScriptObject Invoke(InvocationContext ctx, IScriptExpression<ScriptCodeExpression> arg0)
            {
                return Compile(arg0, ctx.RuntimeState);
            }
        }

        /// <summary>
        /// Represents expression tree converter.
        /// This class cannot be inherited.
        /// </summary>
        [ComVisible(false)]
        private sealed class ExpressionConverter : RuntimeConverter<ScriptCodeExpression>
        {
            /// <summary>
            /// Converts an expression tree to its runtime representation.
            /// </summary>
            /// <param name="input">An expression tree to convert.</param>
            /// <param name="result">Conversion result.</param>
            /// <returns><see langword="true"/> if the specified expression tree is converted successfully; otherwise, <see langword="false"/>.</returns>
            public override bool Convert(ScriptCodeExpression input, out IScriptObject result)
            {
                if (input is ScriptCodeBuiltInContractExpression)
                    result = new ScriptConstantExpression((ScriptCodeBuiltInContractExpression)input);
                else if (input is ScriptCodePrimitiveExpression && input is ILiteralExpression)
                    result = new ScriptConstantExpression((ScriptCodePrimitiveExpression)input);
                else if (input is ScriptCodeBinaryOperatorExpression)
                    result = new ScriptBinaryExpression((ScriptCodeBinaryOperatorExpression)input);
                else if (input is ScriptCodeUnaryOperatorExpression)
                    result = new ScriptUnaryExpression((ScriptCodeUnaryOperatorExpression)input);
                else if (input is ScriptCodeAsyncExpression)
                    result = new ScriptAsyncExpression((ScriptCodeAsyncExpression)input);
                else if (input is ScriptCodeVariableReference)
                    result = new ScriptNameTokenExpression((ScriptCodeVariableReference)input);
                else if (input is ScriptCodeArrayContractExpression)
                    result = new ScriptArrayContractExpression((ScriptCodeArrayContractExpression)input);
                else if (input is ScriptCodeCurrentActionExpression)
                    result = ScriptCurrentActionExpression.Instance;
                else if (input is ScriptCodeThisExpression)
                    result = ScriptThisExpression.Instance;
                else if (input is ScriptCodeArrayExpression)
                    result = new ScriptArrayExpression((ScriptCodeArrayExpression)input);
                else if (input is ScriptCodeAwaitExpression)
                    result = new ScriptAwaitExpression((ScriptCodeAwaitExpression)input);
                else if (input is ScriptCodeForkExpression)
                    result = new ScriptForkExpression((ScriptCodeForkExpression)input);
                else if (input is ScriptCodeConditionalExpression)
                    result = new ScriptConditionalExpression((ScriptCodeConditionalExpression)input);
                else if (input is ScriptCodeInvocationExpression)
                    result = new ScriptInvocationExpression((ScriptCodeInvocationExpression)input);
                else if (input is ScriptCodeIndexerExpression)
                    result = new ScriptIndexerExpression((ScriptCodeIndexerExpression)input);
                else if (input is ScriptCodeForEachLoopExpression)
                    result = new ScriptForEachExpression((ScriptCodeForEachLoopExpression)input);
                else if (input is ScriptCodeForLoopExpression)
                    result = new ScriptForExpression((ScriptCodeForLoopExpression)input);
                else if (input is ScriptCodeWhileLoopExpression)
                    result = new ScriptWhileExpression((ScriptCodeWhileLoopExpression)input);
                else if (input is ScriptCodeObjectExpression)
                    result = new ScriptObjectExpression((ScriptCodeObjectExpression)input);
                else if (input is ScriptCodeActionContractExpression)
                    result = new ScriptActionContractExpression((ScriptCodeActionContractExpression)input);
                else if (input is ScriptCodeActionImplementationExpression)
                    result = new ScriptActionExpression((ScriptCodeActionImplementationExpression)input);
                else if (input is ScriptCodeTryElseFinallyExpression)
                    result = new ScriptSehExpression((ScriptCodeTryElseFinallyExpression)input);
                else if (input is ScriptCodePlaceholderExpression)
                    result = new ScriptPlaceholderExpression((ScriptCodePlaceholderExpression)input);
                else if (input is ScriptCodeComplexExpression)
                    result = new ScriptComplexExpression((ScriptCodeComplexExpression)input);
                else if (input is ScriptCodeExpandExpression)
                    result = new ScriptExpandExpression((ScriptCodeExpandExpression)input);
                else result = null;
                return result != null;
            }
        }

        [ComVisible(false)]
        private sealed class ValueEqualityAction : ScriptFunc<IScriptExpression<ScriptCodeExpression>, IScriptExpression<ScriptCodeExpression>>
        {
            private const string FirstParamName = "value1";
            private const string SecondParamName = "value2";

            public ValueEqualityAction()
                : base(FirstParamName, Instance, SecondParamName, Instance, ScriptBooleanContract.Instance)
            {
            }

            protected override IScriptObject Invoke(InvocationContext ctx, IScriptExpression<ScriptCodeExpression> arg0, IScriptExpression<ScriptCodeExpression> arg1)
            {
                return (ScriptBoolean)ScriptExpressionFactory.Equals(arg0, arg1);
            }
        }

        [ComVisible(false)]
        private sealed class ReferenceEqualityAction : ScriptFunc<IScriptExpression<ScriptCodeExpression>, IScriptExpression<ScriptCodeExpression>>
        {
            private const string FirstParamName = "value1";
            private const string SecondParamName = "value2";

            public ReferenceEqualityAction()
                : base(FirstParamName, Instance, SecondParamName, Instance, ScriptBooleanContract.Instance)
            {
            }

            protected override IScriptObject Invoke(InvocationContext ctx, IScriptExpression<ScriptCodeExpression> arg0, IScriptExpression<ScriptCodeExpression> arg1)
            {
                return (ScriptBoolean)ScriptObject.ReferenceEquals(arg0, arg1);
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

        static ScriptExpressionFactory()
        {
            RegisterConverter<ExpressionConverter>();
        }

        /// <summary>
        /// Gets name of this contract.
        /// </summary>
        internal static string Name
        {
            get { return Keyword.Expr; }
        }

        private IRuntimeSlot m_reduce;
        private IRuntimeSlot m_compile;
        private IRuntimeSlot m_constant;
        private IRuntimeSlot m_nmtok;
        private IRuntimeSlot m_binop;
        private IRuntimeSlot m_unop;
        private IRuntimeSlot m_async;
        private IRuntimeSlot m_equ;
        private IRuntimeSlot m_requ;
        private IRuntimeSlot m_ca;
        private IRuntimeSlot m_arcon;
        private IRuntimeSlot m_thisref;
        private IRuntimeSlot m_array;
        private IRuntimeSlot m_await;
        private IRuntimeSlot m_fork;
        private IRuntimeSlot m_conditional;
        private IRuntimeSlot m_inv;
        private IRuntimeSlot m_indexer;
        private IRuntimeSlot m_init;
        private IRuntimeSlot m_foreach;
        private IRuntimeSlot m_for;
        private IRuntimeSlot m_while;
        private IRuntimeSlot m_obj;
        private IRuntimeSlot m_sig;
        private IRuntimeSlot m_action;
        private IRuntimeSlot m_parse;
        private IRuntimeSlot m_seh;
        private IRuntimeSlot m_selection;
        private IRuntimeSlot m_clone;
        private IRuntimeSlot m_visit;
        private IRuntimeSlot m_placeholder;
        private IRuntimeSlot m_deduce;
        private IRuntimeSlot m_cplx;
        private IRuntimeSlot m_expand;

        /// <summary>
        /// Deserializes runtime expression factory.
        /// </summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        private ScriptExpressionFactory(SerializationInfo info, StreamingContext context)
            : base()
        {
        }

        private ScriptExpressionFactory()
        {
        }

        internal override Keyword Token
        {
            get { return Keyword.Expr; }
        }

        /// <summary>
        /// Represents singleton instance of this contract.
        /// </summary>
        public static readonly ScriptExpressionFactory Instance = new ScriptExpressionFactory();

        /// <summary>
        /// Determines relationship with other contract.
        /// </summary>
        /// <param name="contract"></param>
        /// <returns></returns>
        public override ContractRelationshipType GetRelationship(IScriptContract contract)
        {
            if (contract is ScriptExpressionFactory)
                return ContractRelationshipType.TheSame;
            else if (contract is IScriptExpressionContract<ScriptCodeExpression>)
                return ContractRelationshipType.Superset;
            else if (contract.OneOf<ScriptSuperContract, ScriptMetaContract>())
                return ContractRelationshipType.Subset;
            else if (contract.OneOf<IScriptComplementation, IScriptUnionContract, IScriptCartesianProduct>())
                return Inverse(contract.GetRelationship(this));
            else return ContractRelationshipType.None;
        }

        internal static MemberExpression Expression
        {
            get
            {
                return LinqHelpers.BodyOf<Func<ScriptExpressionFactory>, MemberExpression>(() => Instance);
            }
        }

        /// <summary>
        /// Makes a new string constant expression.
        /// </summary>
        /// <param name="value">A string literal.</param>
        /// <returns>A new constant expression.</returns>
        public static IScriptExpression<ScriptCodePrimitiveExpression> MakeConstant(string value)
        {
            return new ScriptConstantExpression(new ScriptCodeStringExpression(value ?? string.Empty));
        }

        private static IScriptObject Parse(ScriptCodeStatement stmt)
        {
            return stmt is IScriptExpressionStatement ? Convert(((IScriptExpressionStatement)stmt).Expression) : Convert(stmt);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sourceCode"></param>
        /// <returns></returns>
        public static IScriptObject Parse(string sourceCode)
        {
            var result = new ScriptCodeComplexExpression();
            SyntaxAnalyzer.Parse(sourceCode, result);
            switch (result.Body.Count)
            {
                case 0: return Void;
                case 1: return Parse(result.Body[0]);
                default: return Convert(result);
            }
        }

        /// <summary>
        /// Populates quoted expression with the specified array of expressions.
        /// </summary>
        /// <param name="target"></param>
        /// <param name="substitutes"></param>
        /// <param name="state">Internal interpreter state.</param>
        /// <returns></returns>
        public static IScriptObject Expand(ScriptCodeExpression target, ScriptCodeExpression[] substitutes, InterpreterState state)
        {
            var result = ScriptCodePlaceholderExpression.Expand(target, substitutes);
            if (result.CanReduce) result = result.Reduce(state.Context);
            return Compile(result, state);
        }

        /// <summary>
        /// Populates quoted expression with the specified array of expressions.
        /// </summary>
        /// <param name="target"></param>
        /// <param name="substitutes"></param>
        /// <param name="state">Internal interpreter state.</param>
        /// <returns></returns>
        public static IScriptObject Expand(IScriptObject target, ScriptCodeExpression[] substitutes, InterpreterState state)
        {
            if (target is IRuntimeSlot) target = ((IRuntimeSlot)target).GetValue(state);
            return target is IScriptCodeElement<ScriptCodeExpression> ? Expand(((IScriptCodeElement<ScriptCodeExpression>)target).CodeObject, substitutes, state) : target;
        }

        internal static MethodCallExpression Expand(Expression target, IEnumerable<ScriptCodeExpression> substitutes, ParameterExpression state)
        {
            var call = LinqHelpers.BodyOf<IScriptObject, ScriptCodeExpression[], InterpreterState, IScriptObject, MethodCallExpression>((t, s, st) => Expand(t, s, st));
            return call.Update(null, new[] { target, LinqHelpers.NewArray(substitutes), state });
        }

        /// <summary>
        /// Makes a new boolean constant expression.
        /// </summary>
        /// <param name="value">A boolean literal.</param>
        /// <returns>A new constant expression.</returns>
        public static IScriptExpression<ScriptCodePrimitiveExpression> MakeConstant(bool value)
        {
            return new ScriptConstantExpression(new ScriptCodeBooleanExpression(value));
        }

        /// <summary>
        /// Makes a new void constant expression.
        /// </summary>
        /// <returns></returns>
        public static IScriptExpression<ScriptCodePrimitiveExpression> MakeVoidConstant()
        {
            return new ScriptConstantExpression(ScriptCodeVoidExpression.Instance);
        }

        /// <summary>
        /// Creates a new runtime representation of the integer literal.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static IScriptExpression<ScriptCodePrimitiveExpression> MakeConstant(long value)
        {
            return new ScriptConstantExpression(new ScriptCodeIntegerExpression(value));
        }

        /// <summary>
        /// Creates a new runtime representation of the floating-point number literal.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static IScriptExpression<ScriptCodePrimitiveExpression> MakeConstant(double value)
        {
            return new ScriptConstantExpression(new ScriptCodeRealExpression(value));
        }

        /// <summary>
        /// Compiles the specified expression.
        /// </summary>
        /// <param name="expression"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        public static IScriptObject Compile<TExpression>(IScriptExpression<TExpression> expression, InterpreterState state = null)
            where TExpression : ScriptCodeExpression
        {
            if (state == null) state = InterpreterState.Current;
            switch (expression != null)
            {
                case true: return expression.Compile(state);
                default: throw new VoidException(state);
            }
        }

        /// <summary>
        /// Compiles an array of expressions.
        /// </summary>
        /// <param name="expressions"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        public static IScriptObject[] Compile(IScriptExpression<ScriptCodeExpression>[] expressions, InterpreterState state)
        {
            return Array.ConvertAll(expressions, expr => Compile(expr, state));
        }

        internal static IScriptObject[] Compile(ScriptCodeExpression[] expressions, InterpreterState state)
        {
            return Array.ConvertAll(expressions, expr => Compile(expr, state));
        }

        internal static IScriptObject[] Compile(CodeExpressionCollection expressions, InterpreterState state)
        {
            var result = new IScriptObject[expressions.Count];
            for (var i = 0; i < expressions.Count; i++)
                result[i] = Compile(expressions[i] as ScriptCodeExpression, state);
            return result;
        }

        internal static IScriptObject Compile<TExpression>(TExpression expression, InterpreterState state)
            where TExpression : ScriptCodeExpression
        {
            return Compile<TExpression>(Convert(expression) as IScriptExpression<TExpression>, state);
        }

        private static ScriptCodeExpression CreateExpression(IScriptObject expr)
        {
            return expr is IScriptExpression<ScriptCodeExpression> ? ((IScriptExpression<ScriptCodeExpression>)expr).CodeObject : null;
        }

        internal static IEnumerable<ScriptCodeExpression> CreateExpressions(IEnumerable<IScriptObject> exprs)
        {
            if (exprs == null) exprs = Enumerable.Empty<IScriptObject>();
            return exprs.SelectNotNull(CreateExpression);
        }

        internal static void CreateExpressions(IEnumerable<IScriptObject> exprs, ScriptCodeExpressionCollection output)
        {
            foreach (var e in CreateExpressions(exprs))
                output.Add(e);
        }

        internal static IScriptArray CreateExpressions(ScriptCodeExpressionCollection expressions, InterpreterState state)
        {
            var indicies = new long[1];
            var result = new ScriptArray(Instance, expressions.Count);
            for (var i = 0; i < expressions.Count; i++)
                result[indicies, state] = Convert(expressions[i]);
            return result;
        }

        /// <summary>
        /// Reduces runtime expression.
        /// </summary>
        /// <param name="runtimeExpr"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        public static IScriptExpression<ScriptCodeExpression> Reduce(IScriptExpression<ScriptCodeExpression> runtimeExpr, InterpreterState state)
        {
            return Convert(runtimeExpr.CodeObject.Reduce(state.Context)) as IScriptExpression<ScriptCodeExpression>;
        }

        /// <summary>
        /// Determines whether the two script objects represents the same expression tree.
        /// </summary>
        /// <param name="value1"></param>
        /// <param name="value2"></param>
        /// <returns></returns>
        public static bool Equals(IScriptExpression<ScriptCodeExpression> value1, IScriptExpression<ScriptCodeExpression> value2)
        {
            return value1 != null ? Equals(value1.CodeObject, value2.CodeObject) : value2 == null;
        }

        /// <summary>
        /// Gets constant contract.
        /// </summary>
        public static IScriptExpressionContract<ScriptCodePrimitiveExpression> Constant
        {
            get { return ScriptConstantExpressionFactory.Instance; }
        }

        /// <summary>
        /// Gets variable reference contract.
        /// </summary>
        public static IScriptExpressionContract<ScriptCodeVariableReference> NameToken
        {
            get { return ScriptNameTokenExpressionFactory.Instance; }
        }

        /// <summary>
        /// Creates a new runtime representation of the variable reference.
        /// </summary>
        /// <param name="variableName"></param>
        /// <returns></returns>
        public static IScriptExpression<ScriptCodeVariableReference> MakeNameToken(string variableName)
        {
            return ScriptNameTokenExpressionFactory.CreateExpression(variableName);
        }

        /// <summary>
        /// Gets binary expression factory.
        /// </summary>
        public static IScriptExpressionContract<ScriptCodeBinaryOperatorExpression> Binary
        {
            get { return ScriptBinaryExpressionFactory.Instance; }
        }

        /// <summary>
        /// Gets unary expression factory.
        /// </summary>
        public static IScriptExpressionContract<ScriptCodeUnaryOperatorExpression> Unary
        {
            get { return ScriptUnaryExpressionFactory.Instance; }
        }

        /// <summary>
        /// Gets async data type factory.
        /// </summary>
        public static IScriptExpressionContract<ScriptCodeAsyncExpression> Async
        {
            get { return ScriptAsyncExpressionFactory.Instance; }
        }

        /// <summary>
        /// Gets an expression factory that produces a reference to the current action.
        /// </summary>
        public static IScriptExpressionContract<ScriptCodeCurrentActionExpression> CurrentAction
        {
            get { return ScriptCurrentActionExpressionFactory.Instance; }
        }

        /// <summary>
        /// Gets an expression factory that produces runtime representation of the array contract definition.
        /// </summary>
        public static IScriptExpressionContract<ScriptCodeArrayContractExpression> ArrayContract
        {
            get { return ScriptArrayContractExpressionFactory.Instance; }
        }

        /// <summary>
        /// Gets an expression factory that produces this reference.
        /// </summary>
        public static IScriptExpressionContract<ScriptCodeThisExpression> ThisRef
        {
            get { return ScriptThisExpressionFactory.Instance; }
        }

        /// <summary>
        /// Gets an expression factory that produces array expression.
        /// </summary>
        public static IScriptExpressionContract<ScriptCodeArrayExpression> ArrayExpr
        {
            get { return ScriptArrayExpressionFactory.Instance; }
        }

        /// <summary>
        /// Gets an expression factory that produces await expression.
        /// </summary>
        public static IScriptExpressionContract<ScriptCodeAwaitExpression> AwaitExpr
        {
            get { return ScriptAwaitExpressionFactory.Instance; }
        }

        /// <summary>
        /// Gets an expression factory that produces fork expression.
        /// </summary>
        public static IScriptExpressionContract<ScriptCodeForkExpression> ForkExpr
        {
            get { return ScriptForkExpressionFactory.Instance; }
        }

        /// <summary>
        /// Gets an expression factory that produces conditional expression.
        /// </summary>
        public static IScriptExpressionContract<ScriptCodeConditionalExpression> Conditional
        {
            get { return ScriptConditionalExpressionFactory.Instance; }
        }

        /// <summary>
        /// Gets an expression factory that produces invocation expression.
        /// </summary>
        public static IScriptExpressionContract<ScriptCodeInvocationExpression> Invocation
        {
            get { return ScriptInvocationExpressionFactory.Instance; }
        }

        /// <summary>
        /// Gets an expression factory that produces indexer expression.
        /// </summary>
        public static new IScriptExpressionContract<ScriptCodeIndexerExpression> Indexer
        {
            get{return ScriptIndexerExpressionFactory.Instance;}
        }

        /// <summary>
        /// Gets an expression factory that produces 'for-each' loop.
        /// </summary>
        public static IScriptExpressionContract<ScriptCodeForEachLoopExpression> ForEach
        {
            get{return ScriptForEachExpressionFactory.Instance;}
        }

        /// <summary>
        /// Gets an expression factory that produces 'for' loop.
        /// </summary>
        public static IScriptExpressionContract<ScriptCodeForLoopExpression> For
        {
            get{return ScriptForExpressionFactory.Instance;}
        }

        /// <summary>
        /// Gets an expression factory that produces 'while' loop.
        /// </summary>
        public static IScriptExpressionContract<ScriptCodeWhileLoopExpression> While
        {
            get { return ScriptWhileExpressionFactory.Instance; }
        }

        /// <summary>
        /// Gets an expression factory that produces composite object. 
        /// </summary>
        public static IScriptExpressionContract<ScriptCodeObjectExpression> Obj
        {
            get { return ScriptObjectExpressionFactory.Instance; }
        }

        /// <summary>
        /// Gets an expression factory that produces an action contract.
        /// </summary>
        public static IScriptExpressionContract<ScriptCodeActionContractExpression> Signature
        {
            get { return ScriptActionContractExpressionFactory.Instance; }
        }

        /// <summary>
        /// Gets an expression factory that produces an action.
        /// </summary>
        public static IScriptExpressionContract<ScriptCodeActionImplementationExpression> Action
        {
            get { return ScriptActionExpressionFactory.Instance; }
        }

        /// <summary>
        /// Gets an expression factory that produces SEH block.
        /// </summary>
        public static IScriptExpressionContract<ScriptCodeTryElseFinallyExpression> SEH
        {
            get { return ScriptSehExpressionFactory.Instance; }
        }

        /// <summary>
        /// Gets an expression factory that produces SEH block.
        /// </summary>
        public static IScriptExpressionContract<ScriptCodeSelectionExpression> Selection
        {
            get { return ScriptSelectionExpressionFactory.Instance; }
        }

        /// <summary>
        /// Gets an expression factory that produces placeholder expression.
        /// </summary>
        public static IScriptExpressionContract<ScriptCodePlaceholderExpression> Placeholder
        {
            get{return ScriptPlaceholderExpressionFactory.Instance;}
        }

        /// <summary>
        /// Creates a new placeholder.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static IScriptExpression<ScriptCodePlaceholderExpression> MakePlaceholder(long id)
        {
            return new ScriptPlaceholderExpression(id);
        }

        internal static MethodCallExpression MakePlaceholder(ScriptCodePlaceholderExpression id)
        {
            var call = LinqHelpers.BodyOf<long, IScriptExpression<ScriptCodePlaceholderExpression>, MethodCallExpression>(i => MakePlaceholder(i));
            return call.Update(null, new[] { LinqHelpers.Constant(id.PlaceholderID) });
        }

        /// <summary>
        /// Gets an expression factory that produces a complex expression.
        /// </summary>
        public static IScriptExpressionContract<ScriptCodeComplexExpression> Complex
        {
            get{return ScriptComplexExpressionFactory.Instance;}
        }

        /// <summary>
        /// Gets 
        /// </summary>
        public static IScriptExpressionContract<ScriptCodeExpandExpression> Expandq
        {
            get { return ScriptExpandExpressionFactory.Instance; }
        }

        /// <summary>
        /// Releases all memory associated with the cached runtime slots.
        /// </summary>
        public void Clear()
        {
            m_reduce = m_compile = m_constant =
            m_nmtok = m_binop = m_unop = m_async =
            m_equ = m_requ = m_ca =
            m_arcon = m_thisref = m_array =
            m_await = m_fork = m_conditional =
            m_inv =
            m_indexer =
            m_foreach =
            m_for =
            m_while =
            m_obj =
            m_sig =
            m_action =
            m_seh =
            m_clone =
            m_visit =
            m_placeholder = m_deduce =
            m_selection =
            m_cplx =
            m_expand = null;
            ScriptExpandExpressionFactory.Instance.Clear();
            ScriptComplexExpressionFactory.Instance.Clear();
            ScriptPlaceholderExpressionFactory.Instance.Clear();
            ScriptSelectionExpressionFactory.Instance.Clear();
            ScriptSehExpressionFactory.Instance.Clear();
            ScriptActionExpressionFactory.Instance.Clear();
            ScriptActionContractExpressionFactory.Instance.Clear();
            ScriptObjectExpressionFactory.Instance.Clear();
            ScriptWhileExpressionFactory.Instance.Clear();
            ScriptForExpressionFactory.Instance.Clear();
            ScriptForEachExpressionFactory.Instance.Clear();
            ScriptIndexerExpressionFactory.Instance.Clear();
            ScriptInvocationExpressionFactory.Instance.Clear();
            ScriptForkExpressionFactory.Instance.Clear();
            ScriptAwaitExpressionFactory.Instance.Clear();
            ScriptArrayExpressionFactory.Instance.Clear();
            ScriptThisExpressionFactory.Instance.Clear();
            ScriptArrayContractExpressionFactory.Instance.Clear();
            ScriptCurrentActionExpressionFactory.Instance.Clear();
            ScriptAsyncExpressionFactory.Instance.Clear();
            ScriptUnaryExpressionFactory.Instance.Clear();
            ScriptBinaryExpressionFactory.Instance.Clear();
            ScriptNameTokenExpressionFactory.Instance.Clear();
            ScriptConstantExpressionFactory.Instance.Clear();
            ScriptConditionalExpressionFactory.Instance.Clear();
            GC.Collect();
        }

        #region Runtime Slots
        IRuntimeSlot IExpressionFactorySlots.Expand
        {
            get { return CacheConst(ref m_expand, () => Expandq); }
        }

        IRuntimeSlot IExpressionFactorySlots.Cplx
        {
            get { return CacheConst(ref m_cplx, () => Complex); }
        }

        IRuntimeSlot IExpressionFactorySlots.Deduce
        {
            get { return CacheConst<DeduceAction>(ref m_deduce); }
        }

        IRuntimeSlot IExpressionFactorySlots.Placeholder
        {
            get { return CacheConst(ref m_placeholder, () => Placeholder); }
        }

        IRuntimeSlot IExpressionFactorySlots.Visit
        {
            get { return CacheConst<VisitAction>(ref m_visit); }
        }

        IRuntimeSlot IExpressionFactorySlots.Clone
        {
            get { return CacheConst<CloneAction>(ref m_clone); }
        }

        IRuntimeSlot IExpressionFactorySlots.Selection
        {
            get { return CacheConst(ref m_selection, () => Selection); }
        }

        IRuntimeSlot IExpressionFactorySlots.Seh
        {
            get { return CacheConst(ref m_seh, () => SEH); }
        }

        IRuntimeSlot IExpressionFactorySlots.Parse
        {
            get { return CacheConst<ParseAction>(ref m_parse); }
        }

        IRuntimeSlot IExpressionFactorySlots.Action
        {
            get { return CacheConst(ref m_action, () => Action); }
        }

        IRuntimeSlot IExpressionFactorySlots.Signature
        {
            get { return CacheConst(ref m_sig, () => Signature); }
        }

        IRuntimeSlot IExpressionFactorySlots.Obj
        {
            get { return CacheConst(ref m_obj, () => Obj); }
        }

        IRuntimeSlot IExpressionFactorySlots.WhileLoop
        {
            get { return CacheConst(ref m_while, () => While); }
        }

        IRuntimeSlot IExpressionFactorySlots.ForLoop
        {
            get { return CacheConst(ref m_for, () => For); }
        }

        IRuntimeSlot IExpressionFactorySlots.ForEach
        {
            get { return CacheConst(ref m_foreach, () => ForEach); }
        }

        IRuntimeSlot IExpressionFactorySlots.Init
        {
            get { return CacheConst<InitAction>(ref m_init); }
        }

        IRuntimeSlot IExpressionFactorySlots.Indexer
        {
            get { return CacheConst(ref m_indexer, () => Indexer); }
        }

        IRuntimeSlot IExpressionFactorySlots.Inv
        {
            get { return CacheConst(ref m_inv, () => Invocation); }
        }

        IRuntimeSlot IExpressionFactorySlots.Cond
        {
            get { return CacheConst(ref m_conditional, () => Conditional); }
        }

        IRuntimeSlot IExpressionFactorySlots.ForkDef
        {
            get { return CacheConst(ref m_fork, () => ForkExpr); }
        }

        IRuntimeSlot IExpressionFactorySlots.AwaitDef
        {
            get { return CacheConst(ref m_await, () => AwaitExpr); }
        }

        IRuntimeSlot IExpressionFactorySlots.Array
        {
            get { return CacheConst(ref m_array, () => ArrayExpr); }
        }

        IRuntimeSlot IExpressionFactorySlots.ThisRef
        {
            get { return CacheConst(ref m_thisref, () => ThisRef); }
        }

        IRuntimeSlot IExpressionFactorySlots.Arcon
        {
            get { return CacheConst(ref m_arcon, () => ArrayContract); }
        }

        IRuntimeSlot IExpressionFactorySlots.Ca
        {
            get { return CacheConst(ref m_ca, () => CurrentAction); }
        }

        IRuntimeSlot IExpressionFactorySlots.AsyncDef
        {
            get { return CacheConst(ref m_async, () => Async); }
        }

        IRuntimeSlot IExpressionFactorySlots.UnOp
        {
            get { return CacheConst(ref m_unop, () => Unary); }
        }

        IRuntimeSlot IExpressionFactorySlots.BinOp
        {
            get { return CacheConst(ref m_binop, () => Binary); }
        }

        IRuntimeSlot IExpressionFactorySlots.Compile
        {
            get { return CacheConst<CompileAction>(ref m_compile); }
        }

        IRuntimeSlot IExpressionFactorySlots.Constant
        {
            get { return CacheConst(ref m_constant, () => Constant); }
        }

        IRuntimeSlot IExpressionFactorySlots.NmToken
        {
            get { return CacheConst(ref m_nmtok, () => NameToken); }
        }

        IRuntimeSlot IExpressionFactorySlots.Equ
        {
            get { return CacheConst<ValueEqualityAction>(ref m_equ); }
        }

        IRuntimeSlot IExpressionFactorySlots.REqu
        {
            get { return CacheConst<ReferenceEqualityAction>(ref m_requ); }
        }

        IRuntimeSlot IExpressionFactorySlots.Reduce
        {
            get { return CacheConst<ReduceAction>(ref m_reduce); }
        }
        #endregion

    }
}
