#if netfx
using System;
using System.Threading;
using System.Threading.Tasks;

namespace System.Collections.Generic
{
    internal interface IAsyncEnumerable<out T>
    {
        IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default);
    }

    internal static class AsyncEnumerableExtension
    {
        public static async Task<List<T>> ToListAsync<T>(this IAsyncEnumerable<T> source, CancellationToken cancellationToken = default)
        {
            List<T> list = new List<T>();
            var enumerator = source.GetAsyncEnumerator(cancellationToken);
            try
            {
                while (await enumerator.MoveNextAsync())
                {
                    list.Add(enumerator.Current);
                }
            }
            finally
            {
                await enumerator.DisposeAsync();
            }

            return list;
        }

        public static async Task<T> FirstAsync<T>(this IAsyncEnumerable<T> source, CancellationToken cancellationToken = default)
        {
            var enumerator = source.GetAsyncEnumerator(cancellationToken);
            try
            {
                if (await enumerator.MoveNextAsync())
                {
                    return enumerator.Current;
                }
            }
            finally
            {
                await enumerator.DisposeAsync();
            }

            throw new InvalidOperationException("The source sequence is empty.");
        }

        public static async Task<T> FirstOrDefaultAsync<T>(this IAsyncEnumerable<T> source, CancellationToken cancellationToken = default)
        {
            var enumerator = source.GetAsyncEnumerator(cancellationToken);
            try
            {
                if (await enumerator.MoveNextAsync())
                {
                    return enumerator.Current;
                }
            }
            finally
            {
                await enumerator.DisposeAsync();
            }

            return default(T);
        }

        public static async Task<T> SingleAsync<T>(this IAsyncEnumerable<T> source, CancellationToken cancellationToken = default)
        {
            var enumerator = source.GetAsyncEnumerator(cancellationToken);
            try
            {
                if (await enumerator.MoveNextAsync())
                {
                    T t = enumerator.Current;

                    if (await enumerator.MoveNextAsync())
                    {
                        throw new InvalidOperationException("The source has more than one element.");
                    }

                    return t;
                }
            }
            finally
            {
                await enumerator.DisposeAsync();
            }

            throw new InvalidOperationException("The source sequence is empty.");
        }

        public static async Task<bool> AnyAsync<T>(this IAsyncEnumerable<T> source, CancellationToken cancellationToken = default)
        {
            var enumerator = source.GetAsyncEnumerator(cancellationToken);
            try
            {
                return await enumerator.MoveNextAsync();
            }
            finally
            {
                await enumerator.DisposeAsync();
            }
        }
    }
}
#endif
