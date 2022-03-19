using Chloe.Descriptors;
using System.Threading;

namespace Chloe.Sharding.Queries
{
    /// <summary>
    /// 非分页查询
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class NonPagingQuery<T> : FeatureEnumerable<T>
    {
        ShardingQueryPlan _queryPlan;

        public NonPagingQuery(ShardingQueryPlan queryPlan)
        {
            this._queryPlan = queryPlan;
        }

        public override IFeatureEnumerator<T> GetFeatureEnumerator(CancellationToken cancellationToken = default)
        {
            return new Enumerator(this, cancellationToken);
        }

        class Enumerator : QueryFeatureEnumerator<T>
        {
            NonPagingQuery<T> _enumerable;
            CancellationToken _cancellationToken;

            public Enumerator(NonPagingQuery<T> enumerable, CancellationToken cancellationToken = default) : base(enumerable._queryPlan)
            {
                this._enumerable = enumerable;
                this._cancellationToken = cancellationToken;
            }

            protected override async Task<IFeatureEnumerator<T>> CreateEnumerator(bool @async)
            {
                ParallelQueryContext queryContext = new ParallelQueryContext();

                try
                {
                    List<TableDataQueryPlan<T>> dataQueryPlans = this.MakeQueryPlans(queryContext);
                    IFeatureEnumerator<T> enumerator = this.CreateQueryEntityEnumerator(queryContext, dataQueryPlans);
                    return enumerator;
                }
                catch
                {
                    queryContext.Dispose();
                    throw;
                }
            }

            IFeatureEnumerator<T> CreateQueryEntityEnumerator(ParallelQueryContext queryContext, List<TableDataQueryPlan<T>> dataQueryPlans)
            {
                List<OrderProperty> orders = this.QueryModel.MakeOrderProperties();

                ParallelMergeEnumerable<T> mergeResult = new ParallelMergeEnumerable<T>(queryContext, dataQueryPlans.Select(a => new OrderedFeatureEnumerable<T>(a.Query, orders)));

                var enumerator = mergeResult.GetFeatureEnumerator(this._cancellationToken);

                return enumerator;
            }

            List<TableDataQueryPlan<T>> MakeQueryPlans(ParallelQueryContext queryContext)
            {
                List<TableDataQueryPlan<T>> dataQueryPlans = new List<TableDataQueryPlan<T>>(this.QueryPlan.Tables.Count);

                foreach (IPhysicTable table in this.QueryPlan.Tables)
                {
                    DataQueryModel dataQueryModel = ShardingHelpers.MakeDataQueryModel(table, this.QueryModel);

                    TableDataQueryPlan<T> dataQueryPlan = new TableDataQueryPlan<T>();
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
                        SingleTableDataQuery<T> query = new SingleTableDataQuery<T>(queryContext, dbContextPool, dataQueryPlan.QueryModel, lazyQuery);
                        dataQueryPlan.Query = query;
                    }
                }

                return dataQueryPlans;
            }
        }
    }
}
