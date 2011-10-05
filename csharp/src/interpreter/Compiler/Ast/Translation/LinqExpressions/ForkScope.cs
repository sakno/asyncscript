using System;
using System.Linq.Expressions;

namespace DynamicScript.Compiler.Ast.Translation.LinqExpressions
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;
    using IScriptObject = Runtime.IScriptObject;

    [ComVisible(false)]
    sealed class ForkScope : RoutineScope
    {
        private readonly ParameterExpression m_scopeVar;

        private ForkScope(LexicalScope parent)
            : base(parent)
        {
            m_scopeVar = Expression.Parameter(typeof(IScriptObject));
        }

        public static ForkScope Create(LexicalScope parent)
        {
            return new ForkScope(parent);
        }

        public override Expression ScopeVar
        {
            get { return m_scopeVar; }
        }
    }
}
