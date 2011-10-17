using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Linq;

namespace DynamicScript.Runtime.Environment
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;
    using MethodInfo = System.Reflection.MethodInfo;
    using Closure = System.Runtime.CompilerServices.Closure;

    /// <summary>
    /// Represents script-side implementation of the action.
    /// This class cannot be inherited.
    /// </summary>
    [ComVisible(false)]
    public class ScriptRuntimeAction : ScriptActionBase
    {
        private readonly Delegate m_implementation;

        /// <summary>
        /// Initializes a new script action.
        /// </summary>
        /// <param name="contract">The action signature. Cannot be <see langword="null"/>.</param>
        /// <param name="this">An action owner.</param>
        /// <param name="implementation">An implementation of the action. Cannot be <see langword="null"/>.</param>
        /// <exception cref="System.ArgumentNullException"><paramref name="contract"/> or <paramref name="implementation"/> is <see langword="null"/>.</exception>
        /// <remarks>
        /// An implementation should have the following signature:
        /// IScriptObject M(InvocationContext ctx[, IRuntimeSlot arg0, IRuntimeSlot arg1,...]);
        /// </remarks>
        public ScriptRuntimeAction(ScriptActionContract contract, IScriptObject @this, Delegate implementation)
            : base(contract, @this)
        {
            if (implementation == null) throw new ArgumentNullException("implementation");
            m_implementation = MonoRuntime.Available && IsClosure(implementation.Method) && implementation.Target == null ?
                Delegate.CreateDelegate(implementation.GetType(),
                        new Closure(new object[0], new object[0]),
                        implementation.Method, false) :
                        implementation;
        }

        /// <summary>
        /// Initializes a new script action without return value.
        /// </summary>
        /// <param name="implementation">An implementation of the action. Cannot be <see langword="null"/></param>
        /// <param name="param0">A description of the first parameter.</param>
        /// <param name="this">An action owner.</param>
        /// <exception cref="System.ArgumentNullException"><paramref name="implementation"/> is <see langword="null"/>.</exception>
        public ScriptRuntimeAction(Action<InvocationContext, IRuntimeSlot> implementation, ScriptActionContract.Parameter param0, IScriptObject @this = null)
            : this(new ScriptActionContract(new[] { param0 }), @this, implementation)
        {
        }

        /// <summary>
        /// Initializes a new script action without return value.
        /// </summary>
        /// <param name="implementation">An implementation of the action. Cannot be <see langword="null"/></param>
        /// <param name="param0">A description of the first parameter.</param>
        /// <param name="param1">A description of the second parameter.</param>
        /// <param name="this">An action owner.</param>
        /// <exception cref="System.ArgumentNullException"><paramref name="implementation"/> is <see langword="null"/>.</exception>
        public ScriptRuntimeAction(Action<InvocationContext, IRuntimeSlot, IRuntimeSlot> implementation, ScriptActionContract.Parameter param0, ScriptActionContract.Parameter param1, IScriptObject @this = null)
            : this(new ScriptActionContract(new[] { param0, param1 }), @this, implementation)
        {
        }

        /// <summary>
        /// Initializes a new script action without return value.
        /// </summary>
        /// <param name="implementation">An implementation of the action. Cannot be <see langword="null"/>.</param>
        /// <param name="param0">A description of the first parameter.</param>
        /// <param name="param1">A description of the second parameter.</param>
        /// <param name="param2">A description of the third parameter.</param>
        /// <param name="this">An action owner.</param>
        /// <exception cref="System.ArgumentNullException"><paramref name="implementation"/> is <see langword="null"/>.</exception>
        public ScriptRuntimeAction(Action<InvocationContext, IRuntimeSlot, IRuntimeSlot, IRuntimeSlot> implementation, ScriptActionContract.Parameter param0, ScriptActionContract.Parameter param1, ScriptActionContract.Parameter param2, IScriptObject @this = null)
            : this(new ScriptActionContract(new[] { param0, param1, param2 }), @this, implementation)
        {
        }

        /// <summary>
        /// Initializes a new script action without return value.
        /// </summary>
        /// <param name="implementation">An implementation of the action. Cannot be <see langword="null"/>.</param>
        /// <param name="param0">A description of the first parameter.</param>
        /// <param name="param1">A description of the second parameter.</param>
        /// <param name="param2">A description of the third parameter.</param>
        /// <param name="param3">A description of the fourth parameter.</param>
        /// <param name="this">An action owner.</param>
        /// <exception cref="System.ArgumentNullException"><paramref name="implementation"/> is <see langword="null"/>.</exception>
        public ScriptRuntimeAction(Action<InvocationContext, IRuntimeSlot, IRuntimeSlot, IRuntimeSlot, IRuntimeSlot> implementation, ScriptActionContract.Parameter param0, ScriptActionContract.Parameter param1, ScriptActionContract.Parameter param2, ScriptActionContract.Parameter param3, IScriptObject @this = null)
            : this(new ScriptActionContract(new[] { param0, param1, param2, param3 }), @this, implementation)
        {
        }

        /// <summary>
        /// Initializes a new script action without return value.
        /// </summary>
        /// <param name="implementation">An implementation of the action. Cannot be <see langword="null"/>.</param>
        /// <param name="param0">A description of the first parameter.</param>
        /// <param name="param1">A description of the second parameter.</param>
        /// <param name="param2">A description of the third parameter.</param>
        /// <param name="param3">A description of the fourth parameter.</param>
        /// <param name="param4">A description of the fifth parameter.</param>
        /// <param name="this">An action owner.</param>
        /// <exception cref="System.ArgumentNullException"><paramref name="implementation"/> is <see langword="null"/>.</exception>
        public ScriptRuntimeAction(Action<InvocationContext, IRuntimeSlot, IRuntimeSlot, IRuntimeSlot, IRuntimeSlot, IRuntimeSlot> implementation, ScriptActionContract.Parameter param0, ScriptActionContract.Parameter param1, ScriptActionContract.Parameter param2, ScriptActionContract.Parameter param3, ScriptActionContract.Parameter param4, IScriptObject @this = null)
            : this(new ScriptActionContract(new[] { param0, param1, param2, param3, param4 }), @this, implementation)
        {
        }

        /// <summary>
        /// Initializes a new script action without return value.
        /// </summary>
        /// <param name="implementation">An implementation of the action. Cannot be <see langword="null"/>.</param>
        /// <param name="param0">A description of the first parameter.</param>
        /// <param name="param1">A description of the second parameter.</param>
        /// <param name="param2">A description of the third parameter.</param>
        /// <param name="param3">A description of the fourth parameter.</param>
        /// <param name="param4">A description of the fifth parameter.</param>
        /// <param name="param5">A description of the sixth parameter.</param>
        /// <param name="this">An action owner.</param>
        /// <exception cref="System.ArgumentNullException"><paramref name="implementation"/> is <see langword="null"/>.</exception>
        public ScriptRuntimeAction(Action<InvocationContext, IRuntimeSlot, IRuntimeSlot, IRuntimeSlot, IRuntimeSlot, IRuntimeSlot, IRuntimeSlot> implementation, ScriptActionContract.Parameter param0, ScriptActionContract.Parameter param1, ScriptActionContract.Parameter param2, ScriptActionContract.Parameter param3, ScriptActionContract.Parameter param4, ScriptActionContract.Parameter param5, IScriptObject @this = null)
            : this(new ScriptActionContract(new[] { param0, param1, param2, param3, param4, param5 }), @this, implementation)
        {
        }

        /// <summary>
        /// Initializes a new script action with return value.
        /// </summary>
        /// <param name="implementation">An implementation of the action. Cannot be <see langword="null"/>.</param>
        /// <param name="returnValue">The contract of the return value.</param>
        /// <param name="this">An action owner.</param>
        /// <exception cref="System.ArgumentNullException"><paramref name="implementation"/> is <see langword="null"/>.</exception>
        public ScriptRuntimeAction(Func<InvocationContext, IScriptObject> implementation, IScriptContract returnValue, IScriptObject @this = null)
            : this(new ScriptActionContract(new ScriptActionContract.Parameter[0], returnValue), @this, implementation)
        {
        }

        /// <summary>
        /// Initializes a new script action with return value.
        /// </summary>
        /// <param name="implementation">An implementation of the action. Cannot be <see langword="null"/>.</param>
        /// <param name="param0">A description of the first parameter.</param>
        /// <param name="returnValue">The contract of the return value.</param>
        /// <param name="this">An action owner.</param>
        /// <exception cref="System.ArgumentNullException"><paramref name="implementation"/> is <see langword="null"/>.</exception>
        public ScriptRuntimeAction(Func<InvocationContext, IRuntimeSlot, IScriptObject> implementation, ScriptActionContract.Parameter param0, IScriptContract returnValue, IScriptObject @this = null)
            : this(new ScriptActionContract(new[] { param0 }, returnValue), @this, implementation)
        {
        }

        /// <summary>
        /// Initializes a new script action with return value.
        /// </summary>
        /// <param name="implementation">An implementation of the action. Cannot be <see langword="null"/>.</param>
        /// <param name="param0">A description of the first parameter.</param>
        /// <param name="param1">A description of the second parameter.</param>
        /// <param name="returnValue">The contract of the return value.</param>
        /// <param name="this">An action owner.</param>
        /// <exception cref="System.ArgumentNullException"><paramref name="implementation"/> is <see langword="null"/>.</exception>
        public ScriptRuntimeAction(Func<InvocationContext, IRuntimeSlot, IRuntimeSlot, IScriptObject> implementation, ScriptActionContract.Parameter param0, ScriptActionContract.Parameter param1, IScriptContract returnValue, IScriptObject @this = null)
            : this(new ScriptActionContract(new[] { param0, param1 }, returnValue), @this, implementation)
        {
        }

        /// <summary>
        /// Initializes a new script action with return value.
        /// </summary>
        /// <param name="implementation">An implementation of the action. Cannot be <see langword="null"/>.</param>
        /// <param name="param0">A description of the first parameter.</param>
        /// <param name="param1">A description of the second parameter.</param>
        /// <param name="param2">A description of the third parameter.</param>
        /// <param name="returnValue">The contract of the return value.</param>
        /// <param name="this">An action owner.</param>
        /// <exception cref="System.ArgumentNullException"><paramref name="implementation"/> is <see langword="null"/>.</exception>
        public ScriptRuntimeAction(Func<InvocationContext, IRuntimeSlot, IRuntimeSlot, IRuntimeSlot, IScriptObject> implementation, ScriptActionContract.Parameter param0, ScriptActionContract.Parameter param1, ScriptActionContract.Parameter param2, IScriptContract returnValue, IScriptObject @this = null)
            : this(new ScriptActionContract(new[] { param0, param1, param2 }, returnValue), @this, implementation)
        {
        }

        internal static Expression Bind(Expression actionContract, Expression @this, LambdaExpression implementation)
        {
            actionContract = Expression.TypeAs(ScriptContract.Extract(actionContract), typeof(ScriptActionContract));
            var ctor = LinqHelpers.BodyOf<ScriptActionContract, IScriptObject, Delegate, ScriptRuntimeAction, NewExpression>((c, t, i) => new ScriptRuntimeAction(c, t, i)).Constructor;
            return Expression.New(ctor, actionContract, @this, implementation);
        }

        /// <summary>
        /// Determines whether the specified method uses a closure.
        /// </summary>
        /// <param name="m"></param>
        /// <returns></returns>
        public static bool IsClosure(MethodInfo m)
        {
            var parameters = m.GetParameters();
            return parameters.LongLength > 0L && Equals(parameters[0].ParameterType, typeof(Closure));
        }

        /// <summary>
        /// Invokes script action.
        /// </summary>
        /// <param name="ctx">Action invocation context.</param>
        /// <param name="arguments">An array of arguments.</param>
        /// <returns>Invocation result.</returns>
        internal protected sealed override IScriptObject Invoke(InvocationContext ctx, IRuntimeSlot[] arguments)
        {
            var newArgs = new object[arguments.LongLength + 1];
            newArgs[0] = ctx;
            arguments.CopyTo(newArgs, 1);
            return m_implementation.DynamicInvoke(newArgs) as IScriptObject ?? Void;
        }

        internal sealed override byte[] ByteCode
        {
            get { return GetByteCode(m_implementation); }
        }

        /// <summary>
        /// Creates a new action that provides empty implementation.
        /// </summary>
        /// <returns></returns>
        public static ScriptRuntimeAction CreateEmptyImplementation(ScriptActionContract actionContract)
        {
            var @params = new List<ParameterExpression>(actionContract.Parameters.Count + 1) { Expression.Parameter(typeof(InvocationContext)) };
            @params.AddRange(actionContract.Parameters.Select(p => Expression.Parameter(typeof(IRuntimeSlot))));
            var lamdba = Expression.Lambda(ScriptObject.MakeVoid(), @params);
            return new ScriptRuntimeAction(actionContract, null, lamdba.Compile());
        }
    }
}
