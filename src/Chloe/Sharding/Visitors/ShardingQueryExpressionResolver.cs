using Chloe.Query;
using Chloe.QueryExpressions;
using Chloe.Query.Visitors;
using Chloe.Sharding.QueryState;

namespace Chloe.Sharding.Visitors
{
    class ShardingQueryExpressionResolver : QueryExpressionResolverBase
    {
        ShardingQueryContext _queryContext;

        public ShardingQueryExpressionResolver(ShardingQueryContext queryContext)
        {
            this._queryContext = queryContext;
        }

        public static ShardingQueryStateBase Resolve(ShardingQueryContext queryContext, QueryExpression queryExpression)
        {
            ShardingQueryExpressionResolver shardingQueryExpressionResolver = new ShardingQueryExpressionResolver(queryContext);
            return queryExpression.Accept(shardingQueryExpressionResolver) as ShardingQueryStateBase;
        }

        public override IQueryState VisitRootQuery(RootQueryExpression exp)
        {
            IQueryState queryState = new ShardingRootQueryState(this._queryContext, exp);
            return queryState;
        }
    }
}
