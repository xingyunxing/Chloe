using System.Threading;

namespace Chloe.Sharding.Queries
{
    class SingleTableAggregateQuery<TResult> : FeatureEnumerable<TResult>
    {
        IShareDbContextPool _dbContextPool;
        DataQueryModel _queryModel;
        Func<IQuery, bool, Task<TResult>> _executor;

        public SingleTableAggregateQuery(IShareDbContextPool dbContextPool, DataQueryModel queryModel, Func<IQuery, bool, Task<TResult>> executor)
        {
            this._dbContextPool = dbContextPool;
            this._queryModel = queryModel;
            this._executor = executor;
        }

        public override IFeatureEnumerator<TResult> GetFeatureEnumerator(CancellationToken cancellationToken = default)
        {
            return new Enumerator(this, cancellationToken);
        }

        class Enumerator : TableQueryEnumerator<TResult>
        {
            SingleTableAggregateQuery<TResult> _enumerable;

            public Enumerator(SingleTableAggregateQuery<TResult> enumerable, CancellationToken cancellationToken = default) : base(enumerable._dbContextPool, enumerable._queryModel, cancellationToken)
            {
                this._enumerable = enumerable;
            }

            protected override async Task<(IFeatureEnumerable<TResult> Query, bool IsLazyQuery)> CreateQuery(IQuery query, bool async)
            {
                var result = await this._enumerable._executor(query, @async);

                var featureEnumerable = new ScalarFeatureEnumerable<TResult>(result);

                return (featureEnumerable, false);
            }
        }
    }
}
