using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace DynamicScript.Compiler.Ast.Translation.LinqExpressions
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;

    [ComVisible(false)]
    sealed class IfElseBranchScope : GenericScope
    {
        private IfElseBranchScope(LexicalScope parent)
            : base(parent, options: ScopeOptions.InheritedState)
        {
            if (parent == null) throw new ArgumentNullException("parent");
        }

        public static new IfElseBranchScope Create(LexicalScope parent)
        {
            return new IfElseBranchScope(parent);
        }
    }
}
