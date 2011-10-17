using System;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace DynamicScript
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;

    [ComVisible(false)]
    abstract class ParallelAggregator<T1, T2, TResult>
    {
        private TResult m_result;

        protected abstract void Aggregate(T1 item1, T2 item2, ref TResult result);

        public void Aggregate(T1 item1, T2 item2)
        {
            Aggregate(item1, item2, ref m_result);
        }

        public TResult Aggregate(IEnumerable<T1> collection1, IEnumerable<T2> collection2)
        {
            Parallel.ForEach(collection1, item1 => Parallel.ForEach(collection2, item2 => Aggregate(item1, item2)));
            return m_result;
        }

        public static TResult Aggregate<TAggregator>(IEnumerable<T1> collection1, IEnumerable<T2> collection2)
            where TAggregator: ParallelAggregator<T1, T2, TResult>, new()
        {
            var aggregator = new TAggregator();
            return aggregator.Aggregate(collection1, collection2);
        }
    }

    [ComVisible(false)]
    abstract class ParallelAggregator<T, TResult> : ParallelAggregator<T, T, TResult>
    {
    }
}
