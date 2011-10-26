using System;
using System.Linq.Expressions;
using System.Collections.Generic;
using System.Linq;

namespace DynamicScript.Compiler.Ast.Translation.LinqExpressions
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;
    using IScriptObject = Runtime.IScriptObject;
    using RuntimeHelpers = Runtime.Environment.RuntimeHelpers;
    using MethodInfo = System.Reflection.MethodInfo;
    using ScriptList = Runtime.Environment.ScriptList;

    [ComVisible(false)]
    abstract class LoopScope: GenericScope
    {
        private readonly ParameterExpression m_result;
        private readonly ParameterExpression m_continue;
        private readonly bool m_suppressCollection;

        protected LoopScope(LexicalScope parent, bool singleResult, bool suppressCollection = false)
            : base(parent, typeof(void), ScopeOptions.InheritedState)
        {
            if (parent == null) throw new ArgumentNullException("parent");
            m_result = singleResult ? Expression.Variable(typeof(IScriptObject), "$current") : Expression.Variable(typeof(ScriptList), "$result");
            m_continue = Expression.Variable(typeof(bool), "$continue");
            EmitContinueFlag = false;
            m_suppressCollection = suppressCollection;
        }

        /// <summary>
        /// Gets loop continuation flag.
        /// </summary>
        public ParameterExpression ContinueFlag
        {
            get { return m_continue; }
        }

        /// <summary>
        /// Sets that the translator should emit loop continuation flag.
        /// </summary>
        /// <remarks>This property is used for optimization purposes.</remarks>
        public bool EmitContinueFlag
        {
            private set;
            get;
        }

        private bool SupressCollection
        {
            get { return m_suppressCollection; }
        }

        /// <summary>
        /// Gets a variable that holds result of the loop expression.
        /// </summary>
        public ParameterExpression Result
        {
            get { return m_result; }
        }

        private bool SingleResult
        {
            get { return Equals(Result.Type, typeof(IScriptObject)); }
        }

        public Expression Break(IEnumerable<Expression> arguments)
        {
            EmitContinueFlag = true;
            switch (SingleResult)
            {
                case true:
                    var last = arguments.FirstOrDefault();
                    return last != null ?
                        Expression.Block(Expression.Assign(Result, last), Expression.Assign(ContinueFlag, Expression.Constant(false)), Expression.Break(BeginOfScope)) :
                        Expression.Block(Expression.Assign(ContinueFlag, Expression.Constant(false)), Expression.Break(BeginOfScope));
                default:
                    return Expression.Block(Expression.Assign(ContinueFlag, Expression.Constant(false)), SupressCollection ? Expression.Empty() : ScriptList.Add(Result, arguments, StateHolder), Expression.Goto(BeginOfScope));
            }
        }

        public Expression Continue(IEnumerable<Expression> arguments)
        {
            switch (SingleResult)
            {
                case true:
                    var last = arguments.FirstOrDefault();
                    return last != null ? (Expression)Expression.Block(Expression.Assign(Result, last), Expression.Continue(BeginOfScope)) : Expression.Continue(BeginOfScope);
                default:
                    return SupressCollection ? (Expression)Expression.Goto(BeginOfScope) : Expression.Block(ScriptList.Add(Result, arguments, StateHolder), Expression.Goto(BeginOfScope));
            }
        }
    }
}
