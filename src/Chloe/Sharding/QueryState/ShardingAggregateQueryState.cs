using Chloe.Query.QueryExpressions;
using Chloe.Sharding.Models;
using Chloe.Sharding.Queries;
using Chloe.Reflection;
using System.Linq.Expressions;
using System.Threading;

namespace Chloe.Sharding.QueryState
{
    internal class ShardingAggregateQueryState : ShardingQueryStateBase
    {
        public ShardingAggregateQueryState(ShardingQueryStateBase prevQueryState, AggregateQueryExpression exp) : base(prevQueryState)
        {
            this.QueryExpression = exp;
        }

        AggregateQueryExpression QueryExpression { get; set; }

        public override IFeatureEnumerable<object> CreateQuery()
        {
            if (this.QueryExpression.Method.Name == "Average")
            {
                return new AverageQueryEnumerable(this);
            }

            if (this.QueryExpression.Method.Name == "Sum")
            {
                return new SumQueryEnumerable(this);
            }

            if (this.QueryExpression.Method.Name == "Count")
            {
                var longCountQueryEnumerable = new LongCountQueryEnumerable(this);
                return (IFeatureEnumerable<object>)longCountQueryEnumerable.Select(a => (int)a);
            }

            if (this.QueryExpression.Method.Name == "LongCount")
            {
                var longCountQueryEnumerable = new LongCountQueryEnumerable(this);
                return (IFeatureEnumerable<object>)longCountQueryEnumerable;
            }

            throw new NotImplementedException();
        }

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

                    Func<IQuery, bool, Task<object>> executor = async (query, @async) =>
                    {
                        var q = query.Select(aggSelector);
                        var result = @async ? await q.FirstAsync() : q.First();
                        return result;
                    };

                    AggregateQuery aggQuery = new AggregateQuery(queryState.CreateQueryPlan(), executor);

                    decimal? sum = null;
                    long count = 0;

                    await aggQuery.AsAsyncEnumerable().Select(a => (AggregateModel)a.Result).ForEach(a =>
                    {
                        if (a.Sum == null)
                            return;

                        sum = (sum ?? 0) + a.Sum.Value;
                        count = count + a.Count;
                    }, this._cancellationToken);

                    if (sum == null)
                    {
                        return (IFeatureEnumerator<object>)new ScalarFeatureEnumerator<object>(null);
                    }

                    decimal avg = sum.Value / count;


                    Type resultType = queryState.QueryExpression.Method.ReturnType.GetUnderlyingType();
                    if (resultType == typeof(float))
                    {
                        return (IFeatureEnumerator<object>)new ScalarFeatureEnumerator<float?>((float)avg);
                    }

                    if (resultType == typeof(double))
                    {
                        return (IFeatureEnumerator<object>)new ScalarFeatureEnumerator<double?>((double)avg);
                    }

                    if (resultType == typeof(decimal))
                    {
                        return (IFeatureEnumerator<object>)new ScalarFeatureEnumerator<decimal?>((decimal)avg);
                    }

