using System;
using System.Collections.Generic;

namespace DynamicScript.Runtime.Environment
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;
    using Enumerable = System.Linq.Enumerable;

    /// <summary>
    /// Represents a script action with return value.
    /// </summary>
    [ComVisible(false)]
    public abstract class ScriptFunc: ScriptActionBase
    {
        /// <summary>
        /// Initializes a new script action.
        /// </summary>
        /// <param name="returnValue">The contract binding of the return value.</param>
        /// <param name="this">An action owner.</param>
        protected ScriptFunc(IScriptContract returnValue, IScriptObject @this = null)
            : base(new ScriptActionContract(Enumerable.Empty<ScriptActionContract.Parameter>(), returnValue), @this)
        {
        }

        /// <summary>
        /// Invokes an action.
        /// </summary>
        /// <param name="state">Internal interpreter state.</param>
        /// <returns></returns>
        protected abstract IScriptObject Invoke(InterpreterState state);

        /// <summary>
        /// Invokes parameterless action.
        /// </summary>
        /// <param name="args"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        protected sealed override IScriptObject InvokeCore(IList<IScriptObject> args, InterpreterState state)
        {
            return Invoke(state);
        }
    }

    /// <summary>
    /// Represents a script action with return value.
    /// </summary>
    /// <typeparam name="T">Type of the first action parameter.</typeparam>
    [ComVisible(false)]
    public abstract class ScriptFunc<T> : ScriptActionBase
        where T : class, IScriptObject
    {
        /// <summary>
        /// Initializes a new script action.
        /// </summary>
        /// <param name="param0">The description of the first parameter.</param>
        /// <param name="returnValue">The contract binding of the return value.</param>
        /// <param name="this">An action owner.</param>
        protected ScriptFunc(ScriptActionContract.Parameter param0, IScriptContract returnValue, IScriptObject @this = null)
            : base(new ScriptActionContract(new[] { param0 }, returnValue), @this)
        {
        }

        internal ScriptFunc(string firstParamName, IScriptContract firstParamType, IScriptContract returnValue, IScriptObject @this = null)
            : this(new ScriptActionContract.Parameter(firstParamName, firstParamType), returnValue, @this)
        {
        }

        /// <summary>
        /// Invokes an action.
        /// </summary>
        /// <param name="arg0">The first action parameter.</param>
        /// <param name="state">Internal interpreter state.</param>
        /// <returns></returns>
        protected abstract IScriptObject Invoke(T arg0, InterpreterState state);

        /// <summary>
        /// Invokes an action.
        /// </summary>
        /// <param name="args"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        protected sealed override IScriptObject InvokeCore(IList<IScriptObject> args, InterpreterState state)
        {
            return Invoke(Unwrap<T>(args, 0, state), state);
        }
    }

    /// <summary>
    /// Represents a script action with return value.
    /// </summary>
    /// <typeparam name="T1">Type of the first action parameter.</typeparam>
    /// <typeparam name="T2">Type of the second action parameter.</typeparam>
    [ComVisible(false)]
    public abstract class ScriptFunc<T1, T2> : ScriptActionBase
        where T1: class, IScriptObject
        where T2: class, IScriptObject
    {
        /// <summary>
        /// Initializes a new script action.
        /// </summary>
        ///<param name="param0">The description of the first parameter.</param>
        ///<param name="param1">The description of the second parameter.</param>
        /// <param name="returnValue">The contract binding of the return value.</param>
        /// <param name="this">An action owner.</param>
        protected ScriptFunc(ScriptActionContract.Parameter param0, ScriptActionContract.Parameter param1, IScriptContract returnValue, IScriptObject @this = null)
            : base(new ScriptActionContract(new[] { param0, param1 }, returnValue), @this)
        {
        }

        internal ScriptFunc(string firstParamName, IScriptContract firstParamType, string secondParamName, IScriptContract secondParamType, IScriptContract returnValue, IScriptObject @this = null)
            : this(new ScriptActionContract.Parameter(firstParamName, firstParamType), new ScriptActionContract.Parameter(secondParamName, secondParamType), returnValue, @this)
        {
        }

        /// <summary>
        /// Invokes an action.
        /// </summary>
        /// <param name="arg0">The first action parameter.</param>
        /// <param name="arg1">The second action parameter.</param>
        /// <param name="state">Internal interpreter state.</param>
        /// <returns></returns>
        protected abstract IScriptObject Invoke(T1 arg0, T2 arg1, InterpreterState state);

        /// <summary>
        /// Invokes an action.
        /// </summary>
        /// <param name="args"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        protected sealed override IScriptObject InvokeCore(IList<IScriptObject> args, InterpreterState state)
        {
            return Invoke(Unwrap<T1>(args,0, state), Unwrap<T2>(args,1, state), state);
        }
    }

    /// <summary>
    /// Represents a script action with return value.
    /// </summary>
    /// <typeparam name="T1">Type of the first action parameter.</typeparam>
    /// <typeparam name="T2">Type of the second action parameter.</typeparam>
    /// <typeparam name="T3">Type of the third action parameter.</typeparam>
    [ComVisible(false)]
    public abstract class ScriptFunc<T1, T2, T3> : ScriptActionBase
        where T1 : class, IScriptObject
        where T2 : class, IScriptObject
        where T3 : class, IScriptObject
    {
        /// <summary>
        /// Initializes a new script action.
        /// </summary>
        ///<param name="param0">The description of the first parameter.</param>
        ///<param name="param1">The description of the second parameter.</param>
        ///<param name="param2">The description of the third parameter.</param>
        /// <param name="returnValue">The contract binding of the return value.</param>
        /// <param name="this">An action owner.</param>
        protected ScriptFunc(ScriptActionContract.Parameter param0, ScriptActionContract.Parameter param1, ScriptActionContract.Parameter param2, IScriptContract returnValue, IScriptObject @this = null)
            : base(new ScriptActionContract(new[] { param0, param1, param2 }, returnValue), @this)
        {
        }

        internal ScriptFunc(string firstParamName, IScriptContract firstParamType, string secondParamName, IScriptContract secondParamType, string thirdParamName, IScriptContract thirdParamType, IScriptContract returnValue, IScriptObject @this = null)
            : this(new ScriptActionContract.Parameter(firstParamName, firstParamType), new ScriptActionContract.Parameter(secondParamName, secondParamType), new ScriptActionContract.Parameter(thirdParamName, thirdParamType), returnValue, @this)
        {
        }

        /// <summary>
        /// Invokes an action.
        /// </summary>
        /// <param name="arg0">The first action parameter.</param>
        /// <param name="arg1">The second action parameter.</param>
        /// <param name="arg2">The third action parameter.</param>
        /// <param name="state">Internal interpreter state.</param>
        /// <returns></returns>
        public abstract IScriptObject Invoke(T1 arg0, T2 arg1, T3 arg2, InterpreterState state);

        /// <summary>
        /// Invokes an action.
        /// </summary>
        /// <param name="args"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        protected sealed override IScriptObject InvokeCore(IList<IScriptObject> args, InterpreterState state)
        {
            return Invoke(Unwrap<T1>(args,0, state), Unwrap<T2>(args,1, state), Unwrap<T3>(args,2, state), state);
        }
    }
}
