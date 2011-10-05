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
    public abstract class ScriptAction : ScriptActionBase
    {
        /// <summary>
        /// Initializes a new paremeterless action without return value.
        /// </summary>
        /// <param name="this"></param>
        protected ScriptAction(IScriptObject @this = null)
            : base(new ScriptActionContract(Enumerable.Empty<ScriptActionContract.Parameter>()), @this)
        {
        }

        /// <summary>
        /// Invokes an action.
        /// </summary>
        /// <param name="ctx">Action invocation context.</param>
        protected abstract void Invoke(InvocationContext ctx);

        /// <summary>
        /// Invokes an action.
        /// </summary>
        /// <param name="ctx">Action invocation context.</param>
        /// <param name="arguments">Action invocation arguments.</param>
        /// <returns></returns>
        internal protected sealed override IScriptObject Invoke(InvocationContext ctx, IRuntimeSlot[] arguments)
        {
            Invoke(ctx);
            return Void;
        }
    }

    /// <summary>
    /// Represents a script action without return value.
    /// </summary>
    /// <typeparam name="T">Type of the first action parameter.</typeparam>
    [ComVisible(false)]
    public abstract class ScriptAction<T> : ScriptActionBase
        where T: class, IScriptObject
    {
        /// <summary>
        /// Initializes a new script action.
        /// </summary>
        /// <param name="param0">The description of the first parameter.</param>
        /// <param name="this">An action owner.</param>
        protected ScriptAction(ScriptActionContract.Parameter param0, IScriptObject @this = null)
            : base(new ScriptActionContract(new[] { param0 }), @this)
        {
        }

        internal ScriptAction(string firstParamName, IScriptContract firstParamType, IScriptObject @this = null)
            : this(new ScriptActionContract.Parameter(firstParamName, firstParamType), @this)
        {
        }

        /// <summary>
        /// Invokes an action.
        /// </summary>
        /// <param name="ctx">Action invocation context.</param>
        /// <param name="arg0">The first argument of the action.</param>
        protected abstract void Invoke(InvocationContext ctx, T arg0);

        /// <summary>
        /// Invokes an action.
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="arguments"></param>
        /// <returns></returns>
        internal protected sealed override IScriptObject Invoke(InvocationContext ctx, IRuntimeSlot[] arguments)
        {
            Invoke(ctx, arguments[0].GetValue(ctx.RuntimeState) as T);
            return Void;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T1"></typeparam>
    /// <typeparam name="T2"></typeparam>
    [ComVisible(false)]
    public abstract class ScriptAction<T1, T2> : ScriptActionBase
        where T1 : class, IScriptObject
        where T2 : class, IScriptObject
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="param0"></param>
        /// <param name="param1"></param>
        /// <param name="this"></param>
        protected ScriptAction(ScriptActionContract.Parameter param0, ScriptActionContract.Parameter param1, IScriptObject @this = null)
            : base(new ScriptActionContract(new[] { param0, param1 }), @this)
        {
        }

        internal ScriptAction(string firstParamName, IScriptContract firstParamType, string secondParamName, IScriptContract secondParamType, IScriptObject @this = null)
            : this(new ScriptActionContract.Parameter(firstParamName, firstParamType), new ScriptActionContract.Parameter(secondParamName, secondParamType), @this)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="arg0"></param>
        /// <param name="arg1"></param>
        protected abstract void Invoke(InvocationContext ctx, T1 arg0, T2 arg1);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="arguments"></param>
        /// <returns></returns>
        internal protected sealed override IScriptObject Invoke(InvocationContext ctx, IRuntimeSlot[] arguments)
        {
            Invoke(ctx, arguments[0].GetValue(ctx.RuntimeState) as T1, arguments[1].GetValue(ctx.RuntimeState) as T2);
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
    public abstract class ScriptAction<T1, T2, T3> : ScriptActionBase
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
        protected ScriptAction(ScriptActionContract.Parameter param0, ScriptActionContract.Parameter param1, ScriptActionContract.Parameter param2, IScriptObject @this = null)
            : base(new ScriptActionContract(new[] { param0, param1, param2 }), @this)
        {
        }

        internal ScriptAction(string firstParamName, IScriptContract firstParamContract, string secondParamName, IScriptContract secondParamContract, string thirdParamName, IScriptContract thirdParamContract, IScriptObject @this = null)
            : this(new ScriptActionContract.Parameter(firstParamName, firstParamContract), new ScriptActionContract.Parameter(secondParamName, secondParamContract), new ScriptActionContract.Parameter(thirdParamName, thirdParamContract), @this)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="arg0"></param>
        /// <param name="arg1"></param>
        /// <param name="arg2"></param>
        protected abstract void Invoke(InvocationContext ctx, T1 arg0, T2 arg1, T3 arg2);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="arguments"></param>
        /// <returns></returns>
        internal protected sealed override IScriptObject Invoke(InvocationContext ctx, IRuntimeSlot[] arguments)
        {
            Invoke(ctx, arguments[0].GetValue(ctx.RuntimeState) as T1, arguments[1].GetValue(ctx.RuntimeState) as T2, arguments[2].GetValue(ctx.RuntimeState) as T3);
            return Void;
        }
    }
}
