using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Linq;

namespace DynamicScript.Compiler.Ast.Translation.LinqExpressions
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;

    [ComVisible(false)]
    sealed class ForLoopScope: LoopScope
    {
        public ForLoopScope(LexicalScope parent, bool singleResult, bool suppressCollection = false)
            : base(parent, singleResult, suppressCollection)
        {
        }
    }
}
