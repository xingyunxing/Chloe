using Chloe.Threading.Tasks;
using System.Collections;
using System.Threading;

namespace Chloe.Sharding
{
    /// <summary>
    /// 并行合并数据源
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class ParallelMergeOrderedEnumerable<T> : FeatureEnumerable<T>
    {
        IParallelQueryContext _queryContext;
        List<OrderedFeatureEnumerable<T>> _sources;

        /// <summary>
        /// 传入的 IFeatureEnumerable 实现的枚举器必须是  IOrderedFeatureEnumerator 类型
        /// </summary>
        /// <param name="queryContext"></param>
        /// <param name="sources"></param>
        public ParallelMergeOrderedEnumerable(IParallelQueryContext queryContext, IEnumerable<OrderedFeatureEnumerable<T>> sources)
        {
            this._queryContext = queryContext;
            this._sources = sources.ToList();
        }

        public override IFeatureEnumerator<T> GetFeatureEnumerator(CancellationToken cancellationToken = default)
        {
            return new Enumerator(this, cancellationToken);
        }

        class Enumerator : IFeatureEnumerator<T>
        {
            bool _disposed = false;
            ParallelMergeOrderedEnumerable<T> _enumerable;
            CancellationToken _cancellationToken;

            PriorityQueue<IOrderedFeatureEnumerator<T>> _queue;
            IOrderedFeatureEnumerator<T> _currentEnumerator;

            public Enumerator(ParallelMergeOrderedEnumerable<T> enumerable, CancellationToken cancellationToken = default)
            {
                this._enumerable = enumerable;
                this._cancellationToken = cancellationToken;
            }

            public T Current => (this._currentEnumerator as IEnumerator<T>).Current;

            object IEnumerator.Current => this.Current;

            public void Dispose()
            {
                this.Dispose(false).GetResult();
            }

            public async ValueTask DisposeAsync()
            {
                await this.Dispose(true);
            }
            async ValueTask Dispose(bool @async)
            {
                if (this._disposed)
                    return;

                if (this._queue != null)
                {
                    foreach (var enumerator in this._queue)
                    {
                        await enumerator.Dispose(@async);
                    }
                }

                this._enumerable._queryContext.Dispose();
                this._disposed = true;
            }

            public bool MoveNext()
            {
                return this.MoveNext(false).GetResult();
            }

            public BoolResultTask MoveNextAsync()
            {
                return this.MoveNext(true);
            }

            async Task<bool> StartEnumeratorAsync(IFeatureEnumerator<T> enumerator)
            {
                await Task.Yield();
                try
                {
                    return await enumerator.MoveNextAsync();
                }
                catch
                {
                    this._enumerable._queryContext.Cancel();
                    throw;
                }
            }
            async ValueTask LazyInit(bool @async)
            {
                this._queue = new PriorityQueue<IOrderedFeatureEnumerator<T>>(this._enumerable._sources.Count);

                List<IOrderedFeatureEnumerator<T>> enumerators = new List<IOrderedFeatureEnumerator<T>>(this._enumerable._sources.Count);
                Task<bool>[] tasks = new Task<bool>[this._enumerable._sources.Count];

                int i = 0;
                foreach (var source in this._enumerable._sources)
                {
                    var enumerator = (IOrderedFeatureEnumerator<T>)source.GetFeatureEnumerator(this._cancellationToken);
                    enumerators.Add(enumerator);
                    tasks[i] = this.StartEnumeratorAsync(enumerator);
                    i++;
                }

                try
                {
                    await Task.WhenAll(tasks);
                }
                catch
                {
                    foreach (var enumerator in enumerators)
                    {
                        await enumerator.Dispose(@async);
                    }

                    throw;
                }

                i = 0;
                foreach (var task in tasks)
                {
                    var enumerator = enumerators[i];

                    bool hasElement = task.Result;
                    if (hasElement == true)
                    {
                        this._queue.Push(enumerator);
                    }
                    else
                    {
                        await enumerator.Dispose(@async);
                    }

                    i++;
                }
            }
            async BoolResultTask MoveNext(bool @async)
            {
                if (this._queue == null)
                {
                    await this.LazyInit(@async);
                    return this._queue.TryPeek(out this._currentEnumerator);
                }

                if (this._queue.IsEmpty())
                    return false;

                var first = this._queue.Poll();
                var hasNext = await first.MoveNext(@async);
                if (hasNext)
                {
                    this._queue.Push(first);
                }
                else
                {
                    await first.Dispose(@async);
                    if (this._queue.IsEmpty())
                    {
                        return false;
                    }
                }

                this._currentEnumerator = this._queue.Peek();
                return true;
            }

            public void Reset()
            {
                throw new NotImplementedException();
            }
        }
    }
}
