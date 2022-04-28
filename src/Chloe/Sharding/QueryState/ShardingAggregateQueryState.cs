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
            if (this.QueryExpression.Method.Name == "Count")
            {
                var queryEnumerableType = typeof(CountQueryEnumerable<>).MakeGenericType(this.QueryModel.RootEntityType);
                var queryEnumerable = queryEnumerableType.GetConstructor(new Type[1] { typeof(ShardingAggregateQueryState) }).FastCreateInstance(this);
                return (IFeatureEnumerable<object>)queryEnumerable;
            }

            if (this.QueryExpression.Method.Name == "LongCount")
            {
                var queryEnumerableType = typeof(LongCountQueryEnumerable<>).MakeGenericType(this.QueryModel.RootEntityType);
                var queryEnumerable = queryEnumerableType.GetConstructor(new Type[1] { typeof(ShardingAggregateQueryState) }).FastCreateInstance(this);
                return (IFeatureEnumerable<object>)queryEnumerable;
            }

            if (this.QueryExpression.Method.Name == "Average")
            {
                return new AverageQueryEnumerable(this);
            }

            if (this.QueryExpression.Method.Name == "Sum")
            {
                return new SumQueryEnumerable(this);
            }

            if (this.QueryExpression.Method.Name == "Max")
            {
                var queryEnumerableType = typeof(ExtremeQueryEnumerable<,>).MakeGenericType(this.QueryModel.RootEntityType, this.QueryModel.Selector.Body.Type);
                var queryEnumerable = queryEnumerableType.GetConstructor(new Type[2] { typeof(ShardingAggregateQueryState), typeof(bool) }).FastCreateInstance(this, true);
                return (IFeatureEnumerable<object>)queryEnumerable;
            }

            if (this.QueryExpression.Method.Name == "Min")
            {
                var queryEnumerableType = typeof(ExtremeQueryEnumerable<,>).MakeGenericType(this.QueryModel.RootEntityType, this.QueryModel.Selector.Body.Type);
                var queryEnumerable = queryEnumerableType.GetConstructor(new Type[2] { typeof(ShardingAggregateQueryState), typeof(bool) }).FastCreateInstance(this, false);
                return (IFeatureEnumerable<object>)queryEnumerable;
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

                    var aggQuery = new AggregateQuery<object>(queryState.CreateQueryPlan(), executor);

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
        class CountQueryEnumerable<T> : FeatureEnumerable<int>
        {
            ShardingAggregateQueryState QueryState;

            public CountQueryEnumerable(ShardingAggregateQueryState queryState)
            {
                this.QueryState = queryState;
            }

            public override IFeatureEnumerator<int> GetFeatureEnumerator(CancellationToken cancellationToken = default)
            {
                return new Enumerator(this, cancellationToken);
            }

            class Enumerator : FeatureEnumerator<int>
            {
                CountQueryEnumerable<T> _enumerable;
                CancellationToken _cancellationToken;
                public Enumerator(CountQueryEnumerable<T> enumerable, CancellationToken cancellationToken)
                {
                    this._enumerable = enumerable;
                    this._cancellationToken = cancellationToken;
                }

                protected override async Task<IFeatureEnumerator<int>> CreateEnumerator(bool @async)
                {
                    var queryState = this._enumerable.QueryState;

                    Func<IQuery, bool, Task<int>> executor = async (query, @async) =>
                    {
                        IQuery<T> q = (IQuery<T>)query;
                        int result = @async ? await q.CountAsync() : q.Count();
                        return result;
                    };

                    AggregateQuery<int> aggQuery = new AggregateQuery<int>(queryState.CreateQueryPlan(), executor);

                    var aggQueryEnumerable = aggQuery.AsAsyncEnumerable();

                    var totals = await aggQueryEnumerable.Select(a => a.Result).SumAsync(this._cancellationToken);
                    return new ScalarFeatureEnumerator<int>(totals);
                }
            }
        }
        class LongCountQueryEnumerable<T> : FeatureEnumerable<long>
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
                LongCountQueryEnumerable<T> _enumerable;
                CancellationToken _cancellationToken;
                public Enumerator(LongCountQueryEnumerable<T> enumerable, CancellationToken cancellationToken)
                {
                    this._enumerable = enumerable;
                    this._cancellationToken = cancellationToken;
                }

                protected override async Task<IFeatureEnumerator<long>> CreateEnumerator(bool @async)
                {
                    var queryState = this._enumerable.QueryState;

                    Func<IQuery, bool, Task<long>> executor = async (query, @async) =>
                    {
                        IQuery<T> q = (IQuery<T>)query;
                        long result = @async ? await q.LongCountAsync() : q.LongCount();
                        return result;
                    };

                    AggregateQuery<long> aggQuery = new AggregateQuery<long>(queryState.CreateQueryPlan(), executor);

                    var aggQueryEnumerable = aggQuery.AsAsyncEnumerable();

                    var totals = await aggQueryEnumerable.Select(a => a.Result).SumAsync(this._cancellationToken);
                    return new ScalarFeatureEnumerator<long>(totals);
                }
            }
        }

        class ExtremeQueryEnumerable<T, TResult> : FeatureEnumerable<TResult>
        {
            ShardingAggregateQueryState QueryState;
            bool IsMaxQuery;

            public ExtremeQueryEnumerable(ShardingAggregateQueryState queryState, bool isMaxQuery)
            {
                this.QueryState = queryState;
                this.IsMaxQuery = isMaxQuery;
            }

            public override IFeatureEnumerator<TResult> GetFeatureEnumerator(CancellationToken cancellationToken = default)
            {
                return new Enumerator(this, cancellationToken);
            }

            class Enumerator : FeatureEnumerator<TResult>
            {
                ExtremeQueryEnumerable<T, TResult> _enumerable;
                CancellationToken _cancellationToken;
                public Enumerator(ExtremeQueryEnumerable<T, TResult> enumerable, CancellationToken cancellationToken)
                {
                    this._enumerable = enumerable;
                    this._cancellationToken = cancellationToken;
                }

                protected override async Task<IFeatureEnumerator<TResult>> CreateEnumerator(bool @async)
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
                    return new ScalarFeatureEnumerator<TResult>(result);
                }
            }
        }
    }
}
