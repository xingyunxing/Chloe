using System.Threading;

namespace System.Collections.Generic
{
    internal static partial class AsyncEnumerableExtension
    {
        public static async ValueTask ForEach<T>(this IAsyncEnumerable<T> source, Action<T> action, CancellationToken cancellationToken = default(CancellationToken))
        {
            await source.ForEach(async (item, idx) =>
            {
                action(item);
            }, cancellationToken);
        }
        public static async ValueTask ForEach<T>(this IAsyncEnumerable<T> source, Func<T, ValueTask> func, CancellationToken cancellationToken = default(CancellationToken))
        {
            await source.ForEach(async (item, idx) =>
            {
                await func(item);
            }, cancellationToken);
        }
        public static async ValueTask ForEach<T>(this IAsyncEnumerable<T> source, Func<T, int, Task> func, CancellationToken cancellationToken = default(CancellationToken))
        {
            var enumerator = source.GetAsyncEnumerator(cancellationToken);

            try
            {
                int idx = 0;
                while (await enumerator.MoveNextAsync())
                {
                    T obj = enumerator.Current;
                    await func(obj, idx);
                    idx++;
                }
            }
            finally
            {
                await enumerator.DisposeAsync();
            }
        }
    }
}

#if netfx

namespace System.Collections.Generic
{
    using System.Linq;

