using System;
using System.Collections.Generic;

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
    }
}
