﻿using System;
using System.Linq;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.CodeDom;
using System.Reflection;

namespace DynamicScript.Compiler.Ast.Translation.LinqExpressions
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;
    using Runtime.Environment;
    using Runtime.Environment.Threading;
    using Runtime.Environment.ExpressionTrees;
    using Runtime;
    using CodeExpression = System.CodeDom.CodeExpression;
    using DynamicMetaObject = System.Dynamic.DynamicMetaObject;
    using MethodBuilder = System.Reflection.Emit.MethodBuilder;
    using DebugInfoGenerator = System.Runtime.CompilerServices.DebugInfoGenerator;

    /// <summary>
    /// Represents transformer that converts DynamicScript code document into the LINQ expression tree.
    /// This class cannot be inherited.
    /// </summary>
    [ComVisible(false)]
    public sealed class LinqExpressionTranslator : Translator<Expression, LexicalScope>
    {
        private readonly SourceCodeInfo m_dinfo;
        private readonly IDictionary<long, MethodCallExpression> m_intern;

        /// <summary>
        /// Initializes a new CodeDOM-to-LINQET transformer.
        /// </summary>
        /// <param name="parser">An enumerator through code statements. Cannot be <see langword="null"/>.</param>
        /// <param name="errMode">Error mode.</param>
        /// <param name="source">Information about source code.</param>
        public LinqExpressionTranslator(IEnumerator<ScriptCodeStatement> parser, ErrorMode errMode, SourceCodeInfo source = null)
            : base(parser, errMode, source != null ? source.FileName : null)
        {
            m_intern = new Dictionary<long, MethodCallExpression>(300);
            m_dinfo = source;
        }

        /// <summary>
        /// Gets information about source code.
        /// </summary>
        public SourceCodeInfo Source
        {
            get { return m_dinfo; }
        }

        private bool Debug
        {
            get { return Source != null && Source.DebugSource != null; }
        }

        private Expression AsRightSide(Expression rightSide, TranslationContext context)
        {
            return ScriptObject.AsRightSide(rightSide, context.Scope.StateHolder);
        }

        /// <summary>
        /// Extracts interpretation context from translation context.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        protected override InterpretationContext GetInterpretationContext(TranslationContext context)
        {
            var contextScope = context.Lookup<ContextScope>();
            return contextScope != null ? contextScope.Context : InterpretationContext.Default;
        }

        /// <summary>
        /// Initializes a new instance of the root scope.
        /// </summary>
        /// <param name="userData">A collection of user-defined properties.</param>
        /// <returns>A new instance of the root scope.</returns>
        protected override LexicalScope CreateRootScope(dynamic userData)
        {
            return new GlobalScope();
        }

        /// <summary>
        /// Translates comment to LINQ expression.
        /// </summary>
        /// <param name="comment"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        protected override Expression Translate(ScriptCodeCommentStatement comment, Translator<Expression, LexicalScope>.TranslationContext context)
        {
            return Expression.Empty();
        }

        /// <summary>
        /// Emits LINQ-ET debug information.
        /// </summary>
        /// <param name="info">Translation-neutral debug information.</param>
        /// <returns>LINQ-compliant representation of the debug information.</returns>
        protected override Expression Translate(ScriptDebugInfo info)
        {
            return Debug ? Expression.DebugInfo(Source.DebugSource, info.StartLine, info.StartColumn, info.EndLine, info.EndColumn) : null;
        }

        /// <summary>
        /// Translates 
        /// </summary>
        /// <param name="currentQuote"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        protected override Expression Translate(ScriptCodeCurrentQuoteExpression currentQuote, TranslationContext context)
        {
            var actionScope = context.Lookup<FunctionScope>();
            return actionScope != null ? ScriptObject.MakeConverter(actionScope.Expression, context.Scope.StateHolder) : ScriptObject.MakeVoid();
        }

        /// <summary>
        /// Translates asynchronous contract.
        /// </summary>
        /// <param name="expression"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        protected override Expression Translate(ScriptCodeAsyncExpression expression, TranslationContext context)
        {
            return ScriptAcceptorContract.New(Translate(expression.Contract, context), context.Scope.StateHolder);
        }

        /// <summary>
        /// Translates placeholder expression.
        /// </summary>
        /// <param name="placeholder"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        protected override Expression Translate(ScriptCodePlaceholderExpression placeholder, TranslationContext context)
        {
            return ScriptExpressionFactory.MakePlaceholder(placeholder);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="expression"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        protected override Expression Translate(ScriptCodeExpressionContractExpression expression, TranslationContext context)
        {
            return ScriptExpressionFactory.Expression;
        }

        /// <summary>
        /// Translates quoted expression.
        /// </summary>
        /// <param name="expression"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        protected override Expression Translate(ScriptCodeQuoteExpression expression, TranslationContext context)
        {
            return ScriptObject.MakeConverter(LinqHelpers.Restore(expression.Signature.IsEmpty ? expression.Body.Expression : expression), context.Scope.StateHolder);
        }

        /// <summary>
        /// Translates complex expression.
        /// </summary>
        /// <param name="complex"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        protected override Expression Translate(ScriptCodeComplexExpression complex, TranslationContext context)
        {
            IList<Expression> block = new List<Expression>(complex.Body.Count);
            var currentScope = context.Push(parent => new GenericScope(parent, true));
            block.Add(Expression.Label(currentScope.BeginOfScope));
            Translate(complex, context, GotoExpressionKind.Goto, ref block);
            block.Add(Expression.Label(currentScope.EndOfScope, ScriptObject.MakeVoid()));
            context.Pop();
            return Expression.Block(LexicalScope.GetExpressions( currentScope.Locals.Values), block);
        }

        /// <summary>
        /// Translates expand operator.
        /// </summary>
        /// <param name="expandexpr"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        protected override Expression Translate(ScriptCodeExpandExpression expandexpr, TranslationContext context)
        {
            return ScriptExpressionFactory.Expand(Translate(expandexpr.Target, context), expandexpr.Substitutes, context.Scope.StateHolder);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="expression"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        protected override Expression Translate(ScriptCodeStatementContractExpression expression, Translator<Expression, LexicalScope>.TranslationContext context)
        {
            return ScriptStatementFactory.Expression;
        }

        /// <summary>
        /// Translates return statement to LINQ expression.
        /// </summary>
        /// <param name="returnStatement">The statement to be translated.</param>
        /// <param name="context">Translation context.</param>
        /// <returns>LINQ expression that represents return statement.</returns>
        protected override Expression Translate(ScriptCodeReturnStatement returnStatement, TranslationContext context)
        {
            var scope = context.Lookup<RoutineScope, FinallyScope>();
            switch (scope is RoutineScope)
            {
                case true:
                    var routineScope = (RoutineScope)scope;
                    return Expression.Return(routineScope.EndOfScope, AsRightSide(returnStatement.Value != null ? Translate(returnStatement.Value, context) : ScriptObject.MakeVoid(), context));
                default:
                    throw CodeAnalysisException.CannotChangeControlFlow(returnStatement.LinePragma);
            }
        }

        /// <summary>
        /// Translates continuation to LINQ expression.
        /// </summary>
        /// <param name="continueStatement">The statement to be translated.</param>
        /// <param name="context">The translation context.</param>
        /// <returns>LINQ expression that represents continuation.</returns>
        protected override Expression Translate(ScriptCodeContinueStatement continueStatement, TranslationContext context)
        {
            var scope = context.Lookup<FunctionScope, LoopScope, FinallyScope>();
            if (scope is FunctionScope)
            {
                var action = (FunctionScope)scope;
                var result = new List<Expression>(continueStatement.ArgList.Count + 1);
                var i = 0;
                foreach (var p in action.Parameters)
                    if (i < continueStatement.ArgList.Count)
                        result.Add(RuntimeSlot.SetValue(p.Value.Expression, AsRightSide(Translate((ScriptCodeExpression)continueStatement.ArgList[i++], context), context), action.StateHolder));
                result.Add(Expression.Continue(action.BeginOfScope));
                return Expression.Block(result);
            }
            if (scope is LoopScope)
                return ((LoopScope)scope).Continue(from ScriptCodeExpression a in continueStatement.ArgList select AsRightSide(Translate(a, context), context));
            else if (scope is FinallyScope)
                throw CodeAnalysisException.CannotChangeControlFlow(continueStatement.LinePragma);
            else return Expression.Goto(context.Scope.BeginOfScope);
        }

        /// <summary>
        /// Translates break statement to LINQ expression.
        /// </summary>
        /// <param name="breakStatement">The statement to be translated.</param>
        /// <param name="context">Translation context.</param>
        /// <returns></returns>
        protected override Expression Translate(ScriptCodeBreakLexicalScopeStatement breakStatement, TranslationContext context)
        {
            var loopScope = context.Lookup<LoopScope>();
            switch (loopScope != null)
            {
                case true:
                    return ((LoopScope)context.Scope.Parent).Break(from ScriptCodeExpression a in breakStatement.ArgList select AsRightSide(Translate(a, context), context));
                default:
                    var lastExpr = breakStatement.ArgList.Count > 0 ? Translate((ScriptCodeExpression)breakStatement.ArgList[breakStatement.ArgList.Count - 1], context) : ScriptObject.MakeVoid();
                    return Expression.Break(context.Scope.EndOfScope, lastExpr);
            }
        }

        /// <summary>
        /// Translates fault statement to the LINQ-throw expression.
        /// </summary>
        /// <param name="fault">The statement to be translated.</param>
        /// <param name="context">Translation context.</param>
        /// <returns>LINQ-ET representation of the fault statemenet.</returns>
        protected override Expression Translate(ScriptCodeFaultStatement fault, TranslationContext context)
        {
            return ScriptFault.Throw(AsRightSide(Translate(fault.Error, context), context), context.Scope.StateHolder);
        }

        /// <summary>
        /// Translates asynchronous task producer.
        /// </summary>
        /// <param name="forkExpression">The expression to be translated.</param>
        /// <param name="context">Translation context.</param>
        /// <returns>Translated fork expression.</returns>
        protected override Expression Translate(ScriptCodeForkExpression forkExpression, TranslationContext context)
        {
            IList<Expression> body = new List<Expression>(10);
            var currentScope = context.Push(ForkScope.Create);
            body.Label(currentScope.BeginOfScope);
            Translate(forkExpression.Body.UnwrapStatements(), context, GotoExpressionKind.Goto, ref body);
            body.Label(currentScope.EndOfScope, ScriptObject.MakeVoid());
            var result = ScriptFuture.New(forkExpression.Queue != null ? Translate(forkExpression.Queue, context) : null, Expression.Lambda<ScriptWorkItem>(Expression.Block(body), (ParameterExpression)currentScope.ScopeVar, currentScope.StateHolder), AsRightSide(currentScope.Parent.ScopeVar, context), currentScope.Parent.StateHolder);
            context.Pop();
            return result;
        }

        /// <summary>
        /// Translates variable declaration.
        /// </summary>
        /// <param name="variableDeclaration">The variable declaration to be transformed.</param>
        /// <param name="context">Translation context.</param>
        /// <returns>Transformed variable declaration statement.</returns>
        protected override Expression Translate(ScriptCodeVariableDeclaration variableDeclaration, TranslationContext context)
        {
            var variableRef = default(ParameterExpression);
            //Declares a new variable in the current lexical scope.
            switch (context.Scope.DeclareVariable(variableDeclaration.Name, out variableRef, variableDeclaration.GetTypeCode()) && variableRef != null)
            {
                case true:
                    return variableDeclaration.IsConst ? BindToConstant(variableRef, variableDeclaration, context) : BindToVariable(variableRef, variableDeclaration, context);
                default:
                    //Duplicate variable declaration
                    throw CodeAnalysisException.DuplicateIdentifier(variableDeclaration.Name, variableDeclaration.LinePragma);
            }
        }

        #region Constant Binding

        private Expression TranslateConstantValue(ScriptCodeExpression initValue, TranslationContext context)
        {
            var result = Translate(initValue, context);
            switch (initValue is ScriptCodePrimitiveExpression)
            {
                case true: return result;
                default:
                    result = Expression.Block(
                        Expression.Label(context.Scope.BeginOfScope),
                        Expression.Label(context.Scope.EndOfScope, result));
                    result = Expression.Lambda<Func<InterpreterState, IScriptObject>>(result, context.Scope.StateHolder);
                    return result;
            }
        }

        private Expression BindToConstant(ParameterExpression constantRef, ISlot variableDeclaration, TranslationContext context)
        {
            Func<LexicalScope, GenericScope> scopeFactory = parent => new GenericScope(parent, false);
            var value = default(Expression);
            var contractBinding = default(Expression);
            switch (variableDeclaration.Style)
            {
                case SlotDeclarationStyle.InitExpressionOnly:   //bind to constant slot using constant value
                    //Create a new lexical scope associated with the constant value provider (based on lambda)
                    var currentScope = context.Push<GenericScope>(scopeFactory);
                    value = TranslateConstantValue(variableDeclaration.InitExpression, context);
                    //Pop constant value lexical scope
                    context.Pop();
                    break;
                case SlotDeclarationStyle.TypeAndInitExpression:    //bind to constant slot using its value and contract
                    //Create scope for the constant value
                    currentScope = context.Push<GenericScope>(scopeFactory);
                    value = TranslateConstantValue(variableDeclaration.InitExpression, context);
                    context.Pop();
                    //Create scope for the contract binding
                    currentScope = context.Push<GenericScope>(scopeFactory);
                    contractBinding = TranslateContractBinding(variableDeclaration.ContractBinding, context);
                    context.Pop();
                    break;
                default: return ScriptObject.MakeVoid();
            }
            return ScriptConstant.BindToConstant(constantRef, Debug ? variableDeclaration.Name : null, value, contractBinding, context.Scope.StateHolder);
        }

        #endregion

        #region Variable Binding

        private Expression TranslateContractBinding(ScriptCodeExpression contractBinding, TranslationContext context)
        {
            var result = Translate(contractBinding, context);
            switch (contractBinding is ScriptCodePrimitiveExpression)
            {
                case true: return ScriptContract.RequiresContract(result);
                default:
                    result = Expression.Block(
                        Expression.Label(context.Scope.BeginOfScope),
                        Expression.Label(context.Scope.EndOfScope, AsRightSide(result, context)));
                    result = ScriptContract.RequiresContract(result);
                    result = Expression.Lambda<Func<InterpreterState, IScriptContract>>(result, context.Scope.StateHolder);
                    return result;
            }
        }

        private Expression BindToVariable(Expression variableRef, ISlot variableDeclaration, TranslationContext context)
        {
            var value = default(Expression);
            var contractBinding = default(Expression);
            switch (variableDeclaration.Style)
            {
                case SlotDeclarationStyle.ContractBindingOnly:
                    //Binds to variable through contract.
                    contractBinding = TranslateContractBinding(variableDeclaration.ContractBinding, context);
                    break;
                case SlotDeclarationStyle.InitExpressionOnly:
                    //Binds to variable through value.
                    value = Translate(variableDeclaration.InitExpression, context);
                    break;
                case SlotDeclarationStyle.TypeAndInitExpression:
                    //Binds to variable through value and contract.
                    contractBinding = TranslateContractBinding(variableDeclaration.ContractBinding, context);
                    value = Translate(variableDeclaration.InitExpression, context);
                    break;
                default:
                    return ScriptObject.MakeVoid();
            }
            return ScriptVariable.BindToVariable(variableRef, Debug ? variableDeclaration.Name : null, value, contractBinding, context.Scope.StateHolder);
        }

        #endregion

        /// <summary>
        /// Translates 'this' object.
        /// </summary>
        /// <param name="thisExpression"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        protected override Expression Translate(ScriptCodeThisExpression thisExpression, TranslationContext context)
        {
            return context.Scope.ScopeVar;
        }

        /// <summary>
        /// Translates variable reference to LINQ-ET expression.
        /// </summary>
        /// <param name="variableRef">Variable reference expression.</param>
        /// <param name="context">Translation context.</param>
        /// <returns>Translated variable reference.</returns>
        protected override Expression Translate(ScriptCodeVariableReference variableRef, TranslationContext context)
        {
            var resolved = default(bool);
            return Translate(variableRef.VariableName, context, out resolved);
        }

        private static Expression GetBaseObject(LexicalScope scope)
        {
            if (scope == null || scope.IsTopLevel)
                return ScriptObject.MakeVoid();
            else if (scope.Parent is ObjectScope)
                return GetBaseObject(scope.Parent);
            else return scope.Parent.ScopeVar;
        }

        private static Expression GetBaseObject(TranslationContext context)
        {
            return GetBaseObject(context.Scope);   
        }

        /// <summary>
        /// Translates reference to the base scope.
        /// </summary>
        /// <param name="baseref"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        protected override Expression Translate(ScriptCodeBaseObjectExpression baseref, TranslationContext context)
        {
            return GetBaseObject(context);
        }

        private Expression Translate(string variableName, TranslationContext context, out bool resolved)
        {
            //The first, try find out a variable through a scope.
            Expression @var = context.Scope.GetVariableExpression(variableName);
            return (resolved = @var != null) ? @var : ScriptObject.RuntimeSlotBase.Lookup(variableName, context.Scope.StateHolder);
        }

        private Expression TranslateGrouping(ScriptCodeLoopExpression.OperatorGrouping grouping, TranslationContext context)
        {
            return BinaryOperatorInvoker.New(grouping.Operator);
        }

        private Expression TranslateGrouping(ScriptCodeLoopExpression.CustomGrouping grouping, TranslationContext context)
        {
            return Translate(grouping.GroupingAction, context);
        }

        private Expression TranslateGrouping(ScriptCodeLoopExpression.YieldGrouping grouping, TranslationContext context)
        {
            if (grouping is ScriptCodeLoopExpression.OperatorGrouping)
                return TranslateGrouping((ScriptCodeLoopExpression.OperatorGrouping)grouping, context);
            else if (grouping is ScriptCodeLoopExpression.CustomGrouping)
                return TranslateGrouping((ScriptCodeLoopExpression.CustomGrouping)grouping, context);
            else throw new NotSupportedException();
        }


        private BlockExpression TranslatePreWhile(IList<ScriptCodeStatement> loopBody, ScriptCodeExpression condition, ScriptCodeWhileLoopExpression.YieldGrouping grouping, TranslationContext context)
        {
            //Declares a variable that holds loop result.
            var accumulator = Expression.Variable(typeof(IScriptObject), "result");
            ICollection<Expression> expressions = new LinkedList<Expression>();
            var currentScope = context.Push(parent => new WhileLoopScope(parent, true));
            expressions.Add(Expression.Assign(accumulator, ScriptObject.MakeVoid())); //var result = ScriptObject.Void;
            //user-define loop body
            IList<Expression> implementation = new List<Expression>(loopBody.Count + 1);
            if (loopBody.Count > 1) implementation.Add(Expression.Assign(currentScope.Result, ScriptObject.MakeVoid()));   //optimization rules
            Translate(loopBody, context, GotoExpressionKind.Continue, expr => typeof(IScriptObject).IsAssignableFrom(expr.Type) ? Expression.Assign(currentScope.Result, AsRightSide(expr, context)):expr, ref implementation);
            //emit continuation flag if it is necessary
            if (currentScope.EmitContinueFlag)
                expressions.Add(Expression.Assign(currentScope.ContinueFlag, Expression.Constant(true)));   //continue = true;
            //emit continue label
            implementation.Add(Expression.Label(context.Scope.BeginOfScope));
            //result = ScriptObject.IsVoid(result) ? current: grouping.Invoke(new[] { result, current }, state)
            implementation.Add(Expression.Assign(accumulator, ScriptIterator.LoopHelpers.CombineResult(currentScope.Result, accumulator, TranslateGrouping(grouping, context), currentScope.StateHolder)));
            //while(condition)
            Expression cond = RuntimeHelpers.BindIsTrue(Translate(condition, context), currentScope.StateHolder);
            expressions.Add(Expression.Loop(Expression.IfThenElse(currentScope.EmitContinueFlag ? Expression.AndAlso(currentScope.ContinueFlag, cond) : cond, Expression.Block(LexicalScope.GetExpressions( currentScope.Locals.Values), implementation), Expression.Block(Expression.Assign(currentScope.Result, ScriptObject.Null), Expression.Goto(currentScope.EndOfScope))), currentScope.EndOfScope));
            //end of loop
            expressions.Add(accumulator);
            var loop = Expression.Block(currentScope.EmitContinueFlag ? new[] { accumulator, currentScope.Result, currentScope.ContinueFlag } : new[] { accumulator, currentScope.Result }, expressions);
            context.Pop();
            return loop;
        }

        private BlockExpression TranslatePostWhile(IList<ScriptCodeStatement> loopBody, ScriptCodeExpression condition, ScriptCodeWhileLoopExpression.YieldGrouping grouping, TranslationContext context)
        {
            //Declares a variable that holds loop result.
            var accumulator = Expression.Variable(typeof(IScriptObject), "result");
            ICollection<Expression> expressions = new LinkedList<Expression>();
            var currentScope = context.Push(parent => new WhileLoopScope(parent, true));
            expressions.Add(Expression.Assign(accumulator, ScriptObject.MakeVoid())); //var result = ScriptObject.Void;
            //user-define loop body
            IList<Expression> implementation = new List<Expression>(loopBody.Count + 1);
            if (loopBody.Count > 1) implementation.Add(Expression.Assign(currentScope.Result, ScriptObject.MakeVoid()));   //optimization rules
            Translate(loopBody, context, GotoExpressionKind.Continue, expr =>typeof(IScriptObject).IsAssignableFrom(expr.Type) ? Expression.Assign(currentScope.Result, AsRightSide(expr, context)):expr, ref implementation);
            //emit continuation flag if it is necessary
            if (currentScope.EmitContinueFlag)
                expressions.Add(Expression.Assign(currentScope.ContinueFlag, Expression.Constant(true)));   //continue = true;
            //emit continue label
            implementation.Add(Expression.Label(context.Scope.BeginOfScope));
            //result = ScriptObject.IsVoid(result) ? current: grouping.Invoke(new[] { result, current }, state)
            implementation.Add(Expression.Assign(accumulator, ScriptIterator.LoopHelpers.CombineResult(currentScope.Result, accumulator, TranslateGrouping(grouping, context), currentScope.StateHolder)));
            //while(condition)
            Expression cond = RuntimeHelpers.BindIsTrue(Translate(condition, context), currentScope.StateHolder);
            implementation.Add(Expression.IfThenElse(currentScope.EmitContinueFlag ? Expression.AndAlso(currentScope.ContinueFlag, cond) : cond, Expression.Empty(), Expression.Block(Expression.Assign(currentScope.Result, ScriptObject.Null), Expression.Goto(context.Scope.EndOfScope))));
            expressions.Add(Expression.Loop(Expression.Block(LexicalScope.GetExpressions( currentScope.Locals.Values), implementation), currentScope.EndOfScope));
            //end of loop
            expressions.Add(accumulator);
            var loop = Expression.Block(currentScope.EmitContinueFlag ? new[] { accumulator, currentScope.Result, currentScope.ContinueFlag } : new[] { accumulator, currentScope.Result }, expressions);
            context.Pop();
            return loop;
        }

        private BlockExpression TranslateWhile(IList<ScriptCodeStatement> loopBody, ScriptCodeExpression condition, ScriptCodeWhileLoopExpression.LoopStyle style, ScriptCodeWhileLoopExpression.YieldGrouping grouping, TranslationContext context)
        {
            return style == ScriptCodeWhileLoopExpression.LoopStyle.EvaluateConditionBeforeBody ? TranslatePreWhile(loopBody, condition, grouping, context) : TranslatePostWhile(loopBody, condition, grouping, context);
        }

        private BlockExpression TranslatePreWhile(IList<ScriptCodeStatement> loopBody, ScriptCodeExpression condition, TranslationContext context, bool suppressCollection)
        {
            ICollection<Expression> expressions = new LinkedList<Expression>();
            var currentScope = context.Push(parent => new WhileLoopScope(parent, false, suppressCollection));
            expressions.Add(Expression.Assign(currentScope.Result, suppressCollection ? LinqHelpers.Null<ScriptList>() : ScriptList.New())); //var result = new QList();
            //user-defined loop body
            IList<Expression> implementation = new List<Expression>(loopBody.Count + 1);
            Translate(loopBody, context, GotoExpressionKind.Continue, expr => typeof(IScriptObject).IsAssignableFrom(expr.Type) && !suppressCollection ? ScriptList.Add(currentScope.Result, expr, currentScope.StateHolder) : expr, ref implementation);
            //emit continuation flag if it is necessary
            if (currentScope.EmitContinueFlag)
                expressions.Add(Expression.Assign(currentScope.ContinueFlag, Expression.Constant(true)));   //continue = true;
            //while(condition)
            Expression cond = RuntimeHelpers.BindIsTrue(Translate(condition, context), currentScope.StateHolder);
            expressions.Add(Expression.Loop(Expression.IfThenElse(currentScope.EmitContinueFlag ? Expression.AndAlso(currentScope.ContinueFlag, cond) : cond, Expression.Block(LexicalScope.GetExpressions( currentScope.Locals.Values), implementation), Expression.Goto(currentScope.EndOfScope)), currentScope.EndOfScope, currentScope.BeginOfScope));
            //end of loop
            expressions.Add(currentScope.Result);
            var loop = Expression.Block(currentScope.EmitContinueFlag ? new[] { currentScope.Result, currentScope.ContinueFlag } : new[] { currentScope.Result }, expressions);
            context.Pop();
            return loop;
        }

        private BlockExpression TranslatePostWhile(IList<ScriptCodeStatement> loopBody, ScriptCodeExpression condition, TranslationContext context, bool suppressCollection)
        {
            ICollection<Expression> expressions = new LinkedList<Expression>();
            var currentScope = context.Push(parent => new WhileLoopScope(parent, false, suppressCollection));
            expressions.Add(Expression.Assign(currentScope.Result, suppressCollection?LinqHelpers.Null<ScriptList>(): ScriptList.New())); //var result = new QList();
            //user-define loop body
            IList<Expression> implementation = new List<Expression>(loopBody.Count + 1);
            Translate(loopBody, context, GotoExpressionKind.Continue, expr => typeof(IScriptObject).IsAssignableFrom(expr.Type) && !suppressCollection ? ScriptList.Add(currentScope.Result, expr, currentScope.StateHolder) : expr, ref implementation);
            //emit continuation flag if it is necessary
            if (currentScope.EmitContinueFlag)
                expressions.Add(Expression.Assign(currentScope.ContinueFlag, Expression.Constant(true)));   //continue = true;
            //emit continue label
            implementation.Add(Expression.Label(context.Scope.BeginOfScope));
            //while(condition)
            Expression cond = RuntimeHelpers.BindIsTrue(Translate(condition, context), currentScope.StateHolder);
            implementation.Add(Expression.IfThenElse(currentScope.EmitContinueFlag ? Expression.AndAlso(currentScope.ContinueFlag, cond) : cond, Expression.Empty(), Expression.Goto(context.Scope.EndOfScope)));
            expressions.Add(Expression.Loop(Expression.Block(LexicalScope.GetExpressions( currentScope.Locals.Values), implementation), currentScope.EndOfScope));
            //end of loop
            expressions.Add(currentScope.Result);
            var loop = Expression.Block(currentScope.EmitContinueFlag ? new[] { currentScope.Result, currentScope.ContinueFlag } : new[] { currentScope.Result }, expressions);
            context.Pop();
            return loop;
        }

        private BlockExpression TranslateWhile(IList<ScriptCodeStatement> loopBody, ScriptCodeExpression condition, ScriptCodeWhileLoopExpression.LoopStyle style, TranslationContext context, bool suppressCollection)
        {
            return style == ScriptCodeWhileLoopExpression.LoopStyle.EvaluateConditionBeforeBody ? TranslatePreWhile(loopBody, condition, context, suppressCollection) : TranslatePostWhile(loopBody, condition, context, suppressCollection);
        }

        /// <summary>
        /// Translates while loop to LINQ expression.
        /// </summary>
        /// <param name="whileLoop"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        protected override Expression Translate(ScriptCodeWhileLoopExpression whileLoop, TranslationContext context)
        {
            return whileLoop.Grouping != null ?
                TranslateWhile(whileLoop.Body.UnwrapStatements(), whileLoop.Condition, whileLoop.Style, whileLoop.Grouping, context) :
                TranslateWhile(whileLoop.Body.UnwrapStatements(), whileLoop.Condition, whileLoop.Style, context, whileLoop.SuppressResult);
        }

        /// <summary>
        /// Translated void expression.
        /// </summary>
        /// <param name="voidExpression">The expression to be translated.</param>
        /// <param name="context"></param>
        /// <returns></returns>
        protected override Expression Translate(ScriptCodeVoidExpression voidExpression, TranslationContext context)
        {
            return ScriptObject.MakeVoid();
        }

        private IList<Expression> Translate(ICollection<ScriptCodeTryElseFinallyExpression.FailureTrap> traps, ParameterExpression errorReceiver, TranslationContext context)
        {
            var result = new List<Expression>(traps.Count + 1);
            foreach (var t in traps)
            {
                var currentScope = context.Push(CatchScope.Create);    //begin of catch block
                IList<Expression> catchBlock = new List<Expression>(10);
                //declares catch variable
                var catchVar = default(ParameterExpression);
                if (t.Filter != null) context.Scope.DeclareVariable(t.Filter.Name, out catchVar);
                Translate(t.Handler.UnwrapStatements(), context, GotoExpressionKind.Goto, ref catchBlock);
                var filter = catchVar != null ? (Expression)ScriptFault.BindCatch(errorReceiver, catchVar, context.Scope.StateHolder) : Expression.Constant(true);
                result.Add(Expression.Block(LexicalScope.GetExpressions( currentScope.Locals.Values), catchVar != null ? BindToVariable(catchVar, t.Filter, context) : Expression.Empty(),
                    Expression.IfThen(filter, Expression.Block(catchBlock))));
                context.Pop();  //end of catch block
                if (t.Filter == null) break;
            }
            return result;
        }

        private CatchBlock Translate(ICollection<ScriptCodeTryElseFinallyExpression.FailureTrap> traps, TranslationContext context)
        {
            var receiver = Expression.Parameter(typeof(object));
            context.Push(parent => new GenericScope(parent, true));
            var blocks = Translate(traps,  receiver, context);
            blocks.Insert(0, Expression.Label(context.Scope.BeginOfScope));
            //if exception is not handled then rethrow
            blocks.Add(Expression.Throw(receiver));
            blocks.Add(Expression.Label(context.Scope.EndOfScope, ScriptObject.MakeVoid()));
            var @catch = Expression.Catch(receiver, Expression.Block(blocks));
            context.Pop();
            return @catch;
        }

        /// <summary>
        /// Translates structured exception handler.
        /// </summary>
        /// <param name="tryElseFinally"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        protected override Expression Translate(ScriptCodeTryElseFinallyExpression tryElseFinally, TranslationContext context)
        {
            //translates try block
            GenericScope currentScope = context.Push(TryScope.Create);
            IList<Expression> body = new List<Expression>(10) { Expression.Label(context.Scope.BeginOfScope) };
            Translate(tryElseFinally.DangerousCode.UnwrapStatements(), context, GotoExpressionKind.Goto, ref body);
            body.Add(Expression.Label(context.Scope.EndOfScope, ScriptObject.MakeVoid()));
            var @try = Expression.Block(LexicalScope.GetExpressions( currentScope.Locals.Values), body);
            context.Pop();
            //translates catch block
            var @catch = default(CatchBlock);
            switch (tryElseFinally.Traps.Count)
            {
                case 0:
                    @catch = tryElseFinally.Finally.IsVoidExpression ? Expression.Catch(typeof(object), ScriptObject.MakeVoid()) : null;
                    break;
                default:
                    @catch = Translate(tryElseFinally.Traps, context);
                    break;
            }
            var finallyBlock = tryElseFinally.Finally.UnwrapStatements();
            //translates finally block
            switch (finallyBlock.Count)
            {
                case 0: return Expression.TryCatch(@try, @catch);
                default:
                    currentScope = context.Push(FinallyScope.Create);
                    body = new List<Expression>(finallyBlock.Count + 1) { Expression.Label(context.Scope.BeginOfScope) };
                    Translate(finallyBlock, context, GotoExpressionKind.Goto, ref body);
                    body.Add(Expression.Label(context.Scope.EndOfScope, ScriptObject.MakeVoid()));
                    var @finally = Expression.Block(LexicalScope.GetExpressions(currentScope.Locals.Values), body);
                    context.Pop();
                    return @catch != null ? Expression.TryCatchFinally(@try, @finally, @catch) : Expression.TryFinally(@try, @finally);
            }
        }



        /// <summary>
        /// Translates string literal into LINQ-ET constant expression.
        /// </summary>
        /// <param name="stringLiteral">String literal to be translated.</param>
        /// <param name="context">Translation context.</param>
        /// <returns>LINQ-ET expression that represents DynamicScript string literal.</returns>
        protected override Expression Translate(ScriptCodeStringExpression stringLiteral, TranslationContext context)
        {
            return stringLiteral.IsInterned ?
                InterpreterState.FromInternPool<ScriptString>(context.Scope.StateHolder, Intern(context, stringLiteral.Value)) :
                ScriptString.New(stringLiteral.Value);
        }

        /// <summary>
        /// Translates string contract definition to LINQ expression.
        /// </summary>
        /// <param name="stringContract">The string contract definition to be translated.</param>
        /// <param name="context">Translation context.</param>
        /// <returns>LINQ expression that represents string contract definition.</returns>
        protected override Expression Translate(ScriptCodeStringContractExpression stringContract, TranslationContext context)
        {
            return ScriptStringContract.Expression;
        }

        /// <summary>
        /// Translates root contract definition.
        /// </summary>
        /// <param name="rootContract">The contract definition to be translated.</param>
        /// <param name="context">Translation context.</param>
        /// <returns>LINQ expression that represents contract definition.</returns>
        protected override Expression Translate(ScriptCodeSuperContractExpression rootContract, TranslationContext context)
        {
            return ScriptSuperContract.Expression;
        }

        /// <summary>
        /// Translates real literal into LINQ-ET constant expression.
        /// </summary>
        /// <param name="realLiteral">Real literal to be translated.</param>
        /// <param name="context">Translation context.</param>
        /// <returns>LINQ-ET expression that represents DynamicScript real literal.</returns>
        protected override Expression Translate(ScriptCodeRealExpression realLiteral, TranslationContext context)
        {
            return realLiteral.IsInterned ?
                InterpreterState.FromInternPool<ScriptReal>(context.Scope.StateHolder, Intern(context, realLiteral.Value)) :
                ConverterOf(realLiteral.Value);
        }

        /// <summary>
        /// Translates real contract definition to LINQ expression.
        /// </summary>
        /// <param name="realContract">The real contract definition to be translated.</param>
        /// <param name="context">Translation context.</param>
        /// <returns>LINQ expression that represents real contract definition.</returns>
        protected override Expression Translate(ScriptCodeRealContractExpression realContract, TranslationContext context)
        {
            return ScriptRealContract.Expression;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="globalref"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        protected override Expression Translate(ScriptCodeGlobalObjectExpression globalref, TranslationContext context)
        {
            return GlobalScope.GetGlobal(context.Scope);
        }

        /// <summary>
        /// Translates meta contract definition.
        /// </summary>
        /// <param name="metaContract">The contract definition to be translated.</param>
        /// <param name="context">Translation context.</param>
        /// <returns>LINQ expression that represents contract definition.</returns>
        protected override Expression Translate(ScriptCodeMetaContractExpression metaContract, TranslationContext context)
        {
            return ScriptMetaContract.Expression;
        }

        /// <summary>
        /// Translates invocation expression into the LINQ expression.
        /// </summary>
        /// <param name="invocation">Invocation expression to be translated.</param>
        /// <param name="context">Translation context.</param>
        /// <returns>Translated invocation expression.</returns>
        protected override Expression Translate(ScriptCodeInvocationExpression invocation, TranslationContext context)
        {
            var target = AsRightSide(Translate(invocation.Target, context), context);
            var args = new List<Expression>(from a in invocation.ArgList
                                            where a is ScriptCodeExpression
                                            select AsRightSide(Translate(a, context), context));
            return ScriptObject.BindInvoke(target, args, context.Scope.StateHolder);
        }

        #region .NET-to-DynamicScript Compile-time Conversion Routines

        private static Expression ConverterOf(long value)
        {
            return ScriptInteger.New(value);
        }

        private long Intern(TranslationContext context, long value)
        {
            var im = InterpreterState.GetInternMethodInfo<ScriptInteger>();
            var key = IntegerPool.MakeID(value);
            m_intern[key] = Expression.Call(context.Scope.StateHolder, im, ConverterOf(value));
            return key;
        }

        private static MemberExpression ConverterOf(bool value)
        {
            return ScriptBoolean.New(value);
        }

        private static Expression ConverterOf(double value)
        {
            return ScriptReal.New(value);
        }

        private long Intern(TranslationContext context, double value)
        {
            var im = InterpreterState.GetInternMethodInfo<ScriptReal>();
            var key = RealPool.MakeID(value);
            m_intern[key] = Expression.Call(context.Scope.StateHolder, im, ConverterOf(value));
            return key;
        }

        private static Expression ConverterOf(string value)
        {
            return ScriptString.New(value);
        }

        private long Intern(TranslationContext context, string value)
        {
            var im = InterpreterState.GetInternMethodInfo<ScriptString>();
            var key = StringPool.MakeID(value);
            m_intern[key] = Expression.Call(context.Scope.StateHolder, im, ConverterOf(value));
            return key;
        }

        #endregion

        /// <summary>
        /// Translates integer literal into LINQ-ET constant expression.
        /// </summary>
        /// <param name="integerLiteral">Integer literal to be translated.</param>
        /// <param name="context">Translation context.</param>
        /// <returns>LINQ-ET expression that represents DynamicScript integer literal.</returns>
        protected override Expression Translate(ScriptCodeIntegerExpression integerLiteral, TranslationContext context)
        {
            return integerLiteral.IsInterned ?
                InterpreterState.FromInternPool<ScriptInteger>(context.Scope.StateHolder, Intern(context, integerLiteral.Value)) :
                ConverterOf(integerLiteral.Value);
        }

        /// <summary>
        /// Translates integer contract definition to LINQ expression.
        /// </summary>
        /// <param name="integerContract">The integer contract definition to be translated.</param>
        /// <param name="context">Translation context.</param>
        /// <returns>LINQ expression that represents integer contract definition.</returns>
        protected override Expression Translate(ScriptCodeIntegerContractExpression integerContract, TranslationContext context)
        {
            return ScriptIntegerContract.Expression;
        }

        /// <summary>
        /// Translates indexer expression.
        /// </summary>
        /// <param name="indexer"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        protected override Expression Translate(ScriptCodeIndexerExpression indexer, TranslationContext context)
        {
            return ScriptObject.GetValue(Translate(indexer.Target, context), from expr in indexer.ArgList select Translate(expr, context), context.Scope.StateHolder);
        }

        private Expression TranslateFor(IList<ScriptCodeStatement> forBody, ScriptCodeForLoopExpression.LoopVariable variable, ScriptCodeExpression condition, ScriptCodeForLoopExpression.YieldGrouping grouping, TranslationContext context)
        {
            var loopVar = default(ParameterExpression);
            //Declares a variable that holds loop result.
            var accumulator = Expression.Variable(typeof(IScriptObject), "result");
            ICollection<Expression> expressions = new LinkedList<Expression>();
            var currentScope = context.Push(parent => new ForLoopScope(parent, true));
            switch (variable.Temporary)
            {
                case true:
                    currentScope.DeclareVariable(variable.Name, out loopVar);
                    break;
                default:
                    loopVar = currentScope.GetVariableExpression(variable.Name);
                    //loop variable cannot be resolved therefore abort translation
                    if (loopVar == null) { context.Pop(); return SlotNotFoundException.Bind(variable.Name, context.Scope.StateHolder); }
                    break;
            }
            expressions.Add(Expression.Assign(accumulator, ScriptObject.MakeVoid())); //var result = ScriptObject.Void;
            expressions.Add(BindToVariable(loopVar, variable, context));   //loopVar = initial;
            //user-define loop body
            IList<Expression> implementation = new List<Expression>(forBody.Count + 1);
            if (forBody.Count > 1) implementation.Add(Expression.Assign(currentScope.Result, ScriptObject.MakeVoid()));   //optimization rules
            Translate(forBody, context, GotoExpressionKind.Continue, expr => typeof(IScriptObject).IsAssignableFrom(expr.Type)? Expression.Assign(currentScope.Result, AsRightSide(expr, context)):expr, ref implementation);
            //emit continuation flag if it is necessary
            if (currentScope.EmitContinueFlag)
                expressions.Add(Expression.Assign(currentScope.ContinueFlag, Expression.Constant(true)));   //continue = true;
            //emit continue label
            implementation.Add(Expression.Label(context.Scope.BeginOfScope));
            //result = ScriptObject.IsVoid(result) ? current: grouping.Invoke(new[] { result, current }, state)
            implementation.Add(Expression.Assign(accumulator, ScriptIterator.LoopHelpers.CombineResult(currentScope.Result, accumulator, TranslateGrouping(grouping, context), currentScope.StateHolder)));
            //while(condition)
            Expression cond = RuntimeHelpers.BindIsTrue(Translate(condition, context), currentScope.StateHolder);
            expressions.Add(Expression.Loop(Expression.IfThenElse(currentScope.EmitContinueFlag ? Expression.AndAlso(currentScope.ContinueFlag, cond) : cond, Expression.Block(implementation), Expression.Block(Expression.Assign(currentScope.Result, ScriptObject.Null), Expression.Goto(context.Scope.EndOfScope))), context.Scope.EndOfScope));
            //end of loop
            expressions.Add(accumulator);
            var variables = new List<ParameterExpression>(LexicalScope.GetExpressions(currentScope.Locals.Values)) { accumulator, currentScope.Result };
            if (currentScope.EmitContinueFlag) variables.Add(currentScope.ContinueFlag);
            var loop = Expression.Block(variables, expressions);
            context.Pop();
            return loop;
        }

        private Expression TranslateFor(IList<ScriptCodeStatement> forBody, ScriptCodeForLoopExpression.LoopVariable variable, ScriptCodeExpression condition, TranslationContext context, bool suppressCollection)
        {
            var loopVar = default(ParameterExpression);
            ICollection<Expression> expressions = new LinkedList<Expression>();
            var currentScope = context.Push(parent => new ForLoopScope(parent, false, suppressCollection));
            switch (variable.Temporary)
            {
                case true:
                    currentScope.DeclareVariable(variable.Name, out loopVar);
                    break;
                default:
                    loopVar = currentScope.GetVariableExpression(variable.Name);
                    //loop variable cannot be resolved therefore abort translation
                    if (loopVar == null) { context.Pop(); return SlotNotFoundException.Bind(variable.Name, context.Scope.StateHolder); }
                    break;
            }
            expressions.Add(Expression.Assign(currentScope.Result, suppressCollection ? LinqHelpers.Null<ScriptList>() : ScriptList.New())); //var result = new QList();
            expressions.Add(BindToVariable(loopVar, variable, context));   //loopVar = initial;
            //user-define loop body
            IList<Expression> implementation = new List<Expression>(forBody.Count + 1);
            Translate(forBody, context, GotoExpressionKind.Continue, expr => typeof(IScriptObject).IsAssignableFrom(expr.Type) && !suppressCollection ? ScriptList.Add(currentScope.Result, expr, currentScope.StateHolder) : expr, ref implementation);
            //emit continuation flag if it is necessary
            if (currentScope.EmitContinueFlag)
                expressions.Add(Expression.Assign(currentScope.ContinueFlag, Expression.Constant(true)));   //continue = true;
            //while(condition)
            Expression cond = RuntimeHelpers.BindIsTrue(Translate(condition, context), currentScope.StateHolder);
            expressions.Add(Expression.Loop(Expression.IfThenElse(currentScope.EmitContinueFlag ? Expression.AndAlso(currentScope.ContinueFlag, cond) : cond, Expression.Block( implementation), Expression.Goto(context.Scope.EndOfScope)), context.Scope.EndOfScope, context.Scope.BeginOfScope));
            expressions.Add(currentScope.Result);
            var variables = new List<ParameterExpression>(LexicalScope.GetExpressions(currentScope.Locals.Values)) { currentScope.Result };
            if (currentScope.EmitContinueFlag) variables.Add(currentScope.ContinueFlag);
            var loop = Expression.Block(variables, expressions);
            context.Pop();
            return loop;
        }

        /// <summary>
        /// Translates for loop to LINQ expression.
        /// </summary>
        /// <param name="forLoop">The loop expression to be translated.</param>
        /// <param name="context">Translation context.</param>
        /// <returns>LINQ expression that represents for loop.</returns>
        protected override Expression Translate(ScriptCodeForLoopExpression forLoop, TranslationContext context)
        {
            return forLoop.Grouping != null ? 
                TranslateFor(forLoop.Body.UnwrapStatements(), forLoop.Variable, forLoop.Condition, forLoop.Grouping, context) : 
                TranslateFor(forLoop.Body.UnwrapStatements(), forLoop.Variable, forLoop.Condition, context, forLoop.SuppressResult);
        }

        private Expression TranslateForEach(IList<ScriptCodeStatement> forEachBody, ScriptCodeForEachLoopExpression.LoopVariable variable, ScriptCodeExpression iterator, ScriptCodeForEachLoopExpression.YieldGrouping grouping, TranslationContext context)
        {
            var loopVar = default(ParameterExpression);
            //Declares the variable that holds enumerator
            var enumerator = Expression.Variable(typeof(IScriptObject), "enumerator");
            //Declares the variable that holds loop result.
            var accumulator = Expression.Variable(typeof(IScriptObject), "result");
            ICollection<Expression> expressions = new LinkedList<Expression>();
            var currentScope = context.Push(parent => new ForLoopScope(parent, true));
            switch (variable.Temporary)
            {
                case true:
                    currentScope.DeclareVariable(variable.Name, out loopVar);
                    break;
                default:
                    loopVar = currentScope.GetVariableExpression(variable.Name);
                    //loop variable cannot be resolved therefore abort translation
                    if (loopVar == null) { context.Pop(); return SlotNotFoundException.Bind(variable.Name, context.Scope.StateHolder); }
                    break;
            }
            expressions.Add(Expression.Assign(accumulator, ScriptObject.MakeVoid())); //var result = ScriptObject.Void;
            expressions.Add(BindToVariable(loopVar, variable, context));   //loopVar = initial;
            //var enumerator = collection[ScriptObject.IteratorAction, state].Invoke(new IScriptObject[0], state);
            expressions.Add(Expression.Assign(enumerator, ScriptIterator.LoopHelpers.GetEnumerator(Translate(iterator, context), currentScope.StateHolder)));
            //user-define loop body
            IList<Expression> implementation = new List<Expression>(forEachBody.Count + 1);
            if (forEachBody.Count > 1) implementation.Add(Expression.Assign(currentScope.Result, ScriptObject.MakeVoid()));   //optimization rules
            implementation.Add(RuntimeSlot.SetValue(loopVar, ScriptIterator.LoopHelpers.GetNext(enumerator, currentScope.StateHolder), currentScope.StateHolder));
            Translate(forEachBody, context, GotoExpressionKind.Continue, expr => typeof(IScriptObject).IsAssignableFrom(expr.Type) ? Expression.Assign(currentScope.Result, AsRightSide(expr, context)) : expr, ref implementation);
            //emit continuation flag if it is necessary
            if (currentScope.EmitContinueFlag)
                expressions.Add(Expression.Assign(currentScope.ContinueFlag, Expression.Constant(true)));   //continue = true;
            //emit continue label
            implementation.Add(Expression.Label(context.Scope.BeginOfScope));
            //result = ScriptObject.IsVoid(result) ? current: grouping.Invoke(new[] { result, current }, state)
            implementation.Add(ScriptIterator.LoopHelpers.CombineResult(currentScope.Result, accumulator, TranslateGrouping(grouping, context), currentScope.StateHolder));
            //while(Has Next)
            Expression cond = ScriptIterator.LoopHelpers.HasNext(enumerator, currentScope.StateHolder);
            expressions.Add(Expression.Loop(Expression.IfThenElse(currentScope.EmitContinueFlag ? Expression.AndAlso(currentScope.ContinueFlag, cond) : cond, Expression.Block(implementation), Expression.Block(Expression.Assign(currentScope.Result, ScriptObject.Null), Expression.Goto(context.Scope.EndOfScope))), context.Scope.EndOfScope));
            //end of loop
            expressions.Add(accumulator);
            var variables = new List<ParameterExpression>(LexicalScope.GetExpressions( currentScope.Locals.Values)) { accumulator, currentScope.Result, enumerator };
            if (currentScope.EmitContinueFlag) variables.Add(currentScope.ContinueFlag);
            var loop = Expression.Block(variables, expressions);
            context.Pop();
            return loop;
        }

        private Expression TranslateForEach(IList<ScriptCodeStatement> forEachBody, ScriptCodeForEachLoopExpression.LoopVariable variable, ScriptCodeExpression iterator, TranslationContext context, bool suppressCollection)
        {
            var loopVar = default(ParameterExpression);
            //Declares the variable that holds enumerator
            var enumerator = Expression.Variable(typeof(IScriptObject), "enumerator");
            ICollection<Expression> expressions = new LinkedList<Expression>();
            var currentScope = context.Push(parent => new ForLoopScope(parent, false, suppressCollection));
            switch (variable.Temporary)
            {
                case true:
                    currentScope.DeclareVariable(variable.Name, out loopVar);
                    break;
                default:
                    loopVar = currentScope.GetVariableExpression(variable.Name);
                    //loop variable cannot be resolved therefore abort translation
                    if (loopVar == null) { context.Pop(); return SlotNotFoundException.Bind(variable.Name, context.Scope.StateHolder); }
                    break;
            }
            expressions.Add(Expression.Assign(currentScope.Result, suppressCollection ? LinqHelpers.Null<ScriptList>() : ScriptList.New())); //var result = new QList();
            expressions.Add(BindToVariable(loopVar, variable, context));   //loopVar = initial;
            //var enumerator = collection[ScriptObject.IteratorAction, state].Invoke(new IScriptObject[0], state);
            expressions.Add(Expression.Assign(enumerator, ScriptIterator.LoopHelpers.GetEnumerator(Translate(iterator, context), currentScope.StateHolder)));
            //user-define loop body
            IList<Expression> implementation = new List<Expression>(forEachBody.Count + 1);
            implementation.Add(RuntimeSlot.SetValue(loopVar, ScriptIterator.LoopHelpers.GetNext(enumerator, currentScope.StateHolder), currentScope.StateHolder));
            Translate(forEachBody, context, GotoExpressionKind.Continue, expr => typeof(IScriptObject).IsAssignableFrom(expr.Type) && !suppressCollection ? ScriptList.Add(currentScope.Result, expr, currentScope.StateHolder) : expr, ref implementation);
            //emit continuation flag if it is necessary
            if (currentScope.EmitContinueFlag)
                expressions.Add(Expression.Assign(currentScope.ContinueFlag, Expression.Constant(true)));   //continue = true;
            //while(Has next)
            Expression cond = ScriptIterator.LoopHelpers.HasNext(enumerator, currentScope.StateHolder);
            expressions.Add(Expression.Loop(Expression.IfThenElse(currentScope.EmitContinueFlag ? Expression.AndAlso(currentScope.ContinueFlag, cond) : cond, Expression.Block(implementation), Expression.Goto(context.Scope.EndOfScope)), context.Scope.EndOfScope, context.Scope.BeginOfScope));
            //end of loop
            expressions.Add(currentScope.Result);
            var variables = new List<ParameterExpression>(LexicalScope.GetExpressions(currentScope.Locals.Values)) { currentScope.Result, enumerator };
            if (currentScope.EmitContinueFlag) variables.Add(currentScope.ContinueFlag);
            var loop = Expression.Block(variables, expressions);
            context.Pop();
            return loop;
        }

        /// <summary>
        /// Translates for-each loop to LINQ expression.
        /// </summary>
        /// <param name="forEachLoop">The loop to be translated.</param>
        /// <param name="context">Translation context.</param>
        /// <returns>LINQ expression that represents for-each loop.</returns>
        protected override Expression Translate(ScriptCodeForEachLoopExpression forEachLoop, TranslationContext context)
        {
            return forEachLoop.Grouping != null ? 
                TranslateForEach(forEachLoop.Body.UnwrapStatements(), forEachLoop.Variable, forEachLoop.Iterator, forEachLoop.Grouping, context) : 
                TranslateForEach(forEachLoop.Body.UnwrapStatements(), forEachLoop.Variable, forEachLoop.Iterator, context, forEachLoop.SuppressResult);
        }

        /// <summary>
        /// Translates reference to the current action.
        /// </summary>
        /// <param name="currentAction">The expression to be translated.</param>
        /// <param name="context">Translation context.</param>
        /// <returns>LINQ expression that references the current action.</returns>
        protected override Expression Translate(ScriptCodeCurrentActionExpression currentAction, TranslationContext context)
        {
            var actionScope = context.Lookup<FunctionScope>();
            return actionScope != null ? actionScope.CurrentAction : ScriptObject.MakeVoid();
        }

        /// <summary>
        /// Translates context block.
        /// </summary>
        /// <param name="interpretationContext">The context block to be translated.</param>
        /// <param name="context">Translation context.</param>
        /// <returns>LINQ expression that represents context block.</returns>
        protected override Expression Translate(ScriptCodeContextExpression interpretationContext, TranslationContext context)
        {
            var currentScope = context.Push<ContextScope>(parent => ContextScope.Create(parent, interpretationContext.Context));
            var stateHolder = Expression.Assign(currentScope.StateHolder, InterpreterState.Update(currentScope.Parent.StateHolder, interpretationContext.Context));
            var result = default(Expression);
            switch (interpretationContext.Body.IsComplexExpression)
            {
                case true:
                    IList<Expression> body = new List<Expression>(10);
                    body.Label(currentScope.BeginOfScope, stateHolder);
                    Translate((ScriptCodeComplexExpression)interpretationContext.Body, context, GotoExpressionKind.Goto, ref body);
                    body.Label(currentScope.EndOfScope, ScriptObject.MakeVoid());
                    result = Expression.Block(LexicalScope.GetExpressions(currentScope.Locals.Values), body);
                    break;
                default:
                    result = Expression.Block(stateHolder, Translate(interpretationContext.Body, context));
                    break;
            }
            context.Pop();
            return result;
        }

        /// <summary>
        /// Parses condition expression.
        /// </summary>
        /// <param name="conditional"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        protected override Expression Translate(ScriptCodeConditionalExpression conditional, TranslationContext context)
        {
            //Parse test expression
            var test = RuntimeHelpers.BindIsTrue(Translate(conditional.Condition, context), context.Scope.StateHolder);
            return Expression.Condition(test, AsRightSide(Translate(conditional.ThenBranch, context), context), AsRightSide(Translate(conditional.ElseBranch, context), context), typeof(IScriptObject));
        }

        /// <summary>
        /// Translates callable contract definition to LINQ expression.
        /// </summary>
        /// <param name="callableContract">The callable contract definition to be translated.</param>
        /// <param name="context">Translation context.</param>
        /// <returns>LINQ expression that represents callable contract definition.</returns>
        protected override Expression Translate(ScriptCodeCallableContractExpression callableContract, TranslationContext context)
        {
            return ScriptCallableContract.Expression;
        }

        /// <summary>
        /// Translates boolean literal to LINQ expression.
        /// </summary>
        /// <param name="booleanLiteral">The literal to be translated.</param>
        /// <param name="context">Translation context.</param>
        /// <returns>LINQ expression that represents boolean literal.</returns>
        protected override Expression Translate(ScriptCodeBooleanExpression booleanLiteral, TranslationContext context)
        {
            return ConverterOf(booleanLiteral.Value);
        }

        /// <summary>
        /// Translates boolean contract definition to LINQ expression.
        /// </summary>
        /// <param name="booleanContract">The boolean contract definition to be translated.</param>
        /// <param name="context">Translation context.</param>
        /// <returns>LINQ expression that represents boolean contract definition.</returns>
        protected override Expression Translate(ScriptCodeBooleanContractExpression booleanContract, TranslationContext context)
        {
            return ScriptBooleanContract.Expression;
        }

        /// <summary>
        /// Translates argument reference.
        /// </summary>
        /// <param name="argref"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        protected override Expression Translate(ScriptCodeArgumentReferenceExpression argref, TranslationContext context)
        {
            IComplexExpressionScope<ScriptCodeActionImplementationExpression> actionScope = context.Lookup<FunctionScope>();
            switch (actionScope != null)
            {
                case true:
                    var parameters = actionScope.Expression.Signature.ParamList;
                    var resolved = default(bool);
                    var expression = argref.Index.Between(0, parameters.Count - 1) ? Translate(parameters[(int)argref.Index].Name, context, out resolved) : ScriptObject.MakeVoid();
                    return resolved ? expression : ScriptObject.MakeVoid();
                default: return ScriptObject.MakeVoid();
            }
        }

        /// <summary>
        /// Translates DIMENSIONAL contract.
        /// </summary>
        /// <param name="contract">The contract to translate.</param>
        /// <param name="context">Translation context.</param>
        /// <returns>Translated DIMENSIONAL contract.</returns>
        protected override Expression Translate(ScriptCodeDimensionalContractExpression contract, Translator<Expression, LexicalScope>.TranslationContext context)
        {
            return ScriptDimensionalContract.Expression;
        }

        /// <summary>
        /// Translates array expression.
        /// </summary>
        /// <param name="arrayExpression">The expression to be translated.</param>
        /// <param name="context">Translation context.</param>
        /// <returns>The translated array expression.</returns>
        protected override Expression Translate(ScriptCodeArrayExpression arrayExpression, TranslationContext context)
        {
            return ScriptArray.Bind(from ScriptCodeExpression expr in arrayExpression.Elements select AsRightSide(Translate(expr, context), context));
        }

        /// <summary>
        /// Translates array contract.
        /// </summary>
        /// <param name="arrayContract">The array contract to be translated.</param>
        /// <param name="context">Translation context.</param>
        /// <returns>The translated array contract.</returns>
        protected override Expression Translate(ScriptCodeArrayContractExpression arrayContract, TranslationContext context)
        {
            return ScriptArrayContract.New(Translate(arrayContract.ElementContract, context), arrayContract.Rank);
        }

        /// <summary>
        /// Translates action contract definition to LINQ expression.
        /// </summary>
        /// <param name="actionContract">Action contract definition to be translated.</param>
        /// <param name="context">Translation context.</param>
        /// <returns>LINQ expression that represents action contract.</returns>
        protected override Expression Translate(ScriptCodeActionContractExpression actionContract, TranslationContext context)
        {
            return ScriptFunctionContract.New(Expression.NewArrayInit(typeof(ScriptFunctionContract.Parameter), actionContract.ParamList.Select(p => ScriptFunctionContract.Parameter.New(p.Name, AsRightSide(Translate(p.ContractBinding, context), context)))),
                actionContract.NoReturnValue ? ScriptObject.MakeVoid() : AsRightSide(Translate(actionContract.ReturnType, context), context));
        }

        private void Translate(IList<ScriptCodeStatement> statements, TranslationContext context, GotoExpressionKind exitKind, Func<Expression, Expression> exitTransform, ref IList<Expression> output)
        {
            if (output == null) output = new List<Expression>(statements.Count > 0 ? statements.Count : 1);
            if (exitTransform == null) exitTransform = expr => typeof(IScriptObject).IsAssignableFrom(expr.Type) || RuntimeHelpers.IsRuntimeVariable(expr) ? Expression.MakeGoto(exitKind, context.Scope.EndOfScope, AsRightSide(expr, context), typeof(IScriptObject)) : expr;
            //Iterates through statements and emit
            switch (statements.Count)
            {
                case 0:
                    output.Add(exitTransform.Invoke(ScriptObject.MakeVoid()));
                    break;
                case 1:
                    var debugInfo = default(Expression);
                    //Interprets single expression as return value.
                    var instruction = Translate(statements[0], context, out debugInfo);
                    output.AddIf(Debug && debugInfo != null, debugInfo); //emit debug info
                    output.Add(exitTransform.Invoke(instruction));   //emit return stmt
                    break;
                default:    //build body consists of the more than one statement
                    foreach (var stmt in statements)
                    {
                        instruction = Translate(stmt, context, out debugInfo);
                        output.AddIf(Debug && debugInfo != null, debugInfo);
                        output.Add(instruction);
                    }
                    break;
            }
        }

        private void Translate(IList<ScriptCodeStatement> statements, TranslationContext context, GotoExpressionKind exitKind, ref IList<Expression> output)
        {
            Translate(statements, context, exitKind, null, ref output);
        }

        /// <summary>
        /// Translates action implementation to LINQ expression.
        /// </summary>
        /// <param name="action">The action implementation to be translated.</param>
        /// <param name="context">Translation context.</param>
        /// <returns>LINQ expression that represents action implementation.</returns>
        protected override Expression Translate(ScriptCodeActionImplementationExpression action, TranslationContext context)
        {
            var currentScope = context.Push(parent => FunctionScope.Create(parent, action)); //Create a new lexical scope for action
            //Declare all parameters
            var parameters = PopulateActionParameters(action, currentScope);
            //The first parameter of the lambda is invocation context.
            parameters.Insert(0, currentScope.StateHolder);
            //Construct body
            const int DefaultBodySize = 15;
            //The collection of the expressions that represents action implementation
            IList<Expression> body = new List<Expression>(DefaultBodySize);
            var actionExpr = default(LambdaExpression);
            if (action.IsPrimitive)//Action has a trivial body consists of the returning value.
                actionExpr = Expression.Lambda(AsRightSide(Translate(action.Body, context), context), false, parameters);
            else
            {
                body.Add(Expression.Label(currentScope.BeginOfScope));
                //Iterates through statements and emit
                Translate(action.Body.UnwrapStatements(), context, GotoExpressionKind.Return, ref body);
                body.Label(context.Scope.EndOfScope, ScriptObject.MakeVoid());   //marks end of the lexical scope.
                //Local variables translated to the block variables
                actionExpr = Expression.Lambda(Expression.Block(LexicalScope.GetExpressions(currentScope.Locals.Values), body), false, parameters);
            }
            context.Pop();  //Leave action scope
            return action.IsAsynchronous ?
                ScriptLazyFunction.New(
                Translate(action.Signature, context),   //action contract
                context.Scope.ScopeVar,           //'this' reference
                actionExpr,                     //lambda that implements the action
                action.ToString()) :
                ScriptRuntimeFunction.New(
                Translate(action.Signature, context),   //action contract
                context.Scope.ScopeVar,                //'this' reference
                actionExpr,                             //lambda that implements the action
                action.ToString());
        }

        private static IList<ParameterExpression> PopulateActionParameters(ScriptCodeActionImplementationExpression action, FunctionScope scope)
        {
            foreach (var p in action.Signature.ParamList)
            {
                var paramExpr = default(ParameterExpression);
                scope.DeclareParameter(p.Name, out paramExpr);
            }
            return new List<ParameterExpression>(LexicalScope.GetExpressions( scope.Parameters.Values));
        }

        private SwitchCase Translate(ScriptCodeSelectionExpression.SelectionCase @case, TranslationContext context)
        {
            IList<Expression> body = new List<Expression>(10);
            Translate(@case.Handler.UnwrapStatements(), context, GotoExpressionKind.Break, ref body);
            return Expression.SwitchCase(Expression.Block(body), from ScriptCodeExpression testValue in @case.Values select Translate(testValue, context));
        }

        /// <summary>
        /// Translates selection to LINQ expression.
        /// </summary>
        /// <param name="selection">The expression to be translated.</param>
        /// <param name="context">Translation context.</param>
        /// <returns></returns>
        protected override Expression Translate(ScriptCodeSelectionExpression selection, TranslationContext context)
        {
            var cases = from @case in selection.Cases select Translate(@case, context);
            IList<Expression> @default = new List<Expression>(10);
            Translate(selection.DefaultHandler.UnwrapStatements(), context, GotoExpressionKind.Break, ref @default);
            var result = Expression.Switch(typeof(IScriptObject), ScriptObjectComparer.Bind(AsRightSide(Translate(selection.Source, context), context), selection.Comparer != null ? AsRightSide(Translate(selection.Comparer, context), context) : null, context.Scope.StateHolder),
                Expression.Block(typeof(IScriptObject), @default),
                ScriptObjectComparer.EqualsMethod,
                cases);
            return result;
        }

        private IEnumerable<KeyValuePair<string, Expression>> ExtractSlots(ScriptCodeObjectExpression expression, TranslationContext context)
        {
            var slots = ObjectScope.CreateSlotSet();
            foreach (ScriptCodeObjectExpression.Slot s in expression)
                if (slots.Add(s.Name))
                    yield return new KeyValuePair<string, Expression>(s.Name, BindToVariable(LinqHelpers.Null<IStaticRuntimeSlot>(), s, context));
        }

        /// <summary>
        /// Translates object description expression.
        /// </summary>
        /// <param name="expression">The expression to be translated.</param>
        /// <param name="context">Translation context.</param>
        /// <returns>LINQ expression that represents object description.</returns>
        protected override Expression Translate(ScriptCodeObjectExpression expression, TranslationContext context)
        {
            var currentScope = context.Push<ObjectScope>((parent) => ObjectScope.Create(expression, parent));
            var initializer = Expression.Lambda(ScriptCompositeObject.Bind(ExtractSlots(expression, context), (ParameterExpression)currentScope.ScopeVar), currentScope.StateHolder);
            context.Pop();
            return Expression.Invoke(initializer, context.Scope.StateHolder);
        }

        private Expression DeleteValue(string variableName, TranslationContext context)
        {
            var resolved = default(bool);
            var slot = Translate(variableName, context, out resolved);
            return RuntimeHelpers.IsRuntimeVariable(slot) && resolved ?
                InterpreterState.DeleteValue(slot, context.Scope.StateHolder) :
                ScriptObject.MakeVoid();
        }

        private static Expression BinaryOperation(Expression lvalue, ScriptTypeCode ltype, ScriptCodeBinaryOperatorType @operator, Expression rvalue, ScriptTypeCode rtype, ParameterExpression state)
        {
            lvalue = ScriptObject.AsRightSide(lvalue, state);
            rvalue = ScriptObject.AsRightSide(rvalue, state);
            switch (ltype)  //attempts to inline operation if it is possible
            {
                case ScriptTypeCode.Integer:  //the left operand is integer
                    return ScriptInteger.Inline(lvalue, @operator, rvalue, rtype, state);
                default:    //failure to inline
                    return ScriptObject.BinaryOperation(lvalue, @operator, rvalue, state);
            }
        }

        /// <summary>
        /// Translates binary operator expression to LINQ expression.
        /// </summary>
        /// <param name="expression">The binary operator expression to be translated.</param>
        /// <param name="context">Translation context.</param>
        /// <returns>LINQ binary expression.</returns>
        protected override Expression Translate(ScriptCodeBinaryOperatorExpression expression, TranslationContext context)
        {
            /*
             * If binary expression has the following format:
             * a to void;
             * then recognize it as variable erasure.
            */
            if (expression.Left is ScriptCodeVariableReference && expression.Right is ScriptCodeVoidExpression) return DeleteValue(((ScriptCodeVariableReference)expression.Left).VariableName, context);
            else switch (expression.Operator)
                {
                    case ScriptCodeBinaryOperatorType.MemberAccess:
                        var leftExpression = default(Expression);
                        if (expression.Right is ScriptCodeVariableReference)
                            leftExpression = ScriptObject.GetValue(Translate(expression.Left, context), ((ScriptCodeVariableReference)expression.Right).VariableName, context.Scope.StateHolder);
                        else if (expression.Right is ScriptCodeIntegerExpression)
                            leftExpression = ScriptObject.GetValue(Translate(expression.Left, context), new[] { ConverterOf(((ScriptCodeIntegerExpression)expression.Right).Value) }, context.Scope.StateHolder);
                        else leftExpression = ScriptObject.RtlGetValue(Translate(expression.Left, context), Translate(expression.Right, context), context.Scope.StateHolder);
                        return leftExpression;
                    case ScriptCodeBinaryOperatorType.AndAlso:
                        leftExpression = AsRightSide(Translate(expression.Left, context), context);
                        return Expression.Condition(RuntimeHelpers.BindIsTrue(leftExpression, context.Scope.StateHolder), //test
                            AsRightSide(Translate(expression.Right, context), context), //then
                            ConverterOf(false), //else
                            typeof(IScriptObject));
                    case ScriptCodeBinaryOperatorType.OrElse:
                        leftExpression = AsRightSide(Translate(expression.Left, context), context);
                        return Expression.Condition(RuntimeHelpers.BindIsTrue(leftExpression, context.Scope.StateHolder), //test
                            ConverterOf(true),  //then
                            AsRightSide(Translate(expression.Right, context), context), //else
                            typeof(IScriptObject));
                    case ScriptCodeBinaryOperatorType.Initializer:
                        leftExpression = Translate(expression.Left, context);
                        return ScriptObject.RuntimeSlotBase.Initialize(leftExpression, Translate(expression.Right, context), context.Scope.StateHolder);
                case ScriptCodeBinaryOperatorType.MetadataDiscovery:
                        if (expression.Right is ScriptCodeVariableReference)
                            leftExpression = ScriptObject.BinaryOperation(Translate(expression.Left, context), ScriptCodeBinaryOperatorType.MetadataDiscovery, ConverterOf(((ScriptCodeVariableReference)expression.Right).VariableName), context.Scope.StateHolder);
                        else leftExpression = ScriptObject.BinaryOperation(Translate(expression.Left, context), ScriptCodeBinaryOperatorType.MetadataDiscovery, Translate(expression.Right, context), context.Scope.StateHolder);
                        return leftExpression;
                case ScriptCodeBinaryOperatorType.ReferenceEquality:
                        leftExpression = Translate(expression.Left, context);
                        leftExpression = AsRightSide(leftExpression, context);
                        return Expression.ReferenceEqual(leftExpression, AsRightSide(Translate(expression.Right, context), context));
                case ScriptCodeBinaryOperatorType.ReferenceInequality:
                     leftExpression = Translate(expression.Left, context);
                        leftExpression = AsRightSide(leftExpression, context);
                        return Expression.ReferenceNotEqual(leftExpression, AsRightSide(Translate(expression.Right, context), context));
                    default:
                        var ltype = ScriptTypeCode.Unknown;
                        var rtype = ScriptTypeCode.Unknown;
                        return BinaryOperation(Translate(expression.Left, context, out ltype), ltype, expression.Operator, Translate(expression.Right, context, out rtype), rtype, context.Scope.StateHolder);
                }
        }

        private Expression CreateCompositeContract(ScriptCodeObjectExpression expr, TranslationContext context)
        {
            return expr.Count > 0 ? ScriptCompositeContract.New(from ScriptCodeObjectExpression.Slot s in expr
                                                                select new KeyValuePair<string, Expression>(s.Name, AsRightSide(Translate(s.ContractBinding, context), context))) :
                                               (Expression)ScriptCompositeContract.EmptyField;
        }

        /// <summary>
        /// Translates unary operation to LINQ expression.
        /// </summary>
        /// <param name="expression">The unary operation to be translated.</param>
        /// <param name="context">Translation context.</param>
        /// <returns>LINQ expression that represents unary operation.</returns>
        protected override Expression Translate(ScriptCodeUnaryOperatorExpression expression, TranslationContext context)
        {
            if (expression.Operator == ScriptCodeUnaryOperatorType.TypeOf && expression.Operand is ScriptCodeObjectExpression)
                return CreateCompositeContract((ScriptCodeObjectExpression)expression.Operand, context);
            else return ScriptObject.UnaryOperation(Translate(expression.Operand, context), expression.Operator, context.Scope.StateHolder);
        }

        /// <summary>
        /// Translates FINSET contract.
        /// </summary>
        /// <param name="contract">The contract to translate.</param>
        /// <param name="context">Translation context.</param>
        /// <returns>Translated FINSET contract.</returns>
        protected override Expression Translate(ScriptCodeFinSetContractExpression contract, TranslationContext context)
        {
            return ScriptFinSetContract.Expression;
        }

        /// <summary>
        /// Translates code analysis error into the LINQ-ET expression.
        /// </summary>
        /// <param name="error">An exception to be translated.</param>
        /// <returns>Translated LINQ-ET expression that throws the input exception.</returns>
        protected override Expression Translate(CodeAnalysisException error)
        {
            return Expression.Throw(error.Restore());
        }

        /// <summary>
        /// Translates a set of LINQ expressions into the single block of expressions.
        /// </summary>
        /// <param name="instructions">A set of expressions to compose.</param>
        /// <param name="context">Translation context.</param>
        /// <returns>Block of expressions that encapsulates instruction set.</returns>
        protected override Expression Translate(IList<Expression> instructions, TranslationContext context)
        {
            return Translate(instructions, context.Lookup<GlobalScope>(), Debug, m_intern);
        }

        /// <summary>
        /// Translates all statements starting with the current iterator position into the executable expression.
        /// </summary>
        /// <returns>Executable expression.</returns>
        public Expression<ScriptInvoker> Translate()
        {
            return (Expression<ScriptInvoker>)base.Translate(Debug);
        }

        private int InternPoolSize
        {
            get { return m_intern.Count; }
        }

        private static Expression<ScriptInvoker> Translate(IList<Expression> instructions, GlobalScope scope, bool debug, IDictionary<long, MethodCallExpression> internPool)
        {
            //Stores begin of scope.
            instructions.Insert(0, Expression.Label(scope.BeginOfScope));
            //Insert intern calls
            foreach (var intern in internPool.Values)
                instructions.Insert(0, intern);
            //Constructs returning object from module.
            switch (scope.Locals.Count)
            {
                case 0:
                    instructions.Label(scope.EndOfScope, ScriptObject.MakeVoid());
                    break;
                default:
                    instructions.Label(scope.EndOfScope, Expression.Block(ScriptCompositeObject.Bind(scope.Locals.Select(l => new KeyValuePair<string, ParameterExpression>(l.Key, l.Value.Expression)))));
                    break;
            }
            //Combines all 
            return Expression.Lambda<ScriptInvoker>(
                Expression.Block(LexicalScope.GetExpressions(scope.Locals.Values), instructions),
                scope.StateHolder);
        }

        private static Expression<ScriptInvoker> Translate(IEnumerator<ScriptCodeStatement> parser, ErrorMode errMode, SourceCodeInfo source, out int capacity)
        {
            using (var translator = new LinqExpressionTranslator(parser, errMode, source))
            {
                capacity = translator.InternPoolSize;
                return translator.Translate();
            }
        }

        private static Expression<ScriptInvoker> Translate(LexemeAnalyzer lexer, ErrorMode errMode, SourceCodeInfo source, out int capacity)
        {
            using (var parser = new SyntaxAnalyzer(lexer, source != null ? source.FileName : null))
                return Translate(parser, errMode, source, out capacity);
        }

        internal static Expression<ScriptInvoker> Inject(LexemeAnalyzer lexer)
        {
            var internPoolSize = 0;
            return Translate(lexer, ErrorMode.Panic, null, out internPoolSize);
        }

        internal static Expression<ScriptInvoker> Translate(IEnumerator<char> scriptCode, ErrorMode errMode, SourceCodeInfo source, out int capacity)
        {
            using (var lexer = new LexemeAnalyzer(scriptCode))
                return Translate(lexer, errMode, source, out capacity);
        }

        internal static Expression<ScriptInvoker> Inject(IEnumerable<ScriptCodeStatement> statements)
        {
            var internPoolSize = 0;
            using (var parser = statements.GetEnumerator())
                return Translate(parser, ErrorMode.Panic, null, out internPoolSize);
        }

        /// <summary>
        /// Translates code analysis error to the script body implementation.
        /// </summary>
        /// <param name="e">The error to be translated.</param>
        /// <returns>Translated code analysis error.</returns>
        public static Expression<ScriptInvoker> TranslateError(CodeAnalysisException e)
        {
            var input = Expression.Parameter(typeof(ScriptObject));
            return Expression.Lambda<ScriptInvoker>(Expression.Block(e.Restore(), input), input);
        }

        /// <summary>
        /// Obtains type of the variable.
        /// </summary>
        /// <param name="variableName"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        protected override ScriptTypeCode GetType(string variableName, TranslationContext context)
        {
            return context.Scope.GetType(variableName);
        }
    }
}
