using Chloe.Sharding.Queries;
using System.Threading;

namespace Chloe.Sharding.Enumerables
{
    class SingleTablePagingQueryEnumerable : FeatureEnumerable<PagingResult>
    {
        ShardingQueryPlan _queryPlan;

        public SingleTablePagingQueryEnumerable(ShardingQueryPlan queryPlan)
        {
            this._queryPlan = queryPlan;
        }

        public override IFeatureEnumerator<PagingResult> GetFeatureEnumerator(CancellationToken cancellationToken = default)
        {
            return new Enumerator(this, cancellationToken);
        }

        class Enumerator : FeatureEnumerator<PagingResult>
        {
            SingleTablePagingQueryEnumerable _enumerable;
            CancellationToken _cancellationToken;
            public Enumerator(SingleTablePagingQueryEnumerable enumerable, CancellationToken cancellationToken)
            {
                this._enumerable = enumerable;
                this._cancellationToken = cancellationToken;
            }

            protected override async Task<IFeatureEnumerator<PagingResult>> CreateEnumerator(bool @async)
            {
                ShardingQueryPlan queryPlan = this._enumerable._queryPlan;
                var countQuery = ShardingHelpers.GetCountQuery(queryPlan);

                List<QueryResult<long>> routeTableCounts = @async ? await countQuery.ToListAsync() : countQuery.ToList();
                long totals = routeTableCounts.Select(a => a.Result).Sum();

                SingleTableQueryEnumerable queryEnumerable = new SingleTableQueryEnumerable(queryPlan);
                List<object> dataList = @async ? await queryEnumerable.ToListAsync() : queryEnumerable.ToList();

                return new ScalarFeatureEnumerator<PagingResult>(new PagingResult() { Totals = totals, DataList = dataList });
            }
        }
    }

}
