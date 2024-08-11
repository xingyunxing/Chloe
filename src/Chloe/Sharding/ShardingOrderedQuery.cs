using Chloe.QueryExpressions;
using System.Linq.Expressions;

namespace Chloe.Sharding
{
    internal class ShardingOrderedQuery<T> : ShardingQuery<T>, IOrderedQuery<T>
    {
        public ShardingOrderedQuery(ShardingDbContextProvider dbContextProvider, QueryExpression exp) : base(dbContextProvider, exp)
        {

        }

        public IOrderedQuery<T> ThenBy<K>(Expression<Func<T, K>> keySelector)
        {
            OrderExpression e = new OrderExpression(typeof(T), this.QueryExpression, QueryExpressionType.ThenBy, keySelector);
            return new ShardingOrderedQuery<T>(this.DbContextProvider, e);
        }
        public IOrderedQuery<T> ThenByDesc<K>(Expression<Func<T, K>> keySelector)
        {
            OrderExpression e = new OrderExpression(typeof(T), this.QueryExpression, QueryExpressionType.ThenByDesc, keySelector);
            return new ShardingOrderedQuery<T>(this.DbContextProvider, e);
        }
    }
}
