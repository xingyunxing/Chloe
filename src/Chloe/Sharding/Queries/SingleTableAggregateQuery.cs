using System.Threading;

namespace Chloe.Sharding.Queries
{
    class SingleTableAggregateQuery : FeatureEnumerable<object>
    {
        IShareDbContextPool _dbContextPool;
        DataQueryModel _queryModel;
        Func<IQuery, bool, Task<object>> _executor;

        public SingleTableAggregateQuery(IShareDbContextPool dbContextPool, DataQueryModel queryModel, Func<IQuery, bool, Task<object>> executor)
        {
            this._dbContextPool = dbContextPool;
            this._queryModel = queryModel;
            this._executor = executor;
        }

        public override IFeatureEnumerator<object> GetFeatureEnumerator(CancellationToken cancellationToken = default)
        {
            return new Enumerator(this, cancellationToken);
        }

        class Enumerator : TableQueryEnumerator
        {
            SingleTableAggregateQuery _enumerable;

            public Enumerator(SingleTableAggregateQuery enumerable, CancellationToken cancellationToken = default) : base(enumerable._dbContextPool, enumerable._queryModel, cancellationToken)
            {
                this._enumerable = enumerable;
            }

            protected override async Task<(IFeatureEnumerable<object> Query, bool IsLazyQuery)> CreateQuery(IQuery query, bool async)
            {
                var result = await this._enumerable._executor(query, @async);

                var featureEnumerable = new ScalarFeatureEnumerable<object>(result);

                return (featureEnumerable, false);
            }
        }
    }
}
