using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace DynamicScript.Runtime.Environment.ExpressionTrees
{
    using Compiler.Ast;
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;

    [ComVisible(false)]
    [Serializable]
    sealed class ScriptFunctionExpression : ScriptExpression<ScriptCodeActionImplementationExpression, IScriptFunction>
    {
        private ScriptFunctionExpression(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        public ScriptFunctionExpression(ScriptCodeActionImplementationExpression expression)
            : base(expression, ScriptFunctionExpressionFactory.Instance)
        {
        }

        public override IScriptFunction Compile(InterpreterState state)
        {
            return DynamicScriptInterpreter.Run(Expression, state) as IScriptFunction;
        }

        public static ScriptCodeActionImplementationExpression CreateExpression(IScriptCodeElement<ScriptCodeActionContractExpression> signature, IScriptCodeElement<ScriptCodeExpression> body)
        {
            return signature != null && body != null ? new ScriptCodeActionImplementationExpression(signature.CodeObject, new ScriptCodeExpressionStatement(body.CodeObject)) : null;
        }

        protected override ScriptCodeActionImplementationExpression CreateExpression(IList<IScriptObject> args, InterpreterState state)
        {
            return args.Count == 2 ? CreateExpression(args[0] as IScriptCodeElement<ScriptCodeActionContractExpression>, args[1] as IScriptCodeElement<ScriptCodeExpression>) : null;
        }
    }
}
