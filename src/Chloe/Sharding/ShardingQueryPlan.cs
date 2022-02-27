using System.Linq.Expressions;

namespace Chloe.Sharding
{
    internal class ShardingQueryPlan
    {
        public ShardingQueryModel QueryModel { get; set; }

        public List<PhysicTable> RouteTables { get; set; } = new List<PhysicTable>();
        public bool IsOrderedRouteTables { get; set; }

        public IShardingContext ShardingContext { get; set; }
        public LambdaExpression Condition { get; set; }
    }
}
