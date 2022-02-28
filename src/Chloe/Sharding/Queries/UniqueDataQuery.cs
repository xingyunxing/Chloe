using System.Threading;
using System.Threading.Tasks;

namespace Chloe.Sharding.Queries
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class UniqueDataQuery<T> : FeatureEnumerable<T>
    {
        ShardingQueryPlan _queryPlan;

        public UniqueDataQuery(ShardingQueryPlan queryPlan)
        {
            this._queryPlan = queryPlan;
        }

        public override IFeatureEnumerator<T> GetFeatureEnumerator(CancellationToken cancellationToken = default)
        {
            return new Enumerator(this, cancellationToken);
        }

        class Enumerator : FeatureEnumerator<T>
        {
            UniqueDataQuery<T> _enumerable;
            CancellationToken _cancellationToken;

            public Enumerator(UniqueDataQuery<T> enumerable, CancellationToken cancellationToken = default)
            {
                this._enumerable = enumerable;
                this._cancellationToken = cancellationToken;
            }

            ShardingQueryPlan QueryPlan { get { return this._enumerable._queryPlan; } }
            IShardingContext ShardingContext { get { return this._enumerable._queryPlan.ShardingContext; } }
            ShardingQueryModel QueryModel { get { return this._enumerable._queryPlan.QueryModel; } }

            protected override async Task<IFeatureEnumerator<T>> CreateEnumerator(bool @async)
            {
                UniqueDataParallelQueryContext queryContext = new UniqueDataParallelQueryContext();

                try
                {
                    List<TableDataQueryPlan<T>> dataQueryPlans = this.MakeQueryPlans(queryContext);
                    List<OrderProperty> orders = this.QueryModel.MakeOrderProperties();
                    ParallelMergeEnumerable<T> mergeResult = new ParallelMergeEnumerable<T>(queryContext, dataQueryPlans.Select(a => new OrderedFeatureEnumerable<T>(a.Query, orders)));

                    var enumerator = mergeResult.GetFeatureEnumerator(this._cancellationToken);

                    return enumerator;
                }
                catch
                {
                    queryContext.Dispose();
                    throw;
                }
            }

            List<TableDataQueryPlan<T>> MakeQueryPlans(ParallelQueryContext queryContext)
            {
                List<TableDataQueryPlan<T>> dataQueryPlans = new List<TableDataQueryPlan<T>>();

                foreach (RouteTable table in this.QueryPlan.RouteTables)
                {
                    DataQueryModel dataQueryModel = ShardingHelpers.MakeDataQueryModel(table, this.QueryModel);

                    TableDataQueryPlan<T> queryPlan = new TableDataQueryPlan<T>();
                    queryPlan.QueryModel = dataQueryModel;

                    dataQueryPlans.Add(queryPlan);
                }

                foreach (var group in dataQueryPlans.GroupBy(a => a.QueryModel.Table.DataSource.Name))
                {
                    int count = group.Count();

                    ShareDbContextPool dbContextPool = ShardingHelpers.CreateDbContextPool(group.First().QueryModel.Table.DataSource.DbContextFactory, count, this.ShardingContext.MaxConnectionsPerDatabase);
                    queryContext.AddManagedResource(dbContextPool);

                    //因为是根据主键查询了，所以返回的数据肯定是一条，因此直接把数据加载进内存即可
                    bool lazyQuery = false;

                    foreach (var dataQuery in group)
                    {
                        SingleTableDataQuery<T> query = new SingleTableDataQuery<T>(queryContext, dbContextPool, dataQuery.QueryModel, lazyQuery);
                        dataQuery.Query = query;
                    }
                }

                return dataQueryPlans;
            }
        }
    }
}
