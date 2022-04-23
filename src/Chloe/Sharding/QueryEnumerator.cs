using Chloe.Threading.Tasks;
using System.Collections;
using System.Threading;

namespace Chloe.Sharding
{
    internal class QueryEnumerator : IFeatureEnumerator<object>
    {
        IShareDbContextPool _dbContextPool;
        Func<IDbContext, bool, Task<(IFeatureEnumerable<object> Query, bool IsLazyQuery)>> _queryCreator;
        CancellationToken _cancellationToken;
        IFeatureEnumerator<object> _enumerator;

        IPoolItem<IDbContext> _poolResource;

        public QueryEnumerator(IShareDbContextPool dbContextPool, CancellationToken cancellationToken = default) : this(dbContextPool, null, cancellationToken)
        {

        }
        public QueryEnumerator(IShareDbContextPool dbContextPool, Func<IDbContext, bool, Task<(IFeatureEnumerable<object> Query, bool IsLazyQuery)>> queryCreator, CancellationToken cancellationToken = default)
        {
            this._dbContextPool = dbContextPool;
            this._queryCreator = queryCreator;
            this._cancellationToken = cancellationToken;
        }

        public object Current => this._enumerator.GetCurrent();

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
            var poolResource = await this._dbContextPool.GetOne(@async);
            this._poolResource = poolResource;

            var dbContext = poolResource.Resource;

            var result = await this.CreateQuery(dbContext, @async);

            if (!result.IsLazyQuery)
            {
                this._poolResource.Dispose();
                this._poolResource = null;
            }

            this._enumerator = result.Query.GetFeatureEnumerator(this._cancellationToken);
        }

        protected virtual async Task<(IFeatureEnumerable<object> Query, bool IsLazyQuery)> CreateQuery(IDbContext dbContext, bool @async)
        {
            if (this._queryCreator == null)
            {
                throw new NotImplementedException();
            }

            return await this._queryCreator(dbContext, @async);
        }

        public void Reset()
        {
            throw new NotImplementedException();
        }
    }

    class TableQueryEnumerator : QueryEnumerator
    {
        DataQueryModel _queryModel;
        Func<IQuery, bool, Task<(IFeatureEnumerable<object> Query, bool IsLazyQuery)>> _executor;

        public TableQueryEnumerator(IShareDbContextPool dbContextPool, DataQueryModel queryModel, CancellationToken cancellationToken = default) : this(dbContextPool, null, queryModel, cancellationToken)
        {

        }
        public TableQueryEnumerator(IShareDbContextPool dbContextPool, Func<IQuery, bool, Task<(IFeatureEnumerable<object> Query, bool IsLazyQuery)>> executor, DataQueryModel queryModel, CancellationToken cancellationToken = default) : base(dbContextPool, cancellationToken)
        {
            this._executor = executor;
            this._queryModel = queryModel;
        }

        static IQuery MakeQuery(IDbContext dbContext, DataQueryModel queryModel)
        {
            if (queryModel.Selector != null)
            {
                throw new NotSupportedException("不支持这种情况");
            }

            var q = ShardingHelpers.MakeQuery(dbContext, queryModel, true);
            return q;
        }

        protected sealed override async Task<(IFeatureEnumerable<object> Query, bool IsLazyQuery)> CreateQuery(IDbContext dbContext, bool @async)
        {
            var q = MakeQuery(dbContext, this._queryModel);
            var result = await this.CreateQuery(q, @async);

            return result;
        }

        protected virtual async Task<(IFeatureEnumerable<object> Query, bool IsLazyQuery)> CreateQuery(IQuery query, bool @async)
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
