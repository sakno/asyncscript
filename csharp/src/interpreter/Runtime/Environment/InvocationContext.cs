using System;
using System.Linq.Expressions;
using System.Reflection;
using System.ComponentModel;

namespace DynamicScript.Runtime.Environment
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;
    using CriticalFinalizerObject = System.Runtime.ConstrainedExecution.CriticalFinalizerObject;
    using ImmutableObjectAttribute = System.ComponentModel.ImmutableObjectAttribute;
    using CallInfo = System.Dynamic.CallInfo;

    /// <summary>
    /// Represents runtime state of the action during invocation.
    /// This class cannot be inherited.
    /// </summary>
    /// <remarks>
    /// This class encapsulates internal interpreter state, 'this' reference and other sensitive data required
    /// for action invocation.
    /// </remarks>
    [ComVisible(false)]
    [ImmutableObject(true)]
    public sealed class InvocationContext: CriticalFinalizerObject
    {
        /// <summary>
        /// Represents runtime state associated with this invocation context.
        /// </summary>
        public readonly InterpreterState RuntimeState;

        /// <summary>
        /// Represents an action that is invoked inside of this context.
        /// </summary>
        public readonly IScriptAction Action;

        internal InvocationContext(IScriptAction action, InterpreterState state)
        {
            if (action == null) throw new ArgumentNullException("action");
            if (state == null) throw new ArgumentNullException("state");
            RuntimeState = state;
            Action = action;
        }

        /// <summary>
        /// Gets global object passed to the script.
        /// </summary>
        public IScriptObject Global
        {
            get { return RuntimeState.Global; }
        }

        /// <summary>
        /// Gets contract of the returning value.
        /// </summary>
        /// <remarks>It can be <see langword="null"/> if action doesn't return any value(void).</remarks>
        public IScriptContract ReturnValueContract
        {
            get { return Action.ReturnValueContract; }
        }

        /// <summary>
        /// Gets action owner.
        /// </summary>
        public IScriptObject This
        {
            get { return Action.This; }
        }

        internal static PropertyInfo ThisProperty
        {
            get
            {
                return (PropertyInfo)LinqHelpers.BodyOf<InvocationContext, IScriptObject, MemberExpression>(ctx => ctx.This).Member;
            }
        }

        internal static FieldInfo StateField
        {
            get
            {
                return (FieldInfo)LinqHelpers.BodyOf<InvocationContext, InterpreterState, MemberExpression>(ctx => ctx.RuntimeState).Member;
            }
        }

        private static FieldInfo ActionField
        {
            get
            {
                return (FieldInfo)LinqHelpers.BodyOf<InvocationContext, IScriptAction, MemberExpression>(ctx => ctx.Action).Member;
            }
        }

        internal static MemberExpression BindToCurrentAction(ParameterExpression contextVar)
        {
            return Expression.Field(contextVar, ActionField);
        }
    }
}
