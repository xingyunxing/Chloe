using System.Threading;

namespace Chloe.Sharding.Queries
{
    /// <summary>
    /// 非分页查询
    /// </summary>
    internal class NonPagingQuery : FeatureEnumerable<object>
    {
        ShardingQueryPlan _queryPlan;

        public NonPagingQuery(ShardingQueryPlan queryPlan)
        {
            this._queryPlan = queryPlan;
        }

        public override IFeatureEnumerator<object> GetFeatureEnumerator(CancellationToken cancellationToken = default)
        {
            return new Enumerator(this, cancellationToken);
        }

        class Enumerator : QueryFeatureEnumerator<object>
        {
            NonPagingQuery _enumerable;
            CancellationToken _cancellationToken;

            public Enumerator(NonPagingQuery enumerable, CancellationToken cancellationToken = default) : base(enumerable._queryPlan)
            {
                this._enumerable = enumerable;
                this._cancellationToken = cancellationToken;
            }

            protected override async Task<IFeatureEnumerator<object>> CreateEnumerator(bool @async)
            {
                ParallelQueryContext queryContext = new ParallelQueryContext();

                try
                {
                    List<TableDataQueryPlan> dataQueryPlans = this.MakeQueryPlans(queryContext);
                    IFeatureEnumerator<object> enumerator = this.CreateQueryEntityEnumerator(queryContext, dataQueryPlans);
                    return enumerator;
                }
                catch
                {
                    queryContext.Dispose();
                    throw;
                }
            }

            IFeatureEnumerator<object> CreateQueryEntityEnumerator(ParallelQueryContext queryContext, List<TableDataQueryPlan> dataQueryPlans)
            {
                List<OrderProperty> orders = this.QueryModel.MakeOrderProperties();

                ParallelMergeEnumerable<object> mergeResult = new ParallelMergeEnumerable<object>(queryContext, dataQueryPlans.Select(a => new OrderedFeatureEnumerable<object>(a.Query, orders)));

                var enumerator = mergeResult.GetFeatureEnumerator(this._cancellationToken);

                return enumerator;
            }

            List<TableDataQueryPlan> MakeQueryPlans(ParallelQueryContext queryContext)
            {
                List<TableDataQueryPlan> dataQueryPlans = new List<TableDataQueryPlan>(this.QueryPlan.Tables.Count);

                foreach (IPhysicTable table in this.QueryPlan.Tables)
                {
                    DataQueryModel dataQueryModel = ShardingHelpers.MakeDataQueryModel(table, this.QueryModel);

                    TableDataQueryPlan dataQueryPlan = new TableDataQueryPlan();
                    dataQueryPlan.QueryModel = dataQueryModel;

                    dataQueryPlans.Add(dataQueryPlan);
                }

                foreach (var group in dataQueryPlans.GroupBy(a => a.QueryModel.Table.DataSource.Name))
                {
                    int count = group.Count();

                    ShareDbContextPool dbContextPool = ShardingHelpers.CreateDbContextPool(this.ShardingContext, group.First().QueryModel.Table.DataSource, count);
                    queryContext.AddManagedResource(dbContextPool);

                    bool lazyQuery = dbContextPool.Size >= count;

                    foreach (var dataQueryPlan in group)
                    {
                        SingleTableEntityQuery query = new SingleTableEntityQuery(queryContext, dbContextPool, dataQueryPlan.QueryModel, lazyQuery);
                        dataQueryPlan.Query = query;
                    }
                }

                return dataQueryPlans;
            }
        }
    }
}
