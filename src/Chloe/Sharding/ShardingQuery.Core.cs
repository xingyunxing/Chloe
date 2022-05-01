using Chloe.QueryExpressions;
using Chloe.Sharding.Internals;
using System.Linq.Expressions;
using System.Reflection;

namespace Chloe.Sharding
{
    internal partial class ShardingQuery<T> : IQuery<T>
    {
        static readonly List<Expression> EmptyArgumentList = new List<Expression>();

        IFeatureEnumerable<T> GenerateIterator()
        {
            ShardingInternalQuery<T> internalQuery = new ShardingInternalQuery<T>(this);
            return internalQuery;
        }

        async Task<TResult> ExecuteAggregateQueryAsync<TResult>(MethodInfo method, Expression argument, bool checkArgument = true)
        {
            var q = this.CreateAggregateQuery<TResult>(method, argument, checkArgument);
            var iterator = q.GenerateIterator();
            return await iterator.SingleAsync();
        }
        ShardingQuery<TResult> CreateAggregateQuery<TResult>(MethodInfo method, Expression argument, bool checkArgument)
        {
            if (checkArgument)
                PublicHelper.CheckNull(argument);

            List<Expression> arguments = argument == null ? EmptyArgumentList : new List<Expression>(1) { argument };
            var q = this.CreateAggregateQuery<TResult>(method, arguments);
            return q;
        }
        ShardingQuery<TResult> CreateAggregateQuery<TResult>(MethodInfo method, List<Expression> arguments)
        {
            AggregateQueryExpression e = new AggregateQueryExpression(this.QueryExpression, method, arguments);
            var shardingQuery = new ShardingQuery<TResult>(e);
            return shardingQuery;
        }
        MethodInfo GetCalledMethod<TResult>(Expression<Func<TResult>> exp)
        {
            var body = (MethodCallExpression)exp.Body;
            return body.Method;
        }

    }
}
