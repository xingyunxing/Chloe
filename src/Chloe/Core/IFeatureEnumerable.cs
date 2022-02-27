using System.Collections;
using System.Threading;
using System.Threading.Tasks;

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

    static class FeatureEnumerableExtension
    {
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
