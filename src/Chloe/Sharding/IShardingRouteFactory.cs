namespace Chloe.Sharding
{
    public interface IShardingRouteFactory
    {
        IShardingRoute CreateRoute(ShardingDbContext shardingDbContext);
    }
    class ShardingRouteFactory : IShardingRouteFactory
    {
        Func<IShardingRoute> _routeFactory;

        public ShardingRouteFactory(Func<IShardingRoute> routeFactory)
        {
            this._routeFactory = routeFactory;
        }
        public IShardingRoute CreateRoute(ShardingDbContext shardingDbContext)
        {
            return this._routeFactory();
        }
    }
}
