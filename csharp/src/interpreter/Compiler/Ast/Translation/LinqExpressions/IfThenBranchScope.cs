using System;

namespace DynamicScript.Compiler.Ast.Translation.LinqExpressions
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;

    [ComVisible(false)]
    sealed class IfThenBranchScope : GenericScope
    {
        private IfThenBranchScope(LexicalScope parent)
            : base(parent, options: ScopeOptions.InheritedState)
        {
            if (parent == null) throw new ArgumentNullException("parent");
        }

        public static new IfThenBranchScope Create(LexicalScope parent)
        {
            return new IfThenBranchScope(parent);
        }
    }
}
