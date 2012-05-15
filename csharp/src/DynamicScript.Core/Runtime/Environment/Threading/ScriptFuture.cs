using System;
using System.Collections.Generic;
using System.Threading;
using System.Dynamic;
using System.Threading.Tasks;
using System.Linq.Expressions;

namespace DynamicScript.Runtime.Environment.Threading
{
    using Compiler.Ast;
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;
    using Resources = Properties.Resources;

    /// <summary>
    /// Represetns implementation of future pattern.
    /// </summary>
    /// <seealso href="http://en.wikipedia.org/wiki/Future_%28programming%29"/>
    [ComVisible(false)]
    public class ScriptFuture: DynamicObject, IScriptProxyObject
    {
        /// <summary>
        /// Represents maximum timeout for the future wrapper.
        /// </summary>
        public static TimeSpan MaxTimeout = TimeSpan.FromMinutes(10);
        private IScriptContract m_requirement;
        private readonly int m_hashCode;

        /// <summary>
        /// Represents a delegate that can be used to synchronize with the current object.
        /// </summary>
        public readonly IWorkItemState<TimeSpan, IScriptObject> Await;

        /// <summary>
        /// Represents a queue in which the future object is located.
        /// </summary>
        protected readonly IScriptWorkItemQueue OwnerQueue;

