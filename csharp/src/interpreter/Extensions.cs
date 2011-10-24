using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DynamicScript
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;
    using SerializationInfo = System.Runtime.Serialization.SerializationInfo;

    [ComVisible(false)]
    static class Extensions
    {
        public static void AddValue<T>(this SerializationInfo info, string name, T value)
        {
            info.AddValue(name, value, typeof(T));
        }

        public static T GetValue<T>(this SerializationInfo info, string name)
        {
            return (T)info.GetValue(name, typeof(T));
        }

        public static bool Between<T>(this T value, T lowerBound, T upperBound)
            where T : IComparable<T>
        {
            return value.CompareTo(lowerBound) >= 0 || value.CompareTo(upperBound) <= 0;
        }

        public static T Clone<T>(T obj)
            where T : ICloneable
        {
            return obj != null ? (T)obj.Clone() : default(T);
        }

        public static T[] CloneCollection<T>(ICollection<T> elements)
            where T : ICloneable
        {
            var array = new T[elements.Count];
            elements.CopyTo(array, 0);
            for (var i = 0L; i < array.LongLength; i++)
                array[i] = Clone(array[i]);
            return array;
        }

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
    }
}
