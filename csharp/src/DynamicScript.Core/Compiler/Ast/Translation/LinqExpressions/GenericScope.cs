using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace DynamicScript.Compiler.Ast.Translation.LinqExpressions
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;
    using IStaticRuntimeSlot = Runtime.IStaticRuntimeSlot;
    using IScriptObject = Runtime.IScriptObject;

    [ComVisible(false)]
    class GenericScope: LexicalScope
    {
        private const ScopeOptions DefaultOptions = ScopeOptions.InheritedState;
        private const Type DefaultEndOfScope = null;
        public readonly IScopeVariables Locals;

        protected GenericScope(LexicalScope parent, Type endOfScope = DefaultEndOfScope, ScopeOptions options = DefaultOptions)
            : base(parent, endOfScope ?? typeof(IScriptObject), options)
        {
            Locals = CreateVariableTable();
        }

        /// <summary>
        /// Initializes a new generic scope.
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="inheritedState"></param>
        public GenericScope(LexicalScope parent, bool inheritedState)
            : this(parent, options: inheritedState ? ScopeOptions.InheritedState : ScopeOptions.None)
        {
        }

        protected sealed override IEnumerable<string> Variables
        {
            get { return Locals.Keys; }
        }

        public sealed override ScopeVariable this[string variableName]
        {
            get
            {
                var result = default(ScopeVariable);
                if (Locals.TryGetValue(variableName, out result))
                    return result;
                else if (Parent != null) return Parent[variableName];
                else return null;
            }
        }

        protected sealed override bool DeclareVariable<T>(string variableName, out ParameterExpression declaration, params object[] attributes)
        {
            switch (Locals.ContainsKey(variableName))
            {
                case true:
                    declaration = null;
                    return false;
                default:
                    Locals.Add(variableName, new ScopeVariable(declaration = Expression.Parameter(typeof(IStaticRuntimeSlot), variableName), attributes));
                    return true;
            }
        }
    }
}
