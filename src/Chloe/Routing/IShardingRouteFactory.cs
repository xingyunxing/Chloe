using Chloe.Sharding;

namespace Chloe.Routing
{
    public interface IShardingRouteFactory
    {
        IShardingRoute CreateRoute(ShardingDbContextProvider shardingDbContextProvider);
    }
    class ShardingRouteFactory : IShardingRouteFactory
    {
        Func<IShardingRoute> _routeFactory;

        public ShardingRouteFactory(Func<IShardingRoute> routeFactory)
        {
            this._routeFactory = routeFactory;
        }
        public IShardingRoute CreateRoute(ShardingDbContextProvider shardingDbContextProvider)
        {
            return this._routeFactory();
        }
    }
}
