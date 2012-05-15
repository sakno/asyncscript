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
    sealed class ScriptFunctionInvoker : ScriptFunc
    {
        public readonly ScriptInvoker Invoker;

        public ScriptFunctionInvoker(ScriptInvoker invoker)
            : base(ScriptSuperContract.Instance)
        {
            if (invoker == null) throw new ArgumentNullException("invoker");
            Invoker = invoker;
        }

        protected override IScriptObject Invoke(InterpreterState state)
        {
            return Invoker.Invoke(state);
        }

        public static ScriptFunctionInvoker Compile(IEnumerable<ScriptCodeStatement> statements)
        {
            return new ScriptFunctionInvoker(DynamicScriptInterpreter.Compile(statements));
        }

        public static ScriptFunctionInvoker Compile(ScriptCodeStatementCollection statements)
        {
            return Compile(Enumerable.Cast<ScriptCodeStatement>(statements));
        }

        public static ScriptFunctionInvoker Compile(IEnumerable<ScriptCodeExpression> expressions)
        {
            return Compile(Enumerable.Select(expressions, expr => new ScriptCodeExpressionStatement(expr)));
        }

        public static ScriptFunctionInvoker Compile(ScriptCodeExpression expression)
        {
            return Compile(new[] { expression });
        }
    }
}
