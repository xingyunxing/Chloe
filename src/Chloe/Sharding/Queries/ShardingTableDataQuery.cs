using System.Threading;

namespace Chloe.Sharding.Queries
{
    class TableDataQueryPlan
    {
        public DataQueryModel QueryModel { get; set; }

        public ShardingTableDataQuery Query { get; set; }
    }

    /// <summary>
    /// 单表数据查询
    /// </summary>
    class ShardingTableDataQuery : FeatureEnumerable<object>
    {
        IParallelQueryContext QueryContext;
        IShareDbContextPool DbContextPool;
        DataQueryModel QueryModel;
        bool LazyQuery;

        public ShardingTableDataQuery(IParallelQueryContext queryContext, IShareDbContextPool dbContextPool, DataQueryModel queryModel, bool lazyQuery)
        {
            this.QueryContext = queryContext;
            this.DbContextPool = dbContextPool;
            this.QueryModel = queryModel;
            this.LazyQuery = lazyQuery;
        }

        public override IFeatureEnumerator<object> GetFeatureEnumerator(CancellationToken cancellationToken = default)
        {
            return new Enumerator(this, cancellationToken);
        }

        class Enumerator : TableQueryEnumerator<object>
        {
            ShardingTableDataQuery _enumerable;

            public Enumerator(ShardingTableDataQuery enumerable, CancellationToken cancellationToken) : base(enumerable.DbContextPool, enumerable.QueryModel, cancellationToken)
            {
                this._enumerable = enumerable;
            }

            protected override async Task<(IFeatureEnumerable<object> Query, bool IsLazyQuery)> CreateQuery(IQuery query, bool @async)
            {
                var queryContext = this._enumerable.QueryContext;

                bool canceled = queryContext.BeforeExecuteCommand();
                if (canceled)
                {
                    return (NullFeatureEnumerable<object>.Instance, false);
                }

                if (!this._enumerable.LazyQuery)
                {
                    var dataList = @async ? await query.ToListAsync() : query.ToList();

                    queryContext.AfterExecuteCommand(dataList);

                    return (new FeatureEnumerableAdapter<object>(dataList), false);
                }

                var lazyEnumerable = query.AsEnumerable();
                queryContext.AfterExecuteCommand(lazyEnumerable);

                return (new FeatureEnumerableAdapter<object>(lazyEnumerable), true);
            }
        }
    }
}
