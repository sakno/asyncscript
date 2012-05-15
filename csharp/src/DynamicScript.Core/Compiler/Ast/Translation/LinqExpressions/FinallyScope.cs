using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace DynamicScript.Compiler.Ast.Translation.LinqExpressions
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;

    [ComVisible(false)]
    sealed class FinallyScope : GenericScope
    {
        private FinallyScope(LexicalScope parent)
            : base(parent, options: ScopeOptions.InheritedState)
        {
            if (parent == null) throw new ArgumentNullException("parent");
        }

        public static FinallyScope Create(LexicalScope parent)
        {
            return new FinallyScope(parent);
        }
    }
}
