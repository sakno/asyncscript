using System;
using System.Collections.Generic;
using System.CodeDom;

namespace DynamicScript.Runtime.Environment
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;
    using LinqExpressionTranslator = Compiler.Ast.Translation.LinqExpressions.LinqExpressionTranslator;
    using Compiler.Ast;
    using Enumerable = System.Linq.Enumerable;

    [ComVisible(false)]
    sealed class ScriptActionInvoker : ScriptFunc
    {
        public readonly ScriptInvoker Invoker;

        public ScriptActionInvoker(ScriptInvoker invoker)
            : base(ScriptSuperContract.Instance)
        {
            if (invoker == null) throw new ArgumentNullException("invoker");
            Invoker = invoker;
        }

        protected override IScriptObject Invoke(InvocationContext ctx)
        {
            return Invoker.Invoke(ctx.RuntimeState);
        }

        public static ScriptActionInvoker Compile(IEnumerable<ScriptCodeStatement> statements)
        {
            return new ScriptActionInvoker(DynamicScriptInterpreter.Compile(statements));
        }

        public static ScriptActionInvoker Compile(ScriptCodeStatementCollection statements)
        {
            return Compile(Enumerable.Cast<ScriptCodeStatement>(statements));
        }

        public static ScriptActionInvoker Compile(IEnumerable<ScriptCodeExpression> expressions)
        {
            return Compile(Enumerable.Select(expressions, expr => new ScriptCodeExpressionStatement(expr)));
        }

        public static ScriptActionInvoker Compile(ScriptCodeExpression expression)
        {
            return Compile(new[] { expression });
        }
    }
}
