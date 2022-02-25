using Chloe.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Chloe.Sharding
{
    /// <summary>
    /// 并行连接多个数据源
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class ParallelConcatEnumerable<T> : FeatureEnumerable<T>
    {
        List<IFeatureEnumerable<T>> _enumerables;

        public ParallelConcatEnumerable(IEnumerable<IFeatureEnumerable<T>> enumerables)
        {
            this._enumerables = enumerables.ToList();
        }

        public override IFeatureEnumerator<T> GetFeatureEnumerator()
        {
            return new Enumerator(this);
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
            }

            public bool MoveNext()
            {
                return this.MoveNext(false).GetResult();
            }

            public BoolResultTask MoveNextAsync()
            {
                return this.MoveNext(true);
            }

            async Task StartEnumeratorAsync(IFeatureEnumerator<T> enumerator)
            {
                await Task.Yield();
                await enumerator.MoveNextAsync();
            }
            async ValueTask LazyInit(bool @async)
            {
                if (this._currentEnumerator != null)
                {
                    return;
                }

                List<IFeatureEnumerator<T>> enumerators = new List<IFeatureEnumerator<T>>(this._enumerable._enumerables.Count);
                Task[] tasks = new Task[this._enumerable._enumerables.Count];

                int i = 0;
                foreach (var enumerable in this._enumerable._enumerables)
                {
                    var enumerator = enumerable.GetFeatureEnumerator(this._cancellationToken);
                    enumerators.Add(enumerator);
                    tasks[i] = this.StartEnumeratorAsync(enumerator);
                    i++;
                }

                try
                {
                    //TODO 处理异常
                    await Task.WhenAll(tasks);
                }
                finally
                {
                    foreach (var enumerator in enumerators)
                    {
                        await enumerator.Dispose(@async);
                    }
                }

                enumerators = enumerators.Select(a => new FeatureEnumeratorWrapper<T>(a) as IFeatureEnumerator<T>).ToList();

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
