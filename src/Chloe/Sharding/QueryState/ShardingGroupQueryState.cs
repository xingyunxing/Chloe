using Chloe.Sharding.Enumerables;

namespace Chloe.Sharding.QueryState
{
    internal class ShardingGroupQueryState : ShardingQueryStateBase
    {
        public ShardingGroupQueryState(ShardingQueryStateBase prevQueryState) : base(prevQueryState)
        {

        }

        public override IFeatureEnumerable<object> CreateQuery()
        {
            var groupAggregateQueryEnumerable = new GroupAggregateQueryEnumerable(this.CreateQueryPlan());
            return groupAggregateQueryEnumerable;
        }
    }
}
