using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace DynamicScript
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;

    [ComVisible(false)]
    static class LinqHelpers
    {
        public static bool IsNotNull<T>(T obj)
            where T : class
        {
            return obj != null;
        }

        public static IEnumerable<G> SelectNotNull<T, G>(this IEnumerable<T> values, Func<T, G> selector)
            where G : class
        {
            return values.Select(selector).Where(IsNotNull);
        }

        public static TExpression BodyOf<TDelegate, TExpression>(Expression<TDelegate> expr)
            where TExpression : Expression
            where TDelegate : class
        {
            return (TExpression)expr.Body;
        }

        public static TExpression BodyOf<T, TResult, TExpression>(Expression<Func<T, TResult>> expr)
            where TExpression : Expression
        {
            return BodyOf<Func<T, TResult>, TExpression>(expr);
        }

        public static TExpression BodyOf<T1, T2, TResult, TExpression>(Expression<Func<T1, T2, TResult>> expr)
            where TExpression : Expression
        {
            return BodyOf<Func<T1, T2, TResult>, TExpression>(expr);
        }

        public static TExpression BodyOf<T1, T2, T3, TResult, TExpression>(Expression<Func<T1, T2, T3, TResult>> expr)
            where TExpression : Expression
        {
            return BodyOf<Func<T1, T2, T3, TResult>, TExpression>(expr);
        }

        public static TExpression BodyOf<T1, T2, T3, T4, TResult, TExpression>(Expression<Func<T1, T2, T3, T4, TResult>> expr)
            where TExpression : Expression
        {
            return BodyOf<Func<T1, T2, T3, T4, TResult>, TExpression>(expr);
        }

        public static TExpression BodyOf<T1, T2, T3, T4, T5, TResult, TExpression>(Expression<Func<T1, T2, T3, T4, T5, TResult>> expr)
            where TExpression : Expression
        {
            return BodyOf<Func<T1, T2, T3, T4, T5, TResult>, TExpression>(expr);
        }

        public static TExpression BodyOf<T1, T2, T3, T4, T5, T6, TResult, TExpression>(Expression<Func<T1, T2, T3, T4, T5, T6, TResult>> expr)
            where TExpression : Expression
        {
            return BodyOf<Func<T1, T2, T3, T4, T5, T6, TResult>, TExpression>(expr);
        }

        public static ConstantExpression Constant<T>(T value)
        {
            return Expression.Constant(value, typeof(T));
        }

        public static ConstantExpression Null<T>()
            where T : class
        {
            return Constant<T>(null);
        }

        public static DefaultExpression Default<T>()
        {
            return Expression.Default(typeof(T));
        }

        public static Expression Restore<T>(T value)
            where T :  IRestorable
        {
            return value != null ? value.Restore() : Default<T>();
        }

        public static Expression[] RestoreMany<T>(params T[] values)
            where T : IRestorable
        {
            return Array.ConvertAll(values ?? new T[0], Restore<T>);
        }

        public static IEnumerable<Expression> RestoreMany<T>(IEnumerable<T> values)
            where T : IRestorable
        {
            return Enumerable.Select(values ?? Enumerable.Empty<T>(), Restore<T>);
        }

        public static NewArrayExpression NewArray<T>(IEnumerable<T> elements, Func<T, Expression> converter)
        {
            if (elements == null) elements = Enumerable.Empty<T>();
            return Expression.NewArrayInit(typeof(T), Enumerable.Select(elements, converter));
        }

        public static NewArrayExpression NewArray<T>(this IEnumerable<T> elements)
            where T : IRestorable
        {
            return NewArray<T>(elements, Restore<T>);
        }

        public static NewExpression CreateKeyValuePair<TKey, TValue>(ConstantExpression key, Expression value)
        {
            var ctor = BodyOf<TKey, TValue, KeyValuePair<TKey, TValue>, NewExpression>((k, v) => new KeyValuePair<TKey, TValue>(k, v));
            return ctor.Update(new[] { key, value });
        }

        public static NewExpression CreateKeyValuePair<TKey, TValue>(TKey key, Expression value)
            where TKey : IConvertible, IComparable
        {
            return CreateKeyValuePair<TKey, TValue>(Constant(key), value);
        }

        public static NewExpression Restore<TKey, TValue>(KeyValuePair<TKey, TValue> pair, Converter<TValue, Expression> converter)
            where TKey : IConvertible, IComparable
        {
            return CreateKeyValuePair<TKey, TValue>(pair.Key, converter.Invoke(pair.Value));
        }

        public static NewExpression Restore<TKey, TValue>(KeyValuePair<TKey, TValue> pair)
            where TKey : IConvertible, IComparable
            where TValue : IRestorable
        {
            return Restore<TKey, TValue>(pair, Restore<TValue>);
        }

        public static NewArrayExpression NewArray<TKey, TValue>(IEnumerable<KeyValuePair<TKey, TValue>> keyValuePairs, Converter<TValue, Expression> converter)
            where TKey : IConvertible, IComparable
        {
            return NewArray<KeyValuePair<TKey, TValue>>(keyValuePairs, p => CreateKeyValuePair<TKey, TValue>(p.Key, converter(p.Value)));
        }

        public static NewArrayExpression NewArray<TKey, TValue>(IEnumerable<KeyValuePair<TKey, TValue>> keyValuePairs)
            where TKey : IConvertible, IComparable
            where TValue : IRestorable
        {
            return NewArray<TKey, TValue>(keyValuePairs, Restore<TValue>);
        }

        public static NewExpression Restore<T>()
            where T : new()
        {
            return Expression.New(typeof(T).GetConstructor(Type.EmptyTypes));
        }

        public static bool IsTrue<T>(this IEnumerable<T> collection, Predicate<T> condition)
        {
            foreach (var elem in collection ?? Enumerable.Empty<T>())
                if (condition.Invoke(elem)) continue;
                else return false;
            return true;
        }

        public static void ForEach<T>(this IEnumerable<T> collection, Action<T> loopBody)
        {
            foreach (var elem in collection ?? Enumerable.Empty<T>())
                loopBody.Invoke(elem);
        }
    }
}
