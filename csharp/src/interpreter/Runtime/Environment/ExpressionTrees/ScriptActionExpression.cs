using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace DynamicScript.Runtime.Environment.ExpressionTrees
{
    using Compiler.Ast;
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;

    [ComVisible(false)]
    [Serializable]
    sealed class ScriptActionExpression : ScriptExpression<ScriptCodeActionImplementationExpression, IScriptAction>
    {
        private ScriptActionExpression(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        public ScriptActionExpression(ScriptCodeActionImplementationExpression expression)
            : base(expression, ScriptActionExpressionFactory.Instance)
        {
        }

        public override IScriptAction Compile(InterpreterState state)
        {
            return DynamicScriptInterpreter.Run(Expression, state) as IScriptAction;
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
