using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace DynamicScript.Runtime.Environment.ExpressionTrees
{
    using Compiler.Ast;
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;

    [ComVisible(false)]
    [Serializable]
    sealed class ScriptFunctionExpression : ScriptExpression<ScriptCodeFunctionExpression, IScriptFunction>
    {
        private ScriptFunctionExpression(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        public ScriptFunctionExpression(ScriptCodeFunctionExpression expression)
            : base(expression, ScriptFunctionExpressionFactory.Instance)
        {
        }

        public override IScriptFunction Compile(InterpreterState state)
        {
            return DynamicScriptInterpreter.Run(Expression, state) as IScriptFunction;
        }

        public static ScriptCodeFunctionExpression CreateExpression(IScriptCodeElement<ScriptCodeActionContractExpression> signature, IScriptCodeElement<ScriptCodeExpression> body)
        {
            return signature != null && body != null ? new ScriptCodeFunctionExpression(signature.CodeObject, new ScriptCodeExpressionStatement(body.CodeObject)) : null;
        }

        protected override ScriptCodeFunctionExpression CreateExpression(IList<IScriptObject> args, InterpreterState state)
        {
            return args.Count == 2 ? CreateExpression(args[0] as IScriptCodeElement<ScriptCodeActionContractExpression>, args[1] as IScriptCodeElement<ScriptCodeExpression>) : null;
        }
    }
}
