using Chloe.Sharding.Queries;
using System.Threading;

namespace Chloe.Sharding.Enumerables
{
    internal class SingleTableQueryEnumerable : FeatureEnumerable<object>
    {
        ShardingQueryPlan _queryPlan;

        public SingleTableQueryEnumerable(ShardingQueryPlan queryPlan)
        {
            this._queryPlan = queryPlan;
        }

        public override IFeatureEnumerator<object> GetFeatureEnumerator(CancellationToken cancellationToken = default)
        {
            return new Enumerator(this, cancellationToken);
        }

        class Enumerator : QueryFeatureEnumerator<object>
        {
            SingleTableQueryEnumerable _enumerable;
            CancellationToken _cancellationToken;

            public Enumerator(SingleTableQueryEnumerable enumerable, CancellationToken cancellationToken = default) : base(enumerable._queryPlan)
            {
                this._enumerable = enumerable;
                this._cancellationToken = cancellationToken;
            }

            protected override async Task<IFeatureEnumerator<object>> CreateEnumerator(bool @async)
            {
                ParallelQueryContext queryContext = new ParallelQueryContext(this._enumerable._queryPlan.ShardingContext);

                try
                {
                    TableDataQueryPlan dataQueryPlan = this.MakeQueryPlan(queryContext);

                    var enumerator = dataQueryPlan.Query.GetFeatureEnumerator(this._cancellationToken);
                    return enumerator;
                }
                catch
                {
                    queryContext.Dispose();
                    throw;
                }
            }

            TableDataQueryPlan MakeQueryPlan(ParallelQueryContext queryContext)
            {
                IPhysicTable table = this.QueryPlan.Tables[0];
                DataQueryModel dataQueryModel = ShardingHelpers.MakeDataQueryModel(table, this.QueryModel, this.QueryModel.Skip, this.QueryModel.Take);

                TableDataQueryPlan queryPlan = new TableDataQueryPlan();
                queryPlan.QueryModel = dataQueryModel;

                ISharedDbContextProviderPool dbContextProviderPool = queryContext.GetDbContextProviderPool(table.DataSource);

                ShardingTableDataQuery shardingQuery = new ShardingTableDataQuery(queryContext, dbContextProviderPool, queryPlan.QueryModel, false);
                queryPlan.Query = shardingQuery;

                return queryPlan;
            }
        }
    }
}
