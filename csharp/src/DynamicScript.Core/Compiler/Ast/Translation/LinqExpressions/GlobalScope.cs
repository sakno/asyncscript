using System;
using System.Linq.Expressions;

namespace DynamicScript.Compiler.Ast.Translation.LinqExpressions
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;
    using IScriptObject = Runtime.IScriptObject;
    using InterpreterState = Runtime.InterpreterState;

    /// <summary>
    /// Represents a base class for root lexical scope.
    /// </summary>
    [ComVisible(false)]
    sealed class GlobalScope : RoutineScope
    {
        /// <summary>
        /// Initializes a new scope.
        /// </summary>
        public GlobalScope()
            : base(null)
        {
        }

        public static MemberExpression GetGlobal(LexicalScope scope)
        {
            return InterpreterState.GlobalGetterExpression(scope.StateHolder);
        }

        public override Expression ScopeVar
        {
            get { return GetGlobal(this); }
        }
    }
}
