using System.Threading;
using System.Threading.Tasks;

namespace Chloe.Sharding.Queries
{
    /// <summary>
    /// 有序的表数据查询
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class OrderedMultTableDataQuery<T> : FeatureEnumerable<T>
    {
        ShardingQueryPlan _queryPlan;
        List<MultTableCountQueryResult> _countQueryResults;

        public OrderedMultTableDataQuery(ShardingQueryPlan queryPlan, List<MultTableCountQueryResult> countQueryResults)
        {
            this._queryPlan = queryPlan;
            this._countQueryResults = countQueryResults;
        }

        public override IFeatureEnumerator<T> GetFeatureEnumerator(CancellationToken cancellationToken = default)
        {
            return new Enumerator(this, cancellationToken);
        }

        class Enumerator : FeatureEnumerator<T>, IFeatureEnumerator<T>
        {
            OrderedMultTableDataQuery<T> _enumerable;
            CancellationToken _cancellationToken;

            public Enumerator(OrderedMultTableDataQuery<T> enumerable, CancellationToken cancellationToken = default)
            {
                this._enumerable = enumerable;
                this._cancellationToken = cancellationToken;
            }

            ShardingQueryPlan QueryPlan { get { return this._enumerable._queryPlan; } }
            IShardingContext ShardingContext { get { return this._enumerable._queryPlan.ShardingContext; } }

            protected override async Task<IFeatureEnumerator<T>> CreateEnumerator(bool @async)
            {
                ParallelQueryContext queryContext = new ParallelQueryContext();

                try
                {
                    List<TableDataQueryPlan<T>> dataQueryPlans = this.MakeQueryPlans(queryContext);

                    ParallelConcatEnumerable<T> concatEnumerable = new ParallelConcatEnumerable<T>(queryContext, dataQueryPlans.Select(a => a.Query));
                    var enumerator = concatEnumerable.GetFeatureEnumerator(this._cancellationToken);
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
                List<MultTableCountQueryResult> countQueryResults = this._enumerable._countQueryResults;

                List<TableDataQueryPlan<T>> dataQueryPlans = new List<TableDataQueryPlan<T>>();

                long nextTableSkip = this.QueryPlan.QueryModel.Skip ?? 0;
                long nextTableTake = this.QueryPlan.QueryModel.Take ?? int.MaxValue;
                for (int i = 0; i < countQueryResults.Count; i++)
                {
                    var countQueryResult = countQueryResults[i];
                    long canTake = countQueryResult.Count - nextTableSkip;
                    if (canTake > 0)
                    {
                        int skipCount = (int)nextTableSkip;
                        int takeCount = (int)(canTake >= nextTableTake ? nextTableTake : canTake);
                        DataQueryModel dataQueryModel = ShardingHelpers.MakeDataQueryModel(countQueryResult.Table, this.QueryPlan.QueryModel, skipCount, takeCount);

                        TableDataQueryPlan<T> queryPlan = new TableDataQueryPlan<T>();
                        queryPlan.QueryModel = dataQueryModel;

                        nextTableSkip = 0;
                        nextTableTake = nextTableTake - dataQueryModel.Take.Value;

                        dataQueryPlans.Add(queryPlan);
                    }
                    else
                    {
                        nextTableSkip = nextTableSkip - countQueryResult.Count;
                    }

                    if (nextTableTake <= 0)
                    {
                        break;
                    }
                }

                foreach (var group in dataQueryPlans.GroupBy(a => a.QueryModel.Table.DataSource.Name))
                {
                    int count = group.Count();

                    ShareDbContextPool dbContextPool = ShardingHelpers.CreateDbContextPool(group.First().QueryModel.Table.DataSource.DbContextFactory, count, this.ShardingContext.MaxConnectionsPerDatabase);
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
