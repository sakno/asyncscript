using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace DynamicScript.Runtime.Environment.ExpressionTrees
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;
    using Compiler.Ast;

    /// <summary>
    /// Represents runtime representation of the RETURN statement.
    /// This class cannot be inherited.
    /// </summary>
    [ComVisible(false)]
    [Serializable]
    sealed class ScriptReturnStatement: ScriptStatement<ScriptCodeReturnStatement>
    {
        private ScriptReturnStatement(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        public ScriptReturnStatement(ScriptCodeReturnStatement statement)
            : base(statement, ScriptReturnStatementFactory.Instance)
        {
        }

        /// <summary>
        /// Creates a new RETURN statement.
        /// </summary>
        /// <param name="retObj"></param>
        /// <returns></returns>
        public static ScriptCodeReturnStatement CreateStatement(IScriptObject retObj = null)
        {
            if (retObj is IScriptExpression<ScriptCodeExpression>)
                return new ScriptCodeReturnStatement { Value = ((IScriptExpression<ScriptCodeExpression>)retObj).CodeObject };
            else if (retObj == null)
                return new ScriptCodeReturnStatement();
            else return new ScriptCodeReturnStatement { Value = ScriptConstantExpression.CreateExpression(retObj) };
        }

        protected override ScriptCodeReturnStatement CreateStatement(IList<IScriptObject> args, InterpreterState state)
        {
            switch (args.Count)
            {
                case 0:
                    return CreateStatement();
                case 1:
                    return CreateStatement(args[0]);
                default:
                    return null;
            }
        }

        public override bool Execute(IList<IScriptObject> args, InterpreterState state)
        {
            return false;
        }
    }
}
