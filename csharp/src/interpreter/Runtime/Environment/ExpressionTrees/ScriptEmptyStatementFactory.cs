using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace DynamicScript.Runtime.Environment.ExpressionTrees
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;
    using ScriptCodeEmptyStatement = Compiler.Ast.ScriptCodeEmptyStatement;

    [Serializable]
    [ComVisible(false)]
    sealed class ScriptEmptyStatementFactory: ScriptStatementFactory<ScriptCodeEmptyStatement, ScriptEmptyStatement>
    {
        public new const string Name = "empty";
        

        private ScriptEmptyStatementFactory(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        private ScriptEmptyStatementFactory()
            : base(Name)
        {
        }

        public static ScriptEmptyStatementFactory Instance = new ScriptEmptyStatementFactory();

        public override ScriptEmptyStatement CreateCodeElement(IList<IScriptObject> args, InterpreterState state)
        {
            return ScriptEmptyStatement.Instance;
        }

        protected override IRuntimeSlot Modify
        {
            get { return null; }
        }
    }
}
