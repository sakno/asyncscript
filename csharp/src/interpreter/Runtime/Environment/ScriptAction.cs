using System;
using System.Collections.Generic;

namespace DynamicScript.Runtime.Environment
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;
    using Enumerable = System.Linq.Enumerable;

    /// <summary>
    /// Represents parameterless script action without return value.
    /// </summary>
    [ComVisible(false)]
    public abstract class ScriptAction : ScriptFunctionBase
    {
        /// <summary>
        /// Initializes a new paremeterless action without return value.
        /// </summary>
        /// <param name="this"></param>
        protected ScriptAction(IScriptObject @this = null)
            : base(new ScriptFunctionContract(Enumerable.Empty<ScriptFunctionContract.Parameter>()), @this)
        {
        }

        /// <summary>
        /// Invokes an action.
        /// </summary>
        /// <param name="state">Action invocation context.</param>
        protected abstract void Invoke(InterpreterState state);

        /// <summary>
        /// Invokes an action.
        /// </summary>
        /// <param name="args"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        protected sealed override IScriptObject InvokeCore(IList<IScriptObject> args, InterpreterState state)
        {
            Invoke(state);
            return Void;
        }
    }

    /// <summary>
    /// Represents a script action without return value.
    /// </summary>
    /// <typeparam name="T">Type of the first action parameter.</typeparam>
    [ComVisible(false)]
    public abstract class ScriptAction<T> : ScriptFunctionBase
        where T: class, IScriptObject
    {
        /// <summary>
        /// Initializes a new script action.
        /// </summary>
        /// <param name="param0">The description of the first parameter.</param>
        /// <param name="this">An action owner.</param>
        protected ScriptAction(ScriptFunctionContract.Parameter param0, IScriptObject @this = null)
            : base(new ScriptFunctionContract(new[] { param0 }), @this)
        {
        }

        internal ScriptAction(string firstParamName, IScriptContract firstParamType, IScriptObject @this = null)
            : this(new ScriptFunctionContract.Parameter(firstParamName, firstParamType), @this)
        {
        }

        /// <summary>
        /// Invokes an action.
        /// </summary>
        /// <param name="arg0">The first argument of the action.</param>
        /// <param name="state">Internal interpreter state.</param>
        protected abstract void Invoke(T arg0, InterpreterState state);

        /// <summary>
        /// Invokes an action.
        /// </summary>
        /// <param name="args"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        protected sealed override IScriptObject InvokeCore(IList<IScriptObject> args, InterpreterState state)
        {
            Invoke(Unwrap<T>(args,0, state), state);
            return Void;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T1"></typeparam>
    /// <typeparam name="T2"></typeparam>
    [ComVisible(false)]
    public abstract class ScriptAction<T1, T2> : ScriptFunctionBase
        where T1 : class, IScriptObject
        where T2 : class, IScriptObject
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="param0"></param>
        /// <param name="param1"></param>
        /// <param name="this"></param>
        protected ScriptAction(ScriptFunctionContract.Parameter param0, ScriptFunctionContract.Parameter param1, IScriptObject @this = null)
            : base(new ScriptFunctionContract(new[] { param0, param1 }), @this)
        {
        }

        internal ScriptAction(string firstParamName, IScriptContract firstParamType, string secondParamName, IScriptContract secondParamType, IScriptObject @this = null)
            : this(new ScriptFunctionContract.Parameter(firstParamName, firstParamType), new ScriptFunctionContract.Parameter(secondParamName, secondParamType), @this)
        {
        }

        /// <summary>
        /// Invokes an action.
        /// </summary>
        /// <param name="arg0"></param>
        /// <param name="arg1"></param>
        /// <param name="state"></param>
        protected abstract void Invoke(T1 arg0, T2 arg1, InterpreterState state);

        /// <summary>
        /// Invokes an action.
        /// </summary>
        /// <param name="args"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        protected sealed override IScriptObject InvokeCore(IList<IScriptObject> args, InterpreterState state)
        {
            Invoke(Unwrap<T1>(args, 0, state), Unwrap<T2>(args, 1, state), state);
            return Void;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T1"></typeparam>
    /// <typeparam name="T2"></typeparam>
    /// <typeparam name="T3"></typeparam>
    [ComVisible(false)]
    public abstract class ScriptAction<T1, T2, T3> : ScriptFunctionBase
        where T1 : class, IScriptObject
        where T2 : class, IScriptObject
        where T3 : class, IScriptObject
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="param0"></param>
        /// <param name="param1"></param>
        /// <param name="param2"></param>
        /// <param name="this"></param>
        protected ScriptAction(ScriptFunctionContract.Parameter param0, ScriptFunctionContract.Parameter param1, ScriptFunctionContract.Parameter param2, IScriptObject @this = null)
            : base(new ScriptFunctionContract(new[] { param0, param1, param2 }), @this)
        {
        }

        internal ScriptAction(string firstParamName, IScriptContract firstParamContract, string secondParamName, IScriptContract secondParamContract, string thirdParamName, IScriptContract thirdParamContract, IScriptObject @this = null)
            : this(new ScriptFunctionContract.Parameter(firstParamName, firstParamContract), new ScriptFunctionContract.Parameter(secondParamName, secondParamContract), new ScriptFunctionContract.Parameter(thirdParamName, thirdParamContract), @this)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="arg0"></param>
        /// <param name="arg1"></param>
        /// <param name="arg2"></param>
        /// <param name="state"></param>
        protected abstract void Invoke(T1 arg0, T2 arg1, T3 arg2, InterpreterState state);

        /// <summary>
        /// Invokes an action.
        /// </summary>
        /// <param name="args"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        protected sealed override IScriptObject InvokeCore(IList<IScriptObject> args, InterpreterState state)
        {
            Invoke(Unwrap<T1>(args, 0, state), Unwrap<T2>(args, 1, state), Unwrap<T3>(args, 2, state), state);
            return Void;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T1"></typeparam>
    /// <typeparam name="T2"></typeparam>
    /// <typeparam name="T3"></typeparam>
    /// <typeparam name="T4"></typeparam>
    [ComVisible(false)]
    public abstract class ScriptAction<T1, T2, T3, T4> : ScriptFunctionBase
        where T1 : class, IScriptObject
        where T2 : class, IScriptObject
        where T3 : class, IScriptObject
        where T4 : class, IScriptObject
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="param0"></param>
        /// <param name="param1"></param>
        /// <param name="param2"></param>
        /// <param name="param3"></param>
        /// <param name="this"></param>
        protected ScriptAction(ScriptFunctionContract.Parameter param0, ScriptFunctionContract.Parameter param1, ScriptFunctionContract.Parameter param2, ScriptFunctionContract.Parameter param3, IScriptObject @this = null)
            : base(new ScriptFunctionContract(new[] { param0, param1, param2, param3 }), @this)
        {
        }

        internal ScriptAction(string firstParamName, IScriptContract firstParamContract, string secondParamName, IScriptContract secondParamContract, string thirdParamName, IScriptContract thirdParamContract, string fourthParamName, IScriptContract fourthParamContract, IScriptObject @this = null)
            : this(new ScriptFunctionContract.Parameter(firstParamName, firstParamContract), new ScriptFunctionContract.Parameter(secondParamName, secondParamContract), new ScriptFunctionContract.Parameter(thirdParamName, thirdParamContract), new ScriptFunctionContract.Parameter(fourthParamName, fourthParamContract), @this)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="arg0"></param>
        /// <param name="arg1"></param>
        /// <param name="arg2"></param>
        /// <param name="arg3"></param>
        /// <param name="state"></param>
        protected abstract void Invoke(T1 arg0, T2 arg1, T3 arg2, T4 arg3, InterpreterState state);

        /// <summary>
        /// Invokes an action.
        /// </summary>
        /// <param name="args"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        protected sealed override IScriptObject InvokeCore(IList<IScriptObject> args, InterpreterState state)
        {
            Invoke(Unwrap<T1>(args, 0, state), Unwrap<T2>(args, 1, state), Unwrap<T3>(args, 2, state), Unwrap<T4>(args, 3, state), state);
            return Void;
        }
    }
}
