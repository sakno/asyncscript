using System;
using System.Collections.Generic;
using System.Threading;
using System.Dynamic;

namespace DynamicScript.Runtime.Environment.Threading
{
    using Compiler.Ast;
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;
    /// <summary>
    /// Represetns implementation of future pattern.
    /// </summary>
    /// <seealso href="http://en.wikipedia.org/wiki/Future_%28programming%29"/>
    [ComVisible(false)]
    public abstract class ScriptFuture : DynamicObject, IScriptAsyncObject
    {
        #region Nested Types
        /// <summary>
        /// Represents task execution parameters.
        /// </summary>
        private sealed class TaskParameters
        {
            public readonly IScriptObject Target;
            public readonly Func<IScriptObject, InterpreterState, IScriptObject> Task;
            public readonly InterpreterState State;

            public TaskParameters(IScriptObject target, Func<IScriptObject, InterpreterState, IScriptObject> task, InterpreterState state)
            {
                Target = target ?? state.Global;
                State = state;
                Task = task;
            }
        }
        #endregion

        /// <summary>
        /// Represents maximum timeout for the future wrapper.
        /// </summary>
        public static TimeSpan MaxTimeout = TimeSpan.FromMinutes(10);
        private IScriptObject m_result;
        private readonly ManualResetEvent m_handle;
        private Exception m_error;
        private IScriptContract m_requirement;
        private readonly int m_hashCode;

        private ScriptFuture()
        {
            m_handle = new ManualResetEvent(false);
            m_result = null;
            m_error = null;
            m_hashCode = m_handle.GetHashCode();
        }

        /// <summary>
        /// Initializes a new instance of the Future pattern implementation.
        /// </summary>
        /// <param name="target">The target object passed to the task in the parallel thread.</param>
        /// <param name="task">A task implementation to be executed in the parallel thread.</param>
        /// <param name="state">Internal interpreter state.</param>
        protected ScriptFuture(IScriptObject target, Func<IScriptObject, InterpreterState, IScriptObject> task, InterpreterState state)
        {
            ThreadPool.QueueUserWorkItem(ProcessTask, new TaskParameters(target, task, state));
        }

        /// <summary>
        /// Creates a new instance of the future object.
        /// </summary>
        /// <param name="target">An object to be passed into the task.</param>
        /// <param name="task">The task that implements computation logic.</param>
        /// <param name="state">Internal interpreter state.</param>
        /// <returns>A new future script object.</returns>
        protected abstract ScriptFuture Create(IScriptObject target, Func<IScriptObject, InterpreterState, IScriptObject> task, InterpreterState state);

        /// <summary>
        /// Creates a new future runtime slot.
        /// </summary>
        /// <param name="slotName"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        protected abstract IRuntimeSlot Create(string slotName, InterpreterState state);

        /// <summary>
        /// Creates a new future indexer.
        /// </summary>
        /// <param name="indicies"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        protected abstract IRuntimeSlot Create(IScriptObject[] indicies, InterpreterState state);

        private void ProcessTask(IScriptObject target, Func<IScriptObject, InterpreterState, IScriptObject> task, InterpreterState state)
        {
            try
            {
                var result = task.Invoke(target is IScriptProxyObject ? ((IScriptProxyObject)target).Unwrap(state) : target, state);
                ProcessResult(ref result, state);
                m_result = result;
            }
            catch (Exception e)
            {
                m_error = e;
            }
            finally
            {
                m_handle.Set();
                m_requirement = null;
                m_handle.Dispose();
            }
        }

        /// <summary>
        /// Applies additional operations on the synchronized object.
        /// </summary>
        /// <param name="result"></param>
        /// <param name="state"></param>
        protected virtual void ProcessResult(ref IScriptObject result, InterpreterState state)
        {
        }

        private void ProcessTask(object state)
        {
            var parameters = (TaskParameters)state;
            ProcessTask(parameters.Target, parameters.Task, parameters.State);
        }

        void IScriptProxyObject.RequiresContract(IScriptContract contract, InterpreterState state)
        {
            if (m_requirement == null) m_requirement = contract;
            else switch (m_requirement.GetRelationship(contract))
                {
                    case ContractRelationshipType.Superset:
                    case ContractRelationshipType.TheSame:
                        m_requirement = contract; return;
                    case ContractRelationshipType.Subset: return;
                    default: throw new ContractBindingException(this, contract, state);
                }
        }

