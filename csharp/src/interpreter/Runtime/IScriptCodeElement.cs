using System;
using System.Collections.Generic;

namespace DynamicScript.Runtime
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;
    using CodeObject = System.CodeDom.CodeObject;
    using ISyntaxTreeNode = Compiler.Ast.ISyntaxTreeNode;

    /// <summary>
    /// Represents runtime representation of code element.
    /// </summary>
    [ComVisible(false)]
    public interface IScriptCodeElement<out TCodeObject> : IScriptObject
        where TCodeObject : CodeObject, ISyntaxTreeNode
    {
        /// <summary>
        /// Modifies the current expression.
        /// </summary>
        /// <param name="args">An arguments used to modify this expression.</param>
        /// <param name="state">Internal interpreter state.</param>
        /// <returns><see langword="true"/> if the current expression is modified successfully; otherwise, <see langword="false"/>.</returns>
        bool Modify(IList<IScriptObject> args, InterpreterState state);

        /// <summary>
        /// Gets code element associated with this runtime representation.
        /// </summary>
        TCodeObject CodeObject { get; }
    }
}
