using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.CodeDom;

namespace DynamicScript.Compiler.Ast.Translation
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;
    using ExpandoObject = System.Dynamic.ExpandoObject;

    /// <summary>
    /// Represents an abstract class for building code document analyzer,
    /// such as semantic analyzer or compiler.
    /// </summary>
    /// <typeparam name="TScope">Type of the top-level lexical scope.</typeparam>
    /// <typeparam name="TResult">Type of the analysis result.</typeparam>
    [ComVisible(false)]
    
    public abstract class Translator<TResult, TScope>: IEnumerator<TResult>
        where TResult: class
        where TScope: class, ILexicalScope
    {
        #region Nested Types

        /// <summary>
        /// Represents an event arguments occured when program analysis enters or exits
        /// to/from lexical scope. This class cannot be inherited.
        /// </summary>
        [ComVisible(false)]
        protected sealed class LexicalScopeChangedEventArgs : EventArgs
        {
            private readonly TScope m_scope;

            internal LexicalScopeChangedEventArgs(TScope scope)
            {
                m_scope = scope;
            }

            /// <summary>
            /// Gets lexical lexical scope.
            /// </summary>
            public TScope Scope
            {
                get { return m_scope; }
            }
        }

        /// <summary>
        /// Represents translation context.
        /// This class cannot be inherited.
        /// </summary>
        [ComVisible(false)]
        protected sealed class TranslationContext
        {
            private TScope m_scope;
            private ScriptDebugInfo m_dbgInfo;

            /// <summary>
            /// Represents user data stored in the translation context.
            /// </summary>
            public readonly dynamic UserData;

            private TranslationContext()
            {
                UserData = new ExpandoObject();
            }

            /// <summary>
            /// Initializes a new translation context.
            /// </summary>
            /// <param name="root">The root lexical scope. Cannot be <see langword="null"/>.</param>
            /// <exception cref="System.ArgumentNullException"><paramref name="root"/> is <see langword="null"/>.</exception>
            internal TranslationContext(TScope root)
                :this()
            {
                Reset(root);
            }

            /// <summary>
            /// Gets debug information associated with this translation context.
            /// </summary>
            public ScriptDebugInfo DebugInfo
            {
                get { return m_dbgInfo; }
                internal set { m_dbgInfo = value; }
            }

            /// <summary>
            /// Initializes a new translation context.
            /// </summary>
            /// <param name="rootCreator">The delegate that implements root lexical scope creation.</param>
            internal TranslationContext(Func<dynamic, TScope> rootCreator)
                : this()
            {
                Reset(rootCreator.Invoke(UserData));
            }

            /// <summary>
            /// Occurs when translation context enters into lexical scope.
            /// </summary>
            public event EventHandler<LexicalScopeChangedEventArgs> ScopeEnter;

            private void OnScopeEnter(LexicalScopeChangedEventArgs e)
            {
                if (ScopeEnter != null)
                    ScopeEnter(this, e);
            }

            private void OnScopeEnter(TScope scope)
            {
                OnScopeEnter(new LexicalScopeChangedEventArgs(scope));
            }

            /// <summary>
            /// Occurs when translation context leaves lexical scope.
            /// </summary>
            public event EventHandler<LexicalScopeChangedEventArgs> ScopeLeave;

            private void OnScopeLeave(LexicalScopeChangedEventArgs e)
            {
                if (ScopeLeave != null)
                    ScopeLeave(this, e);
            }

            private void OnScopeLeave(TScope scope)
            {
                OnScopeLeave(new LexicalScopeChangedEventArgs(scope));
            }

            /// <summary>
            /// Finds the one of the specified lexical scopes.
            /// </summary>
            /// <typeparam name="G1"></typeparam>
            /// <typeparam name="G2"></typeparam>
            /// <typeparam name="G3"></typeparam>
            /// <returns></returns>
            public TScope Lookup<G1, G2, G3>()
                where G1 : class, TScope
                where G2 : class, TScope
                where G3 : class, TScope
            {
                var current = Scope;
                var transparent = true;
                while (current != null && transparent)
                    switch (current is G1 || current is G2 || current is G3)
                    {
                        case true: return current;
                        default:
                            transparent = current.Transparent;
                            current = (TScope)current.Parent;
                            continue;
                    }
                return null;
            }

            /// <summary>
            /// Finds the one of the specified lexical scopes.
            /// </summary>
            /// <typeparam name="G1"></typeparam>
            /// <typeparam name="G2"></typeparam>
            /// <returns></returns>
            public TScope Lookup<G1, G2>()
                where G1 : class, TScope
                where G2 : class, TScope
            {
                return Lookup<G1, G2, G2>();
            }

            /// <summary>
            /// Finds the lexical scope by its type.
            /// </summary>
            /// <typeparam name="G">Type of the target lexical scope.</typeparam>
            /// <returns>Search result.</returns>
            public G Lookup<G>()
                where G : class, TScope
            {
                return (G)Lookup<G, G>();
            }

            /// <summary>
            /// Determines whether the translation is occured in the specified lexical scope.
            /// </summary>
            /// <typeparam name="G">Type of the lexical scope.</typeparam>
            /// <returns><see langword="true"/> if the translation is occured in the specified lexical scope; otherwise, <see langword="false"/>.</returns>
            public bool IsIn<G>()
                where G : class, TScope
            {
                return Lookup<G>() != null;
            }

            /// <summary>
            /// Gets current lexical scope.
            /// </summary>
            public TScope Scope
            {
                get { return m_scope; }
                private set { m_scope = value != null ? value : m_scope; }
            }

            /// <summary>
            /// Pushes a new lexical scope.
            /// </summary>
            /// <typeparam name="G">Type of the pushed scope.</typeparam>
            /// <param name="scopeCreator">The delegate that implements lexical scope creation.Cannot be <see langword="null"/>.</param>
            /// <exception cref="System.ArgumentNullException"><paramref name="scopeCreator"/> is <see langword="null"/>.</exception>
            /// <returns>A newly created scope.</returns>
            /// <remarks>
            /// This method is used to open nested lexical scope.
            /// </remarks>
            public G Push<G>(Func<TScope, G> scopeCreator)
                where G : class, TScope
            {
                if (scopeCreator == null) throw new ArgumentNullException("scopeCreator");
                var newScope = scopeCreator(Scope);
                OnScopeEnter(Scope = newScope);
                return newScope;
            }

            /// <summary>
            /// Pops lexical scope.
            /// </summary>
            /// <returns>An instance of the lexical scope located at the top of the scope stack.</returns>
            /// <remarks>This method is used to close lexical scope.</remarks>
            public TScope Pop()
            {
                switch (Scope.Parent != null)
                {
                    case true:
                        OnScopeLeave(Scope);
                        return Scope = (TScope)Scope.Parent;
                    default:
                        return null;
                }
            }

            internal void Reset(TScope root)
            {
                if (root == null) throw new ArgumentNullException("root");
                m_scope = root;
                ScopeEnter = null;
                ScopeLeave = null;
            }
        }
        #endregion
        private readonly IEnumerator<ScriptCodeStatement> m_statements;
        private bool m_disposed;
        private TResult m_current;
        private readonly ErrorMode m_mode;
        private readonly TranslationContext Context;
        private readonly string m_sourceFile;

        private Translator()
        {
            m_disposed = false;
            Context = new TranslationContext(CreateRootScope);
        }

        /// <summary>
        /// Initializes a new analyzer for the specified set of statements. 
        /// </summary>
        /// <param name="parser">An enumerator through parsed statements. Cannot be <see langword="null"/>.</param>
        /// <param name="errMode">Type of the strategy that should be applied to the exception situation during analyzing.</param>
        /// <param name="sourceFile">Path to the translated source file.</param>
        /// <exception cref="System.ArgumentNullException"><paramref name="parser"/> is <see langword="null"/>.</exception>
        protected Translator(IEnumerator<ScriptCodeStatement> parser, ErrorMode errMode, string sourceFile = null)
            : this()
        {
            if (parser == null) throw new ArgumentNullException("parser");
            m_statements = parser;
            m_mode = errMode;
            Reset(false);
            m_sourceFile = sourceFile;
        }

        private void LeaveScope(object sender, LexicalScopeChangedEventArgs e)
        {
            LeaveScope(e.Scope);
        }

        private void EnterScope(object sender, LexicalScopeChangedEventArgs e)
        {
            EnterScope(e.Scope);
        }

        /// <summary>
        /// Exits from the lexical scope.
        /// </summary>
        /// <param name="scope">The scope to leave.</param>
        /// <remarks>This method is called automatically.</remarks>
        protected virtual void LeaveScope(TScope scope)
        {
        }

        /// <summary>
        /// Enters to the lexical scope.
        /// </summary>
        /// <param name="scope">The scope to enter.</param>
        /// <remarks>This method is called automatically.</remarks>
        protected virtual void EnterScope(TScope scope)
        {
        }

        private string SourceFile
        {
            get { return m_sourceFile; }
        }

        /// <summary>
        /// Initializes a new root scope.
        /// </summary>
        /// <param name="userData">A dynamic object associated with the translation context.</param>
        /// <returns>A new root scope.</returns>
        protected abstract TScope CreateRootScope(dynamic userData);

        private string ObjectName
        {
            get { return GetType().Name; }
        }

#if DEBUG
        [DebuggerNonUserCode]
        [DebuggerHidden]
#endif
        private void VerifyOnDisposed()
        {
            if (m_disposed) throw new ObjectDisposedException(ObjectName);
        }

        /// <summary>
        /// Gets transformed code expression.
        /// </summary>
        /// <exception cref="System.ObjectDisposedException">Analyzer is disposed.</exception>
        public TResult Current
        {
            get 
            {
                VerifyOnDisposed();
                return m_current;
            }
        }

        object System.Collections.IEnumerator.Current
        {
            get { return Current; }
        }

        /// <summary>
        /// Creates debug information.
        /// </summary>
        /// <param name="info">Translation-neutral debug information.</param>
        /// <returns>Translated debug information.</returns>
        protected virtual TResult Translate(ScriptDebugInfo info)
        {
            return null;
        }

        private TResult EmitDebugInfo(ScriptDebugInfo info)
        {
            switch (info != null)
            {
                case true:
                    info.FileName = SourceFile;
                    Context.DebugInfo = info;
                    return Translate(info);
                default: return null;
            }
        }

        /// <summary>
        /// Translates the next statement.
        /// </summary>
        /// <param name="debugInfo">Debug information emitted by translator.</param>
        /// <returns><see langword="true"/> if statement is translated successfully; otherwise, <see langword="false"/>.</returns>
        public bool MoveNext(out TResult debugInfo)
        {
            debugInfo = null;
            try
            {
                switch (m_statements.MoveNext())
                {
                    case true:
                        m_current = Translate(m_statements.Current, Context, out debugInfo);
                        return m_current != null ? true : MoveNext(out debugInfo);
                    default:
                        debugInfo = null;
                        return false;
                }
            }
            catch (CodeAnalysisException error)
            {
                switch (m_mode)
                {
                    case ErrorMode.Panic:
                        throw error;
                    case ErrorMode.Tolerant:
                        debugInfo = EmitDebugInfo(new ScriptDebugInfo { Start = error.Position });
                        m_current = Translate(error);
                        return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Analizes the next code statement.
        /// </summary>
        /// <returns><see langword="true"/> if the end of the code document is reached; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="System.ObjectDisposedException">Analyzer is disposed.</exception>
        public bool MoveNext()
        {
            VerifyOnDisposed();
            var dinfo = default(TResult);
            return MoveNext(out dinfo);
        }

        #region Statement Translators

        /// <summary>
        /// Translates statemenet.
        /// </summary>
        /// <param name="stmt">The statement to be translated.</param>
        /// <param name="context">Translation context.</param>
        /// <param name="debugInfo">Debug information about statement.</param>
        /// <returns>Translated statement.</returns>
        protected TResult Translate(ScriptCodeStatement stmt, TranslationContext context, out TResult debugInfo)
        {
            debugInfo = null;
            if (stmt is ScriptCodeVariableDeclaration)
                return Translate((ScriptCodeVariableDeclaration)stmt, context, out debugInfo);
            else if (stmt is ScriptCodeExpressionStatement)
                return Translate((ScriptCodeExpressionStatement)stmt, context, out debugInfo);
            else if (stmt is ScriptCodeFaultStatement)
                return Translate((ScriptCodeFaultStatement)stmt, context, out debugInfo);
            else if (stmt is ScriptCodeBreakLexicalScopeStatement)
                return Translate((ScriptCodeBreakLexicalScopeStatement)stmt, context, out debugInfo);
            else if (stmt is ScriptCodeContinueStatement)
                return Translate((ScriptCodeContinueStatement)stmt, context, out debugInfo);
            else if (stmt is ScriptCodeReturnStatement)
                return Translate((ScriptCodeReturnStatement)stmt, context, out debugInfo);
            else if (stmt is ScriptCodeEmptyStatement)
                return Translate((ScriptCodeEmptyStatement)stmt, context, out debugInfo);
            else if (stmt is ScriptCodeCommentStatement)
                return Translate((ScriptCodeCommentStatement)stmt, context, out debugInfo);
            else if (stmt is ScriptCodeMacroCommand)
                return Translate((ScriptCodeMacroCommand)stmt, context, out debugInfo);
            else return null;
        }

        private TResult Translate(ScriptCodeMacroCommand command, TranslationContext context, out bool debugInfo)
        {
            debugInfo = false;
            Macro(command.Command, context);
            return null;
        }

        /// <summary>
        /// Executes macro command.
        /// </summary>
        /// <param name="command"></param>
        /// <param name="context">The current state of the translator.</param>
        protected virtual void Macro(string command, TranslationContext context)
        {
        }

        /// <summary>
        /// Translates comment statement.
        /// </summary>
        /// <param name="comment">The comment to be translated.</param>
        /// <param name="context">Translation context.</param>
        /// <param name="debugInfo">Debug information about statement.</param>
        /// <returns>Translated comment statement.</returns>
        protected TResult Translate(ScriptCodeCommentStatement comment, TranslationContext context, out TResult debugInfo)
        {
            comment.Verify();
            debugInfo = EmitDebugInfo(comment.LinePragma);
            return Translate(comment, Context);
        }

        /// <summary>
        /// Translates comment statement.
        /// </summary>
        /// <param name="comment">The comment to be translated.</param>
        /// <param name="context">Translation context.</param>
        /// <returns>Translated comment statement.</returns>
        /// <remarks>
        /// In the default implementation this method doesn't provide translation and returns <see langword="null"/>.
        /// Comment translation is useful in the debug mode and you can emit comment text into the translated instructions.
        /// </remarks>
        protected virtual TResult Translate(ScriptCodeCommentStatement comment, TranslationContext context)
        {
            return null;
        }

        /// <summary>
        /// Translates empty statement.
        /// </summary>
        /// <param name="emptyStatement">The statement to be translated.</param>
        /// <param name="context">Translation context.</param>
        /// <param name="debugInfo">Debug information about statement.</param>
        /// <returns>Translated statement.</returns>
        protected TResult Translate(ScriptCodeEmptyStatement emptyStatement, TranslationContext context, out TResult debugInfo)
        {
            emptyStatement.Verify();
            debugInfo = EmitDebugInfo(emptyStatement.LinePragma);
            return Translate(emptyStatement, context);
        }

        /// <summary>
        /// Translates empty statement.
        /// </summary>
        /// <param name="emptyStatement">The statement to be translated.</param>
        /// <param name="context">Translation context.</param>
        /// <returns>Translated statement.</returns>
        /// <remarks>In the default implementation this method doesn't provide translation and returns <see langword="null"/>.</remarks>
        protected virtual TResult Translate(ScriptCodeEmptyStatement emptyStatement, TranslationContext context)
        {
            return null;
        }

        /// <summary>
        /// Translates return statement.
        /// </summary>
        /// <param name="returnStatement">The return statement to be translated.</param>
        /// <param name="context">Translation context.</param>
        /// <param name="debugInfo">Debug information about statement.</param>
        /// <returns>Translated return statement.</returns>
        protected TResult Translate(ScriptCodeReturnStatement returnStatement, TranslationContext context, out TResult debugInfo)
        {
            returnStatement.Verify();
            debugInfo = Translate(returnStatement.LinePragma);
            return Translate(returnStatement, context);
        }

        /// <summary>
        /// Translates return statement.
        /// </summary>
        /// <param name="returnStatement">The return statement to be translated.</param>
        /// <param name="context">Translation context.</param>
        /// <returns>Translated return statement.</returns>
        protected abstract TResult Translate(ScriptCodeReturnStatement returnStatement, TranslationContext context);

        /// <summary>
        /// Translates continue statement.
        /// </summary>
        /// <param name="continueStatement">The statement to be translated.</param>
        /// <param name="context">Translation context.</param>
        /// <param name="debugInfo">Debug information about statement.</param>
        /// <returns>Translated continue statement.</returns>
        protected TResult Translate(ScriptCodeContinueStatement continueStatement, TranslationContext context, out TResult debugInfo)
        {
            continueStatement.Verify();
            debugInfo = EmitDebugInfo(continueStatement.LinePragma);
            return Translate(continueStatement, context);
        }

        /// <summary>
        /// Translates continue statement.
        /// </summary>
        /// <param name="continueStatement">The statement to be translated.</param>
        /// <param name="context">Translation context.</param>
        /// <returns>Translated continue statement.</returns>
        protected abstract TResult Translate(ScriptCodeContinueStatement continueStatement, TranslationContext context);

        /// <summary>
        /// Translates break statement.
        /// </summary>
        /// <param name="breakStatement">The statement to be translated.</param>
        /// <param name="context">Translation context.</param>
        /// <param name="debugInfo">Debug information about statement.</param>
        /// <returns>Translated break statement.</returns>
        protected TResult Translate(ScriptCodeBreakLexicalScopeStatement breakStatement, TranslationContext context, out TResult debugInfo)
        {
            breakStatement.Verify();
            debugInfo = EmitDebugInfo(breakStatement.LinePragma);
            return Translate(breakStatement, context);
        }

        /// <summary>
        /// Translates break statement.
        /// </summary>
        /// <param name="breakStatement">The statement to be translated.</param>
        /// <param name="context">Translation context.</param>
        /// <returns>Translated break statement.</returns>
        protected abstract TResult Translate(ScriptCodeBreakLexicalScopeStatement breakStatement, TranslationContext context);

        /// <summary>
        /// Translates fault statement.
        /// </summary>
        /// <param name="fault">The statement to be translated.</param>
        /// <param name="context">Translation context.</param>
        /// <param name="debugInfo">Debug information about statement.</param>
        /// <returns>Translated fault statement.</returns>
        protected TResult Translate(ScriptCodeFaultStatement fault, TranslationContext context, out TResult debugInfo)
        {
            fault.Verify();
            debugInfo = EmitDebugInfo(fault.LinePragma);
            return Translate(fault, context);
        }

        /// <summary>
        /// Translates fault statement.
        /// </summary>
        /// <param name="fault">The statement to be translated.</param>
        /// <param name="context">Translation context.</param>
        /// <returns>Translated fault statement.</returns>
        protected abstract TResult Translate(ScriptCodeFaultStatement fault, TranslationContext context);

        /// <summary>
        /// Translates expression statement.
        /// </summary>
        /// <param name="expressionStmt">The statement to be translated.</param>
        /// <param name="context">Translation context.</param>
        /// <param name="debugInfo">Debug information about statement.</param>
        /// <returns>Translated expression statement.</returns>
        protected TResult Translate(ScriptCodeExpressionStatement expressionStmt, TranslationContext context, out TResult debugInfo)
        {
            expressionStmt.Verify();
            debugInfo = EmitDebugInfo(expressionStmt.LinePragma);
            return Translate(expressionStmt, context);
        }

        /// <summary>
        /// Translates expression statement.
        /// </summary>
        /// <param name="expressionStmt">The statement to be translated.</param>
        /// <param name="context">Translation context.</param>
        /// <returns>Translated expression statement.</returns>
        protected virtual TResult Translate(ScriptCodeExpressionStatement expressionStmt, TranslationContext context)
        {
            if (expressionStmt.Expression is ScriptCodeLoopExpression)
                ((ScriptCodeLoopExpression)expressionStmt.Expression).SuppressResult = true;
            return Translate(expressionStmt.Expression, context);
        }

        /// <summary>
        /// Translates variable declaration.
        /// </summary>
        /// <param name="variableDeclaration">The variable declaration to be transformed.</param>
        /// <param name="context">Translation context.</param>
        /// <param name="debugInfo">Debug information about statement.</param>
        /// <returns>Transformed variable declaration statement.</returns>
        protected TResult Translate(ScriptCodeVariableDeclaration variableDeclaration, TranslationContext context, out TResult debugInfo)
        {
            variableDeclaration.Verify();
            debugInfo = EmitDebugInfo(variableDeclaration.LinePragma);
            
            return Translate(variableDeclaration, context);
        }

        /// <summary>
        /// Translates variable declaration.
        /// </summary>
        /// <param name="variableDeclaration">The variable declaration to be transformed.</param>
        /// <param name="context">Translation context.</param>
        /// <returns>Transformed variable declaration statement.</returns>
        protected abstract TResult Translate(ScriptCodeVariableDeclaration variableDeclaration, TranslationContext context);

        #endregion

        /// <summary>
        /// Extracts interpretation context from translation context.
        /// </summary>
        /// <param name="context">Translation context.</param>
        /// <returns></returns>
        protected abstract InterpretationContext GetInterpretationContext(TranslationContext context);

        #region Expression Translators

        /// <summary>
        /// Translates expression.
        /// </summary>
        /// <param name="expression">The expression to be translated.</param>
        /// <param name="context">Translation context.</param>
        /// <returns>Translated code expression.</returns>
        protected TResult Translate(ScriptCodeExpression expression, TranslationContext context)
        {
            expression = expression.CanReduce ? expression.Reduce(GetInterpretationContext(context)) : expression;
            if (expression is ScriptCodeUnaryOperatorExpression)
                return Translate((ScriptCodeUnaryOperatorExpression)expression, context);
            else if (expression is ScriptCodeExpressionContractExpression)
                return Translate((ScriptCodeExpressionContractExpression)expression, context);
            else if (expression is ScriptCodeStatementContractExpression)
                return Translate((ScriptCodeStatementContractExpression)expression, context);
            else if (expression is ScriptCodeBinaryOperatorExpression)
                return Translate((ScriptCodeBinaryOperatorExpression)expression, context);
            else if (expression is ScriptCodeObjectExpression)
                return Translate((ScriptCodeObjectExpression)expression, context);
            else if (expression is ScriptCodeActionImplementationExpression)
                return Translate((ScriptCodeActionImplementationExpression)expression, context);
            else if (expression is ScriptCodeActionContractExpression)
                return Translate((ScriptCodeActionContractExpression)expression, context);
            else if (expression is ScriptCodeArrayContractExpression)
                return Translate((ScriptCodeArrayContractExpression)expression, context);
            else if (expression is ScriptCodeBooleanContractExpression)
                return Translate((ScriptCodeBooleanContractExpression)expression, context);
            else if (expression is ScriptCodeBooleanExpression)
                return Translate((ScriptCodeBooleanExpression)expression, context);
            else if (expression is ScriptCodeCallableContractExpression)
                return Translate((ScriptCodeCallableContractExpression)expression, context);
            else if (expression is ScriptCodeConditionalExpression)
                return Translate((ScriptCodeConditionalExpression)expression, context);
            else if (expression is ScriptCodeContextExpression)
                return Translate((ScriptCodeContextExpression)expression, context);
            else if (expression is ScriptCodeCurrentActionExpression)
                return Translate((ScriptCodeCurrentActionExpression)expression, context);
            else if (expression is ScriptCodeComplexExpression)
                return Translate((ScriptCodeComplexExpression)expression, context);
            else if (expression is ScriptCodeForEachLoopExpression)
                return Translate((ScriptCodeForEachLoopExpression)expression, context);
            else if (expression is ScriptCodeForLoopExpression)
                return Translate((ScriptCodeForLoopExpression)expression, context);
            else if (expression is ScriptCodeIndexerExpression)
                return Translate((ScriptCodeIndexerExpression)expression, context);
            else if (expression is ScriptCodeIntegerContractExpression)
                return Translate((ScriptCodeIntegerContractExpression)expression, context);
            else if (expression is ScriptCodeIntegerExpression)
                return Translate((ScriptCodeIntegerExpression)expression, context);
            else if (expression is ScriptCodeInvocationExpression)
                return Translate((ScriptCodeInvocationExpression)expression, context);
            else if (expression is ScriptCodeMetaContractExpression)
                return Translate((ScriptCodeMetaContractExpression)expression, context);
            else if (expression is ScriptCodeRealContractExpression)
                return Translate((ScriptCodeRealContractExpression)expression, context);
            else if (expression is ScriptCodeRealExpression)
                return Translate((ScriptCodeRealExpression)expression, context);
            else if (expression is ScriptCodeSuperContractExpression)
                return Translate((ScriptCodeSuperContractExpression)expression, context);
            else if (expression is ScriptCodeStringContractExpression)
                return Translate((ScriptCodeStringContractExpression)expression, context);
            else if (expression is ScriptCodeStringExpression)
                return Translate((ScriptCodeStringExpression)expression, context);
            else if (expression is ScriptCodeTryElseFinallyExpression)
                return Translate((ScriptCodeTryElseFinallyExpression)expression, context);
            else if (expression is ScriptCodeVoidExpression)
                return Translate((ScriptCodeVoidExpression)expression, context);
            else if (expression is ScriptCodeWhileLoopExpression)
                return Translate((ScriptCodeWhileLoopExpression)expression, context);
            else if (expression is ScriptCodeVariableReference)
                return Translate((ScriptCodeVariableReference)expression, context);
            else if (expression is ScriptCodeSelectionExpression)
                return Translate((ScriptCodeSelectionExpression)expression, context);
            else if (expression is ScriptCodeForkExpression)
                return Translate((ScriptCodeForkExpression)expression, context);
            else if (expression is ScriptCodeAwaitExpression)
                return Translate((ScriptCodeAwaitExpression)expression, context);
            else if (expression is ScriptCodeThisExpression)
                return Translate((ScriptCodeThisExpression)expression, context);
            else if (expression is ScriptCodeArrayExpression)
                return Translate((ScriptCodeArrayExpression)expression, context);
            else if (expression is ScriptCodeFinSetContractExpression)
                return Translate((ScriptCodeFinSetContractExpression)expression, context);
            else if (expression is ScriptCodeDimensionalContractExpression)
                return Translate((ScriptCodeDimensionalContractExpression)expression, context);
            else if (expression is ScriptCodeAsyncExpression)
                return Translate((ScriptCodeAsyncExpression)expression, context);
            else if (expression is ScriptCodeQuoteExpression)
                return Translate((ScriptCodeQuoteExpression)expression, context);
            else if (expression is ScriptCodeCurrentQuoteExpression)
                return Translate((ScriptCodeCurrentQuoteExpression)expression, context);
            else if (expression is ScriptCodePlaceholderExpression)
                return Translate((ScriptCodePlaceholderExpression)expression, context);
            else if (expression is ScriptCodeExpandExpression)
                return Translate((ScriptCodeExpandExpression)expression, context);
            else return null;
        }

        /// <summary>
        /// Translates expand expression.
        /// </summary>
        /// <param name="expandexpr"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        protected abstract TResult Translate(ScriptCodeExpandExpression expandexpr, TranslationContext context);

        /// <summary>
        /// Translates complex expression.
        /// </summary>
        /// <param name="complex"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        protected abstract TResult Translate(ScriptCodeComplexExpression complex, TranslationContext context);

        /// <summary>
        /// Translates placeholder expression.
        /// </summary>
        /// <param name="placeholder"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        protected abstract TResult Translate(ScriptCodePlaceholderExpression placeholder, TranslationContext context);

        /// <summary>
        /// Translates reference to the current action as quoted expression tree.
        /// </summary>
        /// <param name="currentQuote"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        protected abstract TResult Translate(ScriptCodeCurrentQuoteExpression currentQuote, TranslationContext context);

        /// <summary>
        /// Translates quoted expression.
        /// </summary>
        /// <param name="expression"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        protected abstract TResult Translate(ScriptCodeQuoteExpression expression, TranslationContext context);

        /// <summary>
        /// Translates runtime expression factory.
        /// </summary>
        /// <param name="expression">An expression to be translate.</param>
        /// <param name="context">Translation context.</param>
        /// <returns>Translated code expression.</returns>
        protected abstract TResult Translate(ScriptCodeExpressionContractExpression expression, TranslationContext context);

        /// <summary>
        /// Translates runtime statement factory.
        /// </summary>
        /// <param name="expression">An expression to be translate.</param>
        /// <param name="context">Translation context.</param>
        /// <returns></returns>
        protected abstract TResult Translate(ScriptCodeStatementContractExpression expression, TranslationContext context);

        /// <summary>
        /// Translates asynchronous contract.
        /// </summary>
        /// <param name="expression"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        protected abstract TResult Translate(ScriptCodeAsyncExpression expression, TranslationContext context);

        /// <summary>
        /// Translates DIMENSIONAL contract.
        /// </summary>
        /// <param name="contract">The contract to translate.</param>
        /// <param name="context">Translation context.</param>
        /// <returns>Translated DIMENSIONAL contract.</returns>
        protected abstract TResult Translate(ScriptCodeDimensionalContractExpression contract, TranslationContext context);

        /// <summary>
        /// Translates FINSET contract.
        /// </summary>
        /// <param name="contract">The contract to translate.</param>
        /// <param name="context">Translation context.</param>
        /// <returns>Translated FINSET contract.</returns>
        protected abstract TResult Translate(ScriptCodeFinSetContractExpression contract, TranslationContext context);

        /// <summary>
        /// Translates array expression.
        /// </summary>
        /// <param name="arrayExpression">The expression to be translated.</param>
        /// <param name="context">Translation context.</param>
        /// <returns>The translated array expression.</returns>
        protected abstract TResult Translate(ScriptCodeArrayExpression arrayExpression, TranslationContext context);

        /// <summary>
        /// Translates 'this'.
        /// </summary>
        /// <param name="thisExpression">The expression to be translated.</param>
        /// <param name="context">Translation context.</param>
        /// <returns></returns>
        protected abstract TResult Translate(ScriptCodeThisExpression thisExpression, TranslationContext context);

        /// <summary>
        /// Translates synchronization expression.
        /// </summary>
        /// <param name="awaitExpression">The expression to be translated.</param>
        /// <param name="context">Translation context.</param>
        /// <returns>The translated expression.</returns>
        protected abstract TResult Translate(ScriptCodeAwaitExpression awaitExpression, TranslationContext context);

        /// <summary>
        /// Translates asynchronous task producer.
        /// </summary>
        /// <param name="forkExpression">The expression to be translated.</param>
        /// <param name="context">Translation context.</param>
        /// <returns>Translated fork expression.</returns>
        protected abstract TResult Translate(ScriptCodeForkExpression forkExpression, TranslationContext context);

        /// <summary>
        /// Translates selection expression.
        /// </summary>
        /// <param name="selection">The expression to be translated.</param>
        /// <param name="context">The translation context.</param>
        /// <returns>Translated selection expression.</returns>
        protected abstract TResult Translate(ScriptCodeSelectionExpression selection, TranslationContext context);

        /// <summary>
        /// Translates variable reference.
        /// </summary>
        /// <param name="variableRef">The variable reference.</param>
        /// <param name="context">Translation context.</param>
        /// <returns>Translated variable reference.</returns>
        protected abstract TResult Translate(ScriptCodeVariableReference variableRef, TranslationContext context);

        /// <summary>
        /// Translates while loop.
        /// </summary>
        /// <param name="whileLoop">The loop expression to be translated.</param>
        /// <param name="context">Translation context.</param>
        /// <returns>Translated while loop.</returns>
        protected abstract TResult Translate(ScriptCodeWhileLoopExpression whileLoop, TranslationContext context);

        /// <summary>
        /// Translates void expression.
        /// </summary>
        /// <param name="voidExpression">The expression to be translated.</param>
        /// <param name="context">Translation context.</param>
        /// <returns>Translated expression.</returns>
        protected abstract TResult Translate(ScriptCodeVoidExpression voidExpression, TranslationContext context);

        /// <summary>
        /// Translates structured exception handler.
        /// </summary>
        /// <param name="tryElseFinally">SEH block to be translated.</param>
        /// <param name="context">Translation context.</param>
        /// <returns>Translated SEH block.</returns>
        protected abstract TResult Translate(ScriptCodeTryElseFinallyExpression tryElseFinally, TranslationContext context);

        /// <summary>
        /// Translates string literal.
        /// </summary>
        /// <param name="stringLiteral">The literal to be translated.</param>
        /// <param name="context">Translation context.</param>
        /// <returns></returns>
        protected abstract TResult Translate(ScriptCodeStringExpression stringLiteral, TranslationContext context);

        /// <summary>
        /// Translates string contract reference.
        /// </summary>
        /// <param name="stringContract">The string contract reference to be translated.</param>
        /// <param name="context">Translation context.</param>
        /// <returns>Translated string contract reference.</returns>
        protected abstract TResult Translate(ScriptCodeStringContractExpression stringContract, TranslationContext context);

        /// <summary>
        /// Translates root contract reference.
        /// </summary>
        /// <param name="rootContract">The root contract reference to be translated.</param>
        /// <param name="context">Translation context.</param>
        /// <returns>Translated root contract reference.</returns>
        protected abstract TResult Translate(ScriptCodeSuperContractExpression rootContract, TranslationContext context);

        /// <summary>
        /// Translates real literal.
        /// </summary>
        /// <param name="realLiteral">The real literal to be translated.</param>
        /// <param name="context">Translation context.</param>
        /// <returns>Translated real literal.</returns>
        protected abstract TResult Translate(ScriptCodeRealExpression realLiteral, TranslationContext context);

        /// <summary>
        /// Translates real contract reference.
        /// </summary>
        /// <param name="realContract">The real contract reference to be translated.</param>
        /// <param name="context">Translation context.</param>
        /// <returns>Translated real contract reference.</returns>
        protected abstract TResult Translate(ScriptCodeRealContractExpression realContract, TranslationContext context);

        /// <summary>
        /// Translates meta contract reference.
        /// </summary>
        /// <param name="metaContract">The meta contract reference to be translated.</param>
        /// <param name="context">Translation context.</param>
        /// <returns>Translated meta contract reference.</returns>
        protected abstract TResult Translate(ScriptCodeMetaContractExpression metaContract, TranslationContext context);

        /// <summary>
        /// Translates invocation expression.
        /// </summary>
        /// <param name="invocation">The invocation expression ot be transformed.</param>
        /// <param name="context">Translation context.</param>
        /// <returns>Translated invocation expression.</returns>
        protected abstract TResult Translate(ScriptCodeInvocationExpression invocation, TranslationContext context);

        /// <summary>
        /// Translates integer literal.
        /// </summary>
        /// <param name="integerLiteral">The integer literal to be transformed.</param>
        /// <param name="context">Translation context.</param>
        /// <returns>Translated integer literal.</returns>
        protected abstract TResult Translate(ScriptCodeIntegerExpression integerLiteral, TranslationContext context);

        /// <summary>
        /// Translates integer contract reference.
        /// </summary>
        /// <param name="integerContract">The integer contract reference to be translated.</param>
        /// <param name="context">Translation context.</param>
        /// <returns></returns>
        protected abstract TResult Translate(ScriptCodeIntegerContractExpression integerContract, TranslationContext context);

        /// <summary>
        /// Translates indexer expression.
        /// </summary>
        /// <param name="indexer">The indexer expression to be translated.</param>
        /// <param name="context">Translation context.</param>
        /// <returns>Translated indexer expression.</returns>
        protected abstract TResult Translate(ScriptCodeIndexerExpression indexer, TranslationContext context);

        /// <summary>
        /// Translates for loop.
        /// </summary>
        /// <param name="forLoop">The loop expression to be translated.</param>
        /// <param name="context">Translation context.</param>
        /// <returns></returns>
        protected abstract TResult Translate(ScriptCodeForLoopExpression forLoop, TranslationContext context);

        /// <summary>
        /// Translates for-each loop.
        /// </summary>
        /// <param name="forEachLoop">The loop expression to be translated.</param>
        /// <param name="context">Translation context.</param>
        /// <returns>Translated loop expression.</returns>
        protected abstract TResult Translate(ScriptCodeForEachLoopExpression forEachLoop, TranslationContext context);

        /// <summary>
        /// Translates reference to the current action in the call stack.
        /// </summary>
        /// <param name="currentAction">The expression to be translated.</param>
        /// <param name="context">Translation context.</param>
        /// <returns>Translated expression.</returns>
        protected abstract TResult Translate(ScriptCodeCurrentActionExpression currentAction, TranslationContext context);

        /// <summary>
        /// Translates context scope.
        /// </summary>
        /// <param name="interpretationContext">The context scope to be translated.</param>
        /// <param name="context">Translation context.</param>
        /// <returns>Translated context scope.</returns>
        protected abstract TResult Translate(ScriptCodeContextExpression interpretationContext, TranslationContext context);

        /// <summary>
        /// Translates conditional expression.
        /// </summary>
        /// <param name="conditional">The conditional expression to be translated.</param>
        /// <param name="context">Translation context.</param>
        /// <returns>Translated conditional expression.</returns>
        protected abstract TResult Translate(ScriptCodeConditionalExpression conditional, TranslationContext context);

        /// <summary>
        /// Translates callable contract definition.
        /// </summary>
        /// <param name="callableContract">The callable contract definition to be translated.</param>
        /// <param name="context">Translation context.</param>
        /// <returns>Translated callable contract.</returns>
        protected abstract TResult Translate(ScriptCodeCallableContractExpression callableContract, TranslationContext context);

        /// <summary>
        /// Translates boolean literal.
        /// </summary>
        /// <param name="booleanLiteral">The literal to be translated.</param>
        /// <param name="context">Translation context.</param>
        /// <returns>Translated boolean literal.</returns>
        protected abstract TResult Translate(ScriptCodeBooleanExpression booleanLiteral, TranslationContext context);

        /// <summary>
        /// Translates boolean contract.
        /// </summary>
        /// <param name="booleanContract">The literal to be translated.</param>
        /// <param name="context">Translation context.</param>
        /// <returns>Translated boolean contract.</returns>
        protected abstract TResult Translate(ScriptCodeBooleanContractExpression booleanContract, TranslationContext context);

        /// <summary>
        /// Translates array contract definition.
        /// </summary>
        /// <param name="arrayContract">The array contract definition to be translated.</param>
        /// <param name="context">Translation context.</param>
        /// <returns>Translated array contract definition.</returns>
        protected abstract TResult Translate(ScriptCodeArrayContractExpression arrayContract, TranslationContext context);

        /// <summary>
        /// Translates action contract definition.
        /// </summary>
        /// <param name="actionContract">The action contract definition to be translated.</param>
        /// <param name="context">Translation context.</param>
        /// <returns>Translated action contract definition.</returns>
        protected abstract TResult Translate(ScriptCodeActionContractExpression actionContract, TranslationContext context);

        /// <summary>
        /// Translates action object.
        /// </summary>
        /// <param name="expression">The expression that describes action object to be translated.</param>
        /// <param name="context">Translation context.</param>
        /// <returns>Translated action implementation expression.</returns>
        protected abstract TResult Translate(ScriptCodeActionImplementationExpression expression, TranslationContext context);

        /// <summary>
        /// Translates inline object expression.
        /// </summary>
        /// <param name="expression">The expression to be translated.</param>
        /// <param name="context">Translation context.</param>
        /// <returns></returns>
        protected abstract TResult Translate(ScriptCodeObjectExpression expression, TranslationContext context);

        /// <summary>
        /// Translates binary expression.
        /// </summary>
        /// <param name="expression">The binary expression to be translated.</param>
        /// <param name="context">Translation context.</param>
        /// <returns>Translated binary expression.</returns>
        protected abstract TResult Translate(ScriptCodeBinaryOperatorExpression expression, TranslationContext context);

        /// <summary>
        /// Translates unary expression.
        /// </summary>
        /// <param name="expression">The unary expression to be translated.</param>
        /// <param name="context">Translation context.</param>
        /// <returns>Translated unary expression.</returns>
        protected abstract TResult Translate(ScriptCodeUnaryOperatorExpression expression, TranslationContext context);

        #endregion

        #region Miscellaneous Translators
        /// <summary>
        /// Translates code analysis error to throwable instruction.
        /// </summary>
        /// <param name="error">Code analysis error to be translated.</param>
        /// <returns>Throwable instructions that represents the input error.</returns>
        protected abstract TResult Translate(CodeAnalysisException error);

        /// <summary>
        /// Translates a set of instructions into the single composite instruction.
        /// </summary>
        /// <param name="instructions">A set of instructions to compose.</param>
        /// <param name="context">Translation context.</param>
        /// <returns>Composite instruction that encapsulates instruction set.</returns>
        protected abstract TResult Translate(IList<TResult> instructions, TranslationContext context);

        /// <summary>
        /// Translates DynamicScript Code Document Model beginning with the current position
        /// of the translator.
        /// </summary>
        /// <param name="emitDebug">Specifies that the debug information should be emitted.</param>
        /// <returns>Translated DynamicScript program CodeDOM.</returns>
        public TResult Translate(bool emitDebug = false)
        {
            const int DefaultCapacity = 100;
            var instructions = new List<TResult>(DefaultCapacity);
            var debugInfo = default(TResult);
            while (MoveNext(out debugInfo) && Current != null)
            {
                if (emitDebug && debugInfo != null) instructions.Add(debugInfo);
                instructions.Add(Current);
            }
            return Translate(instructions, Context);
        }
        #endregion

        private void Reset(bool resetSyntaxAnalyzer)
        {
            VerifyOnDisposed();
            if (resetSyntaxAnalyzer)
                m_statements.Reset();
            Context.Reset(CreateRootScope(Context.UserData));
            Context.ScopeEnter += EnterScope;
            Context.ScopeLeave += LeaveScope;
        }

        /// <summary>
        /// Resets analyzer to its initial state.
        /// </summary>
        /// <exception cref="System.ObjectDisposedException">Analyzer is disposed.</exception>
        public void Reset()
        {
            Reset(true);
        }

        /// <summary>
        /// Releases all resources associated with analyzer.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (!m_disposed && disposing)
            {
                m_statements.Dispose();
                m_current = null;
            }
            m_disposed = true;
        }

        /// <summary>
        /// Prepares analyzer for garbage collection.
        /// </summary>
        ~Translator()
        {
            Dispose(false);
        }
    }
}
