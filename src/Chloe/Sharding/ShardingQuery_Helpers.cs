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
            var aggSelector = MakeAggregateSelector<T>(selector);

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

            var avg = sum.Value / count;
            return avg;
        }

        static Expression<Func<TSource, AggregateModel>> MakeAggregateSelector<TSource>(LambdaExpression selector)
        {
            var parameterExp = Expression.Parameter(typeof(TSource));
            var fieldAccessExp = Expression.Convert(ParameterExpressionReplacer.Replace(selector.Body, parameterExp), typeof(decimal?));

            var Sql_Sum_Call = Expression.Call(PublicConstants.MethodInfo_Sql_Sum_DecimalN, fieldAccessExp);
            MemberAssignment sumBind = Expression.Bind(typeof(AggregateModel).GetProperty(nameof(AggregateModel.Sum)), Sql_Sum_Call);

            var Sql_LongCount_Call = Expression.Call(PublicConstants.MethodInfo_Sql_LongCount.MakeGenericMethod(fieldAccessExp.Type), fieldAccessExp);
            MemberAssignment countBind = Expression.Bind(typeof(AggregateModel).GetProperty(nameof(AggregateModel.Count)), Sql_LongCount_Call);

            List<MemberBinding> bindings = new List<MemberBinding>(2);
            bindings.Add(sumBind);
            bindings.Add(countBind);

            NewExpression newExp = Expression.New(typeof(AggregateModel));
            Expression lambdaBody = Expression.MemberInit(newExp, bindings);

            var lambda = Expression.Lambda<Func<TSource, AggregateModel>>(lambdaBody, parameterExp);

            return lambda;
        }
    }
}
