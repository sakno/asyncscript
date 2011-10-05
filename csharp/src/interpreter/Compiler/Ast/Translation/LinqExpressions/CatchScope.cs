using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace DynamicScript.Compiler.Ast.Translation.LinqExpressions
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;
    using IRuntimeSlot = Runtime.IRuntimeSlot;

    [ComVisible(false)]
    sealed class CatchScope: GenericScope
    {
        private CatchScope(LexicalScope parent)
            : base(parent, options:ScopeOptions.InheritedState | ScopeOptions.InheritedLabels)
        {
        }

        public static new CatchScope Create(LexicalScope parent)
        {
            return new CatchScope(parent);
        }
    }
}
