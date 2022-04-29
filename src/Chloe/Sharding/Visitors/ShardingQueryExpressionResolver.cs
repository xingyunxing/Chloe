using Chloe.Query;
using Chloe.Query.QueryExpressions;
using Chloe.Query.Visitors;
using Chloe.Sharding.QueryState;

namespace Chloe.Sharding.Visitors
{
    class ShardingQueryExpressionResolver : QueryExpressionVisitor
    {
        public static readonly ShardingQueryExpressionResolver Instance = new ShardingQueryExpressionResolver();
        public static ShardingQueryStateBase Resolve(QueryExpression queryExpression)
        {
            return queryExpression.Accept(Instance) as ShardingQueryStateBase;
        }

        public override IQueryState Visit(RootQueryExpression exp)
        {
            IQueryState queryState = new ShardingRootQueryState(exp);
            return queryState;
        }
    }
}
