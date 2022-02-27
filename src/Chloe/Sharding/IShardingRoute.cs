namespace Chloe.Sharding
{
    public interface IShardingRoute
    {
        List<PhysicTable> GetPhysicTables(ShardingDbContext shardingDbContext);
        List<PhysicTable> GetPhysicTables(ShardingDbContext shardingDbContext, object shardingValue, ShardingOperator shardingOperator);
        PhysicTable GetPhysicTable(ShardingDbContext shardingDbContext, object shardingValue);
        List<PhysicTable> GetPhysicTableByKey(ShardingDbContext shardingDbContext, object keyValue);
        SortResult SortTables(ShardingDbContext shardingDbContext, List<PhysicTable> physicTables, List<Ordering> orderings);
    }
}
