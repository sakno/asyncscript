using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DynamicScript.Compiler.Ast
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;

    /// <summary>
    /// Represents an abstract class for built-in types.
    /// </summary>
    [Serializable]
    [ComVisible(false)]
    public abstract class ScriptCodeBuiltInContractExpression: ScriptCodePrimitiveExpression
    {
        internal ScriptCodeBuiltInContractExpression(Token typeName)
        {
            Value = typeName;
        }

        internal new Token Value
        {
            get { return (Token)base.Value; }
            private set { base.Value = value; }
        }

        /// <summary>
        /// Converts expression to the statement.
        /// </summary>
        /// <param name="expr">The expression to be converted.</param>
        /// <returns>The statement that encapsulates the expression.</returns>
        public static explicit operator ScriptCodeExpressionStatement(ScriptCodeBuiltInContractExpression expr)
        {
            return new ScriptCodeExpressionStatement<ScriptCodeBuiltInContractExpression>(expr);
        }
    }
}
