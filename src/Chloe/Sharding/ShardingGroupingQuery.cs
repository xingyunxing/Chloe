using System.Linq.Expressions;

namespace Chloe.Sharding
{
    class ShardingGroupingQuery<T> : IGroupingQuery<T>
    {
        protected IGroupingQuery<T> _query;

        public ShardingGroupingQuery(IGroupingQuery<T> query)
        {
            this._query = query;
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
            return new ShardingQuery<TResult>(this._query.Select(selector));
        }
    }
}
