using System;

namespace DynamicScript.Compiler.Ast.Translation.LinqExpressions
{
    sealed class TryScope: GenericScope
    {
        private TryScope(LexicalScope parent)
            : base(parent, options: ScopeOptions.InheritedState)
        {
            if (parent == null) throw new ArgumentNullException("parent");
        }

        public static TryScope Create(LexicalScope parent)
        {
            return new TryScope(parent);
        }
    }
}
