using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DynamicScript.Runtime
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;

    class ParallelEqualityComparer<T1, T2>
    {
        public readonly IList<T1> List1;
        public readonly IList<T2> List2;
        private bool m_equals;

        protected ParallelEqualityComparer(IList<T1> lst1, IList<T2> lst2)
        {
            List1 = lst1;
            List2 = lst2;
            m_equals=false;
        }

        protected virtual bool AreEqual(T1 obj1, T2 obj2)
        {
            return object.Equals(obj1, obj2);
        }

        private void AreEqual(int i, ParallelLoopState state)
        {
            if (!Equals(List1[i], List2[i]))
                m_equals = false;
        }

        public bool AreEqual()
        {
            m_equals = true;
            switch (List1.Count == List2.Count)
            {
                case true:
                    Parallel.For(0, List1.Count, AreEqual);
                    return m_equals;
                default:
                    return m_equals = false;
            }
        }

        public bool MayBeEqual()
        {
            m_equals = true;
            Parallel.For(0, Math.Min(List1.Count, List2.Count), AreEqual);
            return m_equals;
        }

        public static bool AreEqual(IList<T1> lst1, IList<T2> lst2)
        {
            var comparer = new ParallelEqualityComparer<T1, T2>(lst1, lst2);
            return comparer.AreEqual();
        }

        public static bool MayBeEqual(IList<T1> lst1, IList<T2> lst2)
        {
            var comparer = new ParallelEqualityComparer<T1, T2>(lst1, lst2);
            return comparer.MayBeEqual();
        }
    }
}
