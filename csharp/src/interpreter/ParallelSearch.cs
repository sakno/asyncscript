using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace DynamicScript
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;
    using Enumerable = System.Linq.Enumerable;

    [ComVisible(false)]
    abstract class ParallelSearch<TCriteria, TElement, TResult>
    {
        private bool m_success;
        private TResult m_result;
        public readonly TCriteria Criteria;

        protected ParallelSearch(TCriteria criteria)
        {
            m_success = false;
            m_result = default(TResult);
            Criteria = criteria;
        }

        protected abstract bool Match(TElement element, TCriteria criteria, out TResult result);

        [MethodImpl(MethodImplOptions.Synchronized)]
        private void Complete(TResult result, ParallelLoopState state)
        {
            if (m_success || state.IsStopped) return;
            m_result = result;
            m_success = true;
        }

        private void Match(TElement element, ParallelLoopState state)
        {
            var result = default(TResult);
            if (Match(element, Criteria, out result))
            {
                Complete(result, state);
                state.Break();
            }
        }

        protected void Find(IEnumerable<TElement> elements)
        {
            Parallel.ForEach(elements ?? Enumerable.Empty<TElement>(), Match);
        }

        public bool Success
        {
            get { return m_success; }
        }

        public TResult Result
        {
            get { return m_result; }
        }
    }
}
