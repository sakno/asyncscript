using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace DynamicScript.Compiler.Ast.Translation.LinqExpressions
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;
    using IRuntimeSlot = Runtime.IRuntimeSlot;
    using IScriptObject = Runtime.IScriptObject;

    [ComVisible(false)]
    class GenericScope: LexicalScope
    {
        private const ScopeOptions DefaultOptions = ScopeOptions.InheritedState;
        public readonly IScopeVariables Locals;

        protected GenericScope(LexicalScope parent, Type endOfScope = null, ScopeOptions options = DefaultOptions)
            : base(parent, endOfScope ?? typeof(IScriptObject), options)
        {
            Locals = CreateVariableTable();
        }

        public static GenericScope Create(LexicalScope parent, bool inheritedState)
        {
            return new GenericScope(parent, options: inheritedState ? ScopeOptions.InheritedState : ScopeOptions.None);
        }

        public static GenericScope Create(LexicalScope parent)
        {
            return Create(parent, true);
        }

        protected sealed override IEnumerable<string> Variables
        {
            get { return Locals.Keys; }
        }

        public sealed override ParameterExpression this[string variableName]
        {
            get
            {
                var result = default(ParameterExpression);
                if (Locals.TryGetValue(variableName, out result))
                    return result;
                else if (Parent != null) return Parent[variableName];
                else return null;
            }
        }

        protected sealed override bool DeclareVariable<T>(string variableName, out ParameterExpression declaration)
        {
            switch (Locals.ContainsKey(variableName))
            {
                case true:
                    declaration = null;
                    return false;
                default:
                    Locals.Add(variableName, declaration = Expression.Parameter(typeof(IRuntimeSlot), variableName));
                    return true;
            }
        }
    }
}
