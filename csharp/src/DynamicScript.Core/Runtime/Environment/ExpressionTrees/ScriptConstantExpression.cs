using System;
using System.Runtime.Serialization;
using System.Collections.Generic;

namespace DynamicScript.Runtime.Environment.ExpressionTrees
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;
    using Compiler.Ast;

    /// <summary>
    /// Represents runtime representation of constant expression.
    /// This class cannot be inherited.
    /// </summary>
    [ComVisible(false)]
    [Serializable]
    sealed class ScriptConstantExpression : ScriptExpression<ScriptCodePrimitiveExpression, ScriptObject>
    {
        private ScriptConstantExpression(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        public ScriptConstantExpression(ScriptCodePrimitiveExpression expression)
            : base(expression, ScriptConstantExpressionFactory.Instance)
        {
        }

        /// <summary>
        /// Compiles constant expression.
        /// </summary>
        /// <param name="state">Internal interpreter state.</param>
        /// <returns>Compiled constant expression.</returns>
        public override ScriptObject Compile(InterpreterState state)
        {
            if (Expression is ILiteralExpression<ScriptCodeExpression>)
                return Convert(((ILiteralExpression<ScriptCodeExpression>)Expression).Value, Void) as ScriptObject ?? Void;
            else if (Expression is ScriptCodeIntegerContractExpression)
                return ScriptIntegerContract.Instance;
            else if (Expression is ScriptCodeRealContractExpression)
                return ScriptRealContract.Instance;
            else if (Expression is ScriptCodeStringContractExpression)
                return ScriptStringContract.Instance;
            else if (Expression is ScriptCodeBooleanContractExpression)
                return ScriptBooleanContract.Instance;
            else if (Expression is ScriptCodeExpressionContractExpression)
                return ScriptExpressionFactory.Instance;
            else if (Expression is ScriptCodeStatementContractExpression)
                return ScriptStatementFactory.Instance;
            else if (Expression is ScriptCodeDimensionalContractExpression)
                return ScriptDimensionalContract.Instance;
            else if (Expression is ScriptCodeMetaContractExpression)
                return ScriptMetaContract.Instance;
            else if (Expression is ScriptCodeSuperContractExpression)
                return ScriptSuperContract.Instance;
            else if (Expression is ScriptCodeCallableContractExpression)
                return ScriptCallableContract.Instance;
            else return Void;
        }

        /// <summary>
        /// Gets a value indicating that this constant references a built-in contract.
        /// </summary>
        public bool IsBuiltInContract
        {
            get { return Expression is ScriptCodeBuiltInContractExpression; }
        }

        public static ScriptCodePrimitiveExpression CreateExpression(IScriptObject value)
        {
            return CreatePrimitiveExpression(value);
        }

        protected override ScriptCodePrimitiveExpression CreateExpression(IList<IScriptObject> args, InterpreterState state)
        {
            return args.Count == 0 ? CreateExpression(args[0]) : null;
        }
    }
}
