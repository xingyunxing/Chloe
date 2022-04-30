using Chloe.Sharding.Queries;
using Chloe.Sharding.QueryState;
using System.Threading;

namespace Chloe.Sharding.Enumerables
{
    class CountQueryEnumerable<T> : FeatureEnumerable<object>
    {
        ShardingAggregateQueryState QueryState;
        bool IsLongCount;

        public CountQueryEnumerable(ShardingAggregateQueryState queryState, bool isLongCount)
        {
            this.QueryState = queryState;
            this.IsLongCount = isLongCount;
        }

        public override IFeatureEnumerator<object> GetFeatureEnumerator(CancellationToken cancellationToken = default)
        {
            return new Enumerator(this, cancellationToken);
        }

        class Enumerator : FeatureEnumerator<object>
        {
            CountQueryEnumerable<T> _enumerable;
            CancellationToken _cancellationToken;
            public Enumerator(CountQueryEnumerable<T> enumerable, CancellationToken cancellationToken)
            {
                this._enumerable = enumerable;
                this._cancellationToken = cancellationToken;
            }

            protected override async Task<IFeatureEnumerator<object>> CreateEnumerator(bool @async)
            {
                var queryState = this._enumerable.QueryState;

                Func<IQuery, bool, Task<long>> executor = async (query, @async) =>
                {
                    IQuery<T> q = (IQuery<T>)query;
                    long result;
                    if (this._enumerable.IsLongCount)
                    {
                        result = @async ? await q.LongCountAsync() : q.LongCount();
                    }
                    else
                    {
                        result = @async ? await q.CountAsync() : q.Count();
                    }

                    return result;
                };

                AggregateQuery<long> aggQuery = new AggregateQuery<long>(queryState.CreateQueryPlan(), executor);

                var aggQueryEnumerable = aggQuery.AsAsyncEnumerable();

                var totals = await aggQueryEnumerable.Select(a => a.Result).SumAsync(this._cancellationToken);

                if (this._enumerable.IsLongCount)
                {
                    return new ScalarFeatureEnumerator<object>(totals);
                }
                else
                {
                    return new ScalarFeatureEnumerator<object>((int)totals);
                }
            }
        }
    }
}
