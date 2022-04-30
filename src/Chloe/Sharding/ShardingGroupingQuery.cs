using Chloe.Query.QueryExpressions;
using System.Linq.Expressions;

namespace Chloe.Sharding
{
    class ShardingGroupingQuery<T> : IGroupingQuery<T>
    {
        protected ShardingQuery<T> _fromQuery;
        List<LambdaExpression> _groupKeySelectors;

        public ShardingGroupingQuery(ShardingQuery<T> fromQuery, LambdaExpression keySelector)
        {
            this._fromQuery = fromQuery;
            this._groupKeySelectors = new List<LambdaExpression>(1) { keySelector };
        }

        public IGroupingQuery<T> AndBy<K>(Expression<Func<T, K>> keySelector)
        {
            throw new NotImplementedException();
        }
        public IGroupingQuery<T> Having(Expression<Func<T, bool>> predicate)
        {
            throw new NotImplementedException();
        }
        public IOrderedGroupingQuery<T> OrderBy<K>(Expression<Func<T, K>> keySelector)
        {
            throw new NotSupportedException();
        }
        public IOrderedGroupingQuery<T> OrderByDesc<K>(Expression<Func<T, K>> keySelector)
        {
            throw new NotSupportedException();
        }
        public IQuery<TResult> Select<TResult>(Expression<Func<T, TResult>> selector)
        {
            var e = new GroupingQueryExpression(typeof(TResult), this._fromQuery.QueryExpression, selector);
            e.GroupKeySelectors.AppendRange(this._groupKeySelectors);

            return new ShardingQuery<TResult>(e);
        }
    }
}