        /// <summary>
        /// Initializes a new instance of the Future pattern implementation.
        /// </summary>
        /// <param name="queue">Target queue that is used to allocate a new user work item. Cannot be <see langword="null"/>.</param>
        /// <param name="target">The target object passed to the task in the parallel thread.</param>
        /// <param name="workItem">A task implementation to be executed in the parallel thread.</param>
        /// <param name="state">Internal interpreter state.</param>
        /// <exception cref="System.ArgumentNullException"><paramref name="queue"/> is <see langword="null"/>.</exception>
        protected ScriptFuture(IScriptWorkItemQueue queue, IScriptObject target, ScriptWorkItem workItem, InterpreterState state)
        {
            if (queue == null) throw new ArgumentNullException("queue");
            OwnerQueue = queue;
            m_hashCode = workItem.GetHashCode();
            Await = queue.Enqueue(target, workItem, state);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="queue"></param>
        /// <param name="this"></param>
        /// <param name="workItem"></param>
        /// <param name="state"></param>
        public ScriptFuture(IScriptObject queue, IScriptObject @this, ScriptWorkItem workItem, InterpreterState state)
            : this(ThreadManager.CreateQueue(queue), @this, workItem, state)
        {
        }

        /// <summary>
        /// Creates a new instance of the future object.
        /// </summary>
        /// <param name="queue">Target queue.</param>
        /// <param name="target">An object to be passed into the task.</param>
        /// <param name="task">The task that implements computation logic.</param>
        /// <param name="state">Internal interpreter state.</param>
        /// <returns>A new future script object.</returns>
        protected virtual ScriptFuture Create(IScriptWorkItemQueue queue, IScriptObject target, ScriptWorkItem task, InterpreterState state)
        {
            return new ScriptFuture(queue, target, task, state);
        }

        /// <summary>
        /// Applies additional operations on the synchronized object.
        /// </summary>
        /// <param name="result"></param>
        /// <param name="state"></param>
        protected virtual void ProcessResult(ref IScriptObject result, InterpreterState state)
        {
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

        IScriptObject IScriptProxyObject.Enqueue(IScriptObject left, ScriptCodeBinaryOperatorType @operator, InterpreterState state)
        {
            return Await.IsCompleted ? left.BinaryOperation(@operator, Await.Result, state) :
                Create(OwnerQueue, this, (right, s) => left.BinaryOperation(@operator, right, s), state);
        }

        /// <summary>
        /// Attempts to apply the specified function to the synchronized value.
        /// </summary>
        /// <typeparam name="TResult">Type of the function result.</typeparam>
        /// <param name="f">A function to be applied to the synchronized value.</param>
        /// <returns>A function invocation result.</returns>
        protected TResult TryApply<TResult>(Func<IScriptObject, TResult> f)
        {
            return Await.IsCompleted ? f(Await.Result) : default(TResult);
        }

        /// <summary>
        /// Obtains an asynchronous value without synchronization.
        /// </summary>
        /// <param name="state">Internal interpreter state.</param>
        /// <returns>An unwrapped value if the result is synchronized; otherwise, <see langword="null"/>.</returns>
        protected IScriptObject UnwrapUnsafe(InterpreterState state)
        {
            var result = Await.Result;
            if (result == null)
                return null;
            else if (m_requirement == null)
                return result;
            else if (RuntimeHelpers.IsCompatible(m_requirement, result))
                return result;
            else throw new ContractBindingException(result, m_requirement, state);
        }

        #region Runtime Helpers        
        internal static NewExpression New(Expression queue, Expression<ScriptWorkItem> task, Expression @this, ParameterExpression stateVar)
        {
            queue = queue != null ? ScriptObject.AsRightSide(queue, stateVar) : ScriptObject.Null;
            @this = ScriptObject.AsRightSide(@this, stateVar);
            var ctor = LinqHelpers.BodyOf<IScriptObject, IScriptObject, ScriptWorkItem, InterpreterState, ScriptFuture, NewExpression>((q, o, t, s) => new ScriptFuture(q, o, t, s));
            return ctor.Update(new Expression[] { queue, @this, task, stateVar });
        }     
        #endregion

        /// <summary>
        /// Synchronizes with the underlying value.
        /// </summary>
        /// <param name="timeout">A caller thread blocking timeout.</param>
        /// <param name="result">A result obtained during synchronization.</param>
        /// <param name="state">Internal interpreter state.</param>
        /// <returns><see langword="true"/> if the script object is obtained during timeout; otherwise, <see langword="false"/>.</returns>
        public bool Unwrap(TimeSpan timeout, out IScriptObject result, InterpreterState state)
        {
            switch (Await.WaitOne(timeout))
            {
                case true:
                    result = UnwrapUnsafe(state);
                    return true;
                default:
                    result = null;
                    return false;
            }
        }

        /// <summary>
        /// Synchronizes with the underlying value.
        /// </summary>
        /// <param name="state">Internal interpreter state.</param>
        /// <returns><see langword="true"/> if the script object is obtained during timeout; otherwise, <see langword="false"/>.</returns>
        public IScriptObject Unwrap(InterpreterState state)
        {
            var result = default(IScriptObject);
            return Unwrap(MaxTimeout, out result, state) ? result : ScriptObject.Void;
        }

        /// <summary>
        /// Enqueues a new binary operation.
        /// </summary>
        /// <param name="operator">A binary operator to be applied to the synchronized object as left operand and to the specified
        /// right operand.</param>
        /// <param name="right">The right operand of the operation.</param>
        /// <param name="state">Internal interpreter state.</param>
        /// <returns></returns>
        protected IScriptObject BinaryOperation(ScriptCodeBinaryOperatorType @operator, IScriptObject right, InterpreterState state)
        {
            return Await.IsCompleted ?
                Await.Result.BinaryOperation(@operator, right, state) :
                Create(OwnerQueue, this, (left, s) => left.BinaryOperation(@operator, right, s), state);
        }

        IScriptObject IScriptObject.BinaryOperation(ScriptCodeBinaryOperatorType @operator, IScriptObject right, InterpreterState state)
        {
            return BinaryOperation(@operator, right, state);
        }

        IScriptObject IScriptObject.UnaryOperation(ScriptCodeUnaryOperatorType @operator, InterpreterState state)
        {
            return Await.IsCompleted ? Await.Result.UnaryOperation(@operator, state) :
                Create(OwnerQueue, this, (operand, s) => operand.UnaryOperation(@operator, s), state);
        }

        /// <summary>
        /// Gets a value indicating whether this object is synchronized.
        /// </summary>
        public bool IsCompleted
        {
            get { return Await.IsCompleted; }
        }

        /// <summary>
        /// Returns an expected contract.
        /// </summary>
        /// <returns></returns>
        public IScriptContract GetContractBinding()
        {
            switch (Await.IsCompleted)
            {
                case true: return Await.Result.GetContractBinding();
                default: return m_requirement ?? ScriptSuperContract.Instance;
            }
        }

        IScriptObject IScriptObject.Invoke(IList<IScriptObject> args, InterpreterState state)
        {
            return Await.IsCompleted ?
                Await.Result.Invoke(args, state) :
                Create(OwnerQueue, this, (target, s) => target.Invoke(args, s), state);
        }

        IScriptObject IScriptObject.this[string slotName, InterpreterState state]
        {
            get { return Await.IsCompleted ? Await.Result[slotName, state] : Create(OwnerQueue, this, (obj, s) => obj[slotName, s], state); }
            set
            {
                if (Await.IsCompleted) Await.Result[slotName, state] = value;
                else Create(OwnerQueue, this, (obj, s) => obj[slotName, s] = value, state);
            }
        }

        IScriptObject IScriptObject.this[IList<IScriptObject> indicies, InterpreterState state]
        {
            get { return Await.IsCompleted ? Await.Result[indicies, state] : Create(OwnerQueue, this, (obj, s) => obj[indicies, s], state); }
            set
            {
                if (Await.IsCompleted) Await.Result[indicies, state] = value;
                else Create(OwnerQueue, this, (obj, s) => obj[indicies, s] = value, state);
            }
        }

        /// <summary>
        /// Gets a collection of object members.
        /// </summary>
        public ICollection<string> Slots
        {
            get { return IsCompleted ? Await.Result.Slots : new string[0]; }
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
                return Await.IsCompleted ? Equals(obj, Await.Result) : m_hashCode == obj.GetHashCode();
            else return false;
        }

        /// <summary>
        /// Returns a string representation of this asynchronous object.
        /// </summary>
        /// <returns></returns>
        public sealed override string ToString()
        {
            return Await.IsCompleted ? Await.Result.ToString() : Resources.StillRunning;
        }

        /// <summary>
        /// Returns a collection of the object members.
        /// </summary>
        /// <returns></returns>
        public sealed override IEnumerable<string> GetDynamicMemberNames()
        {
            return Slots;
        }

        /// <summary>
        /// Converts the current object to the specified type in the dynamic context.
        /// </summary>
        /// <param name="binder"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public sealed override bool TryConvert(ConvertBinder binder, out object result)
        {
            switch (binder.ReturnType.Is<IScriptObject, IScriptProxyObject>())
            {
                case true:
                    result = this;
                    return true;
                default:
                    return ScriptObject.TryConvert(Unwrap(binder.GetState()), binder, out result);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="binder"></param>
        /// <param name="arg"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public sealed override bool TryBinaryOperation(BinaryOperationBinder binder, object arg, out object result)
        {
            return ScriptObject.TryBinaryOperation(this, binder, arg, out result);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="binder"></param>
        /// <param name="indexes"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public sealed override bool TryGetIndex(GetIndexBinder binder, object[] indexes, out object result)
        {
            return ScriptObject.TryGetIndex(this, binder, indexes, out result);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="binder"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public sealed override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            return ScriptObject.TryGetMember(this, binder, out result);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="binder"></param>
        /// <param name="args"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public sealed override bool TryInvoke(InvokeBinder binder, object[] args, out object result)
        {
            return ScriptObject.TryInvoke(this, binder, args, out result);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="binder"></param>
        /// <param name="args"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public sealed override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
        {
            return ScriptObject.TryInvokeMember(this, binder, args, out result);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="binder"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public sealed override bool TryUnaryOperation(UnaryOperationBinder binder, out object result)
        {
            return ScriptObject.TryUnaryOperation(this, binder, out result);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="binder"></param>
        /// <param name="indexes"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public sealed override bool TrySetIndex(SetIndexBinder binder, object[] indexes, object value)
        {
            return ScriptObject.TrySetIndex(this, binder, indexes, value);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="binder"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public sealed override bool TrySetMember(SetMemberBinder binder, object value)
        {
            return ScriptObject.TrySetMember(this, binder, value);
        }
    }
}
