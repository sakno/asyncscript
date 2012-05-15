using System;
using System.Runtime.Serialization;
using System.Collections.Generic;

namespace DynamicScript.Runtime.Environment.ExpressionTrees
{
    using Compiler.Ast;
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;

    [ComVisible(false)]
    sealed class ScriptCurrentActionExpression: ScriptExpression<ScriptCodeCurrentActionExpression, IScriptObject>
    {
        private ScriptCurrentActionExpression(SerializationInfo info, StreamingContext context)
            :base(info, context)
        {
        }

        private ScriptCurrentActionExpression()
            : base(ScriptCodeCurrentActionExpression.Instance, ScriptCurrentActionExpressionFactory.Instance)
        {
        }

        public static readonly ScriptCurrentActionExpression Instance = new ScriptCurrentActionExpression();

        public override IScriptObject Compile(InterpreterState state)
        {
            return Void;
        }

        protected override ScriptCodeCurrentActionExpression CreateExpression(IList<IScriptObject> args, InterpreterState state)
        {
            return ScriptCodeCurrentActionExpression.Instance;
        }
    }
}
