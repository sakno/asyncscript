using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace DynamicScript.Runtime.Environment.ExpressionTrees
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;
    using ScriptCodeThisExpression = Compiler.Ast.ScriptCodeThisExpression;

    [ComVisible(false)]
    [Serializable]
    sealed class ScriptThisExpression: ScriptExpression<ScriptCodeThisExpression, IScriptObject>
    {
        private ScriptThisExpression(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        private ScriptThisExpression()
            : base(ScriptCodeThisExpression.Instance, ScriptThisExpressionFactory.Instance)
        {
        }

        public static readonly ScriptThisExpression Instance = new ScriptThisExpression();

        public override IScriptObject Compile(InterpreterState state)
        {
            return state.Global;
        }

        protected override ScriptCodeThisExpression CreateExpression(IList<IScriptObject> args, InterpreterState state)
        {
            return ScriptCodeThisExpression.Instance;
        }
    }
}
