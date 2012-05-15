using System;

namespace DynamicScript.Compiler.Ast
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;
    using CodeExpressionCollection = System.CodeDom.CodeExpressionCollection;

    /// <summary>
    /// Represents an abstract class that represents flow control instruction.
    /// </summary>
    [ComVisible(false)]
    interface ICodeFlowControlInstruction: ISyntaxTreeNode
    {
        /// <summary>
        /// Gets argument list.
        /// </summary>
        ScriptCodeExpressionCollection ArgList
        {
            get;
        }
    }
}
