using System;
using System.Linq.Expressions;

namespace DynamicScript.Compiler.Ast.Translation.LinqExpressions
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;
    using IScriptObject = Runtime.IScriptObject;

    [ComVisible(false)]
    sealed class SynchronizerScope : RoutineScope
    {
        private readonly ParameterExpression m_scopeVar;

        private SynchronizerScope(LexicalScope parent)
            : base(parent)
        {
            m_scopeVar = Expression.Parameter(typeof(IScriptObject));
        }

        public static SynchronizerScope Create(LexicalScope parent)
        {
            return new SynchronizerScope(parent);
        }

        public override Expression ScopeVar
        {
            get { return m_scopeVar; }
        }
    }
}
