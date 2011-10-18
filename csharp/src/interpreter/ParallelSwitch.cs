using System;
using System.Collections.Generic;

namespace DynamicScript
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;
    using MethodInfo = System.Reflection.MethodInfo;
    using Enumerable = System.Linq.Enumerable;

    /// <summary>
    /// Represents parallel switch.
    /// </summary>
    /// <typeparam name="TSource">Type of the object to compare.</typeparam>
    /// <typeparam name="TResult">Type of the switch result.</typeparam>
    [ComVisible(false)]
    class ParallelSwitch<TSource, TResult> : ParallelSearch<TSource, ParallelSwitchCase<TSource, TResult>, TResult>,
        IEnumerable<ParallelSwitchCase<TSource, TResult>>
    {
        #region Nested Types
        /// <summary>
        /// Represents parallel switch case constructed from
        /// </summary>
        [ComVisible(false)]
        private sealed class PredicateSwitchCase
        {
            public readonly Predicate<TSource> Condition;
            public readonly Converter<TSource, TResult> Body;

            public PredicateSwitchCase(Predicate<TSource> condition, Converter<TSource, TResult> body)
            {
                if (condition == null) throw new ArgumentNullException("condition");
                if (body == null) throw new ArgumentNullException("body");
                Condition = condition;
                Body = body;
            }

            public TResult Match(TSource value, out bool result)
            {
                return (result = Condition(value)) ? Body(value) : default(TResult);
            }

            public static implicit operator ParallelSwitchCase<TSource, TResult>(PredicateSwitchCase @case)
            {
                return @case != null ? new ParallelSwitchCase<TSource, TResult>(@case.Match) : null;
            }
        }

        [ComVisible(false)]
        private sealed class OfTypeSwitchCase<T>
            where T: TSource
        {
            private readonly MethodInfo Method;
            private readonly object Target;

            public OfTypeSwitchCase(Converter<T, TResult> body)
            {
                if (body == null) throw new ArgumentNullException("body");
                Method = body.Method;
                Target = body.Target;
            }

            public TResult Match(TSource value, out bool result)
            {
                return (result = value is T) ? (TResult)Method.Invoke(Target, new object[] { value }) : default(TResult);
            }

            public static implicit operator ParallelSwitchCase<TSource, TResult>(OfTypeSwitchCase<T> oftype)
            {
                return oftype != null ? new ParallelSwitchCase<TSource, TResult>(oftype.Match) : null;
            }
        }
        #endregion

        /// <summary>
        /// Represents a collection of switch cases.
        /// </summary>
        protected readonly ICollection<ParallelSwitchCase<TSource, TResult>> Cases;

        /// <summary>
        /// Initializes a new parallel switch block.
        /// </summary>
        /// <param name="src"></param>
        /// <param name="cases">A collection of switch cases.</param>
        public ParallelSwitch(TSource src, IEnumerable< ParallelSwitchCase<TSource, TResult>> cases = null)
            : base(src)
        {
            Cases = new List<ParallelSwitchCase<TSource, TResult>>(cases ?? Enumerable.Empty<ParallelSwitchCase<TSource, TResult>>());
        }

        /// <summary>
        /// Adds a new switch case to the collection of cases.
        /// </summary>
        /// <param name="case"></param>
        public void Add(ParallelSwitchCase<TSource, TResult> @case)
        {
            Cases.Add(@case);
        }

        /// <summary>
        /// Add a new switch case based on the predicate.
        /// </summary>
        /// <param name="condition">A condition of the switch hit.</param>
        /// <param name="body">A body of the case handler.</param>
        public void Add(Predicate<TSource> condition, Converter<TSource, TResult> body)
        {
            Add(new PredicateSwitchCase(condition, body));
        }

        /// <summary>
        /// Executes the specified body only when the value is an instance of the specified type.
        /// </summary>
        /// <typeparam name="T">An expected type of the object.</typeparam>
        /// <param name="body">A case handler.</param>
        public void OfType<T>(Converter<T, TResult> body)
            where T : TSource
        {
            Add(new OfTypeSwitchCase<T>(body));
        }
        
        /// <summary>
        /// Checks the switch case and if it hits the execute handler.
        /// </summary>
        /// <param name="element"></param>
        /// <param name="criteria"></param>
        /// <param name="match"></param>
        /// <returns></returns>
        protected sealed override TResult Match(ParallelSwitchCase<TSource, TResult> element, TSource criteria, out bool match)
        {
            return element.Invoke(criteria, out match);
        }

        /// <summary>
        /// Executes a parallel switch.
        /// </summary>
        /// <param name="defaultHandler">Default switch handler.</param>
        public TResult Do(Converter<TSource, TResult> defaultHandler = null)
        {
            if (defaultHandler == null) defaultHandler = v => default(TResult);
            Find(this);
            return Success ? Result : defaultHandler(Criteria);
        }

        /// <summary>
        /// Executes parallel switch
        /// </summary>
        /// <param name="src"></param>
        /// <param name="cases"></param>
        /// <param name="success"></param>
        /// <param name="defaultHandler"></param>
        /// <returns></returns>
        public static TResult Do(TSource src, IEnumerable<ParallelSwitchCase<TSource, TResult>> cases, out bool success, Converter<TSource, TResult> defaultHandler = null)
        {
            var @switch = new ParallelSwitch<TSource, TResult>(src, cases);
            var result = @switch.Do();
            success = @switch.Success;
            return result;
        }

        /// <summary>
        /// Returns an enumerator through all switch cases.
        /// </summary>
        /// <returns></returns>
        public IEnumerator<ParallelSwitchCase<TSource, TResult>> GetEnumerator()
        {
            return Cases.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
