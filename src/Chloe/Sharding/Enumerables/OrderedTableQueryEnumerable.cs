using Chloe.Sharding.Queries;
using System.Threading;

namespace Chloe.Sharding.Enumerables
{
    /// <summary>
    /// 有序的表数据查询
    /// </summary>
    internal class OrderedTableQueryEnumerable : FeatureEnumerable<object>
    {
        ShardingQueryPlan _queryPlan;
        List<QueryResult<long>> _countQueryResults;

        public OrderedTableQueryEnumerable(ShardingQueryPlan queryPlan, List<QueryResult<long>> countQueryResults)
        {
            this._queryPlan = queryPlan;
            this._countQueryResults = countQueryResults;
        }

        public override IFeatureEnumerator<object> GetFeatureEnumerator(CancellationToken cancellationToken = default)
        {
            return new Enumerator(this, cancellationToken);
        }

        class Enumerator : TrackableFeatureEnumerator<object>
        {
            OrderedTableQueryEnumerable _enumerable;
            CancellationToken _cancellationToken;

            public Enumerator(OrderedTableQueryEnumerable enumerable, CancellationToken cancellationToken = default) : base(enumerable._queryPlan)
            {
                this._enumerable = enumerable;
                this._cancellationToken = cancellationToken;
            }

            protected override Task<IFeatureEnumerator<object>> CreateEnumerator(bool @async)
            {
                ParallelQueryContext queryContext = new ParallelQueryContext(this.QueryPlan.ShardingContext);

                try
                {
                    List<TableDataQueryPlan> dataQueryPlans = this.MakeQueryPlans(queryContext);

                    ParallelConcatEnumerable<object> concatEnumerable = new ParallelConcatEnumerable<object>(queryContext, dataQueryPlans.Select(a => a.Query));
                    var enumerator = concatEnumerable.GetFeatureEnumerator(this._cancellationToken);
                    return Task.FromResult(enumerator);
                }
                catch
                {
                    queryContext.Dispose();
                    throw;
                }
            }

            List<TableDataQueryPlan> MakeQueryPlans(ParallelQueryContext queryContext)
            {
                List<QueryResult<long>> countQueryResults = this._enumerable._countQueryResults;

                List<TableDataQueryPlan> dataQueryPlans = new List<TableDataQueryPlan>();

                long nextTableSkip = this.QueryPlan.QueryModel.Skip ?? 0;
                long nextTableTake = this.QueryPlan.QueryModel.Take ?? int.MaxValue;
                for (int i = 0; i < countQueryResults.Count; i++)
                {
                    var countQueryResult = countQueryResults[i];
                    long canTake = countQueryResult.Result - nextTableSkip;
                    if (canTake > 0)
                    {
                        int skipCount = (int)nextTableSkip;
                        int takeCount = (int)(canTake >= nextTableTake ? nextTableTake : canTake);
                        DataQueryModel dataQueryModel = ShardingHelpers.MakeDataQueryModel(countQueryResult.Table, this.QueryPlan.QueryModel, skipCount, takeCount);

                        TableDataQueryPlan queryPlan = new TableDataQueryPlan();
                        queryPlan.QueryModel = dataQueryModel;

                        nextTableSkip = 0;
                        nextTableTake = nextTableTake - dataQueryModel.Take.Value;

                        dataQueryPlans.Add(queryPlan);
                    }
                    else
                    {
                        nextTableSkip = nextTableSkip - countQueryResult.Result;
                    }

                    if (nextTableTake <= 0)
                    {
                        break;
                    }
                }

                foreach (var group in dataQueryPlans.GroupBy(a => a.QueryModel.Table.DataSource.Name))
                {
                    int count = group.Count();

                    ISharedDbContextProviderPool dbContextProviderPool = queryContext.GetDbContextProviderPool(group.First().QueryModel.Table.DataSource);
                    bool lazyQuery = dbContextProviderPool.Size >= count;

                    foreach (var dataQueryPlan in group)
                    {
                        ShardTableDataQuery shardTableQuery = new ShardTableDataQuery(queryContext, dbContextProviderPool, dataQueryPlan.QueryModel, lazyQuery);
                        dataQueryPlan.Query = shardTableQuery;
                    }
                }

                return dataQueryPlans;
            }
        }
    }
}
