using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace DynamicScript.Compiler.Ast.Translation.LinqExpressions
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;

    [ComVisible(false)]
    sealed class ContextScope : GenericScope
    {
        public readonly InterpretationContext Context;

        private ContextScope(LexicalScope parent, InterpretationContext context)
            : base(parent)
        {
            if (parent == null) throw new ArgumentNullException("parent");
            Context = context;
        }

        public static ContextScope Create(LexicalScope parent, InterpretationContext context)
        {
            return new ContextScope(parent, context);
        }
    }
}
