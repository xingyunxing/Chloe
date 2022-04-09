using System.Threading;

namespace Chloe.Sharding.Queries
{
    /// <summary>
    /// 单表数据查询
    /// </summary>
    /// <typeparam name="T"></typeparam>
    class SingleTableEntityQuery<T> : FeatureEnumerable<T>
    {
        IParallelQueryContext QueryContext;
        IShareDbContextPool DbContextPool;
        DataQueryModel QueryModel;
        bool LazyQuery;

        public SingleTableEntityQuery(IParallelQueryContext queryContext, IShareDbContextPool dbContextPool, DataQueryModel queryModel, bool lazyQuery)
        {
            this.QueryContext = queryContext;
            this.DbContextPool = dbContextPool;
            this.QueryModel = queryModel;
            this.LazyQuery = lazyQuery;
        }

        public override IFeatureEnumerator<T> GetFeatureEnumerator(CancellationToken cancellationToken = default)
        {
            return new Enumerator(this, cancellationToken);
        }

        class Enumerator : TableQueryEnumerator<T, T>
        {
            SingleTableEntityQuery<T> _enumerable;

            public Enumerator(SingleTableEntityQuery<T> enumerable, CancellationToken cancellationToken) : base(enumerable.DbContextPool, enumerable.QueryModel, cancellationToken)
            {
                this._enumerable = enumerable;
            }

            protected override async Task<(IFeatureEnumerable<T> Query, bool IsLazyQuery)> CreateQuery(IQuery<T> query, bool @async)
            {
                var queryContext = this._enumerable.QueryContext;

                bool canceled = queryContext.BeforeExecuteCommand();
                if (canceled)
                {
                    return (NullFeatureEnumerable<T>.Instance, false);
                }

                if (!this._enumerable.LazyQuery)
                {
                    var dataList = @async ? await query.ToListAsync() : query.ToList();

                    queryContext.AfterExecuteCommand(dataList);

                    return (new FeatureEnumerableAdapter<T>(dataList), false);
                }

                var lazyEnumerable = query.AsEnumerable();
                queryContext.AfterExecuteCommand(lazyEnumerable);

                return (new FeatureEnumerableAdapter<T>(lazyEnumerable), true);
            }
        }
    }
}
