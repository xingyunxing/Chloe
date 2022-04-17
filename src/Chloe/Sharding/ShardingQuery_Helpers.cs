using Chloe.Core.Visitors;
using Chloe.Sharding.Models;
using Chloe.Sharding.Queries;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Chloe.Sharding
{
    partial class ShardingQuery<T>
    {
        async Task<decimal?> QueryAverageAsync(LambdaExpression selector)
        {
            var aggSelector = ShardingHelpers.MakeAggregateSelector<T>(selector);

            Func<IQuery<T>, bool, Task<AggregateModel>> executor = async (query, @async) =>
            {
                var q = query.Select(aggSelector);
                AggregateModel result = @async ? await q.FirstAsync() : q.First();
                return result;
            };

            AggregateQuery<T, AggregateModel> aggQuery = new AggregateQuery<T, AggregateModel>(this.MakeQueryPlan(this), executor);

            decimal? sum = null;
            long count = 0;

            await aggQuery.AsAsyncEnumerable().Select(a => a.Result).ForEach(a =>
            {
                if (a.Sum == null)
                    return;

                sum = (sum ?? 0) + a.Sum.Value;
                count = count + a.Count;
            });

            if (sum == null)
                return null;

            decimal avg = sum.Value / count;
            return avg;
        }
    }
}
