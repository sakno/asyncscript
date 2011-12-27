using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace DynamicScript.Runtime.Environment.ExpressionTrees
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;
    using ScriptCodeVariableReference = Compiler.Ast.ScriptCodeVariableReference;

    /// <summary>
    /// Represents name token.
    /// This class cannot be inherited.
    /// </summary>
    [ComVisible(false)]
    sealed class ScriptNameTokenExpression : ScriptExpression<ScriptCodeVariableReference, IScriptObject>
    {
        private ScriptNameTokenExpression(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        public ScriptNameTokenExpression(ScriptCodeVariableReference name)
            : base(name, ScriptNameTokenExpressionFactory.Instance)
        {
        }

        public ScriptNameTokenExpression(string name)
            : this(new ScriptCodeVariableReference { VariableName = name })
        {
        }

        /// <summary>
        /// Compiles name token to its string representation.
        /// </summary>
        /// <param name="state">Internal interpreter state.</param>
        /// <returns></returns>
        public override IScriptObject Compile(InterpreterState state)
        {
            return state.Global[Expression.VariableName, state] ?? Void;
        }

        /// <summary>
        /// Creates a new variable reference.
        /// </summary>
        /// <param name="variableName">The name of the variable.</param>
        /// <returns></returns>
        public static ScriptCodeVariableReference CreateExpression(ScriptString variableName)
        {
            return variableName != null ? new ScriptCodeVariableReference { VariableName = variableName } : null;
        }

        /// <summary>
        /// Creates a new variable reference.
        /// </summary>
        /// <param name="args"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        protected override ScriptCodeVariableReference CreateExpression(IList<IScriptObject> args, InterpreterState state)
        {
            return args.Count == 1 ? CreateExpression(args[0] as ScriptString) : null;
        }
    }
}
