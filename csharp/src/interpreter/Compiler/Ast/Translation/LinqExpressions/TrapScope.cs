using System;

namespace DynamicScript.Compiler.Ast.Translation.LinqExpressions
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;

    [ComVisible(false)]
    sealed class TrapScope: GenericScope
    {
        private TrapScope(LexicalScope parent)
            : base(parent, options: ScopeOptions.InheritedState)
        {
        }

        public static new TrapScope Create(LexicalScope parent)
        {
            return new TrapScope(parent);
        }
    }
}
