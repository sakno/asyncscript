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
    public static class RuntimeSynchronizationManager
    {
        private static readonly TimeSpan InfiniteTimeout = TimeSpan.FromMilliseconds(Timeout.Infinite);

        /// <summary>
        /// Blocks the current thread unti task is not synchronized.
        /// </summary>
        /// <param name="synchronizable">The object that should be synchronized. Cannot be <see langword="null"/>.</param>
        /// <param name="ar">The synchronizer.</param>
        /// <param name="timeout">Synchronization timeout.</param>
        /// <returns><see langword="true"/> if synchronizable object is synchronized successfully; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="System.ArgumentNullException"><paramref name="synchronizable"/> is <see langword="null"/>.</exception>
        public static bool Await(this ISynchronizable synchronizable, IAsyncResult ar, TimeSpan timeout)
        {
            if (synchronizable == null) throw new ArgumentNullException("synchronizable");
            return synchronizable.Await(ar.AsyncWaitHandle, timeout);
        }

        private static bool RtlAwait(ISynchronizable taskToSynchronize, IAsyncResult synchronizer)
        {
            switch (synchronizer != null)
            {
                case true: return Await(taskToSynchronize, synchronizer, InfiniteTimeout);
                default:
                    using (var handle = new ManualResetEvent(false))
                        return taskToSynchronize.Await(handle, InfiniteTimeout);
            }
        }

        private static bool RtlAwait(IAsyncResult taskToSynchronize, IAsyncResult synchronizer)
        {
            switch(synchronizer!=null)
            {
                case true: return WaitHandle.WaitAll(new[]{taskToSynchronize.AsyncWaitHandle, synchronizer.AsyncWaitHandle});
                default:
                    using(var handle=new ManualResetEvent(false))
                        return WaitHandle.WaitAll(new[]{taskToSynchronize.AsyncWaitHandle, handle});
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="taskToSynchronize"></param>
        /// <param name="synchronizer"></param>
        /// <returns></returns>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [CLSCompliant(false)]
        public static ScriptBoolean RtlAwait(IScriptObject taskToSynchronize, IAsyncResult synchronizer)
        {
            if (taskToSynchronize is ISynchronizable)
                return RtlAwait(taskToSynchronize as ISynchronizable, synchronizer);
            else if (taskToSynchronize is IAsyncResult)
                return RtlAwait((IAsyncResult)taskToSynchronize, synchronizer);
            else return ScriptBoolean.True;
        }

        internal static MethodCallExpression BindAwait(Expression asyncObj, Expression synchronizer)
        {
            return RuntimeHelpers.Invoke<IScriptObject, IAsyncResult, ScriptBoolean>(RtlAwait, asyncObj, synchronizer);
        }

        /// <summary>
        /// Executes synchronization task.
        /// </summary>
        /// <param name="synchronizer">The delegate that implements wait logic.</param>
        /// <returns>Synchronizer result.</returns>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static IAsyncResult RtlRunSynchronizer(Action synchronizer)
        {
            switch (synchronizer != null)
            {
                case true:
                    var task = new Task(synchronizer);
                    task.Start();
                    return task;
                default: return null;
            }
        }

        internal static MethodCallExpression BindRunSynchronizer(Expression<Action> synchronizer)
        {
            return RuntimeHelpers.Invoke<Action, IAsyncResult>(RtlRunSynchronizer, synchronizer);
        }
    }
}