        IScriptAsyncObject IScriptAsyncObject.Enqueue(IScriptObject left, ScriptCodeBinaryOperatorType @operator, InterpreterState state)
        {
            return Create(this, (right, s) => left.BinaryOperation(@operator, right, s), state);
        }

        IScriptAsyncObject IScriptAsyncObject.Enqueue(IList<IScriptObject> args, InterpreterState state)
        {
            return Create(this, (target, s) => target.Invoke(args, s), state);
        }

        protected TResult TryApply<TResult>(Func<IScriptObject, TResult> action)
        {
            return m_result != null ? action(m_result) : default(TResult);
        }

        protected IScriptObject UnwrapUnsafe(InterpreterState state)
        {
            if (m_error != null)
                throw m_error;
            else if (m_result == null)
                return null;
            else if (m_requirement == null)
                return m_result;
            else if (RuntimeHelpers.IsCompatible(m_requirement, m_result))
                return m_result;
            else throw new ContractBindingException(m_result, m_requirement, state);
        }

        public bool Unwrap(TimeSpan timeout, out IScriptObject result, InterpreterState state)
        {
            switch (IsCompleted || m_handle.WaitOne(timeout))
            {
                case true:
                    result = UnwrapUnsafe(state);
                    return true;
                default:
                    result = null;
                    return false;
            }
        }

        public IScriptObject Unwrap(InterpreterState state)
        {
            var result = default(IScriptObject);
            return Unwrap(MaxTimeout, out result, state) ? result : ScriptObject.Void;
        }

        protected IScriptObject BinaryOperation(ScriptCodeBinaryOperatorType @operator, IScriptObject right, InterpreterState state)
        {
            return IsCompleted ?
                m_result.BinaryOperation(@operator, right, state) :
                Create(this, (left, s) => left.BinaryOperation(@operator, right, s), state);
        }

        IScriptObject IScriptObject.BinaryOperation(ScriptCodeBinaryOperatorType @operator, IScriptObject right, InterpreterState state)
        {
            return BinaryOperation(@operator, right, state);
        }

        IScriptObject IScriptObject.UnaryOperation(ScriptCodeUnaryOperatorType @operator, InterpreterState state)
        {
            return IsCompleted ? m_result.UnaryOperation(@operator, state) :
                Create(this, (operand, s) => operand.UnaryOperation(@operator, s), state);
        }

        object IAsyncResult.AsyncState
        {
            get { return null; }
        }

        WaitHandle IAsyncResult.AsyncWaitHandle
        {
            get { return m_handle; }
        }

        bool IAsyncResult.CompletedSynchronously
        {
            get { return false; }
        }

        /// <summary>
        /// Gets a value indicating whether this object is synchronized.
        /// </summary>
        public bool IsCompleted
        {
            get { return m_result != null; }
        }

        /// <summary>
        /// Returns an expected contract.
        /// </summary>
        /// <returns></returns>
        public IScriptContract GetContractBinding()
        {
            return m_requirement ?? ScriptSuperContract.Instance;
        }

        IScriptObject IScriptObject.Invoke(IList<IScriptObject> args, InterpreterState state)
        {
            return IsCompleted ?
                m_result.Invoke(args, state) :
                Create(this, (target, s) => target.Invoke(args, s), state);
        }

        IRuntimeSlot IScriptObject.this[string slotName, InterpreterState state]
        {
            get { return IsCompleted ? m_result[slotName, state] : Create(slotName, state); }
        }

        IScriptObject IScriptObject.GetRuntimeDescriptor(string slotName, InterpreterState state)
        {
            return IsCompleted ?
                m_result.GetRuntimeDescriptor(slotName, state) :
                Create(this, (target, s) => target.GetRuntimeDescriptor(slotName, s), state);
        }

        IRuntimeSlot IScriptObject.this[IScriptObject[] args, InterpreterState state]
        {
            get { return IsCompleted ? m_result[args, state] : Create(args, state); }
        }

        ICollection<string> IScriptObject.Slots
        {
            get { return IsCompleted ? m_result.Slots : new string[0]; }
        }

        /// <summary>
        /// Computes a hash code for this object.
        /// </summary>
        /// <returns></returns>
        public sealed override int GetHashCode()
        {
            return m_hashCode;
        }

        /// <summary>
        /// Determines whether this asynchronous object are equal to the specified object.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public sealed override bool Equals(object obj)
        {
            if (obj == null)
                return false;
            if (obj is IScriptObject)
                return m_result != null ? Equals(obj, m_result) : m_hashCode == obj.GetHashCode();
            else return false;
        }
    }
}
