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

        class Enumerator : TableQueryEnumerator<T, bool>
        {
            SingleTableAnyQuery<T> _enumerable;

            public Enumerator(SingleTableAnyQuery<T> enumerable, CancellationToken cancellationToken = default) : base(enumerable._dbContextPool, enumerable._queryModel, cancellationToken)
            {
                this._enumerable = enumerable;
            }

            protected override async Task<(IFeatureEnumerable<bool> Query, bool IsLazyQuery)> CreateQuery(IQuery<T> query, bool async)
            {
                var queryContext = this._enumerable._queryContext;

                bool canceled = queryContext.BeforeExecuteCommand();
                if (canceled)
                {
                    return (NullFeatureEnumerable<bool>.Instance, false);
                }

                bool hasData = @async ? await query.AnyAsync() : query.Any();

                queryContext.AfterExecuteCommand(hasData);

                var featureEnumerable = new ScalarFeatureEnumerable<bool>(hasData);
                return (featureEnumerable, false);
            }
        }
    }
}
