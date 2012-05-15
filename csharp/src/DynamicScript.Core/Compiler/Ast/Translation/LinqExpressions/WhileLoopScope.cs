using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace DynamicScript.Compiler.Ast.Translation.LinqExpressions
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;

    [ComVisible(false)]
    sealed class WhileLoopScope : LoopScope
    {
        public WhileLoopScope(LexicalScope parent, bool singleResult, bool suppressCollection = false)
            : base(parent, singleResult, suppressCollection)
        {
            if (parent == null) throw new ArgumentNullException("parent");
        }
    }
}
