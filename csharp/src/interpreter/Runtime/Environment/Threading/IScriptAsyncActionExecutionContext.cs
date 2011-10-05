using System;

namespace DynamicScript.Runtime.Environment.Threading
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;

    [ComVisible(false)]
    interface IScriptAsyncActionExecutionContext
    {
        /// <summary>
        /// Gets a value indicating that the action is cancelled. 
        /// </summary>
        bool Cancelled { get; }

        /// <summary>
        /// Notifies about asynchronous operation state.
        /// </summary>
        /// <param name="progress">Progress of asynchronous operation processing, in percents.</param>
        /// <param name="asyncState">Information about asynchronous stage.</param>
        /// <param name="state"></param>
        void Notify(double progress, IScriptObject asyncState, InterpreterState state);
    }
}
