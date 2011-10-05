using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace DynamicScript.Runtime.Environment.ExpressionTrees
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;
    using ScriptCodeEmptyStatement = Compiler.Ast.ScriptCodeEmptyStatement;

    [Serializable]
    [ComVisible(false)]
    sealed class ScriptEmptyStatement: ScriptStatement<ScriptCodeEmptyStatement>
    {
        
        private ScriptEmptyStatement(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        private ScriptEmptyStatement()
            : base(ScriptCodeEmptyStatement.Instance, ScriptEmptyStatementFactory.Instance)
        {
        }

        public static readonly ScriptEmptyStatement Instance = new ScriptEmptyStatement();

        protected override ScriptCodeEmptyStatement CreateStatement(IList<IScriptObject> args, InterpreterState state)
        {
            return ScriptCodeEmptyStatement.Instance;
        }
    }
}
