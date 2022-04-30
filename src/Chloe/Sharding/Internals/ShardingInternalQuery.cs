using Chloe.Sharding.QueryState;
using Chloe.Sharding.Visitors;
using System.Threading;

namespace Chloe.Sharding.Internals
{
    internal class ShardingInternalQuery<T> : FeatureEnumerable<T>
    {
        ShardingQuery<T> _query;

        internal ShardingInternalQuery(ShardingQuery<T> query)
        {
            this._query = query;
        }

        public override IFeatureEnumerator<T> GetFeatureEnumerator(CancellationToken cancellationToken = default)
        {
            ShardingQueryStateBase queryState = ShardingQueryExpressionResolver.Resolve(this._query.QueryExpression);

            IFeatureEnumerable<object> queryEnumerable = queryState.CreateQuery();

            var enumerator = queryEnumerable.Select(a => (T)a).GetFeatureEnumerator();

            return enumerator;
        }
    }
}
