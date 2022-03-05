using System.Linq.Expressions;

namespace Chloe.Sharding
{
    internal class ShardingQueryPlan
    {
        public ShardingQueryModel QueryModel { get; set; }

        public bool IsTrackingQuery { get; set; }

        public List<IPhysicTable> Tables { get; set; } = new List<IPhysicTable>();
        public bool IsOrderedTables { get; set; }

        public IShardingContext ShardingContext { get; set; }
        public LambdaExpression Condition { get; set; }
    }
}
