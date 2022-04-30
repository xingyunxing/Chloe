using Chloe.Routing;

namespace Chloe.Sharding
{
    internal class ShardingQueryPlan
    {
        public ShardingQueryPlan()
        {

        }

        public ShardingQueryModel QueryModel { get; set; }

        public List<IPhysicTable> Tables { get; set; } = new List<IPhysicTable>();
        public bool IsOrderedTables { get; set; }

        public IShardingContext ShardingContext { get; set; }
    }
}
