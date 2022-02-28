namespace Chloe.Sharding
{
    public interface IShardingRoute
    {
        List<RouteTable> GetTables(ShardingDbContext shardingDbContext);
        List<RouteTable> GetTables(ShardingDbContext shardingDbContext, object shardingValue, ShardingOperator shardingOperator);
        RouteTable GetTable(ShardingDbContext shardingDbContext, object shardingValue);
        List<RouteTable> GetTablesByKey(ShardingDbContext shardingDbContext, object keyValue);
        SortResult SortTables(ShardingDbContext shardingDbContext, List<RouteTable> tables, List<Ordering> orderings);
    }
}
