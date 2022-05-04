using Chloe.Sharding.Queries;
using Chloe.Sharding.QueryState;
using System.Threading;

namespace Chloe.Sharding.Enumerables
{
    class AnyQueryEnumerable<T> : FeatureEnumerable<object>
    {
        ShardingAggregateQueryState QueryState;

        public AnyQueryEnumerable(ShardingAggregateQueryState queryState)
        {
            this.QueryState = queryState;
        }

        public override IFeatureEnumerator<object> GetFeatureEnumerator(CancellationToken cancellationToken = default)
        {
            return new Enumerator(this, cancellationToken);
        }

        class Enumerator : FeatureEnumerator<object>
        {
            AnyQueryEnumerable<T> _enumerable;
            CancellationToken _cancellationToken;
            public Enumerator(AnyQueryEnumerable<T> enumerable, CancellationToken cancellationToken)
            {
                this._enumerable = enumerable;
                this._cancellationToken = cancellationToken;
            }

            protected override async Task<IFeatureEnumerator<object>> CreateEnumerator(bool @async)
            {
                var queryState = this._enumerable.QueryState;

                Func<IQuery, bool, Task<bool>> executor = async (query, @async) =>
                {
                    IQuery<T> q = (IQuery<T>)query;
                    bool result = @async ? await q.AnyAsync() : q.Any();
                    return result;
                };

                ShardingQueryPlan queryPlan = queryState.CreateQueryPlan();
                AggregateQuery<bool> aggQuery = new AggregateQuery<bool>(queryPlan, executor, () => { return new AnyQueryParallelQueryContext(queryPlan.ShardingContext); });

                var hasData = await aggQuery.AsAsyncEnumerable().Where(a => a.Result == true).AnyAsync(this._cancellationToken);

                return new ScalarFeatureEnumerator<object>(hasData);
            }
        }
    }
}
