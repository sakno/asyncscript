using System;
using System.Threading;
using System.Threading.Tasks;
using System.Linq.Expressions;
using System.ComponentModel;

namespace DynamicScript.Runtime.Environment.Threading
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;
    using DynamicMetaObject = System.Dynamic.DynamicMetaObject;

    /// <summary>
    /// Represents DynamicScript synchronization manager.
    /// </summary>
    [ComVisible(false)]
    public static class ThreadManager
    {
        private static readonly TimeSpan InfiniteTimeout = TimeSpan.FromMilliseconds(Timeout.Infinite);

        /// <summary>
        /// Blocks the current thread unti task is not synchronized.
        /// </summary>
        /// <param name="synchronizable">The object that should be synchronized. Cannot be <see langword="null"/>.</param>
        /// <param name="ar">The synchronizer.</param>
        /// <param name="timeout">Synchronization timeout.</param>
        /// <param name="state">Internal interpreter state.</param>
        /// <returns><see langword="true"/> if synchronizable object is synchronized successfully; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="System.ArgumentNullException"><paramref name="synchronizable"/> is <see langword="null"/>.</exception>
        public static bool Await(this ISynchronizable synchronizable, IAsyncResult ar, TimeSpan timeout, InterpreterState state)
        {
            if (synchronizable == null) throw new ArgumentNullException("synchronizable");
            return synchronizable.Await(ar.AsyncWaitHandle, timeout, state);
        }

        private static bool RtlAwait(ISynchronizable taskToSynchronize, IAsyncResult synchronizer, InterpreterState state)
        {
            switch (synchronizer != null)
            {
                case true: return Await(taskToSynchronize, synchronizer, InfiniteTimeout, state);
                default:
                    using (var handle = new ManualResetEvent(false))
                        return taskToSynchronize.Await(handle, InfiniteTimeout, state);
            }
        }

        private static bool RtlAwait(IAsyncResult taskToSynchronize, IAsyncResult synchronizer)
        {
            switch(synchronizer!=null)
            {
                case true: return WaitHandle.WaitAll(new[]{taskToSynchronize.AsyncWaitHandle, synchronizer.AsyncWaitHandle});
                default:
                    using (var handle = new ManualResetEvent(false))
                        return WaitHandle.WaitAny(new[] { taskToSynchronize.AsyncWaitHandle, handle }) == 0;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="taskToSynchronize"></param>
        /// <param name="synchronizer"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [CLSCompliant(false)]
        public static ScriptBoolean RtlAwait(IScriptObject taskToSynchronize, IAsyncResult synchronizer, InterpreterState state)
        {
            if (taskToSynchronize is ISynchronizable)
                return RtlAwait(taskToSynchronize as ISynchronizable, synchronizer, state);
            else if (taskToSynchronize is IAsyncResult)
                return RtlAwait((IAsyncResult)taskToSynchronize, synchronizer);
            else return ScriptBoolean.True;
        }

        internal static MethodCallExpression BindAwait(Expression asyncObj, Expression synchronizer, ParameterExpression state)
        {
            return LinqHelpers.Call<IScriptObject, IAsyncResult, InterpreterState, ScriptBoolean>((a, s, t) => RtlAwait(a, s, t), null, asyncObj, synchronizer, state);
        }

        /// <summary>
        /// Executes synchronization task.
        /// </summary>
        /// <param name="target"></param>
        /// <param name="synchronizer">The delegate that implements wait logic.</param>
        /// <param name="state"></param>
        /// <returns>Synchronizer result.</returns>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static IAsyncResult RtlRunSynchronizer(IScriptObject target, ScriptWorkItem synchronizer, InterpreterState state)
        {
            return Task.Factory.StartNew<IScriptObject>(new WorkItemStartParameters(target, synchronizer, state).Start);
        }

        internal static MethodCallExpression BindRunSynchronizer(Expression scopeObj, Expression<ScriptWorkItem> synchronizer, ParameterExpression state)
        {
            return LinqHelpers.Call<IScriptObject, ScriptWorkItem, InterpreterState, IAsyncResult>((t, a, s) => RtlRunSynchronizer(t, a, s), null, scopeObj, synchronizer, state);
        }

        /// <summary>
        /// Creates a new work item queue from its script representation.
        /// </summary>
        /// <param name="queue"></param>
        /// <returns></returns>
        public static IScriptWorkItemQueue CreateQueue(IScriptObject queue)
        {
            if (queue == null)
                return null;
            else if (queue is IScriptWorkItemQueue)
                return (IScriptWorkItemQueue)queue;
            else return new ScriptWorkItemQueue(queue);
        }
    }
}
