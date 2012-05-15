using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace DynamicScript.Runtime.Environment.ExpressionTrees
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;
    using ScriptCodeThisExpression = Compiler.Ast.ScriptCodeThisExpression;

    sealed class ScriptThisExpressionFactory: ScriptExpressionFactory<ScriptCodeThisExpression, ScriptThisExpression>
    {
        public new const string Name = "`this";

        private ScriptThisExpressionFactory(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        private ScriptThisExpressionFactory()
            : base(Name)
        {
        }

        public static readonly ScriptThisExpressionFactory Instance = new ScriptThisExpressionFactory();

        public override ScriptThisExpression CreateCodeElement(IList<IScriptObject> args, InterpreterState state)
        {
            return ScriptThisExpression.Instance;
        }
    }
}
