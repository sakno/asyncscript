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

        protected abstract TResult Match(TElement element, TCriteria criteria, out bool result);

        [MethodImpl(MethodImplOptions.Synchronized)]
        private void Complete(TResult result)
        {
            if (!m_success)
                m_result = result;
            m_success = true;
        }

        private void Match(TElement element, ParallelLoopState state)
        {
            var success = default(bool);
            var result = Match(element, Criteria, out success);
            if (success)
            {
                Complete(result);
                state.Stop();
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
