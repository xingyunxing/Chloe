using Chloe.Descriptors;
using System.Threading;
using System.Threading.Tasks;

namespace Chloe.Sharding.Queries
{
    /// <summary>
    /// 无序的表数据查询
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class DisorderedMultTableDataQuery<T> : FeatureEnumerable<T>
    {
        ShardingQueryPlan _queryPlan;

        public DisorderedMultTableDataQuery(ShardingQueryPlan queryPlan)
        {
            this._queryPlan = queryPlan;
        }

        public override IFeatureEnumerator<T> GetFeatureEnumerator(CancellationToken cancellationToken = default)
        {
            return new Enumerator(this, cancellationToken);
        }

        class Enumerator : QueryFeatureEnumerator<T>
        {
            DisorderedMultTableDataQuery<T> _enumerable;
            CancellationToken _cancellationToken;

            public Enumerator(DisorderedMultTableDataQuery<T> enumerable, CancellationToken cancellationToken = default) : base(enumerable._queryPlan)
            {
                this._enumerable = enumerable;
                this._cancellationToken = cancellationToken;
            }

            TypeDescriptor EntityTypeDescriptor { get { return this._enumerable._queryPlan.ShardingContext.TypeDescriptor; } }

            protected override async Task<IFeatureEnumerator<T>> CreateEnumerator(bool @async)
            {
                List<MultTableKeyQueryResult> keyResult = await this.GetKeys();

                ParallelQueryContext queryContext = new ParallelQueryContext();

                try
                {
                    IFeatureEnumerator<T> enumerator = this.CreateQueryEntityEnumerator(queryContext, keyResult);
                    return enumerator;
                }
                catch
                {
                    queryContext.Dispose();
                    throw;
                }
            }

            async Task<List<MultTableKeyQueryResult>> GetKeys()
            {
                MultTableKeyQuery<T> keyQuery = new MultTableKeyQuery<T>(this.QueryPlan);
                List<MultTableKeyQueryResult> keyResult = await keyQuery.ToListAsync(this._cancellationToken);
                return keyResult;
            }

            IFeatureEnumerator<T> CreateQueryEntityEnumerator(ParallelQueryContext queryContext, List<MultTableKeyQueryResult> keyResult)
            {
                List<TableDataQueryPlan<T>> dataQueryPlans = this.MakeQueryPlans(queryContext, keyResult);

                List<OrderProperty> orders = this.QueryModel.MakeOrderProperties();

                ParallelMergeEnumerable<T> mergeResult = new ParallelMergeEnumerable<T>(queryContext, dataQueryPlans.Select(a => new OrderedFeatureEnumerable<T>(a.Query, orders)));

                var enumerator = mergeResult.GetFeatureEnumerator(this._cancellationToken);

                return enumerator;
            }

            List<TableDataQueryPlan<T>> MakeQueryPlans(ParallelQueryContext queryContext, List<MultTableKeyQueryResult> keyResult)
            {
                //构建 in (id1,id2...) 查询

                List<TableDataQueryPlan<T>> dataQueryPlans = ShardingHelpers.MakeEntityQueryPlans<T>(this.QueryModel, keyResult, this.EntityTypeDescriptor, this.ShardingContext.MaxInItems);

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
