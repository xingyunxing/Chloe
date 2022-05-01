using Chloe.QueryExpressions;
using System.Linq.Expressions;

namespace Chloe.Query
{
    class OrderedQuery<T> : Query<T>, IOrderedQuery<T>
    {
        public OrderedQuery(QueryExpression exp) : base(exp)
        {

        }
        public IOrderedQuery<T> ThenBy<K>(Expression<Func<T, K>> keySelector)
        {
            OrderExpression e = new OrderExpression(typeof(T), this.QueryExpression, QueryExpressionType.ThenBy, keySelector);
            return new OrderedQuery<T>(e);
        }
        public IOrderedQuery<T> ThenByDesc<K>(Expression<Func<T, K>> keySelector)
        {
            OrderExpression e = new OrderExpression(typeof(T), this.QueryExpression, QueryExpressionType.ThenByDesc, keySelector);
            return new OrderedQuery<T>(e);
        }
    }
}
