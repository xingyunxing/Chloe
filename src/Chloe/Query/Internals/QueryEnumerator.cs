using Chloe.Collections;
using Chloe.Core;
using Chloe.Mapper;
using Chloe.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

#if netfx
using BoolResultTask = System.Threading.Tasks.Task<bool>;
#else
using BoolResultTask = System.Threading.Tasks.ValueTask<bool>;
#endif

namespace Chloe.Query.Internals
{
    internal class QueryEnumerator<T> : IEnumerator<T>, Chloe.Collections.Generic.IAsyncEnumerator<T>
    {
        bool _disposed;
        DataReaderEnumerator _dataReaderEnumerator;
        Func<IDataReader, IObjectActivator> _objectActivatorCreator;
        IObjectActivator _objectActivator;
        T _current;

        public QueryEnumerator(Func<bool, Task<IDataReader>> dataReaderCreator, IObjectActivator objectActivator) : this(dataReaderCreator, dataReader => objectActivator)
        {

        }
        public QueryEnumerator(Func<bool, Task<IDataReader>> dataReaderCreator, Func<IDataReader, IObjectActivator> objectActivatorCreator)
        {
            this._dataReaderEnumerator = new DataReaderEnumerator(dataReaderCreator);
            this._objectActivatorCreator = objectActivatorCreator;
        }

        public T Current { get { return this._current; } }
        object IEnumerator.Current { get { return this._current; } }

        public bool MoveNext()
        {
            return this.MoveNext(false).GetResult();
        }

        public BoolResultTask MoveNextAsync()
        {
            return this.MoveNext(true);
        }

        async BoolResultTask MoveNext(bool @async)
        {
            if (this._disposed)
                return false;

            bool nextResult = @async ? await this._dataReaderEnumerator.MoveNextAsync() : this._dataReaderEnumerator.MoveNext();

            if (nextResult)
            {
                if (this._objectActivator == null)
                {
                    this._objectActivator = this._objectActivatorCreator(this._dataReaderEnumerator.Current);
                }

                this._current = (T)(await this._objectActivator.CreateInstance(this._dataReaderEnumerator.Current, @async));
            }
            else
            {
                this._current = default;
            }

            return nextResult;
        }

        public void Dispose()
        {
            if (this._disposed)
                return;

            this._dataReaderEnumerator.Dispose();
            this._current = default;
            this._disposed = true;
        }

        public void Reset()
        {
            throw new NotSupportedException();
        }
    }
}
