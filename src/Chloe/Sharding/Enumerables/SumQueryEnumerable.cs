using Chloe.Reflection;
using Chloe.Sharding.Queries;
using Chloe.Sharding.QueryState;
using System.Linq.Expressions;
using System.Threading;

namespace Chloe.Sharding.Enumerables
{
    class SumQueryEnumerable : FeatureEnumerable<object>
    {
        ShardingAggregateQueryState QueryState;

        public SumQueryEnumerable(ShardingAggregateQueryState queryState)
        {
            this.QueryState = queryState;
        }

        public override IFeatureEnumerator<object> GetFeatureEnumerator(CancellationToken cancellationToken = default)
        {
            return new Enumerator(this, cancellationToken);
        }

        class Enumerator : FeatureEnumerator<object>
        {
            SumQueryEnumerable _enumerable;
            CancellationToken _cancellationToken;
            public Enumerator(SumQueryEnumerable enumerable, CancellationToken cancellationToken)
            {
                this._enumerable = enumerable;
                this._cancellationToken = cancellationToken;
            }

            protected override async Task<IFeatureEnumerator<object>> CreateEnumerator(bool @async)
            {
                var queryState = this._enumerable.QueryState;
                var selector = (LambdaExpression)queryState.QueryExpression.Arguments[0];

                Func<IQuery, bool, Task<object>> executor = async (query, @async) =>
                {
                    object result = @async ? await query.SumAsync(selector) : query.Sum(selector);
                    return result;
                };

                var aggQuery = new AggregateQuery<object>(queryState.CreateQueryPlan(), executor);

                var aggQueryEnumerable = aggQuery.AsAsyncEnumerable();

                Type resultType = queryState.QueryExpression.Method.ReturnType.GetUnderlyingType();

                object sum;

                if (resultType == typeof(int))
                {
                    sum = await aggQueryEnumerable.Select(a => (int?)a.Result).SumAsync();

                }
                else if (resultType == typeof(long))
                {
                    sum = await aggQueryEnumerable.Select(a => (long?)a.Result).SumAsync();
                }
                else if (resultType == typeof(float))
                {
                    sum = await aggQueryEnumerable.Select(a => (float?)a.Result).SumAsync();
                }
                else if (resultType == typeof(double))
                {
                    sum = await aggQueryEnumerable.Select(a => (double?)a.Result).SumAsync();
                }
                else if (resultType == typeof(decimal))
                {
                    sum = await aggQueryEnumerable.Select(a => (decimal?)a.Result).SumAsync();
                }
                else
                {
                    throw new NotSupportedException();
                }

                return new ScalarFeatureEnumerator<object>(sum);
            }
        }
    }
}