    internal interface IAsyncEnumerable<out T>
    {
        IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default);
    }

    internal static partial class AsyncEnumerableExtension
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
        public static async Task<IEnumerable<T>> ToEnumerableAsync<T>(this IAsyncEnumerable<T> source, CancellationToken cancellationToken = default)
        {
            return await source.ToListAsync(cancellationToken);
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

        public static IAsyncEnumerable<T> Where<T>(this IAsyncEnumerable<T> source, Func<T, bool> predicate)
        {
            return new WhereEnumerable<T>(source, predicate);
        }

        public static IAsyncEnumerable<TResult> Select<T, TResult>(this IAsyncEnumerable<T> source, Func<T, TResult> selector)
        {
            return new SelectEnumerable<T, TResult>(source, selector);
        }

        public static IAsyncEnumerable<T> Skip<T>(this IAsyncEnumerable<T> source, int count)
        {
            return new SkipEnumerable<T>(source, count);
        }
        public static IAsyncEnumerable<T> Take<T>(this IAsyncEnumerable<T> source, int count)
        {
            return new TakeEnumerable<T>(source, count);
        }

        public static async Task<int> SumAsync(this IAsyncEnumerable<int> source, CancellationToken cancellationToken = default)
        {
            int sum = 0;

            await source.ForEach(a =>
            {
                sum = sum + a;
            }, cancellationToken);

            return sum;
        }
        public static async Task<int?> SumAsync(this IAsyncEnumerable<int?> source, CancellationToken cancellationToken = default)
        {
            int sum = 0;

            await source.ForEach(a =>
            {
                if (a == null)
                    return;

                sum = sum + a.Value;
            }, cancellationToken);

            return sum;
        }

        public static async Task<long> SumAsync(this IAsyncEnumerable<long> source, CancellationToken cancellationToken = default)
        {
            long sum = 0;

            await source.ForEach(a =>
            {
                sum = sum + a;
            }, cancellationToken);

            return sum;
        }
        public static async Task<long?> SumAsync(this IAsyncEnumerable<long?> source, CancellationToken cancellationToken = default)
        {
            long sum = 0;

            await source.ForEach(a =>
            {
                if (a == null)
                    return;

                sum = sum + a.Value;
            }, cancellationToken);

            return sum;
        }

        public static async Task<decimal> SumAsync(this IAsyncEnumerable<decimal> source, CancellationToken cancellationToken = default)
        {
            decimal sum = 0;

            await source.ForEach(a =>
            {
                sum = sum + a;
            }, cancellationToken);

            return sum;
        }
        public static async Task<decimal?> SumAsync(this IAsyncEnumerable<decimal?> source, CancellationToken cancellationToken = default)
        {
            decimal sum = 0;

            await source.ForEach(a =>
            {
                if (a == null)
                    return;

                sum = sum + a.Value;
            }, cancellationToken);

            return sum;
        }

        public static async Task<double> SumAsync(this IAsyncEnumerable<double> source, CancellationToken cancellationToken = default)
        {
            double sum = 0;

            await source.ForEach(a =>
            {
                sum = sum + a;
            }, cancellationToken);

            return sum;
        }
        public static async Task<double?> SumAsync(this IAsyncEnumerable<double?> source, CancellationToken cancellationToken = default)
        {
            double sum = 0;

            await source.ForEach(a =>
            {
                if (a == null)
                    return;

                sum = sum + a.Value;
            }, cancellationToken);

            return sum;
        }

        public static async Task<float> SumAsync(this IAsyncEnumerable<float> source, CancellationToken cancellationToken = default)
        {
            float sum = 0;

            await source.ForEach(a =>
            {
                sum = sum + a;
            }, cancellationToken);

            return sum;
        }
        public static async Task<float?> SumAsync(this IAsyncEnumerable<float?> source, CancellationToken cancellationToken = default)
        {
            float sum = 0;

            await source.ForEach(a =>
            {
                if (a == null)
                    return;

                sum = sum + a.Value;
            }, cancellationToken);

            return sum;
        }

        public static async Task<double?> AverageAsync(this IAsyncEnumerable<int?> source, CancellationToken cancellationToken = default)
        {
            var list = await source.ToEnumerableAsync(cancellationToken);
            double? avg = list.Average();

            return avg;
        }

        public static async Task<double?> AverageAsync(this IAsyncEnumerable<long?> source, CancellationToken cancellationToken = default)
        {
            var list = await source.ToEnumerableAsync(cancellationToken);
            double? avg = list.Average();

            return avg;
        }

        public static async Task<double?> AverageAsync(this IAsyncEnumerable<double?> source, CancellationToken cancellationToken = default)
        {
            var list = await source.ToEnumerableAsync(cancellationToken);
            double? avg = list.Average();

            return avg;
        }

        public static async Task<float?> AverageAsync(this IAsyncEnumerable<float?> source, CancellationToken cancellationToken = default)
        {
            var list = await source.ToEnumerableAsync(cancellationToken);
            float? avg = list.Average();

            return avg;
        }

        public static async Task<decimal?> AverageAsync(this IAsyncEnumerable<decimal?> source, CancellationToken cancellationToken = default)
        {
            var list = await source.ToEnumerableAsync(cancellationToken);
            decimal? avg = list.Average();

            return avg;
        }

        public class WhereEnumerable<T> : IAsyncEnumerable<T>
        {
            IAsyncEnumerable<T> _source;
            Func<T, bool> _predicate;

            public WhereEnumerable(IAsyncEnumerable<T> source, Func<T, bool> predicate)
            {
                this._source = source;
                this._predicate = predicate;
            }

            public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
            {
                return new Enumerator(this, cancellationToken);
            }

            class Enumerator : IAsyncEnumerator<T>
            {
                WhereEnumerable<T> _enumerable;
                CancellationToken _cancellationToken;

                IAsyncEnumerator<T> _enumerator;

                T _current;

                public Enumerator(WhereEnumerable<T> enumerable, CancellationToken cancellationToken)
                {
                    this._enumerable = enumerable;
                    this._cancellationToken = cancellationToken;
                }

                public T Current => this._current;

                public async Task<bool> MoveNextAsync()
                {
                    if (this._enumerator == null)
                    {
                        this._enumerator = this._enumerable._source.GetAsyncEnumerator(this._cancellationToken);
                    }

                    bool hasNext;
                    while (true)
                    {
                        hasNext = await this._enumerator.MoveNextAsync();
                        if (hasNext)
                        {
                            bool match = this._enumerable._predicate(this._enumerator.Current);

                            if (!match)
                            {
                                continue;
                            }

                            this._current = this._enumerator.Current;
                            break;
                        }
                        else
                        {
                            this._current = default;
                            break;
                        }
                    }

                    return hasNext;
                }

                public async Task DisposeAsync()
                {
                    if (this._enumerator != null)
                    {
                        await this._enumerator.DisposeAsync();
                    }
                }
            }
        }
        public class SelectEnumerable<T, TResult> : IAsyncEnumerable<TResult>
        {
            IAsyncEnumerable<T> _source;
            Func<T, TResult> _selector;

            public SelectEnumerable(IAsyncEnumerable<T> source, Func<T, TResult> selector)
            {
                this._source = source;
                this._selector = selector;
            }

            public IAsyncEnumerator<TResult> GetAsyncEnumerator(CancellationToken cancellationToken = default)
            {
                return new Enumerator(this, cancellationToken);
            }

            class Enumerator : IAsyncEnumerator<TResult>
            {
                SelectEnumerable<T, TResult> _enumerable;
                CancellationToken _cancellationToken;

                IAsyncEnumerator<T> _enumerator;

                TResult _current;

                public Enumerator(SelectEnumerable<T, TResult> enumerable, CancellationToken cancellationToken)
                {
                    this._enumerable = enumerable;
                    this._cancellationToken = cancellationToken;
                }

                public TResult Current => this._current;

                public async Task<bool> MoveNextAsync()
                {
                    if (this._enumerator == null)
                    {
                        this._enumerator = this._enumerable._source.GetAsyncEnumerator(this._cancellationToken);
                    }

                    bool hasNext = await this._enumerator.MoveNextAsync();
                    if (hasNext)
                    {
                        this._current = this._enumerable._selector(this._enumerator.Current);
                    }
                    else
                    {
                        this._current = default;
                    }

                    return hasNext;
                }

                public async Task DisposeAsync()
                {
                    if (this._enumerator != null)
                    {
                        await this._enumerator.DisposeAsync();
                    }
                }
            }
        }
        public class SkipEnumerable<T> : IAsyncEnumerable<T>
        {
            IAsyncEnumerable<T> _source;
            int _count;

            public SkipEnumerable(IAsyncEnumerable<T> source, int count)
            {
                this._source = source;
                this._count = count;
            }

            public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
            {
                return new Enumerator(this, cancellationToken);
            }

            class Enumerator : IAsyncEnumerator<T>
            {
                SkipEnumerable<T> _enumerable;
                CancellationToken _cancellationToken;

                IAsyncEnumerator<T> _enumerator;

                T _current;

                public Enumerator(SkipEnumerable<T> enumerable, CancellationToken cancellationToken)
                {
                    this._enumerable = enumerable;
                    this._cancellationToken = cancellationToken;
                }

                public T Current => this._current;

                public async Task<bool> MoveNextAsync()
                {
                    if (this._enumerator == null)
                    {
                        this._enumerator = this._enumerable._source.GetAsyncEnumerator(this._cancellationToken);

                        int skips = 0;
                        while (this._enumerable._count > skips)
                        {
                            await this._enumerator.MoveNextAsync();
                            skips++;
                        }
                    }

                    bool hasNext = await this._enumerator.MoveNextAsync();
                    if (hasNext)
                    {
                        this._current = this._enumerator.Current;
                    }
                    else
                    {
                        this._current = default;
                    }

                    return hasNext;
                }

                public async Task DisposeAsync()
                {
                    if (this._enumerator != null)
                    {
                        await this._enumerator.DisposeAsync();
                    }
                }
            }
        }
        public class TakeEnumerable<T> : IAsyncEnumerable<T>
        {
            IAsyncEnumerable<T> _source;
            int _count;

            public TakeEnumerable(IAsyncEnumerable<T> source, int count)
            {
                this._source = source;
                this._count = count;
            }

            public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
            {
                return new Enumerator(this, cancellationToken);
            }

            class Enumerator : IAsyncEnumerator<T>
            {
                TakeEnumerable<T> _enumerable;
                CancellationToken _cancellationToken;

                IAsyncEnumerator<T> _enumerator;
                int _takens;

                T _current;

                public Enumerator(TakeEnumerable<T> enumerable, CancellationToken cancellationToken)
                {
                    this._enumerable = enumerable;
                    this._cancellationToken = cancellationToken;
                }

                public T Current => this._current;

                public async Task<bool> MoveNextAsync()
                {
                    if (this._enumerator == null)
                    {
                        this._enumerator = this._enumerable._source.GetAsyncEnumerator(this._cancellationToken);
                    }

                    if (this._takens >= this._enumerable._count)
                    {
                        this._current = default;
                        return false;
                    }

                    bool hasNext = await this._enumerator.MoveNextAsync();
                    if (hasNext)
                    {
                        this._current = this._enumerator.Current;
                    }
                    else
                    {
                        this._current = default;
                    }

                    this._takens++;

                    return hasNext;
                }

                public async Task DisposeAsync()
                {
                    if (this._enumerator != null)
                    {
                        await this._enumerator.DisposeAsync();
                    }
                }
            }
        }
    }
}
#endif

