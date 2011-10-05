using System;

namespace DynamicScript.Compiler.Ast.Translation.LinqExpressions
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;
    using IScriptObject = Runtime.IScriptObject;

    [ComVisible(false)]
    sealed class SelectionScope: GenericScope
    {
        private SelectionScope(LexicalScope parent)
            : base(parent, options: ScopeOptions.InheritedState)
        {
            if (parent == null) throw new ArgumentNullException("parent");
        }

        public static new SelectionScope Create(LexicalScope parent)
        {
            return new SelectionScope(parent);
        }
    }
}
