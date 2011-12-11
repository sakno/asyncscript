using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Linq;

namespace DynamicScript.Compiler.Ast.Translation.LinqExpressions
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;
    using ScriptCompositeObject = Runtime.Environment.ScriptCompositeObject;
    using LinqExpression = System.Linq.Expressions.Expression;

    [ComVisible(false)]
    sealed class ObjectScope: LexicalScope
    {
        private readonly ParameterExpression m_object;
        public readonly ScriptCodeObjectExpression Expression;

        private ObjectScope(ScriptCodeObjectExpression definition, LexicalScope parent)
            : base(parent, options: ScopeOptions.None)
        {
            if (parent == null) throw new ArgumentNullException("parent");
            if (definition == null) throw new ArgumentNullException("definition");
            m_object = LinqExpression.Parameter(typeof(ScriptCompositeObject));
            Expression = definition;
        }

        public static ISet<string> CreateSlotSet()
        {
            return new HashSet<string>(new StringEqualityComparer());
        }

        public static ObjectScope Create(ScriptCodeObjectExpression def, LexicalScope parent)
        {
            return new ObjectScope(def, parent);
        }

        public override Expression ScopeVar
        {
            get
            {
                return m_object;
            }
        }

        protected override IEnumerable<string> Variables
        {
            get { return Enumerable.Empty<string>(); }
        }

        public override ParameterExpression this[string variableName]
        {
            get { return null; }
        }

        protected override bool DeclareVariable<T>(string variableName, out ParameterExpression declaration)
        {
            declaration = null;
            return false;
        }
    }
}
