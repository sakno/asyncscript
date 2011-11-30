using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DynamicScript
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;
    using SerializationInfo = System.Runtime.Serialization.SerializationInfo;

    /// <summary>
    /// Represents common extensions.
    /// </summary>
    [ComVisible(false)]
    public static class Extensions
    {
        internal static void AddValue<T>(this SerializationInfo info, string name, T value)
        {
            info.AddValue(name, value, typeof(T));
        }

        internal static T GetValue<T>(this SerializationInfo info, string name)
        {
            return (T)info.GetValue(name, typeof(T));
        }

        /// <summary>
        /// Determines whether the specified value is in range.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <param name="lowerBound"></param>
        /// <param name="upperBound"></param>
        /// <returns></returns>
        public static bool Between<T>(this T value, T lowerBound, T upperBound)
            where T : IComparable<T>
        {
            return value.CompareTo(lowerBound) >= 0 && value.CompareTo(upperBound) <= 0;
        }

        internal static T Clone<T>(T obj)
            where T : ICloneable
        {
            return obj != null ? (T)obj.Clone() : default(T);
        }

        internal static T[] CloneCollection<T>(ICollection<T> elements)
            where T : ICloneable
        {
            var array = new T[elements.Count];
            elements.CopyTo(array, 0);
            for (var i = 0L; i < array.LongLength; i++)
                array[i] = Clone(array[i]);
            return array;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="sourceIndex"></param>
        /// <param name="destination"></param>
        /// <param name="destinationIndex"></param>
        /// <param name="length"></param>
        public static void CopyTo<T>(this IList<T> source, int sourceIndex, IList<T> destination, int destinationIndex, int length)
        {
            switch (length)
            {
                case 0: return;
                case 1:
                case 2:
                case 3:
                    length += sourceIndex;
                    while (sourceIndex < length)
                        destination[destinationIndex++] = source[sourceIndex++];
                    return;
                default:
                    Parallel.For(0, length, i => destination[i + destinationIndex] = source[i + sourceIndex]);
                    return;
            }
            
        }

        /// <summary>
        /// Determines whether the specified type is equal to one of the passed generic arguments.
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <param name="t"></param>
        /// <returns></returns>
        public static bool Is<T1, T2>(this Type t)
        {
            return Equals(t, typeof(T1)) || Equals(t, typeof(T2));
        }

        /// <summary>
        /// Determines whether the specified type is equal to one of the passed generic arguments.
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <param name="t"></param>
        /// <returns></returns>
        public static bool Is<T1, T2, T3>(this Type t)
        {
            return Equals(t, typeof(T1)) || Equals(t, typeof(T2)) || Equals(t, typeof(T3));
        }
    }
}
