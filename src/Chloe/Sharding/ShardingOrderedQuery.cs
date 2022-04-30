using Chloe.Query.QueryExpressions;
using System.Linq.Expressions;

namespace Chloe.Sharding
{
    internal class ShardingOrderedQuery<T> : ShardingQuery<T>, IOrderedQuery<T>
    {
        public ShardingOrderedQuery(QueryExpression exp) : base(exp)
        {

        }

        public IOrderedQuery<T> ThenBy<K>(Expression<Func<T, K>> keySelector)
        {
            OrderExpression e = new OrderExpression(typeof(T), this.QueryExpression, QueryExpressionType.ThenBy, keySelector);
            return new ShardingOrderedQuery<T>(e);
        }
        public IOrderedQuery<T> ThenByDesc<K>(Expression<Func<T, K>> keySelector)
        {
            OrderExpression e = new OrderExpression(typeof(T), this.QueryExpression, QueryExpressionType.ThenByDesc, keySelector);
            return new ShardingOrderedQuery<T>(e);
        }
    }
}
