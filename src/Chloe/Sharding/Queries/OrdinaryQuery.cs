using Chloe.Descriptors;
using System.Threading;

namespace Chloe.Sharding.Queries
{
    /// <summary>
    /// 普通查询
    /// </summary>
    internal class OrdinaryQuery : FeatureEnumerable<object>
    {
        ShardingQueryPlan _queryPlan;

        public OrdinaryQuery(ShardingQueryPlan queryPlan)
        {
            this._queryPlan = queryPlan;
        }

        public override IFeatureEnumerator<object> GetFeatureEnumerator(CancellationToken cancellationToken = default)
        {
            return new Enumerator(this, cancellationToken);
        }

        class Enumerator : QueryFeatureEnumerator<object>
        {
            OrdinaryQuery _enumerable;
            CancellationToken _cancellationToken;

            public Enumerator(OrdinaryQuery enumerable, CancellationToken cancellationToken = default) : base(enumerable._queryPlan)
            {
                this._enumerable = enumerable;
                this._cancellationToken = cancellationToken;
            }

            TypeDescriptor EntityTypeDescriptor { get { return this._enumerable._queryPlan.ShardingContext.TypeDescriptor; } }

            protected override async Task<IFeatureEnumerator<object>> CreateEnumerator(bool @async)
            {
                List<KeyQueryResult> keyResult = await this.GetKeys();

                ParallelQueryContext queryContext = new ParallelQueryContext();

                try
                {
                    List<TableDataQueryPlan> dataQueryPlans = this.MakeQueryPlans(queryContext, keyResult);
                    IFeatureEnumerator<object> enumerator = this.CreateQueryEntityEnumerator(queryContext, dataQueryPlans);
                    return enumerator;
                }
                catch
                {
                    queryContext.Dispose();
                    throw;
                }
            }

            async Task<List<KeyQueryResult>> GetKeys()
            {
                KeyPagingQuery keyQuery = new KeyPagingQuery(this.QueryPlan);
                List<KeyQueryResult> keyResult = await keyQuery.ToListAsync(this._cancellationToken);
                return keyResult;
            }

            IFeatureEnumerator<object> CreateQueryEntityEnumerator(ParallelQueryContext queryContext, List<TableDataQueryPlan> dataQueryPlans)
            {
                List<OrderProperty> orders = this.QueryModel.MakeOrderProperties();

                ParallelMergeEnumerable<object> mergeResult = new ParallelMergeEnumerable<object>(queryContext, dataQueryPlans.Select(a => new OrderedFeatureEnumerable<object>(a.Query, orders)));

                var enumerator = mergeResult.GetFeatureEnumerator(this._cancellationToken);

                return enumerator;
            }

            List<TableDataQueryPlan> MakeQueryPlans(ParallelQueryContext queryContext, List<KeyQueryResult> keyResult)
            {
                //构建 in (id1,id2...) 查询

                List<TableDataQueryPlan> dataQueryPlans = ShardingHelpers.MakeEntityQueryPlans(this.QueryModel, keyResult, this.EntityTypeDescriptor, this.ShardingContext.MaxInItems);

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