                    throw new NotSupportedException();
                }
            }
        }

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

                    AggregateQuery aggQuery = new AggregateQuery(queryState.CreateQueryPlan(), executor);

                    var aggQueryEnumerable = aggQuery.AsAsyncEnumerable();

                    Type resultType = queryState.QueryExpression.Method.ReturnType.GetUnderlyingType();
                    if (resultType == typeof(int))
                    {
                        var sum = await aggQueryEnumerable.Select(a => (int?)a.Result).SumAsync();
                        return (IFeatureEnumerator<object>)new ScalarFeatureEnumerator<int?>(sum);
                    }

                    if (resultType == typeof(long))
                    {
                        var sum = await aggQueryEnumerable.Select(a => (long?)a.Result).SumAsync();
                        return (IFeatureEnumerator<object>)new ScalarFeatureEnumerator<long?>(sum);
                    }

                    if (resultType == typeof(float))
                    {
                        var sum = await aggQueryEnumerable.Select(a => (float?)a.Result).SumAsync();
                        return (IFeatureEnumerator<object>)new ScalarFeatureEnumerator<float?>(sum);
                    }

                    if (resultType == typeof(double))
                    {
                        var sum = await aggQueryEnumerable.Select(a => (double?)a.Result).SumAsync();
                        return (IFeatureEnumerator<object>)new ScalarFeatureEnumerator<double?>(sum);
                    }

                    if (resultType == typeof(decimal))
                    {
                        var sum = await aggQueryEnumerable.Select(a => (decimal?)a.Result).SumAsync();
                        return (IFeatureEnumerator<object>)new ScalarFeatureEnumerator<decimal?>(sum);
                    }

                    throw new NotSupportedException();
                }
            }
        }

        class CountQueryEnumerable : FeatureEnumerable<object>
        {
            ShardingAggregateQueryState QueryState;

            public CountQueryEnumerable(ShardingAggregateQueryState queryState)
            {
                this.QueryState = queryState;
            }

            public override IFeatureEnumerator<object> GetFeatureEnumerator(CancellationToken cancellationToken = default)
            {
                return new Enumerator(this, cancellationToken);
            }

            class Enumerator : FeatureEnumerator<object>
            {
                CountQueryEnumerable _enumerable;
                CancellationToken _cancellationToken;
                public Enumerator(CountQueryEnumerable enumerable, CancellationToken cancellationToken)
                {
                    this._enumerable = enumerable;
                    this._cancellationToken = cancellationToken;
                }

                protected override async Task<IFeatureEnumerator<object>> CreateEnumerator(bool @async)
                {
                    var queryState = this._enumerable.QueryState;

                    Func<IQuery, bool, Task<object>> executor = async (query, @async) =>
                    {
                        object result = @async ? await query.CountAsync() : query.Count();
                        return result;
                    };

                    AggregateQuery aggQuery = new AggregateQuery(queryState.CreateQueryPlan(), executor);

                    var aggQueryEnumerable = aggQuery.AsAsyncEnumerable();

                    var totals = await aggQueryEnumerable.Select(a => (long)a.Result).SumAsync(this._cancellationToken);

                    return (IFeatureEnumerator<object>)new ScalarFeatureEnumerator<long?>(totals);
                }
            }
        }
        class LongCountQueryEnumerable : FeatureEnumerable<long>
        {
            ShardingAggregateQueryState QueryState;

            public LongCountQueryEnumerable(ShardingAggregateQueryState queryState)
            {
                this.QueryState = queryState;
            }

            public override IFeatureEnumerator<long> GetFeatureEnumerator(CancellationToken cancellationToken = default)
            {
                return new Enumerator(this, cancellationToken);
            }

            class Enumerator : FeatureEnumerator<long>
            {
                LongCountQueryEnumerable _enumerable;
                CancellationToken _cancellationToken;
                public Enumerator(LongCountQueryEnumerable enumerable, CancellationToken cancellationToken)
                {
                    this._enumerable = enumerable;
                    this._cancellationToken = cancellationToken;
                }

                protected override async Task<IFeatureEnumerator<long>> CreateEnumerator(bool @async)
                {
                    var queryState = this._enumerable.QueryState;

                    Func<IQuery, bool, Task<object>> executor = async (query, @async) =>
                    {
                        object result = @async ? await query.LongCountAsync() : query.LongCount();
                        return result;
                    };

                    AggregateQuery aggQuery = new AggregateQuery(queryState.CreateQueryPlan(), executor);

                    var aggQueryEnumerable = aggQuery.AsAsyncEnumerable();

                    var totals = await aggQueryEnumerable.Select(a => (long)a.Result).SumAsync(this._cancellationToken);

                    return new ScalarFeatureEnumerator<long>(totals);
                }
            }
        }
    }
}
