using Chloe.Reflection;
using Chloe.Sharding.Models;
using Chloe.Sharding.Queries;
using Chloe.Sharding.QueryState;
using System.Linq.Expressions;
using System.Threading;

namespace Chloe.Sharding.Enumerables
{
    class AverageQueryEnumerable : FeatureEnumerable<object>
    {
        ShardingAggregateQueryState QueryState;

        public AverageQueryEnumerable(ShardingAggregateQueryState queryState)
        {
            this.QueryState = queryState;
        }

        public override IFeatureEnumerator<object> GetFeatureEnumerator(CancellationToken cancellationToken = default)
        {
            return new Enumerator(this, cancellationToken);
        }

        class Enumerator : FeatureEnumerator<object>
        {
            AverageQueryEnumerable _enumerable;
            CancellationToken _cancellationToken;
            public Enumerator(AverageQueryEnumerable enumerable, CancellationToken cancellationToken)
            {
                this._enumerable = enumerable;
                this._cancellationToken = cancellationToken;
            }

            protected override async Task<IFeatureEnumerator<object>> CreateEnumerator(bool @async)
            {
                var queryState = this._enumerable.QueryState;
                var selector = (LambdaExpression)queryState.QueryExpression.Arguments[0];
                var aggSelector = ShardingHelpers.MakeAggregateSelector(selector);

                Func<IQuery, bool, Task<AggregateModel>> executor = async (query, @async) =>
                {
                    IQuery<AggregateModel> q = (IQuery<AggregateModel>)query.Select(aggSelector);
                    var result = @async ? await q.FirstAsync() : q.First();
                    return result;
                };

                var aggQuery = new AggregateQuery<AggregateModel>(queryState.CreateQueryPlan(), executor);

                decimal? sum = null;
                long count = 0;

                await aggQuery.AsAsyncEnumerable().Select(a => a.Result).ForEach(a =>
                {
                    if (a.Sum == null)
                        return;

                    sum = (sum ?? 0) + a.Sum.Value;
                    count = count + a.Count;
                }, this._cancellationToken);

                if (sum == null)
                {
                    return new ScalarFeatureEnumerator<object>(null);
                }

                decimal avg = sum.Value / count;


                Type resultType = queryState.QueryExpression.Method.ReturnType.GetUnderlyingType();
                if (resultType == typeof(float))
                {
                    return new ScalarFeatureEnumerator<object>((float)avg);
                }

                if (resultType == typeof(double))
                {
                    return new ScalarFeatureEnumerator<object>((double)avg);
                }

                if (resultType == typeof(decimal))
                {
                    return new ScalarFeatureEnumerator<object>((decimal)avg);
                }

                throw new NotSupportedException();
            }
        }
    }
}
