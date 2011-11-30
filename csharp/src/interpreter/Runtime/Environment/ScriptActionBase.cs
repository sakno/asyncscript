using System;
using System.Runtime.Serialization;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using System.IO;

namespace DynamicScript.Runtime.Environment
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;
    using TransparentActionAttribute = Debugging.TransparentActionAttribute;
    using CallInfo = System.Dynamic.CallInfo;
    using InterpretationContext = Compiler.Ast.InterpretationContext;
    using CallStack = Debugging.CallStack;
    using SystemConverter = System.Convert;
    using MethodInfo = System.Reflection.MethodInfo;
    using Encoding = System.Text.Encoding;

    /// <summary>
    /// Represents action implementation.
    /// </summary>
    [ComVisible(false)]
    public abstract class ScriptActionBase : ScriptObjectWithStaticBinding<ScriptActionContract>, IScriptAction, IScriptActionSlots
    {
        #region Nested Types
        /// <summary>
        /// Represents a delegate for native implementation of the action.
        /// </summary>
        /// <param name="state">Internal interpreter state.</param>
        /// <param name="args">The arguments of the action.</param>
        /// <returns>The invocation result.</returns>
        [ComVisible(false)]
        public delegate IScriptObject ActionInvoker(InterpreterState state, params IRuntimeSlot[] args);

        [ComVisible(false)]
        internal interface IComposition : IScriptAction
        {
            IScriptAction Left { get; }
            IScriptAction Right { get; }
        }

        /// <summary>
        /// Represents action signature analyzer that uses automatic parallelism.
        /// </summary>
        [ComVisible(false)]
        internal abstract class SignatureAnalyzer
        {
            public readonly IList<ScriptActionContract.Parameter> Parameters;
            public readonly IList<IScriptObject> Arguments;
            public readonly InterpreterState State;

            protected SignatureAnalyzer(IList<ScriptActionContract.Parameter> @params, IList<IScriptObject> args, InterpreterState s)
            {
                Parameters = @params;
                Arguments = args;
                State = s;
            }

            protected abstract void Analyze(ParallelLoopState state, int index, ScriptActionContract.Parameter p, IScriptObject a);

            private void Analyze(int index, ParallelLoopState state)
            {
                Analyze(state, index, Parameters[index], Arguments[index]);
            }

            public void Analyze()
            {
                Parallel.For(0, Math.Min(Parameters.Count, Arguments.Count), Analyze);
            }
        }

        [ComVisible(false)]
        private sealed class ParameterCompatibilityAnalyzer : SignatureAnalyzer
        {
            private object m_error;

            public ParameterCompatibilityAnalyzer(IList<ScriptActionContract.Parameter> @params, IList<IScriptObject> args, InterpreterState s)
                : base(@params, args, s)
            {
                m_error = null;
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            private void Complete(ParallelLoopState state, ScriptActionContract.Parameter p, IScriptObject a)
            {
                m_error = State == null ? (object)string.Empty : new ContractBindingException(a, p.ContractBinding, State);
            }

            protected override void Analyze(ParallelLoopState state, int index, ScriptActionContract.Parameter p, IScriptObject a)
            {
                if (!IsCompatible(p, a))
                {
                    Complete(state, p, a);
                    state.Break();
                }
            }

            public new bool Analyze()
            {
                base.Analyze();
                if (m_error == null) return true;
                else if (m_error is Exception) throw (Exception)m_error;
                else return false;
            }

            public static bool IsCompatible(ScriptActionContract.Parameter p, IScriptObject a, InterpreterState state)
            {
                switch (IsCompatible(p, a))
                {
                    case true: return true;
                    default: if (state == null) return false;
                        else throw new ContractBindingException(a, p.ContractBinding, state);
                }
            }

            public static bool IsCompatible(ScriptActionContract.Parameter p, IScriptObject a)
            {
                return p.ContractBinding.IsCompatible(a);
            }
        }

        /// <summary>
        /// Represents composed action.
        /// This class cannot be inherited.
        /// </summary>
        [ComVisible(false)]
        [TransparentAction]
        private sealed class Composition : ScriptActionBase, IComposition
        {
            private const string LeftSlotName = "left";
            private const string RightSlotName = "right";
            public readonly IScriptAction Left;
            public readonly IScriptAction Right;
            private readonly bool NeedCurrying;

            private static ScriptActionContract ConstructorHelper(IScriptAction left, IScriptAction right, out bool noCarrying)
            {
                noCarrying = IsVoid(left.ReturnValueContract) || right.SignatureInfo.ArgumentCount == 0;
                var leftContract = (ScriptActionContract)left.GetContractBinding();
                var rightContract = (ScriptActionContract)right.GetContractBinding();
                return new ScriptActionContract(leftContract.Parameters.Concat(noCarrying ? rightContract.Parameters : rightContract.Parameters.Skip(1)), right.ReturnValueContract);
            }

            private Composition(Composition composition, IScriptObject @this)
                : base(composition.ContractBinding)
            {
                Left = composition.Left.Bind(@this);
                Right = composition.Right.Bind(@this);
                NeedCurrying = composition.NeedCurrying;
            }

            private Composition(IScriptAction left, IScriptAction right, bool dummy)
                : base(ConstructorHelper(left, right, out dummy), null)
            {
                Left = left;
                Right = right;
                NeedCurrying = !dummy;
            }

            public Composition(IScriptAction left, IScriptAction right)
                : this(left, right, false)
            {
            }

            protected override IScriptObject InvokeCore(IList<IScriptObject> arguments, InterpreterState state)
            {
                //adjustment is used for arguments array splitting based on the return value carrying
                var adjustment = SystemConverter.ToInt32(NeedCurrying);
                //check whether the count of arguments is valid
                if (arguments.Count != Left.SignatureInfo.ArgumentCount + Right.SignatureInfo.ArgumentCount - adjustment)
                    throw new ActionArgumentsMistmatchException(state);
                var buffer = new IScriptObject[Left.SignatureInfo.ArgumentCount];
                arguments.CopyTo(0, buffer, 0, buffer.Length);
                var result = Left.Invoke(buffer, state);
                buffer = new IScriptObject[Right.SignatureInfo.ArgumentCount];
                arguments.CopyTo(Left.SignatureInfo.ArgumentCount, buffer, adjustment, Right.SignatureInfo.ArgumentCount - adjustment);
                if (NeedCurrying) buffer[0] = result;
                return Right.Invoke(buffer, state);
            }

            public override ScriptActionBase Bind(IScriptObject @this)
            {
                return new Composition(this, @this);   
            }

            public override IRuntimeSlot this[string slotName, InterpreterState state]
            {
                get
                {
                    if (StringEqualityComparer.Equals(LeftSlotName, slotName))
                        return new ScriptConstant(Left);
                    else if (StringEqualityComparer.Equals(RightSlotName, slotName))
                        return new ScriptConstant(Right);
                    else return base[slotName, state];
                }
            }

            IScriptAction IComposition.Left
            {
                get { return Left; }
            }

            IScriptAction IComposition.Right
            {
                get { return Right; }
            }
        }

        [ComVisible(false)]
        [TransparentAction]
        private sealed class UnifiedAction : ScriptActionBase
        {
            private readonly IScriptObject[] m_cache;
            private readonly int m_matchedCount;
            private readonly IScriptAction m_invoker;

            private static ScriptActionContract ConstructorHelper(ScriptActionBase implementation, IScriptCompositeObject args, InterpreterState state, out IScriptObject[] cache, out int matchedCount)
            {
                var parameters = new List<ScriptActionContract.Parameter>(implementation.Parameters);
                cache = new IScriptObject[implementation.Parameters.Count];
                matchedCount = 0;
                foreach (var a in args.GetSlotValues(state))
                    for (var i = 0; i < implementation.Parameters.Count; i++)
                    {
                        var p = implementation.Parameters[i];
                        if (p.Equals(a.Key))
                        {
                            cache[i] = a.Value;
                            matchedCount += 1;
                            parameters.Remove(p);
                        }
                    }
                return matchedCount > 0 ? new ScriptActionContract(parameters, implementation.ReturnValueContract) : null;
            }

            private UnifiedAction(ScriptActionBase implementation, IScriptCompositeObject args, InterpreterState state, IScriptObject[] cache, int matchedCount)
                : base(ConstructorHelper(implementation, args, state, out cache, out matchedCount), null)
            {
                m_cache = cache;
                m_matchedCount = matchedCount;
                m_invoker = implementation;
            }

            public UnifiedAction(ScriptActionBase implementation, IScriptCompositeObject args, InterpreterState state)
                : this(implementation, args, state, null, 0)
            {

            }

            protected override IScriptObject InvokeCore(IList<IScriptObject> arguments, InterpreterState state)
            {
                switch (m_cache.LongLength - m_matchedCount == arguments.Count)
                {
                    case true:
                        var clonedCache = (IScriptObject[])m_cache.Clone();
                        Parallel.For<int>(0, clonedCache.Length, () => 0,
                            delegate(int i, ParallelLoopState ls, int j)
                            {
                                if (clonedCache[i] == null)
                                    clonedCache[i] = arguments[j++];
                                return j;
                            }, j => { });
                        return m_invoker.Invoke(clonedCache, state);
                    default: throw new ActionArgumentsMistmatchException(state);
                }
            }
        }

        [ComVisible(false)]
        [TransparentAction]
        private sealed class ConstantProvider : ScriptActionBase
        {
            public readonly IScriptObject Value;

            public ConstantProvider(IScriptObject value)
                : base(new ScriptActionContract(new ScriptActionContract.Parameter[0], value.GetContractBinding()), null)
            {
                Value = value;
            }

            protected override IScriptObject InvokeCore(IList<IScriptObject> args, InterpreterState state)
            {
                return Value;
            }
        }

        /// <summary>
        /// Represents combination of two or more actions.
        /// </summary>
        [ComVisible(false)]
        public interface ICombination : IScriptObject, IEnumerable<IScriptAction>
        {
            /// <summary>
            /// Gets a collection of combined actions.
            /// </summary>
            IEnumerable<IScriptAction> Actions { get; }
        }

        [ComVisible(false)]
        private sealed class MostRelevantActionSearcher : ParallelSearch<IList<IScriptObject>, IScriptAction, IScriptAction>
        {
            public MostRelevantActionSearcher(IList<IScriptObject> arguments)
                : base(arguments)
            {
            }

            protected override bool Match(IScriptAction target, IList<IScriptObject> args, out IScriptAction result)
            {
                switch (target.CanInvoke(args))
                {
                    case true:
                        result = target; return true;
                    default:
                        result = null; return false;
                }
            }

            public static IScriptAction Find(IEnumerable<IScriptAction> actions, IList<IScriptObject> args)
            {
                var searcher = new MostRelevantActionSearcher(args);
                searcher.Find(actions);
                return searcher.Result;
            }
        }

        [ComVisible(false)]
        private sealed class Combination : ScriptObject, ICombination
        {
            public readonly IEnumerable<IScriptAction> Actions;
            private readonly IScriptContract m_contract;

            public Combination(IEnumerable<IScriptAction> actions, InterpreterState state)
            {
                m_contract = ScriptContract.Unite(actions.Select(a => a.GetContractBinding()), state);
                Actions = actions;
            }

            public Combination(IScriptAction a, IEnumerable<IScriptAction> actions, InterpreterState state)
                : this(Combine(a, actions), state)
            {
            }

            private static IEnumerable<IScriptAction> Combine(IScriptAction func, IEnumerable<IScriptAction> actions)
            {
                foreach (var a in actions) yield return a;
                yield return func;
            }

            IEnumerable<IScriptAction> ICombination.Actions
            {
                get { return Actions; }
            }

            /// <summary>
            /// Invokes combined action.
            /// </summary>
            /// <param name="args">The arguments for the action.</param>
            /// <param name="state">Internal interpreter state.</param>
            /// <returns>Invocation result.</returns>
            /// <exception cref="ActionArgumentsMistmatchException">No one action in the collection can be invoked with specified arguments.</exception>
            public override IScriptObject Invoke(IList<IScriptObject> args, InterpreterState state)
            {
                var target = MostRelevantActionSearcher.Find(Actions, args);
                switch (target != null)
                {
                    case true: return target.Invoke(args, state);
                    default: throw new ActionArgumentsMistmatchException(state);
                }
            }

            /// <summary>
            /// Returns contract binding of the action list.
            /// </summary>
            /// <returns></returns>
            public override IScriptContract GetContractBinding()
            {
                return m_contract;
            }

            protected override IScriptObject Add(IScriptObject right, InterpreterState state)
            {
                if (ReferenceEquals(this, right))
                    return this;
                else if (right is IScriptAction)
                    return new Combination(Combine((IScriptAction)right, Actions), state);
                else if (right is ICombination)
                    return new Combination(Enumerable.Concat(Actions, ((ICombination)right).Actions), state);
                else if (state.Context == InterpretationContext.Unchecked)
                    return this;
                else throw new UnsupportedOperationException(state);
            }

            protected override IScriptObject Subtract(IScriptObject right, InterpreterState state)
            {
                if (ReferenceEquals(this, right))
                    return Void;
                else if (right is IScriptAction)
                    return new Combination(from a in Actions where !ReferenceEquals(a, right) select a, state);
                else if (right is ICombination)
                {
                    var elems = Enumerable.ToArray(Enumerable.Intersect(Actions, ((ICombination)right).Actions));
                    switch (elems.LongLength)
                    {
                        case 0L: return Void;
                        case 1L: return elems[0];
                        default: return new Combination(elems, state);
                    }
                }
                else if (state.Context == InterpretationContext.Unchecked)
                    return this;
                else throw new UnsupportedOperationException(state);
            }

            public override RuntimeSlotBase this[IScriptObject[] args, InterpreterState state]
            {
                get
                {
                    foreach (var a in Actions)
                    {
                        var result = a[args, state];
                        if (RuntimeSlotBase.IsMissing(result)) continue;
                        else if (result is RuntimeSlotBase) return (RuntimeSlotBase)result;
                    }
                    return base[args, state];
                }
            }

            public override IRuntimeSlot this[string slotName, InterpreterState state]
            {
                get
                {
                    if (StringEqualityComparer.Equals(slotName, IteratorAction))
                        return new ScriptConstant(new ScriptIterator(Actions));
                    else foreach (var a in Actions)
                    {
                        var result = a[slotName, state];
                        if (RuntimeSlotBase.IsMissing(result)) continue;
                        else if (result is RuntimeSlotBase) return (RuntimeSlotBase)result;
                    }
                    return base[slotName, state];
                }
            }

            /// <summary>
            /// Returns an enumerator through combined actions.
            /// </summary>
            /// <returns>An enumerator through combined actions.</returns>
            public new IEnumerator<IScriptAction> GetEnumerator()
            {
                return Actions.GetEnumerator();
            }

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }

        [ComVisible(false)]
        [TransparentAction]
        internal class WrappedAction : ScriptActionBase
        {
            public readonly new ScriptActionBase Implementation;

            public WrappedAction(ScriptActionBase implementation, IScriptObject @this)
                : base(implementation, @this)
            {
                Implementation = implementation;
            }

            protected override IScriptObject InvokeCore(IList<IScriptObject> args, InterpreterState state)
            {
                return Implementation.InvokeCore(args, state);
            }

            public override ScriptActionBase Bind(IScriptObject @this)
            {
                return new WrappedAction(Implementation, @this);
            }
        }
        #endregion

        /// <summary>
        /// Represents an owner of this action.
        /// </summary>
        public readonly IScriptObject This;
        internal readonly bool VoidReturn;
        private IRuntimeSlot m_owner;

        /// <summary>
        /// Indicates that the current action is not visible from call stack.
        /// </summary>
        public readonly bool IsTransparent;

        /// <summary>
        /// Initializes a new action.
        /// </summary>
        /// <param name="contract">Action contract description. Cannot be <see langword="null"/>.</param>
        /// <param name="this">Action owner.</param>
        protected ScriptActionBase(ScriptActionContract contract, IScriptObject @this = null)
            : base(contract)
        {
            This = @this ?? Void; 
            IsTransparent = TransparentActionAttribute.IsDefined(GetType());
            VoidReturn = IsVoid(contract.ReturnValueContract);
        }

        internal ScriptActionBase(ScriptActionBase action, IScriptObject @this = null)
            : this(action.ContractBinding, @this ?? action.This)
        {
        }

        /// <summary>
        /// Gets action owner.
        /// </summary>
        IScriptObject IScriptAction.This
        {
            get { return This; }
        }

        /// <summary>
        /// Gets contract of the returning value.
        /// </summary>
        public IScriptContract ReturnValueContract
        {
            get { return ContractBinding.ReturnValueContract; }
        }

        /// <summary>
        /// Gets parameters of the action.
        /// </summary>
        public ReadOnlyCollection<ScriptActionContract.Parameter> Parameters
        {
            get { return ContractBinding.Parameters; }
        }

        /// <summary>
        /// Gets contract of the action.
        /// </summary>
        /// <returns>The contract of the action.</returns>
        public new ScriptActionContract GetContractBinding()
        {
            return ContractBinding;
        }

        /// <summary>
        /// Gets implementation of this action.
        /// </summary>
        public Func<IList<IScriptObject>, InterpreterState, IScriptObject> Implementation
        {
            get { return new Func<IList<IScriptObject>, InterpreterState, IScriptObject>(InvokeCore); } 
        }

        Delegate IScriptAction.Implementation
        {
            get { return Implementation; }
        }

        /// <summary>
        /// Determines whether the action can be executed with the specified set of arguments.
        /// </summary>
        /// <param name="args">Set of arguments for action invocation.</param>
        /// <param name="state">Internal interpreter state.</param>
        /// <returns><see langword="true"/> if the action can be executed with the specified set of arguments; otherwise, <see langword="false"/>.</returns>
        private bool CanInvoke(IList<IScriptObject> args, InterpreterState state)
        {
            var @params = Parameters;
            if (@params.Count == args.Count)
                switch (@params.Count)
                {
                    case 0: return true;
                    case 1: return ParameterCompatibilityAnalyzer.IsCompatible(@params[0], args[0], state);
                    default:
                        var analyzer = new ParameterCompatibilityAnalyzer(@params, args, state);
                        return analyzer.Analyze();
                }
            else if (state == null) return false;
            else throw new ActionArgumentsMistmatchException(state);
        }

        private T Unwrap<T>(IScriptObject a, int index, InterpreterState state)
            where T : class, IScriptObject
        {
            if (a is IRuntimeSlot)
                a = ((IRuntimeSlot)a).GetValue(state);
            return Parameters[index].ContractBinding.Convert(Conversion.Implicit, a, state) as T;
        }

        /// <summary>
        /// Unwraps an action argument.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="args"></param>
        /// <param name="index"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        protected T Unwrap<T>(IList<IScriptObject> args, int index, InterpreterState state)
            where T : class, IScriptObject
        {
            return Unwrap<T>(args[index], index, state);
        }

        /// <summary>
        /// Determines whether the action can be executed with the specified set of arguments.
        /// </summary>
        /// <param name="args">Set of arguments for action invocation.</param>
        /// <returns><see langword="true"/> if the action can be executed with the specified set of arguments; otherwise, <see langword="false"/>.</returns>
        public bool CanInvoke(IList<IScriptObject> args)
        {
            return CanInvoke(args, null);
        }

        private static IScriptObject BindResult(IScriptContract returnContract, IScriptObject returnValue, InterpreterState state)
        {
            switch (returnContract.IsCompatible(returnValue))
            {
                case true: return returnContract.Convert(Conversion.Implicit, returnValue, state);
                default: throw new ContractBindingException(returnValue, returnContract, state);
            }
        }

        /// <summary>
        /// Invokes the current action.
        /// </summary>
        /// <param name="args"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        protected abstract IScriptObject InvokeCore(IList<IScriptObject> args, InterpreterState state);

        /// <summary>
        /// Invokes the action.
        /// </summary>
        /// <param name="args">Action arguments.</param>
        /// <param name="state">Internal interpreter state.</param>
        /// <returns>Invocation result.</returns>
        /// <exception cref="ContractBindingException">The contract of the argument doesn't match to the parameter restriction.</exception>
        /// <exception cref="ActionArgumentsMistmatchException">The count of arguments doesn't match to the count of formal parameters.</exception>
        public sealed override IScriptObject Invoke(IList<IScriptObject> args, InterpreterState state)
        {
            CanInvoke(args, state);
            //Invokes compiled implementation.
            var result = default(IScriptObject);
            var previous = InvocationContext.SetCurrent(this);
            Push(state);  //register the action in the call stack
            try
            {
                result = InvokeCore(args, state) ?? Void;
            }
#if !DEBUG
            catch (System.Reflection.TargetInvocationException e)
            {
                throw e.InnerException;
            }
            catch (NullReferenceException)
            {
                throw new VoidException(state);
            }
#endif
            finally
            {
                Pop();
                InvocationContext.Current = previous;
            }
            return VoidReturn ? Void : BindResult(ReturnValueContract, result, state);
        }

        CallInfo IScriptAction.SignatureInfo
        {
            get
            {
                var @params = Parameters.Select(c => c.Name).ToArray();
                return new CallInfo(@params.Length, @params);
            }
        }

        /// <summary>
        /// Gets a value indicating that this action is produced from composition of two or more
        /// actions.
        /// </summary>
        public bool IsComposition
        {
            get { return this is IComposition; }
        }

        private static void Decompose(IComposition composition, out IScriptAction left, out IScriptAction right)
        {
            left = composition.Left;
            right = composition.Right;
        }

        /// <summary>
        /// Divides this action on its components.
        /// </summary>
        /// <param name="left">The left part of composition.</param>
        /// <param name="right">The right part of composition.</param>
        /// <returns><see langword="true"/> if this action is composite action; otherwise, <see langword="false"/>.</returns>
        public bool Decompose(out IScriptAction left, out IScriptAction right)
        {
            switch (this is IComposition)
            {
                case true:
                    Decompose((IComposition)this, out left, out right);
                    return true;
                default:
                    left = right = this;
                    return false;
            }
        }

        /// <summary>
        /// Composes this action with the specified action.
        /// </summary>
        /// <param name="right">The action to be composed with this action.</param>
        /// <param name="result">The composite action.</param>
        /// <returns><see langword="true"/> if composition is applicable; otherwise, <see langword="false"/>.</returns>
        public bool Compose(IScriptAction right, out IScriptAction result)
        {
            switch (ContractBinding.IsComposable((ScriptActionContract)right.GetContractBinding()))
            {
                case true:
                    result = new Composition(this, right);
                    return true;
                default:
                    result = this;
                    return false;
            }
        }

        private IScriptObject Compose(IScriptAction right, InterpreterState state)
        {
            var result = default(IScriptAction);
            if (Compose(right, out result))
                return result;
            else if (state.Context == InterpretationContext.Unchecked)
                return ScriptObject.Void;
            else throw new UnsupportedOperationException(state);
        }

        /// <summary>
        /// Returns composition between the current action and the specified action.
        /// </summary>
        /// <param name="right"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        protected sealed override IScriptObject Multiply(IScriptObject right, InterpreterState state)
        {
            if (right is IScriptAction)
                return Compose((IScriptAction)right, state);
            else if (state.Context == InterpretationContext.Unchecked)
                return Void;
            else throw new UnsupportedOperationException(state);
        }

        /// <summary>
        /// Intersects the current action with the specified set of arguments and unifies the signature
        /// of the action.
        /// </summary>
        /// <param name="args">A composite object that describes default arguments for the action.</param>
        /// <param name="state">Internal interpreter state.</param>
        /// <returns>A new script action with replaced parameters.</returns>
        public ScriptActionBase Unify(IScriptCompositeObject args, InterpreterState state)
        {
            return args != null ? new UnifiedAction(this, args, state) : this;
        }

        /// <summary>
        /// Intersects the current action with the specified set of arguments and unifies the signature
        /// of the action.
        /// </summary>
        /// <param name="args">A composite object that describes default arguments for the action.</param>
        /// <param name="state">Internal interpreter state.</param>
        /// <returns>A new script action with replaced parameters.</returns>
        protected sealed override IScriptObject And(IScriptObject args, InterpreterState state)
        {
            if (args is IScriptCompositeObject)
                return Unify((IScriptCompositeObject)args, state);
            else if (state.Context == InterpretationContext.Unchecked)
                return Void;
            else throw new UnsupportedOperationException(state);
        }

        /// <summary>
        /// Pushes the current action into the call stack.
        /// </summary>
        /// <param name="state">Internal interpreter state.</param>
        private void Push(InterpreterState state)
        {
            if (!IsTransparent)
                CallStack.Push(this, state);
        }

        /// <summary>
        /// Pops the current action from the call stack.
        /// </summary>
        private void Pop()
        {
            if (!IsTransparent)
                CallStack.Pop();
        }

        /// <summary>
        /// Creates a new runtime slot for the action parameter.
        /// </summary>
        /// <param name="p">The action parameter.</param>
        /// <returns>A new runtime slot that holds the value of the parameter.</returns>
        protected virtual IRuntimeSlot CreateParameterHolder(ScriptActionContract.Parameter p)
        {
            return new ScriptVariable(p.ContractBinding);
        }

        #region Runtime Slots

        IRuntimeSlot IScriptActionSlots.Owner
        {
            get { return CacheConst(ref m_owner, () => This); }
        }
        #endregion

        /// <summary>
        /// Creates lambda that returns the specified value and takes no parameters.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static ScriptActionBase CreateConstantLambda(IScriptObject value)
        {
            return new ConstantProvider(value ?? Void);
        }

        /// <summary>
        /// Combines this action with the specified set of actions.
        /// </summary>
        /// <param name="action">An action to combine with this action.</param>
        /// <param name="actions">A collection of actions to combine with this action.</param>
        /// <param name="state">Internal interpreter state.</param>
        /// <returns>>An action that represents overloading.</returns>
        public ICombination Combine(IScriptAction action, IEnumerable<IScriptAction> actions, InterpreterState state)
        {
            actions = actions != null ? Enumerable.Concat(actions, new[] { action }) : new[] { action };
            return new Combination(this, actions, state);
        }

        /// <summary>
        /// Determines whether the the current object is equal to another.
        /// </summary>
        /// <param name="right">Other object to be compared.</param>
        /// <param name="state">Internal interpreter state.</param>
        /// <returns>The comparison result.</returns>
        protected sealed override IScriptObject Equals(IScriptObject right, InterpreterState state)
        {
            if (right is IScriptAction)
                return Equals((IScriptAction)right);
            else if (state.Context == InterpretationContext.Unchecked)
                return ScriptObject.Void;
            else throw new UnsupportedOperationException(state);
        }

        private ScriptBoolean Equals(IScriptAction action)
        {
            return ReferenceEquals(this, action) || GetHashCode() == action.GetHashCode();
        }

        /// <summary>
        /// Determines whether the current object is not equal to another.
        /// </summary>
        /// <param name="right">The second operand.</param>
        /// <param name="state">Internal interpreter state.</param>
        /// <returns>The comparison result.</returns>
        protected sealed override IScriptObject NotEquals(IScriptObject right, InterpreterState state)
        {
            if (right is IScriptAction)
                return NotEquals((IScriptAction)right, state);
            else if (state.Context == InterpretationContext.Unchecked)
                return ScriptObject.Void;
            else throw new UnsupportedOperationException(state);
        }

        private ScriptBoolean NotEquals(IScriptAction action)
        {
            return !Equals(action);
        }

        /// <summary>
        /// Provides combination of the current action with the specified.
        /// </summary>
        /// <param name="right">The action to combine or collection of actions.</param>
        /// <param name="state">Internal interpreter state.</param>
        /// <returns>The combined action.</returns>
        protected sealed override IScriptObject Add(IScriptObject right, InterpreterState state)
        {
            if (ReferenceEquals(this, right))
                return this;
            else if (right is IScriptAction)
                return Combine((IScriptAction)right, null, state);
            else if (state.Context == InterpretationContext.Unchecked)
                return ScriptObject.Void;
            else throw new UnsupportedOperationException(state);
        }

        internal static byte[] GetByteCode(MethodInfo m)
        {
            if (m == null) return new byte[0];
            try
            {
                var body = m.GetMethodBody();
                return body.GetILAsByteArray();
            }
            catch (InvalidOperationException)
            {
                return new byte[0];
            }
        }

        internal static byte[] GetByteCode(Delegate d)
        {
            return GetByteCode(d.Method);
        }

        /// <summary>
        /// Gets byte code of this script action.
        /// </summary>
        internal virtual byte[] ByteCode
        {
            get { return GetByteCode(Implementation); }
        }

        /// <summary>
        /// Computes hash code of the action implementation.
        /// </summary>
        /// <returns>The hash code of the action implementation.</returns>
        public sealed override int GetHashCode()
        {
            return StringEqualityComparer.GetHashCode(ByteCode);
        }

        /// <summary>
        /// Gets parameter by its name.
        /// </summary>
        /// <param name="paramName">The name of the parameter.</param>
        /// <returns>The parameter description.</returns>
        public ScriptActionContract.Parameter GetParameterByName(string paramName)
        {
            return ContractBinding.GetParameterByName(paramName);
        }

        /// <summary>
        /// Changes THIS reference of the action. 
        /// </summary>
        /// <param name="this">A new action owner.</param>
        /// <returns></returns>
        public virtual ScriptActionBase Bind(IScriptObject @this)
        {
            return new WrappedAction(this, @this);
        }

        IScriptAction IScriptAction.Bind(IScriptObject @this)
        {
            return Bind(@this);
        }

        /// <summary>
        /// Determines whether the specified action implementation is provided by script RTL.
        /// </summary>
        /// <param name="action">An action to check. Cannot be <see langword="null"/>.</param>
        /// <returns><see langword="true"/> if the specified action is implemented in the RTL; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="System.ArgumentNullException"><paramref name="action"/> is <see langword="null"/>.</exception>
        public static bool IsStandardAction(IScriptAction action)
        {
            if (action == null) throw new ArgumentNullException("action");
            return typeof(ScriptActionBase).Assembly.Equals(action.GetType().Assembly);
        }

        /// <summary>
        /// Gets whether this action implementation is provided by script RTL.
        /// </summary>
        public bool IsStandard
        {
            get { return IsStandardAction(this); }
        }

        /// <summary>
        /// Returns parameter metadata.
        /// </summary>
        /// <param name="slotName">The name of the parameter.</param>
        /// <param name="state">Internal interpreter state.</param>
        /// <returns></returns>
        protected sealed override IScriptObject GetSlotMetadata(string slotName, InterpreterState state)
        {
            return string.IsNullOrWhiteSpace(slotName) ?
            Void :
            ContractBinding[new[] { new ScriptString(slotName) }, state].GetValue(state);
        }
    }
}
