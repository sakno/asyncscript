using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace DynamicScript.Runtime.Environment
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;
    using InterpretationContext = Compiler.Ast.InterpretationContext;
    using IList = System.Collections.IList;
    using Punctuation = Compiler.Punctuation;
    using SystemConverter = System.Convert;
    using ICollection = System.Collections.ICollection;

    /// <summary>
    /// Represents array object.
    /// This class cannot be inherited.
    /// </summary>
    [ComVisible(false)]
    [Serializable]
    public sealed class ScriptArray : ScriptObject, ISerializable, IScriptArray, IScriptObjectCollection
    {
        #region Nested Types

        [ComVisible(false)]
        private sealed class ReadArrayElementFunction : ScriptGetItemFunction
        {
            private readonly Array m_elements;

            public ReadArrayElementFunction(IScriptArrayContract contract, Array elements)
                : base(contract.ElementContract, ScriptIntegerContract.Instance.AsArray(elements.Rank))
            {
                m_elements = elements;
            }

            private IScriptObject GetItem(long[] indicies)
            {
                return m_elements.GetValue(indicies) as IScriptObject;
            }

            protected override IScriptObject GetItem(IScriptObject[] indicies, InterpreterState state)
            {
                return GetItem(Array.ConvertAll(indicies, SystemConverter.ToInt64));
            }
        }

        [ComVisible(false)]
        private sealed class WriteArrayElementFunction : ScriptSetItemFunction
        {
            private readonly Array m_elements;

            public WriteArrayElementFunction(IScriptArrayContract contract, Array elements)
                : base(contract.ElementContract, ScriptIntegerContract.Instance.AsArray(elements.Rank))
            {
                m_elements = elements;
            }

            private static void SetItem(Array elements, IScriptObject value, long[] indicies)
            {
                elements.SetValue(value, indicies);
            }

            protected override void SetItem(IScriptObject value, IScriptObject[] indicies, InterpreterState state)
            {
                SetItem(m_elements, value, Array.ConvertAll(indicies, SystemConverter.ToInt64));
            }
        }

        [ComVisible(false)]
        internal sealed class UpperBoundFunction<T> : ScriptFunc<ScriptInteger>
            where T: class, ICollection
        {
            private const string FirstParamName = "dimension";
            private readonly T m_elements;

            public UpperBoundFunction(T elements)
                : base(FirstParamName, ScriptIntegerContract.Instance, ScriptIntegerContract.Instance)
            {
                m_elements=elements;
            }

            private static ScriptInteger GetUpperBound(ICollection elements, int dimension)
            {
                if (elements is Array)
                    return ((Array)elements).GetUpperBound(dimension);
                else if (dimension == 0)
                    return elements.Count;
                else return ScriptInteger.Zero;
            }

            protected override IScriptObject Invoke(ScriptInteger dimension, InterpreterState state)
            {
                if (dimension == null) dimension = ScriptInteger.Zero;
                return dimension.IsInt32 ? GetUpperBound(m_elements, SystemConverter.ToInt32(dimension)) : ScriptInteger.Zero;
            }
        }
        
        [ComVisible(false)]
        private sealed class ArrayCopier
        {
            private readonly Array m_destination;

            public ArrayCopier(Array destination)
            {
                m_destination = destination;
            }

            private bool Copy(object element, long index)
            {
                m_destination.SetValue(element, index);
                return true;
            }

            public static implicit operator Func<object, long, bool>(ArrayCopier copier)
            {
                return copier != null ? new Func<object, long, bool>(copier.Copy) : null;
            }
        }
        #endregion

        /// <summary>
        /// Represents collection of static slots.
        /// </summary>
        private static readonly AggregatedSlotCollection<ScriptArray> StaticSlots = new AggregatedSlotCollection<ScriptArray>
        {
            {ScriptArrayContract.RankSlotName, (owner, state) => new ScriptInteger(owner.m_contract.Rank), ScriptIntegerContract.Instance},
            //Getter
            {GetItemAction, (owner, state)=>
                {
                    if(owner.m_getter == null)owner.m_getter=new ReadArrayElementFunction(owner.m_contract, owner.m_elements);
                    return owner.m_getter;
                }},
            //Setter
            {SetItemAction, (owner, state)=>
                {
                    if(owner.m_setter == null) owner.m_setter = new WriteArrayElementFunction(owner.m_contract, owner.m_elements);
                    return owner.m_setter;
                }},
            {ScriptArrayContract.LengthSlotName, (owner, state) => new ScriptInteger(GetTotalLength(owner)), ScriptIntegerContract.Instance},
            {ScriptArrayContract.UpperBoundSlotName, (owner, state) =>
                {
                    if(owner.m_ubound == null) owner.m_ubound = new UpperBoundFunction<Array>(owner.m_elements);
                    return owner.m_ubound;
                }},
                {IteratorAction, (owner, state) =>
                    {
                        if(owner.m_iterator == null) owner.m_iterator = new ScriptIteratorFunction(owner.m_elements, owner.m_contract.ElementContract);
                        return owner.m_iterator;
                    }}
        };

        private const string ArraySerializationSlot = "Elements";
        private const string ArrayContractSerializationSlot = "ElementContract";

        private readonly Array m_elements;
        private readonly ScriptArrayContract m_contract;

        private IScriptFunction m_getter;
        private IScriptFunction m_setter;
        private IScriptFunction m_ubound;
        private IScriptFunction m_iterator;

        private ScriptArray(Array elements, ScriptArrayContract contract)
        {
            if (elements == null) throw new ArgumentNullException("elements");
            if (contract == null) throw new ArgumentNullException("contract");
            m_elements = elements;
            m_contract = contract;
        }

        private ScriptArray(SerializationInfo info, StreamingContext context)
            : this(info.GetValue(ArrayContractSerializationSlot, typeof(Array)) as Array, info.GetValue(ArrayContractSerializationSlot, typeof(ScriptArrayContract)) as ScriptArrayContract)
        {
        }

        internal static IScriptContract InferContract(IEnumerable<IScriptObject> elements)
        {
            return ScriptContract.Infer(elements.Select(t => t.GetContractBinding()));
        }

        /// <summary>
        /// Initializes a new single-dimensional array.
        /// </summary>
        /// <param name="elements">The elements of the single-dimensional array.</param>
        public ScriptArray(params IScriptObject[] elements)
            : this(elements ?? EmptyArray, new ScriptArrayContract(InferContract(elements), elements.Rank))
        {
        }

        /// <summary>
        /// Initializes a new array.
        /// </summary>
        /// <param name="elementContract">The contract of the array.</param>
        /// <param name="lengths">The lengths of the each dimension.</param>
        /// <exception cref="System.ArgumentException">The rank of the array contract is not equal to length of <paramref name="lengths"/>.</exception>
        public ScriptArray(ScriptContract elementContract, params long[] lengths)
            : this(Array.CreateInstance(typeof(IScriptObject), lengths), new ScriptArrayContract(elementContract, lengths.Length))
        {
        }

        /// <summary>
        /// Initializes a new DynamicScript array from collection of DynamicScript objects.
        /// </summary>
        /// <param name="elements">The collection of DynamicScript objects.</param>
        /// <returns>A new instance of DynamicScript array.</returns>
        public static ScriptArray Create(ICollection<IScriptObject> elements)
        {
            if (elements == null) elements = EmptyArray;
            var array = new IScriptObject[elements.Count];
            elements.CopyTo(array, 0);
            return new ScriptArray(array);
        }

        /// <summary>
        /// Gets or sets element in the array.
        /// </summary>
        /// <param name="indicies">The position of the element.</param>
        /// <param name="state">Internal interpreter state.</param>
        /// <returns>The array element.</returns>
        public IScriptObject this[long[] indicies, InterpreterState state]
        {
            get
            {
                return m_elements.GetValue(indicies) as IScriptObject ?? m_contract.ElementContract.FromVoid(state);
            }
            set
            {
                if (value == null) value = Void;
                switch (RuntimeHelpers.IsCompatible(m_contract.ElementContract, value))
                {
                    case true:
                        m_elements.SetValue(value, indicies);
                        return;
                    default:
                        if (state.Context == InterpretationContext.Checked)
                            throw new ContractBindingException(value, m_contract.ElementContract, state);
                        return;
                }
            }
        }

        /// <summary>
        /// Gets or sets a value in single-dimensional array.
        /// </summary>
        /// <param name="index"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        public IScriptObject this[long index, InterpreterState state]
        {
            get
            {
                return m_elements.GetValue(index) as IScriptObject ?? m_contract.ElementContract.FromVoid(state);
            }
            set
            {
                if (value == null) value = Void;
                switch (RuntimeHelpers.IsCompatible(m_contract.ElementContract, value))
                {
                    case true:
                        m_elements.SetValue(value, index);
                        return;
                    default:
                        if (state.Context == InterpretationContext.Checked)
                            throw new ContractBindingException(value, m_contract.ElementContract, state);
                        return;
                }
            }
        }

        /// <summary>
        /// Returns array contract.
        /// </summary>
        /// <returns></returns>
        public override IScriptContract GetContractBinding()
        {
            return m_contract;
        }

        /// <summary>
        /// Returns size of the specified dimension.
        /// </summary>
        /// <param name="dimension">Zero-based index of dimension.</param>
        /// <returns>The size of the specified dimension.</returns>
        public long GetLength(int dimension)
        {
            return m_elements.GetLongLength(dimension);
        }

        /// <summary>
        /// Computes a total length of the multidimensional array.
        /// </summary>
        /// <param name="elements"></param>
        /// <returns></returns>
        public static long GetTotalLength(IScriptArray elements)
        {
            var result = 1L;
            for (var i = 0; i < elements.GetContractBinding().Rank; i++)
                result *= elements.GetLength(i);
            return result;
        }

        IScriptArrayContract IScriptArray.GetContractBinding()
        {
            return m_contract;
        }

        /// <summary>
        /// Converts DynamicScript array to managed array.
        /// </summary>
        /// <param name="array"></param>
        /// <returns></returns>
        public static implicit operator Array(ScriptArray array)
        {
            return array != null ? array.m_elements : null;
        }

        /// <summary>
        /// Returns an empty single-dimensional script array.
        /// </summary>
        /// <param name="arrayContract">The default contract of the array elements.</param>
        /// <returns>An empty single-dimensional script array.</returns>
        public static ScriptArray Empty(ScriptContract arrayContract = null)
        {
            return new ScriptArray(arrayContract, 1, 0L);
        }

        internal static Expression Bind(IEnumerable<Expression> elements)
        {
            var ctor = LinqHelpers.BodyOf<IScriptObject[], IScriptObject, NewExpression>(e => new ScriptArray(e));
            return ctor.Update(new[] { Expression.NewArrayInit(typeof(IScriptObject), elements) });
        }

        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue(ArrayContractSerializationSlot, m_contract, typeof(ScriptArrayContract));
            info.AddValue(ArraySerializationSlot, m_elements, typeof(Array));
        }

        void ICollection<IScriptObject>.Add(IScriptObject item)
        {
            IList items = m_elements;
            items.Add(item);
        }

        void ICollection<IScriptObject>.Clear()
        {
            IList items = m_elements;
            items.Clear();
        }

        private static bool ForEach(Array input, int dimension, Func<object, long, bool> setter, ref long lookup, long[] indicies)
        {
            if (indicies == null) indicies = new long[input.Rank];
            for (var i = 0L; i < input.GetLongLength(dimension); i++)
            {
                indicies[dimension] = i;
                switch (dimension < input.Rank - 1 ? ForEach(input, dimension + 1, setter, ref lookup, indicies) : setter.Invoke(input.GetValue(indicies), lookup++))
                {
                    case true: continue;
                    default: return false;
                }
            }
            return true;
        }

        private static bool ForEach(Array input, int dimension, Func<object, long, bool> setter)
        {
            var lookup = 0L;
            return ForEach(input, dimension, setter, ref lookup, null);
        }

        private static bool ForEach(Array input, Func<object, long, bool> setter)
        {
            return ForEach(input, 0, setter);
        } 

        /// <summary>
        /// Converts this array to single dimensional array.
        /// </summary>
        /// <returns>A new single dimensional array.</returns>
        public ScriptArray ToSingleDimensional()
        {
            switch (m_elements.Rank)
            {
                case 1: return this;
                default:
                    var result = Array.CreateInstance(m_elements.GetType().GetElementType(), GetTotalLength(this));
                    ForEach(m_elements, new ArrayCopier(result));
                    return new ScriptArray(result, new ScriptArrayContract(m_contract.ElementContract));
            }
        }

        IScriptArray IScriptArray.ToSingleDimensional()
        {
            return ToSingleDimensional();
        }

        private bool Contains(IScriptObject item, bool byRef)
        {
            return !ForEach(m_elements, (e, i) => !(byRef ? ReferenceEquals(e, item) : item.Equals(e)));
        }

        bool IScriptContainer.Contains(IScriptObject item, bool byRef, InterpreterState state)
        {
            return Contains(item, byRef);
        }

        bool ICollection<IScriptObject>.Contains(IScriptObject item)
        {
            return Contains(item, false);
        }

        /// <summary>
        /// Copies all elements from this array to the specified managed array.
        /// </summary>
        /// <param name="array"></param>
        /// <param name="arrayIndex"></param>
        public void CopyTo(IScriptObject[] array, int arrayIndex)
        {
            m_elements.CopyTo(array, arrayIndex);
        }

        int ICollection<IScriptObject>.Count
        {
            get
            {
                IList items = m_elements;
                return items.Count;
            }
        }

        bool ICollection<IScriptObject>.IsReadOnly
        {
            get 
            {
                IList items = m_elements;
                return items.IsReadOnly; 
            }
        }

        bool ICollection<IScriptObject>.Remove(IScriptObject item)
        {
            IList items = m_elements;
            items.Remove(item);
            return true;
        }

        /// <summary>
        /// Returns an enumerator through all array elements.
        /// </summary>
        /// <returns></returns>
        public IEnumerator<IScriptObject> GetEnumerator()
        {
            foreach (IScriptObject element in m_elements)
                yield return element;
        }

        IEnumerator<IScriptObject> IEnumerable<IScriptObject>.GetEnumerator()
        {
            return GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        internal static string ToString(IEnumerable<IScriptObject> elements)
        {
            return string.Concat(Punctuation.LeftSquareBracket, string.Join<IScriptObject>(Punctuation.Comma, elements), Punctuation.RightSquareBracket);
        }

        /// <summary>
        /// Returns a string representation of the list.
        /// </summary>
        /// <returns>The string representation of the list.</returns>
        public override string ToString()
        {
            return ToString(this);
        }

        /// <summary>
        /// Creates a new script array from array of bytes.
        /// </summary>
        /// <param name="elements"></param>
        /// <returns></returns>
        public static ScriptArray Create(byte[] elements)
        {
            return new ScriptArray(Array.ConvertAll<byte, IScriptObject>(elements ?? new byte[0], b => new ScriptInteger(b)), new ScriptArrayContract(ScriptIntegerContract.Instance));
        }

        /// <summary>
        /// Creates a new script array from array of strings.
        /// </summary>
        /// <param name="elements"></param>
        /// <returns></returns>
        public static ScriptArray Create(string[] elements)
        {
            return new ScriptArray(Array.ConvertAll<string, IScriptObject>(elements ?? new string[0], b => new ScriptString(b)), new ScriptArrayContract(ScriptStringContract.Instance));
        }

        /// <summary>
        /// Concatenates a two script arrays.
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        public static IScriptArray Concat(IScriptArray left, IScriptArray right, InterpreterState state)
        {
            var leftLength = GetTotalLength(left);
            var rightLength = GetTotalLength(right);
            var result = new IScriptObject[leftLength + rightLength];
            Parallel.For(0L, leftLength, i => result[i] = left[new[] { i }, state]);
            Parallel.For(0L, rightLength, i => result[i + leftLength] = right[new[] { i }, state]);
            return new ScriptArray(result);
        }

        /// <summary>
        /// Concatenates a two arrays.
        /// </summary>
        /// <param name="right"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        protected override IScriptObject Add(IScriptObject right, InterpreterState state)
        {
            if (right is IScriptArray)
                return Concat(this, (IScriptArray)right, state);
            else if (state.Context == InterpretationContext.Unchecked)
                return Void;
            else throw new UnsupportedOperationException(state);
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="slotName"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        protected override IScriptObject GetSlotMetadata(string slotName, InterpreterState state)
        {
            return StaticSlots.GetSlotMetadata(this, slotName, state);
        }
    }
}
