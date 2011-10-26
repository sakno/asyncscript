using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Linq;
using System.ComponentModel;

namespace DynamicScript.Runtime.Environment
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;
    using MethodInfo = System.Reflection.MethodInfo;
    using Closure = System.Runtime.CompilerServices.Closure;
    using ParallelLoopState = System.Threading.Tasks.ParallelLoopState;
    using CallStack = Debugging.CallStack;

    /// <summary>
    /// Represents script-side implementation of the action.
    /// This class cannot be inherited.
    /// </summary>
    [ComVisible(false)]
    public class ScriptRuntimeAction : ScriptActionBase
    {
        #region Nested Types
        [ComVisible(false)]
        private sealed class ArgumentConverter : SignatureAnalyzer
        {
            public readonly Converter<ScriptActionContract.Parameter, IRuntimeSlot> HolderFactory;
            public readonly Debugging.CallStackFrame StackFrame;
            public readonly object[] Holders;

            public ArgumentConverter(IList<ScriptActionContract.Parameter> @params, IList<IScriptObject> args, InterpreterState s, Converter<ScriptActionContract.Parameter, IRuntimeSlot> factory, Debugging.CallStackFrame sf)
                : base(@params, args, s)
            {
                Holders = new object[args.Count + 1];
                HolderFactory = factory;
                StackFrame = sf;
            }

            protected override void Analyze(ParallelLoopState state, int index, ScriptActionContract.Parameter p, IScriptObject a)
            {
                PrepareArgument(p, a, index + 1, HolderFactory, StackFrame, Holders, State);
            }

            public static void PrepareArgument(ScriptActionContract.Parameter p, IScriptObject a, int index, Converter<ScriptActionContract.Parameter, IRuntimeSlot> factory, Debugging.CallStackFrame stackFrame, object[] output, InterpreterState state)
            {
                var holder = factory(p);
                holder.SetValue(a, state);
                if (stackFrame != null)
                    stackFrame.RegisterStorage(p.Name, holder);
                output[index] = holder;
            }

            public new object[] Analyze()
            {
                base.Analyze();
                return Holders;
            }
        }
        #endregion

        private readonly Delegate m_implementation;
        private readonly long? m_token;

        /// <summary>
        /// Initializes a new script action.
        /// </summary>
        /// <param name="contract">The action signature. Cannot be <see langword="null"/>.</param>
        /// <param name="this">An action owner.</param>
        /// <param name="implementation">An implementation of the action. Cannot be <see langword="null"/>.</param>
        /// <exception cref="System.ArgumentNullException"><paramref name="contract"/> or <paramref name="implementation"/> is <see langword="null"/>.</exception>
        /// <remarks>
        /// An implementation should have the following signature:
        /// IScriptObject M(InterpreterState state[, IRuntimeSlot arg0, IRuntimeSlot arg1,...]);
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
            m_token = null;
        }

        /// <summary>
        /// Initializes a new script action.
        /// </summary>
        /// <param name="contract">The action signature. Cannot be <see langword="null"/>.</param>
        /// <param name="this">An action owner.</param>
        /// <param name="implementation">An implementation of the action. Cannot be <see langword="null"/>.</param>
        /// <param name="unqiueID">An unique identifier of the action.</param>
        /// <exception cref="System.ArgumentNullException"><paramref name="contract"/> or <paramref name="implementation"/> is <see langword="null"/>.</exception>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public ScriptRuntimeAction(ScriptActionContract contract, IScriptObject @this, Delegate implementation, long unqiueID)
            : this(contract, @this, implementation)
        {
            m_token = unqiueID;
        }

        /// <summary>
        /// Initializes a new script action without return value.
        /// </summary>
        /// <param name="implementation">An implementation of the action. Cannot be <see langword="null"/></param>
        /// <param name="param0">A description of the first parameter.</param>
        /// <param name="this">An action owner.</param>
        /// <exception cref="System.ArgumentNullException"><paramref name="implementation"/> is <see langword="null"/>.</exception>
        public ScriptRuntimeAction(Action<InterpreterState, IRuntimeSlot> implementation, ScriptActionContract.Parameter param0, IScriptObject @this = null)
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
        public ScriptRuntimeAction(Action<InterpreterState, IRuntimeSlot, IRuntimeSlot> implementation, ScriptActionContract.Parameter param0, ScriptActionContract.Parameter param1, IScriptObject @this = null)
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
        public ScriptRuntimeAction(Action<InterpreterState, IRuntimeSlot, IRuntimeSlot, IRuntimeSlot> implementation, ScriptActionContract.Parameter param0, ScriptActionContract.Parameter param1, ScriptActionContract.Parameter param2, IScriptObject @this = null)
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
        public ScriptRuntimeAction(Action<InterpreterState, IRuntimeSlot, IRuntimeSlot, IRuntimeSlot, IRuntimeSlot> implementation, ScriptActionContract.Parameter param0, ScriptActionContract.Parameter param1, ScriptActionContract.Parameter param2, ScriptActionContract.Parameter param3, IScriptObject @this = null)
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
        public ScriptRuntimeAction(Action<InterpreterState, IRuntimeSlot, IRuntimeSlot, IRuntimeSlot, IRuntimeSlot, IRuntimeSlot> implementation, ScriptActionContract.Parameter param0, ScriptActionContract.Parameter param1, ScriptActionContract.Parameter param2, ScriptActionContract.Parameter param3, ScriptActionContract.Parameter param4, IScriptObject @this = null)
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
        public ScriptRuntimeAction(Action<InterpreterState, IRuntimeSlot, IRuntimeSlot, IRuntimeSlot, IRuntimeSlot, IRuntimeSlot, IRuntimeSlot> implementation, ScriptActionContract.Parameter param0, ScriptActionContract.Parameter param1, ScriptActionContract.Parameter param2, ScriptActionContract.Parameter param3, ScriptActionContract.Parameter param4, ScriptActionContract.Parameter param5, IScriptObject @this = null)
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
        public ScriptRuntimeAction(Func<InterpreterState, IScriptObject> implementation, IScriptContract returnValue, IScriptObject @this = null)
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
        public ScriptRuntimeAction(Func<InterpreterState, IRuntimeSlot, IScriptObject> implementation, ScriptActionContract.Parameter param0, IScriptContract returnValue, IScriptObject @this = null)
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
        public ScriptRuntimeAction(Func<InterpreterState, IRuntimeSlot, IRuntimeSlot, IScriptObject> implementation, ScriptActionContract.Parameter param0, ScriptActionContract.Parameter param1, IScriptContract returnValue, IScriptObject @this = null)
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
        public ScriptRuntimeAction(Func<InterpreterState, IRuntimeSlot, IRuntimeSlot, IRuntimeSlot, IScriptObject> implementation, ScriptActionContract.Parameter param0, ScriptActionContract.Parameter param1, ScriptActionContract.Parameter param2, IScriptContract returnValue, IScriptObject @this = null)
            : this(new ScriptActionContract(new[] { param0, param1, param2 }, returnValue), @this, implementation)
        {
        }

        internal static Expression New(Expression actionContract, Expression @this, LambdaExpression implementation, string sourceCode)
        {
            actionContract = Expression.TypeAs(ScriptContract.Extract(actionContract), typeof(ScriptActionContract));
            var ctor = LinqHelpers.BodyOf<ScriptActionContract, IScriptObject, Delegate, long, ScriptRuntimeAction, NewExpression>((c, t, i, u) => new ScriptRuntimeAction(c, t, i, u)).Constructor;
            return Expression.New(ctor, actionContract, @this, implementation, LinqHelpers.Constant(StringEqualityComparer.GetHashCodeLong(sourceCode)));
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

        internal sealed override byte[] ByteCode
        {
            get
            {
                var byteCode = GetByteCode(m_implementation);
                return byteCode.LongLength == 0L && m_token.HasValue ? BitConverter.GetBytes(m_token.Value) : byteCode;
            }
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

        private object[] PrepareInvocation(IList<IScriptObject> args, InterpreterState state)
        {
            var slots = default(object[]);
            //Transform each argument 
            switch (args.Count)
            {
                case 0: slots = new object[1]; break;
                case 1:
                    ArgumentConverter.PrepareArgument(Parameters[0], args[0], 1, CreateParameterHolder, IsTransparent ? null : CallStack.Current, slots = new object[2], state);
                    slots[0] = state;
                    break;
                default:
                    var converter = new ArgumentConverter(Parameters, args, state, CreateParameterHolder, IsTransparent ? null : CallStack.Current);
                    slots = converter.Analyze();
                    break;
            }
            slots[0] = state;
            return slots;
        }


        private IScriptObject InvokeCore(object[] arguments, InterpreterState state)
        {
            return m_implementation.DynamicInvoke(arguments) as IScriptObject ?? Void;
        }

        /// <summary>
        /// Invokes an action using the specified list of arguments.
        /// </summary>
        /// <param name="arguments"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        protected sealed override IScriptObject InvokeCore(IList<IScriptObject> arguments, InterpreterState state)
        {
            return InvokeCore(PrepareInvocation(arguments, state), state);
        }
    }
}
