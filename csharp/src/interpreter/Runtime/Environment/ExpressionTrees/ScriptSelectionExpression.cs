using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace DynamicScript.Runtime.Environment.ExpressionTrees
{
    using Compiler.Ast;
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;

    [ComVisible(false)]
    [Serializable]
    sealed class ScriptSelectionExpression: ScriptExpression<ScriptCodeSelectionExpression, IScriptObject>
    {
        private ScriptSelectionExpression(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        public ScriptSelectionExpression(ScriptCodeSelectionExpression expression)
            : base(expression, ScriptSelectionExpressionFactory.Instance)
        {
        }

        public ScriptSelectionExpression()
            : this(new ScriptCodeSelectionExpression())
        {
        }

        public override IScriptObject Compile(InterpreterState state)
        {
            return DynamicScriptInterpreter.Run(Expression, state);
        }

        protected override ScriptCodeSelectionExpression CreateExpression(IList<IScriptObject> args, InterpreterState state)
        {
            return new ScriptCodeSelectionExpression();
        }
    }
}
