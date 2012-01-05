using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;

namespace DynamicScript.Compiler.Ast.Translation.LinqExpressions
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;
    using InvocationContext = Runtime.Environment.InvocationContext;
    using LinqExpression = System.Linq.Expressions.Expression;
    using IStaticRuntimeSlot = Runtime.IStaticRuntimeSlot;

    /// <summary>
    /// Represents lexical scope that represents location inside of the 
    /// action implementation. This class cannot be inherited.
    /// </summary>
    [ComVisible(false)]
    sealed class FunctionScope : RoutineScope, IActionLexicalScope
    {
        public readonly ScriptCodeActionImplementationExpression Expression;
        private readonly IScopeVariables m_parameters;

        /// <summary>
        /// Initializes a new action scope.
        /// </summary>
        /// <param name="parent">The parent lexical scope. Cannot be <see langword="null"/>.</param>
        /// <param name="expression">An expression associated with this scope. Cannot be <see langword="null"/>.</param>
        /// <exception cref="System.ArgumentNullException"><paramref name="parent"/> or <paramref name="expression"/> is <see langword="null"/>.</exception>
        private FunctionScope(LexicalScope parent, ScriptCodeActionImplementationExpression expression)
            : base(parent)
        {
            if (parent == null) throw new ArgumentNullException("parent");
            if (expression == null) throw new ArgumentNullException("expression");
            Expression = expression;
            m_parameters = CreateVariableTable();
        }

        public static FunctionScope Create(LexicalScope parent, ScriptCodeActionImplementationExpression expression)
        {
            return new FunctionScope(parent, expression);
        }

        /// <summary>
        /// Gets an expression that is used to obtain current action.
        /// </summary>
        public Expression CurrentAction
        {
            get { return InvocationContext.ActionRef; }
        }

        /// <summary>
        /// Gets a dictionary of parameters
        /// </summary>
        public override IScopeVariables Parameters
        {
            get { return m_parameters; }
        }

        /// <summary>
        /// Declares a new parameter.
        /// </summary>
        /// <param name="parameterName"></param>
        /// <param name="declaration"></param>
        /// <param name="attributes"></param>
        /// <returns></returns>
        public bool DeclareParameter(string parameterName, out ParameterExpression declaration, params object[] attributes)
        {
            return DeclareParameter<IStaticRuntimeSlot>(parameterName, out declaration);
        }

        /// <summary>
        /// Gets a reference to 'this' object.
        /// </summary>
        public override Expression ScopeVar
        {
            get { return InvocationContext.ThisRef; }
        }

        ScriptCodeActionImplementationExpression IComplexExpressionScope<ScriptCodeActionImplementationExpression>.Expression
        {
            get { return Expression; }
        }
    }
}
