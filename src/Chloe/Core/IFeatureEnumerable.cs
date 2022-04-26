using System.Collections;
using System.Threading;

namespace Chloe
{
    internal interface IFeatureEnumerable<out T> : IAsyncEnumerable<T>, IEnumerable<T>
    {
        IFeatureEnumerator<T> GetFeatureEnumerator(CancellationToken cancellationToken = default(CancellationToken));
    }

    abstract class FeatureEnumerable<T> : IFeatureEnumerable<T>
    {
        protected FeatureEnumerable()
        {

        }

        public virtual IFeatureEnumerator<T> GetFeatureEnumerator()
        {
            return this.GetFeatureEnumerator(CancellationToken.None);
        }
        public abstract IFeatureEnumerator<T> GetFeatureEnumerator(CancellationToken cancellationToken = default);

        public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
        {
            return this.GetFeatureEnumerator(cancellationToken);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return this.GetFeatureEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetFeatureEnumerator();
        }
    }

    class FeatureEnumerableAdapter<T> : FeatureEnumerable<T>
    {
        object _enumerable;

        public FeatureEnumerableAdapter(object enumerable)
        {
            if (!(enumerable is IAsyncEnumerable<T>) && !(enumerable is IEnumerable<T>))
            {
                throw new ArgumentException();
            }

            this._enumerable = enumerable;
        }

        public override IFeatureEnumerator<T> GetFeatureEnumerator(CancellationToken cancellationToken = default)
        {
            if (this._enumerable is IFeatureEnumerable<T> featureEnumerable)
            {
                return featureEnumerable.GetFeatureEnumerator(cancellationToken);
            }

            if (this._enumerable is IAsyncEnumerable<T> asyncEnumerable)
            {
                return new FeatureEnumeratorAdapter<T>(asyncEnumerable.GetAsyncEnumerator(cancellationToken));
            }

            if (this._enumerable is IEnumerable<T> enumerable)
            {
                return new FeatureEnumeratorAdapter<T>(enumerable.GetEnumerator());
            }

            throw new NotImplementedException();
        }
    }

    class NullFeatureEnumerable<T> : FeatureEnumerable<T>
    {
        public static readonly NullFeatureEnumerable<T> Instance = new NullFeatureEnumerable<T>();

        public NullFeatureEnumerable()
        {

        }

        public override IFeatureEnumerator<T> GetFeatureEnumerator(CancellationToken cancellationToken = default)
        {
            return NullFeatureEnumerator<T>.Instance;
        }
    }
    class ScalarFeatureEnumerable<T> : FeatureEnumerable<T>
    {
        T _result;

        public ScalarFeatureEnumerable(T result)
        {
            this._result = result;
        }

        public override IFeatureEnumerator<T> GetFeatureEnumerator(CancellationToken cancellationToken = default)
        {
            return new ScalarFeatureEnumerator<T>(this._result);
        }
    }

    static class FeatureEnumerableExtension
    {
        public static IAsyncEnumerable<T> AsAsyncEnumerable<T>(this IFeatureEnumerable<T> source)
        {
            return source;
        }
        public static IEnumerable<T> AsEnumerable<T>(this IFeatureEnumerable<T> source)
        {
            return source;
        }

        public static async ValueTask ForEachAsync<T>(this IFeatureEnumerable<T> source, Action<T, int> action, CancellationToken cancellationToken = default(CancellationToken))
        {
            await source.ForEachAsync(async (item, idx) =>
            {
                action(item, idx);
            }, cancellationToken);
        }

        public static async ValueTask ForEachAsync<T>(this IFeatureEnumerable<T> source, Func<T, int, Task> func, CancellationToken cancellationToken = default(CancellationToken))
        {
            using (var enumerator = source.GetFeatureEnumerator(cancellationToken))
            {
                int idx = 0;
                while (await enumerator.MoveNextAsync())
                {
                    T obj = enumerator.GetCurrent();
                    await func(obj, idx);
                    idx++;
                }
            }
        }

        public static IFeatureEnumerable<TResult> Select<T, TResult>(this IFeatureEnumerable<T> source, Func<T, TResult> selector)
        {
            throw new NotImplementedException();
        }
    }
}
