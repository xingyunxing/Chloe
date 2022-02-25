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
    /// 并行合并数据源
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class ParallelMergeEnumerable<T> : FeatureEnumerable<T>
    {
        List<OrderedFeatureEnumerable<T>> _enumerables;

        /// <summary>
        /// 传入的 IFeatureEnumerable 实现的枚举器必须是  IOrderedFeatureEnumerator 类型
        /// </summary>
        /// <param name="enumerables"></param>
        public ParallelMergeEnumerable(IEnumerable<OrderedFeatureEnumerable<T>> enumerables)
        {
            this._enumerables = enumerables.ToList();
        }

        public override IFeatureEnumerator<T> GetFeatureEnumerator()
        {
            return this.GetFeatureEnumerator(default(CancellationToken));
        }

        public override IFeatureEnumerator<T> GetFeatureEnumerator(CancellationToken cancellationToken = default)
        {
            return new Enumerator(this, cancellationToken);
        }

        class Enumerator : IFeatureEnumerator<T>
        {
            ParallelMergeEnumerable<T> _enumerable;
            CancellationToken _cancellationToken;

            PriorityQueue<IOrderedFeatureEnumerator<T>> _queue;
            IOrderedFeatureEnumerator<T> _currentEnumerator;

            public Enumerator(ParallelMergeEnumerable<T> enumerable, CancellationToken cancellationToken = default)
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
                if (this._queue != null)
                {
                    foreach (var enumerator in this._queue)
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

            async Task<bool> StartEnumeratorAsync(IFeatureEnumerator<T> enumerator)
            {
                await Task.Yield();
                return await enumerator.MoveNextAsync();
            }
            async ValueTask LazyInit(bool @async)
            {
                this._queue = new PriorityQueue<IOrderedFeatureEnumerator<T>>(this._enumerable._enumerables.Count);

                List<IOrderedFeatureEnumerator<T>> enumerators = new List<IOrderedFeatureEnumerator<T>>(this._enumerable._enumerables.Count);
                Task<bool>[] tasks = new Task<bool>[this._enumerable._enumerables.Count];

                int i = 0;
                foreach (var enumerable in this._enumerable._enumerables)
                {
                    var enumerator = (IOrderedFeatureEnumerator<T>)enumerable.GetFeatureEnumerator(this._cancellationToken);
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
