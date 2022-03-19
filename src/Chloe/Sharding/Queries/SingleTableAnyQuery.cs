using Chloe.Threading.Tasks;
using System.Collections;
using System.Linq.Expressions;
using System.Threading;

namespace Chloe.Sharding.Queries
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    class SingleTableAnyQuery<T> : FeatureEnumerable<bool>
    {
        IParallelQueryContext _queryContext;
        IShareDbContextPool _dbContextPool;
        DataQueryModel _queryModel;

        public SingleTableAnyQuery(IParallelQueryContext queryContext, IShareDbContextPool dbContextPool, DataQueryModel queryModel)
        {
            this._queryContext = queryContext;
            this._dbContextPool = dbContextPool;
            this._queryModel = queryModel;
        }

        public override IFeatureEnumerator<bool> GetFeatureEnumerator(CancellationToken cancellationToken = default)
        {
            return new Enumerator(this, cancellationToken);
        }

        class Enumerator : IFeatureEnumerator<bool>
        {
            SingleTableAnyQuery<T> _enumerable;
            CancellationToken _cancellationToken;
            bool? Result = null;

            public Enumerator(SingleTableAnyQuery<T> enumerable, CancellationToken cancellationToken = default)
            {
                this._enumerable = enumerable;
                this._cancellationToken = cancellationToken;
            }

            public bool Current => this.Result.Value;

            object IEnumerator.Current => this.Result;

            public void Dispose()
            {

            }

            public ValueTask DisposeAsync()
            {
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

            async Task<bool> Query(IDbContext dbContext, bool @async)
            {
                var q = dbContext.Query<T>(this._enumerable._queryModel.Table.Name);

                foreach (var condition in this._enumerable._queryModel.Conditions)
                {
                    q = q.Where((Expression<Func<T, bool>>)condition);
                }

                if (this._enumerable._queryModel.IgnoreAllFilters)
                {
                    q = q.IgnoreAllFilters();
                }

                bool hasData = @async ? await q.AnyAsync() : q.Any();
                return hasData;
            }

            async BoolResultTask MoveNext(bool @async)
            {
                if (this.Result != null)
                {
                    this.Result = default;
                    return false;
                }

                var hasData = await ShardingHelpers.ExecuteQuery<bool>(this.Query, this._enumerable._queryContext, this._enumerable._dbContextPool, @async);

                this.Result = hasData;

                return true;
            }

            public void Reset()
            {
                throw new NotImplementedException();
            }
        }
    }
}
