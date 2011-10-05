using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace DynamicScript.Runtime.Environment.ExpressionTrees
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;
    using Compiler.Ast;

    [ComVisible(false)]
    [Serializable]
    sealed class ScriptLoopVariableStatement : ScriptStatement<ScriptCodeLoopWithVariableExpression.LoopVariable>
    {
        private ScriptLoopVariableStatement(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        public ScriptLoopVariableStatement(ScriptCodeLoopWithVariableExpression.LoopVariable loopvar)
            : base(loopvar, ScriptLoopVariableStatementFactory.Instance)
        {
        }

        public static ScriptCodeLoopWithVariableExpression.LoopVariable CreateStatement(ScriptString name, ScriptBoolean temporary, IScriptCodeElement<ScriptCodeExpression> initExpr)
        {
            return name != null && name.Length > 0 ?
                new ScriptCodeLoopWithVariableExpression.LoopVariable(name, temporary, initExpr != null ? initExpr.CodeObject : null) :
                null;
        }

        protected override ScriptCodeLoopWithVariableExpression.LoopVariable CreateStatement(IList<IScriptObject> args, InterpreterState state)
        {
            switch (args.Count)
            {
                case 1:
                    return CreateStatement(args[0] as ScriptString, ScriptBoolean.False, null);
                case 2:
                    return CreateStatement(args[0] as ScriptString, args[1] as ScriptBoolean, null);
                case 3:
                    return CreateStatement(args[0] as ScriptString, args[1] as ScriptBoolean, args[2] as IScriptCodeElement<ScriptCodeExpression>);
                default:
                    return null;
            }
        }
    }
}
