using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace DynamicScript.Runtime
{
    /// <summary>
    /// Represents object pool.
    /// </summary>
    /// <typeparam name="TKey">Type of the object identifier.</typeparam>
    /// <typeparam name="TScriptObject">Type of the cached object.</typeparam>
    [ComVisible(false)]
    abstract class InternPool<TKey, TScriptObject> : Dictionary<TKey, TScriptObject>
        where TKey : struct, IEquatable<TKey>
        where TScriptObject : class, IScriptObject
    {
        protected InternPool(int capacity, IEqualityComparer<TKey> comparer)
            : base(capacity, comparer)
        {
        }

        /// <summary>
        /// Retreives unique identifier of object.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        protected abstract TKey MakeID(TScriptObject obj);

        public TKey Intern(TScriptObject obj)
        {
            var key = MakeID(obj);
            lock (this)
                switch (ContainsKey(key))
                {
                    case true: return key;
                    default:
                        Add(key, obj);
                        return key;
                }
        }

        public bool IsInterned(TScriptObject obj)
        {
            var key = MakeID(obj);
            lock (this)
            {
                var result = default(TScriptObject);
                return TryGetValue(key, out result) ? ReferenceEquals(result, obj) : false;
            }
        }

        public new TScriptObject this[TKey key]
        {
            get 
            {
                var result = default(TScriptObject);
                lock (this)
                    return TryGetValue(key, out result) ? result : null;
            }
        }

        public static Type ObjectType
        {
            get { return typeof(TScriptObject); }
        }
    }
}
