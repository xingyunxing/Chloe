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
                return this.CreateFeatureEnumerable(typeof(CountQueryEnumerable<>), this, false);
            }

            if (this.QueryExpression.Method.Name == "LongCount")
            {
                return this.CreateFeatureEnumerable(typeof(CountQueryEnumerable<>), this, true);
            }

            if (this.QueryExpression.Method.Name == "Any")
            {
                return this.CreateFeatureEnumerable(typeof(AnyQueryEnumerable<>), this);
            }

            if (this.QueryExpression.Method.Name == "Average")
            {
                return new AverageQueryEnumerable(this);
            }

            if (this.QueryExpression.Method.Name == "Sum")
            {
                return new SumQueryEnumerable(this);
            }

            if (this.QueryExpression.Method.Name == "Max" || this.QueryExpression.Method.Name == "Min")
            {
                var resultType = (this.QueryExpression.Arguments[0] as LambdaExpression).Body.Type;
                var queryEnumerableType = typeof(MaxMinQueryEnumerable<,>).MakeGenericType(this.QueryModel.RootEntityType, resultType);
                var queryEnumerable = queryEnumerableType.GetConstructor(new Type[2] { typeof(ShardingAggregateQueryState), typeof(bool) }).FastCreateInstance(this, this.QueryExpression.Method.Name == "Max");
                return (IFeatureEnumerable<object>)queryEnumerable;
            }

            throw new NotImplementedException();
        }

        IFeatureEnumerable<object> CreateFeatureEnumerable(Type queryEnumerableType, params object[] parameters)
        {
            queryEnumerableType = queryEnumerableType.MakeGenericType(this.QueryModel.RootEntityType);
            var queryEnumerable = queryEnumerableType.GetConstructors()[0].FastCreateInstance(parameters);
            return (IFeatureEnumerable<object>)queryEnumerable;
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

                    AggregateQuery<bool> aggQuery = new AggregateQuery<bool>(queryState.CreateQueryPlan(), executor);

                    var hasData = await aggQuery.AsAsyncEnumerable().Where(a => a.Result == true).AnyAsync(this._cancellationToken);

                    return new ScalarFeatureEnumerator<object>(hasData);
                }
            }
        }
    }
}
