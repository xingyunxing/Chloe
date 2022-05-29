using Chloe.Threading.Tasks;
using System.Collections;
using System.Threading;

namespace Chloe.Sharding
{
    /// <summary>
    /// 并行连接多个数据源
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class ParallelConcatEnumerable<T> : FeatureEnumerable<T>
    {
        ParallelQueryContext _queryContext;
        List<IFeatureEnumerable<T>> _sources;

        public ParallelConcatEnumerable(ParallelQueryContext queryContext, IEnumerable<IFeatureEnumerable<T>> sources)
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
            ParallelConcatEnumerable<T> _enumerable;
            CancellationToken _cancellationToken;

            List<IFeatureEnumerator<T>> _enumerators;
            IFeatureEnumerator<T> _currentEnumerator;
            int _currentEnumeratorIndex = -1;

            public Enumerator(ParallelConcatEnumerable<T> enumerable, CancellationToken cancellationToken = default)
            {
                this._enumerable = enumerable;
                this._cancellationToken = cancellationToken;
            }

            public T Current => this._currentEnumerator.GetCurrent();

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
                if (this._enumerators != null)
                {
                    foreach (var enumerator in this._enumerators)
                    {
                        await enumerator.Dispose(@async);
                    }
                }

                this._enumerable._queryContext.Dispose();
            }

            public bool MoveNext()
            {
                return this.MoveNext(false).GetResult();
            }

            public BoolResultTask MoveNextAsync()
            {
                return this.MoveNext(true);
            }

            async Task StartEnumeratorAsync(PreloadableFeatureEnumerator<T> enumerator)
            {
                await Task.Yield();

                try
                {
                    await enumerator.Initialize();
                }
                catch
                {
                    this._enumerable._queryContext.Cancel();
                    throw;
                }
            }
            async ValueTask LazyInit(bool @async)
            {
                if (this._currentEnumerator != null)
                {
                    return;
                }

                List<IFeatureEnumerator<T>> enumerators = new List<IFeatureEnumerator<T>>(this._enumerable._sources.Count);
                Task[] tasks = new Task[this._enumerable._sources.Count];

                int i = 0;
                foreach (var source in this._enumerable._sources)
                {
                    var enumerator = new PreloadableFeatureEnumerator<T>(source.GetFeatureEnumerator(this._cancellationToken));
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

                if (enumerators.Count == 0)
                {
                    enumerators.Add(new NullFeatureEnumerator<T>());
                }

                this._enumerators = enumerators;
                this._currentEnumeratorIndex = 0;
                this._currentEnumerator = this._enumerators[this._currentEnumeratorIndex];
            }
            async BoolResultTask MoveNext(bool @async)
            {
                await this.LazyInit(@async);
                bool hasNext = @async ? await this._currentEnumerator.MoveNextAsync() : this._currentEnumerator.MoveNext();
                if (hasNext)
                {
                    return true;
                }

                if (this._currentEnumeratorIndex < this._enumerators.Count - 1)
                {
                    this._currentEnumeratorIndex++;
                    this._currentEnumerator = this._enumerators[this._currentEnumeratorIndex];

                    return await this.MoveNext(@async);
                }

                return false;
            }

            public void Reset()
            {
                throw new NotImplementedException();
            }
        }
    }
}
