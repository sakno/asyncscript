using System;
using System.Collections.Generic;

namespace DynamicScript.Runtime
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;
    using CodeObject = System.CodeDom.CodeObject;
    using ISyntaxTreeNode = Compiler.Ast.ISyntaxTreeNode;

    /// <summary>
    /// Represents factory of runtime code element.
    /// </summary>
    /// <typeparam name="TCodeObject">Type of the code element.</typeparam>
    /// <typeparam name="TRuntimeElement">Type of the runtime representation of the code element to be produced by this factory.</typeparam>
    [ComVisible(false)]
    public interface IScriptCodeElementFactory<out TCodeObject, out TRuntimeElement>: IScriptContract
        where TCodeObject: CodeObject, ISyntaxTreeNode
        where TRuntimeElement: IScriptCodeElement<TCodeObject>
    {
        /// <summary>
        /// Creates a new runtime representation of the code element.
        /// </summary>
        /// <param name="args"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        TRuntimeElement CreateCodeElement(IList<IScriptObject> args, InterpreterState state);
    }
}
