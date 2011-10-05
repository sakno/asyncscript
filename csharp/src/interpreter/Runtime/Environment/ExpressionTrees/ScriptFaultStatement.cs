using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace DynamicScript.Runtime.Environment.ExpressionTrees
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;
    using Compiler.Ast;
    using LinqExpressionTranslator = Compiler.Ast.Translation.LinqExpressions.LinqExpressionTranslator;

    /// <summary>
    /// Represents runtime representation of the FAULT statement.
    /// This class cannot be inherited.
    /// </summary>
    [ComVisible(false)]
    [Serializable]
    sealed class ScriptFaultStatement : ScriptStatement<ScriptCodeFaultStatement>
    {
        private ScriptFaultStatement(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        public ScriptFaultStatement(ScriptCodeFaultStatement fault)
            : base(fault, ScriptFaultStatementFactory.Instance)
        {
        }

        /// <summary>
        /// Creates a new FAULT statement.
        /// </summary>
        /// <param name="faultObj"></param>
        /// <returns></returns>
        public static ScriptCodeFaultStatement CreateStatement(IScriptObject faultObj)
        {
            return new ScriptCodeFaultStatement
            {
                Error = faultObj is IScriptExpression<ScriptCodeExpression> ?
                ((IScriptExpression<ScriptCodeExpression>)faultObj).CodeObject :
                ScriptConstantExpression.CreateExpression(faultObj)
            };
        }

        public static bool Execute(IScriptStatement<ScriptCodeFaultStatement> faultStmt, IScriptCompositeObject obj, InterpreterState state)
        {
            if (faultStmt == null || state == null) return false;
            else if (obj != null)
            {
                DynamicScriptInterpreter.Run(faultStmt.CodeObject, state.Update(obj));
                return true;
            }
            else
            {
                var error = Convert(faultStmt.CodeObject.Error) as IScriptExpression<ScriptCodeExpression> ?? new ScriptConstantExpression(ScriptCodeVoidExpression.Instance);
                throw new ScriptFault(error.Compile(state), state);
            }
        }

        public override bool Execute(IList<IScriptObject> args, InterpreterState state)
        {
            switch (args.Count)
            {
                case 0:
                    return Execute(this, null, state);
                case 1:
                    return Execute(this, args[0] as IScriptCompositeObject, state);
                default: return false;
            }
        }

        protected override ScriptCodeFaultStatement CreateStatement(IList<IScriptObject> args, InterpreterState state)
        {
            return args.Count == 1 ? CreateStatement(args[0]) : null;
        }
    }
}
