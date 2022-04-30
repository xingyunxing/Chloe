using Chloe.Sharding.Queries;
using Chloe.Routing;
using System.Threading;

namespace Chloe.Sharding.Enumerables
{
    internal class UniqueDataQueryEnumerable : FeatureEnumerable<object>
    {
        ShardingQueryPlan _queryPlan;

        public UniqueDataQueryEnumerable(ShardingQueryPlan queryPlan)
        {
            this._queryPlan = queryPlan;
        }

        public override IFeatureEnumerator<object> GetFeatureEnumerator(CancellationToken cancellationToken = default)
        {
            return new Enumerator(this, cancellationToken);
        }

        class Enumerator : QueryFeatureEnumerator<object>
        {
            UniqueDataQueryEnumerable _enumerable;
            CancellationToken _cancellationToken;

            public Enumerator(UniqueDataQueryEnumerable enumerable, CancellationToken cancellationToken = default) : base(enumerable._queryPlan)
            {
                this._enumerable = enumerable;
                this._cancellationToken = cancellationToken;
            }

            protected override async Task<IFeatureEnumerator<object>> CreateEnumerator(bool @async)
            {
                UniqueDataParallelQueryContext queryContext = new UniqueDataParallelQueryContext();

                try
                {
                    List<TableDataQueryPlan> dataQueryPlans = this.MakeQueryPlans(queryContext);
                    List<OrderProperty> orders = new List<OrderProperty>();
                    ParallelMergeEnumerable<object> mergeResult = new ParallelMergeEnumerable<object>(queryContext, dataQueryPlans.Select(a => new OrderedFeatureEnumerable<object>(a.Query, orders)));

                    var enumerator = mergeResult.GetFeatureEnumerator(this._cancellationToken);

                    return enumerator;
                }
                catch
                {
                    queryContext.Dispose();
                    throw;
                }
            }

            List<TableDataQueryPlan> MakeQueryPlans(ParallelQueryContext queryContext)
            {
                List<TableDataQueryPlan> dataQueryPlans = new List<TableDataQueryPlan>();

                foreach (IPhysicTable table in this.QueryPlan.Tables)
                {
                    DataQueryModel dataQueryModel = ShardingHelpers.MakeDataQueryModel(table, this.QueryModel);

                    TableDataQueryPlan queryPlan = new TableDataQueryPlan();
                    queryPlan.QueryModel = dataQueryModel;

                    dataQueryPlans.Add(queryPlan);
                }

                foreach (var group in dataQueryPlans.GroupBy(a => a.QueryModel.Table.DataSource.Name))
                {
                    int count = group.Count();

                    SharedDbContextProviderPool dbContextProviderPool = ShardingHelpers.CreateDbContextProviderPool(this.ShardingContext, group.First().QueryModel.Table.DataSource, count);
                    queryContext.AddManagedResource(dbContextProviderPool);

                    //因为是根据主键查询了，所以返回的数据肯定是一条，因此直接把数据加载进内存即可
                    bool lazyQuery = false;

                    foreach (var dataQuery in group)
                    {
                        ShardingTableDataQuery shardingQuery = new ShardingTableDataQuery(queryContext, dbContextProviderPool, dataQuery.QueryModel, lazyQuery);
                        dataQuery.Query = shardingQuery;
                    }
                }

                return dataQueryPlans;
            }
        }
    }
}
