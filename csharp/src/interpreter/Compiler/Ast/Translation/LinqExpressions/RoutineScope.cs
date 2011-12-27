using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace DynamicScript.Compiler.Ast.Translation.LinqExpressions
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;
    using Enumerable = System.Linq.Enumerable;
    using IStaticRuntimeSlot = Runtime.IStaticRuntimeSlot;

    /// <summary>
    /// Represents an abstract lexical scope that represents a block of executable scope.
    /// </summary>
    [ComVisible(false)]
    abstract class RoutineScope : LexicalScope
    {
        /// <summary>
        /// Represents a dictionary of local variables.
        /// </summary>
        private readonly IScopeVariables m_locals;

        /// <summary>
        /// Initializes a new scope.
        /// </summary>
        /// <param name="parent">The parent scope.</param>
        protected RoutineScope(LexicalScope parent)
            : base(parent, ScopeOptions.None)
        {
            m_locals = CreateVariableTable();
        }

        /// <summary>
        /// Gets a collection of local variables.
        /// </summary>
        public IScopeVariables Locals
        {
            get { return m_locals; }
        }

        /// <summary>
        /// Gets a collection of subroutine parameters.
        /// </summary>
        public virtual IScopeVariables Parameters
        {
            get { return EmptyScopeVariables.Instance; }
        }

        /// <summary>
        /// Gets a collection of scope variables(locals and parameters).
        /// </summary>
        protected sealed override IEnumerable<string> Variables
        {
            get { return Enumerable.Select(Enumerable.Concat(Locals, Parameters), p => p.Key); }
        }

        public sealed override ParameterExpression this[string variableName]
        {
            get 
            {
                var result = default(ParameterExpression);
                if (Locals.TryGetValue(variableName, out result))
                    return result;
                else if (Parameters.TryGetValue(variableName, out result))
                    return result;
                else return Parent != null ? Parent[variableName] : null;
            }
        }

        protected bool DeclareParameter<T>(string parameterName, out ParameterExpression declaration)
        {
            switch (Locals.ContainsKey(parameterName) || Parameters.ContainsKey(parameterName))
            {
                case true:
                    declaration = null;
                    return false;
                default:
                    
                    Parameters.Add(parameterName, declaration = Expression.Parameter(typeof(IStaticRuntimeSlot), parameterName));
                    return true;
            }
        }

        protected sealed override bool DeclareVariable<T>(string variableName, out ParameterExpression declaration)
        {
            switch (Locals.ContainsKey(variableName) || Parameters.ContainsKey(variableName))
            {
                case true:
                    declaration = null;
                    return false;
                default:
                    Locals.Add(variableName, declaration = Expression.Variable(typeof(IStaticRuntimeSlot), variableName));
                    return true;
            }
        }
    }
}
