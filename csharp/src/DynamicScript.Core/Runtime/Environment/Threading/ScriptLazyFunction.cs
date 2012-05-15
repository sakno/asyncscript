using System;
using System.Linq.Expressions;
using System.Threading;
using System.Collections.Generic;
using System.Reflection;

namespace DynamicScript.Runtime.Environment.Threading
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;
    using TransparentActionAttribute = Debugging.TransparentActionAttribute;

    /// <summary>
    /// Represents asynchronous action.
    /// This class cannot be inherited.
    /// </summary>
    [ComVisible(false)]
    sealed class ScriptLazyFunction : ScriptFunctionBase, IScriptLazyFunction
    {
        #region Nested Types
        /// <summary>
        /// Represents work item that executes the passed script object synchronously.
        /// This class cannot be inherited.
        /// </summary>
        [ComVisible(false)]
        private sealed class SynchronousItem
        {
            /// <summary>
            /// Represents synchronous action invocation arguments.
            /// </summary>
            public readonly IList<IScriptObject> Arguments;

            /// <summary>
            /// Initializes a new synchronous work item with the specified set of arguments.
            /// </summary>
            /// <param name="arguments">Invocation arguments.</param>
            public SynchronousItem(IList<IScriptObject> arguments)
            {
                Arguments = arguments ?? EmptyArray;
            }

            public static IScriptObject Invoke(IScriptObject synchronous, IList<IScriptObject> arguments, IScriptObject acceptor, InterpreterState state)
            {
                var result = default(IScriptObject);
                var error = default(IScriptObject);
                try
                {
                    result = synchronous.Invoke(arguments, state);
                }
                catch (Exception e)
                {
                    result = Void;
                    error = e is ScriptFault ? ((ScriptFault)e).Fault : new ScriptWrappedException(e);
                }
                finally
                {
                    if (!IsVoid(acceptor))
                        acceptor.Invoke(new[] { error ?? Void, result }, state);
                }
                return result;
            }

            public IScriptObject Invoke(IScriptObject synchronous, InterpreterState state)
            {
                var acceptor = default(IScriptObject);
                var args = default(IList<IScriptObject>);
                switch (Arguments.Count)
                {
                    case 0: acceptor = null; args = Arguments; break;   //no callback
                    case 1: acceptor = Arguments[0]; args = EmptyArray; break;   //only callback
                    default:
                        args = new IScriptObject[Arguments.Count - 1];
                        acceptor = Arguments[args.Count];               //acceptor is the last argument
                        Arguments.CopyTo(0, args, 0, args.Count);
                        break;
                }
                return Invoke(synchronous, args, acceptor, state);
            }

            public static implicit operator ScriptWorkItem(SynchronousItem item)
            {
                return item != null ? new ScriptWorkItem(item.Invoke) : null;
            }
        }

        [ComVisible(false)]
        private sealed class QueueSlot : AggregatedSlot<ScriptLazyFunction, IScriptObject>
        {
            public const string Name = "queue";


            public override IScriptObject GetValue(ScriptLazyFunction owner, InterpreterState state)
            {
                return owner.Queue is IScriptObject ? (IScriptObject)owner.Queue : new ScriptNativeQueue(owner.Queue);
            }

            public override void SetValue(ScriptLazyFunction owner, IScriptObject value, InterpreterState state)
            {
                owner.Queue = ThreadManager.CreateQueue(value);
            }

            public override IScriptContract GetContractBinding(ScriptLazyFunction owner, InterpreterState state)
            {
                return ScriptSuperContract.Instance;
            }
        }
        #endregion

        private static readonly AggregatedSlotCollection<ScriptLazyFunction> StaticSlots = new AggregatedSlotCollection<ScriptLazyFunction>()
        {
            {QueueSlot.Name, new QueueSlot()}
        };

        private readonly ScriptFunctionBase m_synchronous;
        private IScriptWorkItemQueue m_queue;

        private static IEnumerable<ScriptFunctionContract.Parameter> CreateAsyncParameters(IEnumerable<ScriptFunctionContract.Parameter> parameters, IScriptContract resultType)
        {
            foreach (var p in parameters) yield return p;
            yield return new ScriptFunctionContract.Parameter("acceptor", new ScriptAcceptorContract(IsVoid(resultType) ? null : resultType));
        }

        private static ScriptFunctionContract CreateAsyncContract(ScriptFunctionContract synchronousSignature)
        {
            return new ScriptFunctionContract(CreateAsyncParameters(synchronousSignature.Parameters, synchronousSignature.ReturnValueContract), ScriptAwaitContract.Instance);
        }

        /// <summary>
        /// Initializes a new asynchronous action using the specified synchronous action.
        /// </summary>
        /// <param name="synchronous">Synchronous implementation.</param>
        public ScriptLazyFunction(ScriptFunctionBase synchronous)
            : base(CreateAsyncContract(synchronous.ContractBinding), synchronous.This)
        {
            m_synchronous = synchronous;
        }

        internal static NewExpression New(Expression actionContract, Expression @this, LambdaExpression implementation, string sourceCode)
        {
            var ctor = LinqHelpers.BodyOf<ScriptFunctionBase, ScriptLazyFunction, NewExpression>(a => new ScriptLazyFunction(a));
            return ctor.Update(new[] { ScriptRuntimeFunction.New(actionContract, @this, implementation, sourceCode) });
        }

        /// <summary>
        /// Gets or sets work item queue that is used to enqueue encapsulated synchronous action.
        /// </summary>
        public IScriptWorkItemQueue Queue
        {
            get { return m_queue ?? ThreadManager.Queue; }
            set { m_queue = value; }
        }

        /// <summary>
        /// Enqueues synchronous action.
        /// </summary>
        /// <param name="arguments">Invocation arguments.</param>
        /// <param name="state">Internal interpreter state.</param>
        /// <returns>The lambda object that can be used to synchronize with enqueued lambda.</returns>
        protected override IScriptObject InvokeCore(IList<IScriptObject> arguments, InterpreterState state)
        {
            return ScriptNativeQueue.CreateAwaitLambda(Queue.Enqueue(m_synchronous, new SynchronousItem(arguments), state));
        }

        /// <summary>
        /// Returns parameter metadata.
        /// </summary>
        /// <param name="slotName">The name of the parameter.</param>
        /// <param name="state">Internal interpreter state.</param>
        /// <returns></returns>
        protected override IScriptObject GetSlotMetadata(string slotName, InterpreterState state)
        {
            return StaticSlots.GetSlotMetadata(this, slotName, state);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="slotName"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        public override IScriptObject this[string slotName, InterpreterState state]
        {
            get { return StaticSlots.GetValue(this, slotName, state); }
            set { StaticSlots.SetValue(this, slotName, value, state); }
        }

        /// <summary>
        /// 
        /// </summary>
        public override ICollection<string> Slots
        {
            get { return StaticSlots.Keys; }
        }
    }
}
