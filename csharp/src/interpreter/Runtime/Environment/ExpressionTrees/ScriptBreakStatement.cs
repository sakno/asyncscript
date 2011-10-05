using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace DynamicScript.Runtime.Environment.ExpressionTrees
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;
    using Compiler.Ast;
    using Enumerable = System.Linq.Enumerable;

    /// <summary>
    /// Represents runtime representation of BREAK statement.
    /// This class cannot be inherited.
    /// </summary>
    [ComVisible(false)]
    [Serializable]
    sealed class ScriptBreakStatement: ScriptStatement<ScriptCodeBreakLexicalScopeStatement>
    {
        private ScriptBreakStatement(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        public ScriptBreakStatement(ScriptCodeBreakLexicalScopeStatement statement)
            : base(statement, ScriptBreakStatementFactory.Instance)
        {
        }

        public static ScriptCodeBreakLexicalScopeStatement CreateStatement(IEnumerable<IScriptObject> args = null)
        {
            var result = new ScriptCodeBreakLexicalScopeStatement();
            foreach (var a in args ?? Enumerable.Empty<IScriptObject>())
                result.ArgList.Add(a is IScriptExpression<ScriptCodeExpression> ? ((IScriptExpression<ScriptCodeExpression>)a).CodeObject : ScriptConstantExpression.CreateExpression(a));
            return result;
        }

        protected override ScriptCodeBreakLexicalScopeStatement CreateStatement(IList<IScriptObject> args, InterpreterState state)
        {
            switch (args.Count)
            {
                default:
                case 0: return CreateStatement();
                case 1: return CreateStatement(args[0] as IEnumerable<IScriptObject>);
            }
        }

        public override bool Execute(IList<IScriptObject> args, InterpreterState state)
        {
            return false;
        }
    }
}
