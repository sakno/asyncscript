using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace DynamicScript.Runtime.Environment.ExpressionTrees
{
    using Compiler.Ast;
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;

    [ComVisible(false)]
    [Serializable]
    sealed class ScriptExpandExpression: ScriptExpression<ScriptCodeExpandExpression, IScriptObject>
    {
        private ScriptExpandExpression(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        public ScriptExpandExpression(ScriptCodeExpandExpression expression)
            : base(expression, ScriptExpandExpressionFactory.Instance)
        {
        }

        public static ScriptCodeExpandExpression CreateExpression(IScriptObject target, IEnumerable<IScriptObject> substitutions)
        {
            var result = new ScriptCodeExpandExpression
            {
                Target = target is IScriptCodeElement<ScriptCodeExpression> ? ((IScriptCodeElement<ScriptCodeExpression>)target).CodeObject :
                ScriptConstantExpression.CreateExpression(target)
            };
            ScriptExpressionFactory.CreateExpressions(substitutions, result.Substitutes);
            return result;
        }

        public override IScriptObject Compile(InterpreterState state)
        {
            return DynamicScriptInterpreter.Run(Expression, state);
        }

        protected override ScriptCodeExpandExpression CreateExpression(IList<IScriptObject> args, InterpreterState state)
        {
            return args.Count == 2 ? CreateExpression(args[0], args[1] as IEnumerable<IScriptObject>) : null;
        }
    }
}
