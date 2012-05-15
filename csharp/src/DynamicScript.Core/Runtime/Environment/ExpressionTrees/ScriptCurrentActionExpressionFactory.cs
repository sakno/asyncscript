using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace DynamicScript.Runtime.Environment.ExpressionTrees
{
    using Compiler.Ast;
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;

    [Serializable]
    [ComVisible(false)]
    sealed class ScriptCurrentActionExpressionFactory : ScriptExpressionFactory<ScriptCodeCurrentActionExpression, ScriptCurrentActionExpression>
    {
        public new const string Name = "callee";

        private ScriptCurrentActionExpressionFactory(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        private ScriptCurrentActionExpressionFactory()
            : base(Name)
        {
        }

        public static readonly ScriptCurrentActionExpressionFactory Instance = new ScriptCurrentActionExpressionFactory();

        public override ScriptCurrentActionExpression CreateCodeElement(IList<IScriptObject> args, InterpreterState state)
        {
            return ScriptCurrentActionExpression.Instance;
        }
    }
}
