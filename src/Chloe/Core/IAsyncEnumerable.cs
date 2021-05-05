using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Chloe.Collections.Generic
{
    internal interface IAsyncEnumerable<out T>
    {
        IAsyncEnumerator<T> GetEnumerator();
    }

    internal static class AsyncEnumerableExtension
    {
        public static async Task<List<T>> ToList<T>(this IAsyncEnumerable<T> source)
        {
            List<T> list = new List<T>();
            using (var enumerator = source.GetEnumerator())
            {
                while (await enumerator.MoveNext())
                {
                    list.Add(enumerator.Current);
                }
            }

            return list;
        }

        public static async Task<T> First<T>(this IAsyncEnumerable<T> source)
        {
            using (var enumerator = source.GetEnumerator())
            {
                if (await enumerator.MoveNext())
                {
                    return enumerator.Current;
                }
            }

            throw new InvalidOperationException("The source sequence is empty.");
        }

        public static async Task<T> FirstOrDefault<T>(this IAsyncEnumerable<T> source)
        {
            using (var enumerator = source.GetEnumerator())
            {
                if (await enumerator.MoveNext())
                {
                    return enumerator.Current;
                }
            }

            return default(T);
        }

        public static async Task<T> Single<T>(this IAsyncEnumerable<T> source)
        {
            using (var enumerator = source.GetEnumerator())
            {
                if (await enumerator.MoveNext())
                {
                    T t = enumerator.Current;

                    if (await enumerator.MoveNext())
                    {
                        throw new InvalidOperationException("The source has more than one element.");
                    }

                    return t;
                }
            }

            throw new InvalidOperationException("The source sequence is empty.");
        }

        public static async Task<bool> Any<T>(this IAsyncEnumerable<T> source)
        {
            using (var enumerator = source.GetEnumerator())
            {
                return await enumerator.MoveNext();
            }
        }
    }
}
