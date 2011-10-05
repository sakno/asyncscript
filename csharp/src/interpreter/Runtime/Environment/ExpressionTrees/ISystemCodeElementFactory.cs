using System;

namespace DynamicScript.Runtime.Environment.ExpressionTrees
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;
    using CodeObject = System.CodeDom.CodeObject;
    using ISyntaxTreeNode = Compiler.Ast.ISyntaxTreeNode;

    [ComVisible(false)]
    interface ISystemCodeElementFactory<out TCodeObject, out TRuntimeElement>: ICodeElementFactorySlots, IScriptCodeElementFactory<TCodeObject, TRuntimeElement>
        where TCodeObject : CodeObject, ISyntaxTreeNode
        where TRuntimeElement: IScriptCodeElement<TCodeObject>
    {
        void Clear();
    }
}
