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
        public static async Task<List<T>> ToListAsync<T>(this IAsyncEnumerable<T> source)
        {
            IAsyncEnumerator<T> enumerator = source.GetEnumerator();

            List<T> list = new List<T>();
            using (enumerator)
            {
                while (await enumerator.MoveNextAsync())
                {
                    list.Add(enumerator.Current);
                }
            }

            return list;
        }
    }
}
