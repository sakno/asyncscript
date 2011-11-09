using System;
using System.Linq.Expressions;

namespace DynamicScript.Runtime.Environment.Threading
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;
    using Compiler.Ast;
    using IScopeVariable = Microsoft.Scripting.IScopeVariable;
    using ValueQueue = System.Collections.Concurrent.ConcurrentQueue<Action<IRuntimeSlot>>;
    using IScriptProducerConsumerCollection = System.Collections.Concurrent.IProducerConsumerCollection<Action<IRuntimeSlot>>;

    /// <summary>
    /// Represents asynchronous object that holds asynchronous task result.
    /// This class cannot be inherited.
    /// </summary>
    [ComVisible(false)]
    public sealed class ScriptLazyObject: ScriptFuture
    {
        #region Nested Types
        /// <summary>
        /// Represents future runtime storage.
        /// This class cannot be inherited.
        /// </summary>
        private abstract class ScriptAsyncSlotBase : ScriptFuture, IRuntimeSlot
        {
            private readonly ValueQueue SetValueQueue;

            protected ScriptAsyncSlotBase(IScriptWorkItemQueue queue, IScriptProxyObject owner, ScriptWorkItem task, InterpreterState state)
                : base(queue, owner, task, state)
            {
                SetValueQueue = new ValueQueue();
            }

            public IScriptObject GetValue(InterpreterState state)
            {
                var value = UnwrapUnsafe(state);
                if (value is IRuntimeSlot)
                    return ((IRuntimeSlot)value).GetValue(state);
                else if (value != null)
                    return value;
                else return Create(OwnerQueue, this, (target, s) => target is IRuntimeSlot ? ((IRuntimeSlot)target).GetValue(s) : target, state);
            }

            private static void SetValue(IRuntimeSlot storage, IScriptProducerConsumerCollection queue)
            {
                foreach (var value in queue)
                    value(storage);
            }

            public void SetValue(IScriptObject value, InterpreterState state)
            {
                var underlyingObject = UnwrapUnsafe(state);
                if (underlyingObject == null)
                    SetValueQueue.Enqueue(s => s.SetValue(value, state));
                else if (underlyingObject is IRuntimeSlot)
                    SetValue((IRuntimeSlot)underlyingObject, SetValueQueue);
            }

            RuntimeSlotAttributes IRuntimeSlot.Attributes
            {
                get { return RuntimeSlotAttributes.Lazy; }
            }

            private static bool DeleteValue(IScriptObject obj)
            {
                return obj is IRuntimeSlot ? ((IRuntimeSlot)obj).DeleteValue() : false;
            }

            bool IScopeVariable.DeleteValue()
            {
                return TryApply<bool>(DeleteValue);
            }

            private static bool HasValue(IScriptObject value)
            {
                return value is IRuntimeSlot ?
                    ((IRuntimeSlot)value).HasValue :
                    value != null;
            }

            bool IScopeVariable.HasValue
            {
                get { return IsCompleted && TryApply<bool>(HasValue); }
            }

            void IScopeVariable.SetValue(object value)
            {
                SetValue(ScriptObject.Convert(value), InterpreterState.Current);
            }

            private static dynamic TryGetValue(IScriptObject obj)
            {
                dynamic result;
                if (obj is IRuntimeSlot)
                    ((IRuntimeSlot)obj).TryGetValue(out result);
                else if (obj != null)
                    result = obj;
                else result = null;
                return result;
            }

            bool IScopeVariable.TryGetValue(out dynamic value)
            {
                value = TryApply<dynamic>(TryGetValue);
                return value != null;
            }

            bool IEquatable<IRuntimeSlot>.Equals(IRuntimeSlot other)
            {
                return base.Equals(other);
            }
        }

        /// <summary>
        /// Represents a implementation of runtime slot.
        /// This class cannot be inherited.
        /// </summary>
        [ComVisible(false)]
        private sealed class ScriptAsyncSlot : ScriptAsyncSlotBase
        {
            public ScriptAsyncSlot(IScriptWorkItemQueue queue, IScriptProxyObject owner, string slotName, InterpreterState state)
                : base(queue, owner, (target, s) => owner[slotName, s], state)
            {
            }

            protected override ScriptFuture Create(IScriptWorkItemQueue queue, IScriptObject target, ScriptWorkItem task, InterpreterState state)
            {
                return new ScriptLazyObject(queue, target, task, state);
            }

            protected override IRuntimeSlot Create(IScriptWorkItemQueue queue, string slotName, InterpreterState state)
            {
                return new ScriptAsyncSlot(queue, this, slotName, state);
            }

            protected override IRuntimeSlot Create(IScriptWorkItemQueue queue, IScriptObject[] indicies, InterpreterState state)
            {
                return new ScriptAsyncIndexer(queue, this, indicies, state);
            }
        }

        [ComVisible(false)]
        private sealed class ScriptAsyncIndexer : ScriptAsyncSlotBase
        {
            public ScriptAsyncIndexer(IScriptWorkItemQueue queue, IScriptProxyObject owner, IScriptObject[] indicies, InterpreterState state)
                : base(queue, owner, (target, s) => target[indicies, s], state)
            {
            }

            protected override ScriptFuture Create(IScriptWorkItemQueue queue, IScriptObject target, ScriptWorkItem task, InterpreterState state)
            {
                return new ScriptLazyObject(queue, target, task, state);
            }

            protected override IRuntimeSlot Create(IScriptWorkItemQueue queue, string slotName, InterpreterState state)
            {
                return new ScriptAsyncSlot(queue, this, slotName, state);
            }

            protected override IRuntimeSlot Create(IScriptWorkItemQueue queue, IScriptObject[] indicies, InterpreterState state)
            {
                return new ScriptAsyncIndexer(queue, this, indicies, state);
            }
        }
        #endregion

        /// <summary>
        /// Creates a new asynchronous object.
        /// </summary>
        /// <param name="queue">Target queue.</param>
        /// <param name="workItem">The delegate that produces the object and will be executed synchronously.</param>
        /// <param name="this">Scope object.</param>
        /// <param name="state">Internal interpreter state.</param>
        public ScriptLazyObject(IScriptWorkItemQueue queue, IScriptObject @this, ScriptWorkItem workItem, InterpreterState state)
            : base(queue, @this, workItem, state)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="queue"></param>
        /// <param name="this"></param>
        /// <param name="workItem"></param>
        /// <param name="state"></param>
        public ScriptLazyObject(IScriptObject queue, IScriptObject @this, ScriptWorkItem workItem, InterpreterState state)
            : this(ThreadManager.CreateQueue(queue), @this, workItem, state)
        {
        }

        #region Runtime Helpers

        internal static NewExpression New(Expression queue, Expression<ScriptWorkItem> task, Expression @this, ParameterExpression stateVar)
        {
            queue = queue != null ? ScriptObject.AsRightSide(queue, stateVar) : LinqHelpers.Null<IScriptObject>();
            @this = ScriptObject.AsRightSide(@this, stateVar);
            var ctor = LinqHelpers.BodyOf<IScriptObject, IScriptObject, ScriptWorkItem, InterpreterState, ScriptLazyObject, NewExpression>((q, o, t, s) => new ScriptLazyObject(q, o, t, s));
            return ctor.Update(new Expression[] { queue, @this, task, stateVar });
        }
        #endregion

        /// <summary>
        /// Creates a new lazy object.
        /// </summary>
        /// <param name="queue"></param>
        /// <param name="target"></param>
        /// <param name="task"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        protected override ScriptFuture Create(IScriptWorkItemQueue queue, IScriptObject target, ScriptWorkItem task, InterpreterState state)
        {
            return new ScriptLazyObject(queue, target, task, state);
        }
        
        /// <summary>
        /// Creates a new lazy slot.
        /// </summary>
        /// <param name="queue"></param>
        /// <param name="slotName"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        protected override IRuntimeSlot Create(IScriptWorkItemQueue queue, string slotName, InterpreterState state)
        {
            return new ScriptAsyncSlot(queue, this, slotName, state);
        }

        /// <summary>
        /// Creates a new lazy indexer.
        /// </summary>
        /// <param name="queue"></param>
        /// <param name="indicies"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        protected override IRuntimeSlot Create(IScriptWorkItemQueue queue, IScriptObject[] indicies, InterpreterState state)
        {
            return new ScriptAsyncIndexer(queue, this, indicies, state);
        }
    }
}
