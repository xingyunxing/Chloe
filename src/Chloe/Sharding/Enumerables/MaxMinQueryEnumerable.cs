using Chloe.Sharding.Queries;
using Chloe.Sharding.QueryState;
using System.Linq.Expressions;
using System.Threading;

namespace Chloe.Sharding.Enumerables
{
    class MaxMinQueryEnumerable<T, TResult> : FeatureEnumerable<object>
    {
        ShardingAggregateQueryState QueryState;
        bool IsMaxQuery;

        public MaxMinQueryEnumerable(ShardingAggregateQueryState queryState, bool isMaxQuery)
        {
            this.QueryState = queryState;
            this.IsMaxQuery = isMaxQuery;
        }

        public override IFeatureEnumerator<object> GetFeatureEnumerator(CancellationToken cancellationToken = default)
        {
            return new Enumerator(this, cancellationToken);
        }

        class Enumerator : FeatureEnumerator<object>
        {
            MaxMinQueryEnumerable<T, TResult> _enumerable;
            CancellationToken _cancellationToken;
            public Enumerator(MaxMinQueryEnumerable<T, TResult> enumerable, CancellationToken cancellationToken)
            {
                this._enumerable = enumerable;
                this._cancellationToken = cancellationToken;
            }

            protected override async Task<IFeatureEnumerator<object>> CreateEnumerator(bool @async)
            {
                var queryState = this._enumerable.QueryState;
                var selector = (Expression<Func<T, TResult>>)queryState.QueryExpression.Arguments[0];

                Func<IQuery, bool, Task<TResult>> executor = async (query, @async) =>
                {
                    IQuery<T> q = (IQuery<T>)query;
                    TResult result;
                    if (this._enumerable.IsMaxQuery)
                    {
                        result = @async ? await q.MaxAsync(selector) : q.Max(selector);
                    }
                    else
                    {
                        result = @async ? await q.MinAsync(selector) : q.Min(selector);
                    }

                    return result;
                };

                AggregateQuery<TResult> aggQuery = new AggregateQuery<TResult>(queryState.CreateQueryPlan(), executor);

                var results = await aggQuery.Select(a => a.Result).AsAsyncEnumerable().ToListAsync(this._cancellationToken);

                TResult result = this._enumerable.IsMaxQuery ? results.Max() : results.Min();
                return new ScalarFeatureEnumerator<object>(result);
            }
        }
    }
}
