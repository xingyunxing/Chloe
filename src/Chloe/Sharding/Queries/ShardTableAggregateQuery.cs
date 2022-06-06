using System.Threading;

namespace Chloe.Sharding.Queries
{
    class ShardTableAggregateQuery<TResult> : FeatureEnumerable<TResult>
    {
        IParallelQueryContext _queryContext;
        ISharedDbContextProviderPool _dbContextProviderPool;
        DataQueryModel _queryModel;
        Func<IQuery, bool, Task<TResult>> _executor;

        public ShardTableAggregateQuery(IParallelQueryContext queryContext, ISharedDbContextProviderPool dbContextProviderPool, DataQueryModel queryModel, Func<IQuery, bool, Task<TResult>> executor)
        {
            this._queryContext = queryContext;
            this._dbContextProviderPool = dbContextProviderPool;
            this._queryModel = queryModel;
            this._executor = executor;
        }

        public override IFeatureEnumerator<TResult> GetFeatureEnumerator(CancellationToken cancellationToken = default)
        {
            return new Enumerator(this, cancellationToken);
        }

        class Enumerator : TableQueryEnumerator<TResult>
        {
            ShardTableAggregateQuery<TResult> _enumerable;

            public Enumerator(ShardTableAggregateQuery<TResult> enumerable, CancellationToken cancellationToken = default) : base(enumerable._queryContext, enumerable._dbContextProviderPool, enumerable._queryModel, cancellationToken)
            {
                this._enumerable = enumerable;
            }

            protected override async Task<(IFeatureEnumerable<TResult> Query, bool IsLazyQuery)> CreateQuery(IQuery query, bool async)
            {
                var queryContext = this._enumerable._queryContext;
                bool canceled = queryContext.Canceled;
                ParallelQueryContext.LogQueryCanceled(canceled);
                if (canceled)
                {
                    return (NullFeatureEnumerable<TResult>.Instance, false);
                }

                var result = await this._enumerable._executor(query, @async);
                queryContext.AfterExecuteCommand(result);

                var featureEnumerable = new ScalarFeatureEnumerable<TResult>(result);

                return (featureEnumerable, false);
            }
        }
    }
}
