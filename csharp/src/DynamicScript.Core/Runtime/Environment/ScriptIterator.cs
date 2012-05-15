using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Linq.Expressions;

namespace DynamicScript.Runtime.Environment
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;
    using SystemConverter = System.Convert;

    /// <summary>
    /// Represents DynamicScript iterator.
    /// This class cannot be inherited.
    /// </summary>
    [ComVisible(false)]
    [Serializable]
    public sealed class ScriptIterator : ScriptCompositeObject
    {
        #region Nested Types
        [ComVisible(false)]
        private sealed class InternalIterator
        {
            private readonly IEnumerator m_enumerator;
            private bool m_hasNext;

            private InternalIterator()
            {
                m_hasNext = false;
            }

            public InternalIterator(IEnumerator enumerator)
                : this()
            {
                if (enumerator == null) throw new ArgumentNullException("enumerator");
                m_enumerator = enumerator;
            }

            public bool HasNext
            {
                get
                {
                    if (!m_hasNext) m_hasNext = m_enumerator.MoveNext();
                    return m_hasNext;
                }
                private set { m_hasNext = value; }
            }

            public IScriptObject GetNext()
            {
                switch (HasNext)
                {
                    case true: HasNext = false; return m_enumerator.Current as IScriptObject ?? Void;
                    default: HasNext = false; throw new InvalidOperationException(ErrorMessages.EndOfCollection);
                }
            }
        }

        [ComVisible(false)]
        private sealed class HasNextSlot : RuntimeSlotBase, IStaticRuntimeSlot
        {
            private readonly InternalIterator m_iterator;

            public HasNextSlot(InternalIterator iterator)
            {
                m_iterator = iterator;
            }

            public InternalIterator Iterator
            {
                get { return m_iterator; }
            }

            public override IScriptObject GetValue(InterpreterState state)
            {
                return (ScriptBoolean)Iterator.HasNext;
            }

            public IScriptContract ContractBinding
            {
                get { return ScriptBooleanContract.Instance; }
            }

            public override bool DeleteValue()
            {
                return false;
            }

            public override IScriptObject SetValue(IScriptObject value, InterpreterState state)
            {
                throw new ConstantCannotBeChangedException(state);
            }

            public override RuntimeSlotAttributes Attributes
            {
                get { return RuntimeSlotAttributes.Immutable; }
            }

            public override bool HasValue
            {
                get { return true; }
                protected set { }
            }
        }

        [ComVisible(false)]
        private sealed class GetNextFunctionContract : ScriptFunctionContract
        {
            public GetNextFunctionContract(IScriptContract elementContract)
                : base(EmptyParameters, elementContract)
            {
            }
        }

        /// <summary>
        /// Represents iterator contract.
        /// This class cannot be inherited.
        /// </summary>
        [ComVisible(false)]
        [Serializable]
        private sealed class IteratorContract : ScriptCompositeContract
        {
            private IteratorContract(SerializationInfo info, StreamingContext context)
                : base(info, context)
            {
            }

            /// <summary>
            /// Initializes a new iterator contract.
            /// </summary>
            /// <param name="elementContract">The contract of the items returned by the iterator.</param>
            public IteratorContract(IScriptContract elementContract)
                : base(Slots(elementContract ?? ScriptSuperContract.Instance))
            {
            }

            private static new IEnumerable<KeyValuePair<string, SlotMeta>> Slots(IScriptContract elementContract)
            {
                yield return DefineSlot(HasNextSlotName, ScriptBooleanContract.Instance);
                yield return DefineSlot(GetNextSlotName, new GetNextFunctionContract(elementContract), true);
            }
        }

        [ComVisible(false)]
        private sealed class GetNextFunction : ScriptFunc
        {
            private readonly InternalIterator m_iterator;

            public GetNextFunction(InternalIterator iterator, IScriptContract elementContract)
                : base(elementContract)
            {
                if (iterator == null) throw new ArgumentNullException("iterator");
                m_iterator = iterator;
            }

            protected override IScriptObject Invoke(InterpreterState state)
            {
                return m_iterator.GetNext();
            }
        }

        [ComVisible(false)]
        private sealed class IteratorSlotCollection : ObjectSlotCollection
        {
            public IteratorSlotCollection(InternalIterator iterator, ref IScriptContract elementContract)
            {
                if (iterator == null) throw new ArgumentNullException("iterator");
                if (elementContract == null) elementContract = ScriptSuperContract.Instance;
                Add(HasNextSlotName, new HasNextSlot(iterator));
                AddConstant(GetNextSlotName, new GetNextFunction(iterator, elementContract));
            }

            public IteratorSlotCollection(IEnumerator enumerator, ref IScriptContract elementContract)
                : this(new InternalIterator(enumerator), ref elementContract)
            {
            }
        }

        /// <summary>
        /// Represents runtime library that contains low-level loop operation.
        /// </summary>
        [ComVisible(false)]
        public static class LoopHelpers
        {
            /// <summary>
            /// Returns a script-specific iterator through collection elements.
            /// </summary>
            /// <param name="collection"></param>
            /// <param name="state"></param>
            /// <returns></returns>
            public static IScriptObject GetEnumerator(IScriptObject collection, InterpreterState state)
            {
                if (collection is IScriptProxyObject)
                    collection = ((IScriptProxyObject)collection).Unwrap(state);
                else if (collection is IScriptIterable)
                    return new ScriptIterator(((IScriptIterable)collection).GetIterator(state));
                return collection[IteratorAction, state].Invoke(EmptyArray, state);
            }

            internal static MethodCallExpression GetEnumerator(Expression collection, ParameterExpression state)
            {
                collection = AsRightSide(collection, state);
                var geten = LinqHelpers.BodyOf<IScriptObject, InterpreterState, IScriptObject, MethodCallExpression>((c, s) => GetEnumerator(c, s));
                return geten.Update(null, new[] { collection, state });
            }

            /// <summary>
            /// Returns next element form the iterator.
            /// </summary>
            /// <param name="iterator"></param>
            /// <param name="state"></param>
            /// <returns></returns>
            public static IScriptObject GetNext(IScriptObject iterator, InterpreterState state)
            {
                return iterator[GetNextSlotName, state].Invoke(ScriptObject.EmptyArray, state);
            }

            internal static MethodCallExpression GetNext(Expression iterator, ParameterExpression state)
            {
                iterator = AsRightSide(iterator, state);
                var getnxt = LinqHelpers.BodyOf<IScriptObject, InterpreterState, IScriptObject, MethodCallExpression>((i, s) => GetNext(i, s));
                return getnxt.Update(null, new[] { iterator, state });
            }

            /// <summary>
            /// Combines the result returned from single iteration with the accumulator.
            /// </summary>
            /// <param name="result"></param>
            /// <param name="accumulator"></param>
            /// <param name="grouping"></param>
            /// <param name="state"></param>
            /// <returns></returns>
            public static IScriptObject CombineResult(IScriptObject result, IScriptObject accumulator, IScriptObject grouping, InterpreterState state)
            {
                return IsVoid(result) ? accumulator : grouping.Invoke(new[] { result, accumulator }, state);
            }

            internal static MethodCallExpression CombineResult(Expression result, Expression accumulator, Expression grouping, ParameterExpression state)
            {
                result = AsRightSide(result, state);
                accumulator = AsRightSide(accumulator, state);
                grouping = AsRightSide(grouping, state);
                var comres = LinqHelpers.BodyOf<IScriptObject, IScriptObject, IScriptObject, InterpreterState, IScriptObject, MethodCallExpression>((r, a, g, s) => CombineResult(r, a, g, s));
                return comres.Update(null, new[] { result, accumulator, grouping, state });
            }

            /// <summary>
            /// Determines whether the specified script iterator has next element.
            /// </summary>
            /// <param name="iterator"></param>
            /// <param name="state"></param>
            /// <returns></returns>
            public static bool HasNext(IScriptObject iterator, InterpreterState state)
            {
                var hasnext = iterator[HasNextSlotName, state];
                return SystemConverter.GetTypeCode(hasnext) == TypeCode.Boolean && SystemConverter.ToBoolean(hasnext);
            }

            internal static MethodCallExpression HasNext(Expression iterator, ParameterExpression state)
            {
                iterator = AsRightSide(iterator, state);
                var hasnext = LinqHelpers.BodyOf<IScriptObject, InterpreterState, bool, MethodCallExpression>((i, s) => HasNext(i, s));
                return hasnext.Update(null, new[] { iterator, state });
            }
        }
        #endregion

        /// <summary>
        /// Represents name of the 'hasNext' slot.
        /// </summary>
        public const string HasNextSlotName = "hasNext";

        /// <summary>
        /// Represents name of the 'getNext' slot.
        /// </summary>
        public const string GetNextSlotName = "getNext";

        private readonly IScriptContract m_elementContract;

        private ScriptIterator(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        /// <summary>
        /// Initializes a new iterator.
        /// </summary>
        /// <param name="elements">The enumerator through DynamicScript objects.</param>
        /// <param name="elementContract">The contract binding for all elements in the collection.</param>
        internal ScriptIterator(IEnumerator elements, IScriptContract elementContract = null)
            : base(new IteratorSlotCollection(elements, ref elementContract))
        {
            m_elementContract = elementContract;
        }

        internal ScriptIterator(IEnumerable elements, IScriptContract elementContract = null)
            : this((elements ?? EmptyArray).GetEnumerator(), elementContract)
        {
        }

        /// <summary>
        /// Initializes a new iterator.
        /// </summary>
        /// <param name="elements">The collection of the DynamicScript objects.</param>
        /// <param name="elementContract">The contract binding for all elements in the collection.</param>
        public ScriptIterator(IEnumerable<IScriptObject> elements, IScriptContract elementContract = null)
            : this((IEnumerable)elements, elementContract)
        {
        }

        /// <summary>
        /// Returns contract of the iterator object.
        /// </summary>
        /// <param name="collection"></param>
        /// <returns></returns>
        protected override ScriptCompositeContract GetContractBinding(ObjectSlotCollection collection)
        {
            return GetContractBinding(m_elementContract);
        }

        /// <summary>
        /// Returns contract of the iterator object.
        /// </summary>
        /// <param name="elementContract"></param>
        /// <returns></returns>
        public static ScriptCompositeContract GetContractBinding(IScriptContract elementContract)
        {
            return new IteratorContract(elementContract);
        }

        /// <summary>
        /// Determines whether the specified object represents an iterator.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static bool IsIterator(IScriptObject obj)
        {
            return obj != null && obj.Slots.Contains(GetNextSlotName) && obj.Slots.Contains(HasNextSlotName);
        }

        /// <summary>
        /// Returns an object that represents 
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        public static IScriptCompositeObject GetIterator(IScriptObject obj, InterpreterState state)
        {
            var iteratorAct = ScriptIteratorFunction.IsIterable(obj) ? obj[IteratorAction, state] as ScriptFunctionBase : null;
            return iteratorAct != null ? iteratorAct.Invoke(EmptyArray, state) as IScriptCompositeObject : null;
        }

        /// <summary>
        /// Converts DynamicScript object to .NET enumerable object.
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        public static IEnumerable<IScriptObject> AsEnumerable(IScriptObject obj, InterpreterState state)
        {
            if (obj == null) throw new ArgumentNullException("obj");
            var iterator = GetIterator(obj, state);
            switch (IsIterator(iterator))
            {
                case true:
                    while (RuntimeHelpers.IsTrue(iterator[HasNextSlotName, state]))
                        yield return iterator[GetNextSlotName, state].Invoke(EmptyArray, state);
                    break;
                default: yield break;
            }
        }
    }
}
