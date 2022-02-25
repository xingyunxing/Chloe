using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chloe.Sharding
{
    /// <summary>
    /// 流式合并
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class StreamMergeEnumerator<T> : IFeatureEnumerator<T>
    {
        IEnumerable<IOrderedFeatureEnumerator<T>> _enumerators;
        IOrderedFeatureEnumerator<T> _currentEnumerator;

        PriorityQueue<IOrderedFeatureEnumerator<T>> _queue;

        public StreamMergeEnumerator(IEnumerable<IOrderedFeatureEnumerator<T>> enumerators)
        {
            this._enumerators = enumerators;
        }

        async ValueTask InitQueue(bool @async)
        {
            this._queue = new PriorityQueue<IOrderedFeatureEnumerator<T>>(this._enumerators.Count());
            foreach (var enumerator in this._enumerators)
            {
                bool hasElement = await enumerator.MoveNext(@async);

                if (!hasElement)
                {
                    enumerator.Dispose();
                    continue;
                }

                this._queue.Push(enumerator);
            }
        }

        public T Current
        {
            get
            {
                return this._currentEnumerator.GetCurrent();
            }
        }
        object IEnumerator.Current
        {
            get
            {
                return this.Current;
            }
        }

        public bool MoveNext()
        {
            return this.MoveNext(false).GetAwaiter().GetResult();
        }
        public BoolResultTask MoveNextAsync()
        {
            return this.MoveNext(true);
        }

        public void Reset()
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            foreach (var enumerator in this._enumerators)
            {
                enumerator.Dispose();
            }
        }
        public async ValueTask DisposeAsync()
        {
            foreach (var enumerator in this._enumerators)
            {
                await enumerator.DisposeAsync();
            }
        }

        async BoolResultTask MoveNext(bool @async)
        {
            if (this._queue == null)
            {
                await this.InitQueue(@async);
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
                first.Dispose();
                if (this._queue.IsEmpty())
                {
                    return false;
                }
            }

            this._currentEnumerator = this._queue.Peek();
            return true;
        }
    }
}
