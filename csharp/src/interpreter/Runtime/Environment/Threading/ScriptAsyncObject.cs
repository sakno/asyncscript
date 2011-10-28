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
    public sealed class ScriptAsyncObject: ScriptFuture
    {
        #region Nested Types
        /// <summary>
        /// Represents future runtime storage.
        /// This class cannot be inherited.
        /// </summary>
        private abstract class ScriptAsyncSlotBase : ScriptFuture, IRuntimeSlot
        {
            private readonly ValueQueue SetValueQueue;

            protected ScriptAsyncSlotBase(IScriptAsyncObject owner, Func<IScriptObject, InterpreterState, IScriptObject> task, InterpreterState state)
                : base(owner, task, state)
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
                else return Create(this, (target, s) => target is IRuntimeSlot ? ((IRuntimeSlot)target).GetValue(s) : target, state);
            }

            private static void SetValue(IRuntimeSlot storage, IScriptProducerConsumerCollection queue)
            {
                foreach (var value in queue)
                    value(storage);
            }

            void IRuntimeSlot.SetValue(IScriptObject value, InterpreterState state)
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

            public void SetValue(object value)
            {
                throw new NotImplementedException();
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
         
        }
        #endregion

        /// <summary>
        /// Creates a new asynchronous object.
        /// </summary>
        /// <param name="task">The delegate that produces the object and will be executed synchronously.</param>
        /// <param name="this">Scope object.</param>
        /// <param name="state">Internal interpreter state.</param>
        public ScriptAsyncObject(IScriptObject @this, Func<IScriptObject, InterpreterState, IScriptObject> task, InterpreterState state)
            : base(@this, task, state)
        {
        }

        #region Runtime Helpers

        internal static NewExpression New(Expression<Func<IScriptObject, InterpreterState, IScriptObject>> task, Expression @this, ParameterExpression stateVar)
        {
            var ctor = LinqHelpers.BodyOf<IScriptObject, Func<IScriptObject, InterpreterState, IScriptObject>, InterpreterState, ScriptAsyncObject, NewExpression>((o, t, s) => new ScriptAsyncObject(o, t, s));
            return ctor.Update(new Expression[] { task, @this, stateVar });
        }
        #endregion


        protected override ScriptFuture Create(IScriptObject target, Func<IScriptObject, InterpreterState, IScriptObject> task, InterpreterState state)
        {
            return new ScriptAsyncObject(target, task, state);
        }

        protected override IRuntimeSlot Create(string slotName, InterpreterState state)
        {
            throw new NotImplementedException();
        }

        protected override IRuntimeSlot Create(IScriptObject[] indicies, InterpreterState state)
        {
            throw new NotImplementedException();
        }
    }
}
