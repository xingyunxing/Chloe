using Chloe.Threading.Tasks;
using System.Collections;
using System.Threading;

namespace Chloe.Sharding
{
    internal class QueryEnumerator<TResult> : IFeatureEnumerator<TResult>
    {
        ISharedDbContextProviderPool _dbContextProviderPool;
        Func<IDbContextProvider, bool, Task<(IFeatureEnumerable<TResult> Query, bool IsLazyQuery)>> _queryCreator;
        CancellationToken _cancellationToken;
        IFeatureEnumerator<TResult> _enumerator;

        IPoolItem<IDbContextProvider> _poolResource;

        public QueryEnumerator(ISharedDbContextProviderPool dbContextProviderPool, CancellationToken cancellationToken = default) : this(dbContextProviderPool, null, cancellationToken)
        {

        }
        public QueryEnumerator(ISharedDbContextProviderPool dbContextProviderPool, Func<IDbContextProvider, bool, Task<(IFeatureEnumerable<TResult> Query, bool IsLazyQuery)>> queryCreator, CancellationToken cancellationToken = default)
        {
            this._dbContextProviderPool = dbContextProviderPool;
            this._queryCreator = queryCreator;
            this._cancellationToken = cancellationToken;
        }

        public TResult Current => this._enumerator.GetCurrent();

        object IEnumerator.Current => this.Current;

        public void Dispose()
        {
            this.Dispose(false).GetResult();
        }

        public async ValueTask DisposeAsync()
        {
            await this.Dispose(true);
        }

        protected virtual ValueTask Dispose(bool @async)
        {
            this._poolResource?.Dispose();
            return default;
        }

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
            if (this._enumerator == null)
            {
                await this.InitEnumerator(@async);
            }

            return await this._enumerator.MoveNext(@async);
        }

        async ValueTask InitEnumerator(bool @async)
        {
            var poolResource = await this._dbContextProviderPool.GetOne(@async);
            this._poolResource = poolResource;

            var dbContextProvider = poolResource.Resource;

            (IFeatureEnumerable<TResult> Query, bool IsLazyQuery) result;
            try
            {
                result = await this.CreateQuery(dbContextProvider, @async);
            }
            catch
            {
                this._poolResource.Dispose();
                this._poolResource = null;
                throw;
            }

            if (!result.IsLazyQuery)
            {
                this._poolResource.Dispose();
                this._poolResource = null;
            }

            this._enumerator = result.Query.GetFeatureEnumerator(this._cancellationToken);
        }

        protected virtual async Task<(IFeatureEnumerable<TResult> Query, bool IsLazyQuery)> CreateQuery(IDbContextProvider dbContextProvider, bool @async)
        {
            if (this._queryCreator == null)
            {
                throw new NotImplementedException();
            }

            return await this._queryCreator(dbContextProvider, @async);
        }

        public void Reset()
        {
            throw new NotImplementedException();
        }
    }

    class TableQueryEnumerator<TResult> : QueryEnumerator<TResult>
    {
        DataQueryModel _queryModel;
        Func<IQuery, bool, Task<(IFeatureEnumerable<TResult> Query, bool IsLazyQuery)>> _executor;

        public TableQueryEnumerator(ISharedDbContextProviderPool dbContextProviderPool, DataQueryModel queryModel, CancellationToken cancellationToken = default) : this(dbContextProviderPool, null, queryModel, cancellationToken)
        {

        }
        public TableQueryEnumerator(ISharedDbContextProviderPool dbContextProviderPool, Func<IQuery, bool, Task<(IFeatureEnumerable<TResult> Query, bool IsLazyQuery)>> executor, DataQueryModel queryModel, CancellationToken cancellationToken = default) : base(dbContextProviderPool, cancellationToken)
        {
            this._executor = executor;
            this._queryModel = queryModel;
        }


        protected sealed override async Task<(IFeatureEnumerable<TResult> Query, bool IsLazyQuery)> CreateQuery(IDbContextProvider dbContextProvider, bool @async)
        {
            var q = ShardingHelpers.MakeQuery(dbContextProvider, this._queryModel);
            var result = await this.CreateQuery(q, @async);

            return result;
        }

        protected virtual async Task<(IFeatureEnumerable<TResult> Query, bool IsLazyQuery)> CreateQuery(IQuery query, bool @async)
        {
            if (this._executor == null)
            {
                throw new NotImplementedException();
            }

            var result = await this._executor(query, @async);

            return result;
        }
    }
}
