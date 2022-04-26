using Chloe.Sharding.Queries;

namespace Chloe.Sharding.QueryState
{
    internal class ShardingGroupQueryState : ShardingQueryStateBase
    {
        public ShardingGroupQueryState(ShardingQueryStateBase prevQueryState) : base(prevQueryState)
        {

        }

        public override IFeatureEnumerable<object> CreateQuery()
        {
            GroupAggregateQuery groupAggregateQuery = new GroupAggregateQuery(this.CreateQueryPlan());
            return groupAggregateQuery;
        }
    }
}
